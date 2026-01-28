from __future__ import annotations

import json
import urllib.parse
from datetime import datetime, timezone
from typing import Set, List, Any
from pathlib import Path

import pandas as pd
from sqlalchemy import create_engine, text
from sqlalchemy.engine import Engine

import gspread
from google.oauth2.service_account import Credentials
from datetime import datetime, timezone, timedelta

from pathlib import Path
import os

import warnings
warnings.filterwarnings('ignore')

# =========================================================
# CONFIG
# =========================================================

SEP = " | "
NEWLINE_COLS = ["description", "COLOR"]

FABRIC_TABLE = "dbo.Fabric"
TRIM_TABLE = "dbo.TrimAndLabels"

SPREADSHEET_ID = "1HVERYwjbyLroIpJY28-p9RNKz4Q6SyZEYamiP0rLhcY"
FABRIC_SHEET_NAME = "Fabric"
TRIM_SHEET_NAME = "TrimAndLabels"

BASE_DIR = Path(__file__).resolve().parent


# Put these JSON files next to this script for portability
GSERVICE_ACCOUNT_JSON = os.environ.get(
    "HADDAD_GS_SERVICE_JSON",
    str(BASE_DIR / "Account_LS_EHD_MANAGEMENT.json")
    )


SQL_CONFIG_PATH = os.environ.get(
    "HADDAD_SQL_CONFIG",
    str(BASE_DIR / "ServerPassword.json")
    )

# Sheet layout
META_ROW = 1      # Row 1: last load time
HEADER_ROW = 2    # Row 2: header
DATA_START_ROW = 3


# =========================================================
# SQL HELPERS
# =========================================================
def get_selected_file_names_from_input_dir(input_dir: str) -> List[str]:
    folder = Path(input_dir)
    pdfs = sorted([p.name for p in folder.glob("*.pdf")])
    return [fn for fn in pdfs if fn]

def read_sql_config(path: str) -> tuple[str, str, str, str]:
    with open(path, "r", encoding="utf-8") as f:
        p = json.load(f)["profiles"]["ERP_Import"]
    return p["server"], p["database"], p["user"], p["password"]


def create_engine_sqlserver(server: str, database: str, user: str, password: str) -> Engine:
    conn_str = (
        "DRIVER=ODBC Driver 17 for SQL Server;"
        f"SERVER={server};DATABASE={database};UID={user};PWD={password}"
    )
    return create_engine(f"mssql+pyodbc:///?odbc_connect={urllib.parse.quote_plus(conn_str)}")


def read_sql_table(engine: Engine, full_table: str) -> pd.DataFrame:
    schema, table = full_table.split(".", 1)
    q = text(f"SELECT * FROM {schema}.{table} ORDER BY file_name, matched_groups, page")
    return pd.read_sql(q, engine)


# =========================================================
# GOOGLE SHEETS HELPERS
# =========================================================
def delete_rows_by_file_names(ws: gspread.Worksheet, file_names: List[str], file_col_name: str = "file_name") -> int:
    """
    Instead of deleting rows (can fail when deleting all non-frozen rows),
    we CLEAR the row contents for matching file_name.
    Return number of cleared rows.
    """
    if not file_names:
        return 0

    header = get_header(ws)
    if not header or file_col_name not in header:
        return 0

    idx = header.index(file_col_name) + 1  # 1-based col
    col_values = ws.col_values(idx)  # includes row1..N

    to_clear = []
    for r, v in enumerate(col_values, start=1):
        if r < DATA_START_ROW:
            continue
        fn = (v or "").strip()
        if fn and fn in file_names:
            to_clear.append(r)

    if not to_clear:
        return 0

    # Clear full row range A..last_col for each row
    last_col = len(header)
    cleared = 0
    for r in to_clear:
        ws.batch_clear([f"A{r}:{chr(64+last_col)}{r}"])
        cleared += 1

    return cleared

def sync_sql_to_google_sheet_replace_files(
    engine: Engine,
    spreadsheet_id: str,
    service_account_json_path: str,
    file_names_to_replace: List[str],
    fabric_table: str = FABRIC_TABLE,
    trim_table: str = TRIM_TABLE,
    fabric_sheet_name: str = FABRIC_SHEET_NAME,
    trim_sheet_name: str = TRIM_SHEET_NAME,
    file_col: str = "file_name",
) -> None:
    file_names_to_replace = sorted(list({(x or "").strip() for x in file_names_to_replace if x}))
    if not file_names_to_replace:
        print("No file names to replace.")
        return

    # 1) Read SQL
    df_fabric = read_sql_table(engine, fabric_table)
    df_trim = read_sql_table(engine, trim_table)

    # 2) Convert SEP -> newline only for TRIM rows
    df_trim = convert_sep_to_newline_for_trim_rows(df_trim)

    # 3) Connect to Google Sheet
    gc = get_gspread_client(service_account_json_path)
    sh = gc.open_by_key(spreadsheet_id)

    ws_fabric = ensure_worksheet(sh, fabric_sheet_name)
    ws_trim = ensure_worksheet(sh, trim_sheet_name)

    # 4) Headers
    fabric_cols = list(df_fabric.columns) if not df_fabric.empty else [file_col]
    trim_cols = list(df_trim.columns) if not df_trim.empty else [file_col]
    if file_col not in fabric_cols:
        fabric_cols = [file_col] + fabric_cols
    if file_col not in trim_cols:
        trim_cols = [file_col] + trim_cols

    write_header_if_needed(ws_fabric, fabric_cols)
    write_header_if_needed(ws_trim, trim_cols)

    # 5) DELETE rows in sheets for these file_names
    del_f = delete_rows_by_file_names(ws_fabric, file_names_to_replace, file_col_name=file_col)
    del_t = delete_rows_by_file_names(ws_trim, file_names_to_replace, file_col_name=file_col)
    print(f"Deleted rows: Fabric={del_f}, Trim={del_t}")

    # 6) Filter SQL rows for these file_names, then append back
    df_f = df_fabric[df_fabric[file_col].astype(str).isin(file_names_to_replace)].copy() if not df_fabric.empty else pd.DataFrame(columns=fabric_cols)
    df_t = df_trim[df_trim[file_col].astype(str).isin(file_names_to_replace)].copy() if not df_trim.empty else pd.DataFrame(columns=trim_cols)

    rows_f = df_to_rows_for_sheet(df_f, fabric_cols) if not df_f.empty else []
    rows_t = df_to_rows_for_sheet(df_t, trim_cols) if not df_t.empty else []

    if rows_f:
        ws_fabric.append_rows(rows_f, value_input_option="RAW")
        print(f"  - Fabric: appended {len(rows_f)} row(s)")
    if rows_t:
        ws_trim.append_rows(rows_t, value_input_option="RAW")
        print(f"  - TrimAndLabels: appended {len(rows_t)} row(s)")

    update_last_load_time(ws_fabric)
    update_last_load_time(ws_trim)
    print("Done (REPLACE mode).")

def get_gspread_client(service_account_json_path: str) -> gspread.Client:
    scopes = [
        "https://www.googleapis.com/auth/spreadsheets",
        "https://www.googleapis.com/auth/drive",
    ]
    creds = Credentials.from_service_account_file(service_account_json_path, scopes=scopes)
    return gspread.authorize(creds)


def ensure_worksheet(sh: gspread.Spreadsheet, title: str, rows: int = 2000, cols: int = 50) -> gspread.Worksheet:
    try:
        return sh.worksheet(title)
    except gspread.WorksheetNotFound:
        return sh.add_worksheet(title=title, rows=str(rows), cols=str(cols))


def update_last_load_time(ws: gspread.Worksheet) -> None:
    """
    Row 1:
      A1 = "Last load"
      B1 = ISO timestamp (UTC)  (you can change timezone format if you want)
    """
    ts = datetime.now(timezone(timedelta(hours=7))).strftime("%Y-%m-%d %H:%M:%S ICT")
    ws.update("A1:B1", [["Last load", ts]], value_input_option="RAW")


def get_header(ws: gspread.Worksheet) -> List[str]:
    """
    Header is stored at row 2.
    """
    row = ws.row_values(HEADER_ROW)
    return [c.strip() for c in row] if row else []


def write_header_if_needed(ws: gspread.Worksheet, cols: List[str]) -> None:
    """
    Keep row 1 for last-load.
    Put header in row 2.
    """
    existing = get_header(ws)

    # If sheet is empty, we should ensure row1 meta exists too (optional)
    if not ws.get_all_values():
        update_last_load_time(ws)

    if existing != cols:
        ws.update(f"A{HEADER_ROW}", [cols], value_input_option="RAW")


def read_sheet_file_names(ws: gspread.Worksheet, file_col_name: str = "file_name") -> Set[str]:
    """
    Read existing file_name from the sheet.
    - Row 1: meta
    - Row 2: header
    - Row 3+: data
    """
    header = get_header(ws)
    if not header or file_col_name not in header:
        return set()

    idx = header.index(file_col_name) + 1  # 1-based col index in gspread
    # Read the whole column values from row 3 down
    col_values = ws.col_values(idx)

    # col_values includes rows 1..N, so skip first 2 rows
    out = set()
    for v in col_values[DATA_START_ROW - 1 :]:
        fn = (v or "").strip()
        if fn:
            out.add(fn)
    return out


def df_to_rows_for_sheet(df: pd.DataFrame, cols: List[str]) -> List[List[Any]]:
    df2 = df.copy()

    for c in cols:
        if c not in df2.columns:
            df2[c] = ""

    df2 = df2[cols].fillna("")

    rows: List[List[Any]] = []
    for _, r in df2.iterrows():
        row = []
        for c in cols:
            v = r[c]
            if isinstance(v, float) and pd.isna(v):
                v = ""
            row.append(v)
        rows.append(row)
    return rows


def convert_sep_to_newline_for_trim_rows(df: pd.DataFrame) -> pd.DataFrame:
    """
    ONLY for rows where matched_groups contains 'TRIM':
      - description: " | " -> "\n"
      - COLOR: " | " -> "\n"
    """
    if df.empty:
        return df

    df = df.copy()
    if "matched_groups" not in df.columns:
        return df

    mask_trim = df["matched_groups"].astype(str).str.upper().str.contains("TRIM", na=False)
    for col in NEWLINE_COLS:
        if col in df.columns:
            s = df.loc[mask_trim, col].astype(str)
            s = s.str.replace(SEP, "\n", regex=False)
            df.loc[mask_trim, col] = s

    return df


# =========================================================
# MAIN SYNC
# =========================================================
def sync_sql_to_google_sheet_append_only_new_files(
        engine: Engine,
        spreadsheet_id: str,
        service_account_json_path: str,
        fabric_table: str = FABRIC_TABLE,
        trim_table: str = TRIM_TABLE,
        fabric_sheet_name: str = FABRIC_SHEET_NAME,
        trim_sheet_name: str = TRIM_SHEET_NAME,
        file_col: str = "file_name",
    ) -> None:
    # 1) Read SQL
    df_fabric = read_sql_table(engine, fabric_table)
    df_trim = read_sql_table(engine, trim_table)

    # 2) Convert SEP -> newline only for TRIM rows
    df_trim = convert_sep_to_newline_for_trim_rows(df_trim)

    # 3) Connect to Google Sheet
    gc = get_gspread_client(service_account_json_path)
    sh = gc.open_by_key(spreadsheet_id)

    ws_fabric = ensure_worksheet(sh, fabric_sheet_name)
    ws_trim = ensure_worksheet(sh, trim_sheet_name)

    # 4) Headers (row 2)
    fabric_cols = list(df_fabric.columns) if not df_fabric.empty else [file_col]
    trim_cols = list(df_trim.columns) if not df_trim.empty else [file_col]

    if file_col not in fabric_cols:
        fabric_cols = [file_col] + fabric_cols
    if file_col not in trim_cols:
        trim_cols = [file_col] + trim_cols

    write_header_if_needed(ws_fabric, fabric_cols)
    write_header_if_needed(ws_trim, trim_cols)

    # 5) Determine new files comparing with sheet
    sheet_files_all = read_sheet_file_names(ws_fabric, file_col).union(
        read_sheet_file_names(ws_trim, file_col)
    )

    sql_files = set(df_fabric[file_col].dropna().astype(str).str.strip().tolist()).union(
        set(df_trim[file_col].dropna().astype(str).str.strip().tolist())
    )
    new_files = sorted([fn for fn in sql_files if fn and fn not in sheet_files_all])

    if not new_files:
        print("No new files to append to Google Sheet.")
        # still update last load time (optional)
        update_last_load_time(ws_fabric)
        update_last_load_time(ws_trim)
        return

    print(f"Appending {len(new_files)} new file(s) to Google Sheet...")

    # 6) Filter only new files rows
    df_f_new = df_fabric[df_fabric[file_col].astype(str).isin(new_files)].copy() if not df_fabric.empty else pd.DataFrame(columns=fabric_cols)
    df_t_new = df_trim[df_trim[file_col].astype(str).isin(new_files)].copy() if not df_trim.empty else pd.DataFrame(columns=trim_cols)

    rows_f = df_to_rows_for_sheet(df_f_new, fabric_cols) if not df_f_new.empty else []
    rows_t = df_to_rows_for_sheet(df_t_new, trim_cols) if not df_t_new.empty else []

    # 7) Append after existing data (append_rows does that automatically)
    if rows_f:
        ws_fabric.append_rows(rows_f, value_input_option="RAW")
        print(f"  - Fabric: appended {len(rows_f)} row(s)")

    if rows_t:
        ws_trim.append_rows(rows_t, value_input_option="RAW")
        print(f"  - TrimAndLabels: appended {len(rows_t)} row(s)")

    # 8) Update last load time in row 1
    update_last_load_time(ws_fabric)
    update_last_load_time(ws_trim)

    print("Done.")


if __name__ == "__main__":
    import argparse

    ap = argparse.ArgumentParser()
    ap.add_argument("--input-dir", required=True, help="Folder contains uploaded PDFs (from API)")
    args = ap.parse_args()

    selected_files = get_selected_file_names_from_input_dir(args.input_dir)
    print(f"Selected files to REPLACE in Sheet: {len(selected_files)}")

    server, database, user, password = read_sql_config(SQL_CONFIG_PATH)
    engine = create_engine_sqlserver(server, database, user, password)

    sync_sql_to_google_sheet_replace_files(
        engine=engine,
        spreadsheet_id=SPREADSHEET_ID,
        service_account_json_path=GSERVICE_ACCOUNT_JSON,
        file_names_to_replace=selected_files,
    )