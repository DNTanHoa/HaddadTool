from __future__ import annotations

import os
import re
import json
import urllib.parse
from pathlib import Path
from typing import List, Dict, Any, Tuple

import pandas as pd
import pdfplumber
from sqlalchemy import create_engine, text
from sqlalchemy.engine import Engine

# optional camelot
try:
    import camelot  # type: ignore
    HAVE_CAMELOT = True
except Exception:
    HAVE_CAMELOT = False


# =========================================================
# 1) DB CONFIG + ENGINE
# =========================================================
def read_config(path: str) -> tuple[str, str, str, str]:
    with open(path, "r", encoding="utf-8") as f:
        p = json.load(f)["profiles"]["ERP_Import"]
    return p["server"], p["database"], p["user"], p["password"]


def create_engine_sqlserver(server: str, database: str, user: str, password: str) -> Engine:
    conn_str = (
        "DRIVER=ODBC Driver 17 for SQL Server;"
        f"SERVER={server};DATABASE={database};UID={user};PWD={password}"
    )
    return create_engine(f"mssql+pyodbc:///?odbc_connect={urllib.parse.quote_plus(conn_str)}")


# =========================================================
# 2) Helpers
# =========================================================
def sanitize_sheet_name(name: str) -> str:
    name = re.sub(r"[:\\/?*\[\]]", "_", name).strip()
    return (name or "sheet")[:31]


def norm(s):
    if s is None:
        return ""
    if isinstance(s, float) and pd.isna(s):
        return ""
    return re.sub(r"\s+", " ", str(s)).strip().replace("•", "")


def get_joined_cell(row_series, cols_to_join):
    parts = [norm(row_series.get(c, "")) for c in cols_to_join]
    parts = [p for p in parts if p]
    return norm(" ".join(parts))


def read_color_under_position(row_i, col, all_cols, join_width=2):
    """
    Đọc cell theo đúng position column `col`, nếu bị split sang cột kế bên thì ghép.
    """
    try:
        j = all_cols.index(col)
    except ValueError:
        return norm(row_i.get(col, ""))

    span = all_cols[j : j + join_width]
    return get_joined_cell(row_i, span)


def extract_top_right_text(page, x_min_ratio=0.60, y_max_ratio=0.25) -> str:
    W, H = page.width, page.height
    x0 = W * x_min_ratio
    y1 = H * y_max_ratio
    try:
        cropped = page.crop((x0, 0, W, y1))
        text_ = cropped.extract_text() or ""
        return re.sub(r"\s+", " ", text_).strip()
    except Exception:
        words = page.extract_words() or []
        tr = [w for w in words if w.get("x0", 0) >= x0 and w.get("top", 1e9) <= y1]
        tr.sort(key=lambda w: (round(w.get("top", 0), 1), w.get("x0", 0)))
        return re.sub(r"\s+", " ", " ".join(w["text"] for w in tr)).strip()


def extract_top_left_first_line(page, x_max_ratio=0.55, y_max_ratio=0.20, line_tol=2.5) -> str:
    W, H = page.width, page.height
    x1 = W * x_max_ratio
    y1 = H * y_max_ratio

    words = page.extract_words() or []
    if not words:
        return ""

    cand = [w for w in words if w.get("x0", 1e9) <= x1 and w.get("top", 1e9) <= y1]
    if not cand:
        return ""

    cand.sort(key=lambda w: (w.get("top", 1e9), w.get("x0", 1e9)))
    first_top = cand[0].get("top", 0)

    first_line = [w for w in cand if abs(w.get("top", 0) - first_top) <= line_tol]
    first_line.sort(key=lambda w: w.get("x0", 1e9))

    text_ = " ".join(w["text"] for w in first_line if w.get("text"))
    return re.sub(r"\s+", " ", text_).strip()


def is_valid_table_df(df: pd.DataFrame, min_rows=2, min_cols=2, min_nonempty_cells=4) -> bool:
    if df is None or df.empty:
        return False
    if df.shape[0] < min_rows or df.shape[1] < min_cols:
        return False

    nonempty = 0
    for r in range(df.shape[0]):
        for c in range(df.shape[1]):
            v = df.iat[r, c]
            if v is None:
                continue
            if isinstance(v, str):
                v = re.sub(r"\s+", " ", v).strip()
                if not v:
                    continue
            nonempty += 1
    return nonempty >= min_nonempty_cells


def detect_tables_on_page(pdf_path: str, page_idx_1based: int, page) -> Tuple[bool, str, List[pd.DataFrame]]:
    # 1) Camelot lattice
    if HAVE_CAMELOT:
        try:
            tables = camelot.read_pdf(pdf_path, pages=str(page_idx_1based), flavor="lattice")
            dfs: List[pd.DataFrame] = []
            if tables and len(tables) > 0:
                for t in tables:
                    df = t.df.copy().replace({"": None})
                    if is_valid_table_df(df):
                        dfs.append(df)
                if dfs:
                    return True, "camelot", dfs
        except Exception:
            pass

    # 2) pdfplumber fallback
    try:
        tbls = page.extract_tables() or []
        dfs2: List[pd.DataFrame] = []
        for tbl in tbls:
            df = pd.DataFrame(tbl).replace({"": None})
            if is_valid_table_df(df):
                dfs2.append(df)
        if dfs2:
            return True, "pdfplumber", dfs2
    except Exception:
        pass

    return False, "none", []


def find_keywords_in_text(text_: str, keywords: List[str]) -> Tuple[bool, str]:
    if not text_:
        return False, ""
    hits = []
    for kw in keywords:
        if re.search(kw, text_, flags=re.IGNORECASE):
            hits.append(kw)
    return (len(hits) > 0), ", ".join(hits)


def df_to_wide_rows(df: pd.DataFrame, page: int, table_index: int, meta: Dict[str, Any]) -> List[Dict[str, Any]]:
    out: List[Dict[str, Any]] = []
    ncols = df.shape[1]
    for r in range(df.shape[0]):
        row_dict: Dict[str, Any] = {"page": page, "table_index": table_index, "row": r, **meta}
        for c in range(ncols):
            v = df.iat[r, c]
            if isinstance(v, str):
                v = re.sub(r"\s+", " ", v).strip()
            row_dict[f"c{c}"] = v if v not in ("", None) else None
        out.append(row_dict)
    return out

def pick_dev_or_vendor(label_to_row, col):
    dev = pick_first_value(label_to_row, ["DEV CODE", "DEV_CODE", "DEV"], col)
    if dev:
        return dev
    return pick_first_value(label_to_row, ["VENDOR REF NO", "VENDOR REF", "VENDOR"], col)

# =========================================================
# 3) PDF -> (detect + tables_wide) IN MEMORY
# =========================================================
def pdf_to_detect_and_tables_wide(
    pdf_path: str,
    filter_groups: List[Dict[str, Any]],
    x_min_ratio=0.60,
    y_max_ratio=0.25,
) -> tuple[pd.DataFrame, pd.DataFrame]:
    detect_rows: List[Dict[str, Any]] = []
    tables_wide_rows: List[Dict[str, Any]] = []

    with pdfplumber.open(pdf_path) as pdf:
        for page_idx, page in enumerate(pdf.pages, start=1):
            top_right_text = extract_top_right_text(page, x_min_ratio=x_min_ratio, y_max_ratio=y_max_ratio)
            top_left_first_line = extract_top_left_first_line(page, x_max_ratio=0.55, y_max_ratio=0.20)

            has_table, method, dfs = detect_tables_on_page(pdf_path, page_idx, page)

            matched_groups: List[Tuple[str, str]] = []
            for g in filter_groups:
                group_name = sanitize_sheet_name(str(g.get("sheet", "group")))
                keywords = g.get("keywords", []) or []
                matched, matched_kws = find_keywords_in_text(top_right_text, keywords)
                if matched:
                    matched_groups.append((group_name, matched_kws))

            if matched_groups and has_table:
                detect_row = {
                    "page": page_idx,
                    "top_left_first_line": top_left_first_line,
                    "top_right_text": top_right_text,
                    "table_method": method,
                    "tables_found": len(dfs),
                    "matched_groups": ", ".join(g for g, _ in matched_groups),
                    "matched_keywords": " | ".join(f"{g}: {kws}" for g, kws in matched_groups),
                }
                detect_rows.append(detect_row)

                meta = {
                    "matched_groups": detect_row["matched_groups"],
                    "matched_keywords": detect_row["matched_keywords"],
                    "top_left_first_line": top_left_first_line,
                    "top_right_text": top_right_text,
                    "table_method": method,
                }

                for t_i, df in enumerate(dfs, start=1):
                    tables_wide_rows.extend(df_to_wide_rows(df, page_idx, t_i, meta))

    return pd.DataFrame(detect_rows), pd.DataFrame(tables_wide_rows)


# =========================================================
# 4) Parser: COLORWAYS row finder
# =========================================================
def normalize_for_match(s: str) -> str:
    s = norm(s).upper()
    return re.sub(r"[^A-Z0-9]+", "", s)


def is_color_value(s: str) -> bool:
    s = norm(s)
    if not s:
        return False
    if s.upper() in {"N/A", "NA"}:
        return False
    return len(s) >= 3


def split_color(s: str) -> tuple[str, str]:
    """
    Fabric rule: 3 ký tự đầu là code, phần còn lại là name
    """
    s = norm(s)
    code = s[:3].strip()
    name = s[3:].strip()
    name = re.sub(r"^[\-\–\—\s]+", "", name).strip()
    return code, name


def find_colorway_row_and_span(df_table: pd.DataFrame, cols: list[str], max_window: int = 6):
    kw1 = normalize_for_match("COLORWAY")
    kw2 = normalize_for_match("COLORWAYS")

    best = None
    for i in range(len(df_table)):
        row = df_table.iloc[i]
        whole = normalize_for_match(" ".join(norm(row.get(c, "")) for c in cols))
        if kw1 not in whole and kw2 not in whole:
            continue

        for start in range(len(cols)):
            for w in range(1, min(max_window, len(cols) - start) + 1):
                span = cols[start : start + w]
                joined = normalize_for_match(get_joined_cell(row, span))
                if not joined:
                    continue

                matched = None
                if joined == kw2 or kw2 in joined:
                    matched = kw2
                elif joined == kw1 or kw1 in joined:
                    matched = kw1

                if matched:
                    extra = max(0, len(joined) - len(matched))
                    score = (w, extra, start)
                    cand = (score, i, span)
                    if best is None or cand[0] < best[0]:
                        best = cand

    if best is None:
        for i in range(len(df_table)):
            row = df_table.iloc[i]
            row_text = " | ".join(norm(row.get(c, "")) for c in cols if norm(row.get(c, "")))
            if re.search(r"\bCOLORWAYS?\b", row_text, flags=re.IGNORECASE):
                return i, []
        return None, []

    _, row_idx, span_cols = best
    return row_idx, span_cols


# =========================================================
# 5) Parse TRIM/LABELS
#    - Zip merge ONLY for TRIM group
#    - Use separator " | " instead of newline (SQL-safe)
# =========================================================
def pick_value(label_to_row, label, col):
    if label not in label_to_row:
        return ""
    v = label_to_row[label].get(col, "")
    return norm(v)


def pick_first_value(label_to_row, labels, col):
    for lb in labels:
        key = lb.upper()
        if key in label_to_row:
            return norm(label_to_row[key].get(col, ""))
    return ""


def join_trim_description(internal_code: str, position: str, name: str) -> str:
    parts = [p for p in [norm(internal_code), norm(position), norm(name)] if p]
    return " + ".join(parts)


def parse_one_table_to_trim_rows(df_table: pd.DataFrame, meta: dict) -> list[dict]:
    SEP = " | "  # ✅ store in SQL, convert back to newline when writing to Google Sheet if needed

    cols = [c for c in df_table.columns if re.fullmatch(r"c\d+", c)]
    if not cols:
        return []

    # 1) Position row = row đầu tiên
    header = df_table.iloc[0].to_dict()
    position_cols = []
    for c in cols:
        if c == "c0":
            continue
        pos = norm(header.get(c))
        if pos:
            position_cols.append((c, pos))
    if not position_cols:
        return []

    # 2) Map label rows (c0 là label)
    label_to_row = {}
    for i in range(len(df_table)):
        label = norm(df_table.iloc[i].get("c0", ""))
        if label:
            label_to_row[label.upper()] = df_table.iloc[i]

    # 3) Find COLORWAY(S)
    color_idx, _span_cols = find_colorway_row_and_span(df_table, cols, max_window=6)
    if color_idx is None:
        return []

    # ---------------------------------------------------------
    # Helper: detect zipper teeth/tape/pull columns
    # ---------------------------------------------------------
    def _pos_norm(p: str) -> str:
        return re.sub(r"\s+", " ", norm(p).lower())

    teeth_col = None
    tape_col = None
    pull_cols: list[str] = []

    for c, p in position_cols:
        pn = _pos_norm(p)
        if re.search(r"\bzipper\s*teeth\b", pn) and teeth_col is None:
            teeth_col = c
        elif re.search(r"\bzipper\s*tape\b", pn) and tape_col is None:
            tape_col = c
        elif re.search(r"\bzipper\s*pull\b", pn):
            pull_cols.append(c)

    is_trim_group = "TRIM" in str(meta.get("matched_groups", "")).upper()
    has_zipper_triplet = bool(is_trim_group and teeth_col and tape_col and len(pull_cols) > 0)

    out: list[dict] = []

    # ---------------------------------------------------------
    # SPECIAL RULE (TRIM only): zipper teeth + tape + pull
    # -> 1 row only (per COLORWAYS line)
    # ---------------------------------------------------------
    skip_cols = set()
    if has_zipper_triplet:
        skip_cols.update([teeth_col, tape_col, *pull_cols])

        def _build_desc_for_col(col: str, position: str) -> str:
            dev_or_vendor = pick_dev_or_vendor(label_to_row, col)
            internal_code = pick_value(label_to_row, "INTERNAL CODE", col)
            name = pick_first_value(label_to_row, ["NAME", "ITEM NAME", "TRIM NAME", "DESCRIPTION"], col)


            parts = [internal_code, position, dev_or_vendor, name]
            parts = [p for p in parts if p]
            return " - ".join(parts)

        teeth_pos = next((p for c, p in position_cols if c == teeth_col), "zipper teeth")
        tape_pos = next((p for c, p in position_cols if c == tape_col), "zipper tape")
        pull_positions = [next((p for c, p in position_cols if c == pc), "zipper pull") for pc in pull_cols]

        desc_teeth = _build_desc_for_col(teeth_col, teeth_pos)
        desc_tape = _build_desc_for_col(tape_col, tape_pos)
        desc_pulls = [_build_desc_for_col(pc, ppos) for pc, ppos in zip(pull_cols, pull_positions)]

        # ✅ store combined description using SEP
        merged_description = SEP.join([d for d in [desc_teeth, desc_tape, *desc_pulls] if d])

        # ITEM DESCRIPTION: lấy theo teeth
        item_desc_teeth = pick_first_value(
            label_to_row,
            ["LOCATION/PLACEMENT", "LOCATION / PLACEMENT", "LOCATION", "PLACEMENT"],
            teeth_col
        )
        if not item_desc_teeth:
            item_desc_teeth = teeth_pos

        # COLORWAYS rows:
        # - garment color = c0
        # - trim tape/pull = by their columns (do NOT use teeth col)
        for i in range(color_idx + 1, len(df_table)):
            row_i = df_table.iloc[i]

            color_garment = norm(row_i.get("c0", ""))
            if not is_color_value(color_garment):
                continue

            tape_trim = read_color_under_position(row_i, tape_col, cols, join_width=2)
            pull_trims = [read_color_under_position(row_i, pc, cols, join_width=2) for pc in pull_cols]
            
            color_combined = SEP.join([x for x in [color_garment, norm(tape_trim), *[norm(x) for x in pull_trims]] if norm(x)])

            out.append({
                "SUPPLIER": "",
                "STYLE_NO": meta.get("style_number", ""),
                "description": merged_description,
                "ITEM DESCRIPTION": item_desc_teeth,   # theo teeth
                "COLOR": color_combined,               # garment | tape | pull
                "color TRIM": norm(tape_trim),         # Color Trim lấy theo tape
                "DEL": "",
                "date approved": "",
                "Status2": "",
                "page": meta.get("page"),
                "matched_groups": meta.get("matched_groups", ""),
                "top_right_text": meta.get("top_right_text", ""),
            })

    # ---------------------------------------------------------
    # NORMAL RULE (UPDATED): apply GARMENT (c0) for ALL TRIM + LABELS
    # COLOR = "garment | trim"
    # ---------------------------------------------------------
    for col, position in position_cols:
        if col in skip_cols:
            continue

        internal_code = pick_value(label_to_row, "INTERNAL CODE", col)
        name = pick_first_value(label_to_row, ["NAME", "ITEM NAME", "TRIM NAME", "DESCRIPTION"], col)

        item_desc = pick_first_value(
            label_to_row,
            ["LOCATION/PLACEMENT", "LOCATION / PLACEMENT", "LOCATION", "PLACEMENT"],
            col
        )
        if not item_desc:
            item_desc = position

        desc = join_trim_description(internal_code, position, name)

        for i in range(color_idx + 1, len(df_table)):
            row_i = df_table.iloc[i]

            # ✅ GARMENT COLOR always from c0
            color_garment = norm(row_i.get("c0", ""))
            if not is_color_value(color_garment):
                continue

            # ✅ TRIM/LABEL color under current position column
            trim_cell = read_color_under_position(row_i, col, cols, join_width=2)
            trim_cell = norm(trim_cell)

            # nếu trim cell rỗng thì skip (tuỳ bạn)
            if not is_color_value(trim_cell):
                continue

            # ✅ Combined COLOR: garment | trim
            color_combined = SEP.join([x for x in [color_garment, trim_cell] if x])

            out.append({
                "SUPPLIER": "",
                "STYLE_NO": meta.get("style_number", ""),
                "description": desc,
                "ITEM DESCRIPTION": item_desc,
                "COLOR": color_combined,      # ✅ garment | trim
                "color TRIM": trim_cell,      # ✅ trim only
                "DEL": "",
                "date approved": "",
                "Status2": "",
                "page": meta.get("page"),
                "matched_groups": meta.get("matched_groups", ""),
                "top_right_text": meta.get("top_right_text", ""),
            })
    return out


# =========================================================
# 6) Parse FABRIC
# =========================================================
def parse_one_table_to_fabric_rows(df_table: pd.DataFrame, meta: dict) -> list[dict]:
    cols = [c for c in df_table.columns if re.fullmatch(r"c\d+", c)]
    if not cols:
        return []

    header = df_table.iloc[0].to_dict()
    position_cols = []
    for c in cols:
        if c == "c0":
            continue
        pos = norm(header.get(c))
        if pos:
            position_cols.append((c, pos))
    if not position_cols:
        return []

    color_idx, _span_cols = find_colorway_row_and_span(df_table, cols, max_window=6)
    if color_idx is None:
        return []

    out = []
    for col, position in position_cols:
        for i in range(color_idx + 1, len(df_table)):
            row_i = df_table.iloc[i]
            cell = read_color_under_position(row_i, col, cols, join_width=2)
            if not is_color_value(cell):
                continue

            color_code, color_name = split_color(cell)

            out.append({
                "STYLE_NO": meta.get("style_number", ""),
                "POSITION": position,
                "COLOR_RAW": cell,
                "COLOR_CODE": color_code,
                "COLOR_NAME": color_name,
                "page": meta.get("page"),
                "matched_groups": meta.get("matched_groups", ""),
                "top_right_text": meta.get("top_right_text", ""),
            })

    return out


# =========================================================
# 7) Build 2 DataFrames từ PDF (Fabric + TrimAndLabels)
# =========================================================
def build_fabric_and_trimlabels_from_pdf(pdf_path: str) -> tuple[pd.DataFrame, pd.DataFrame]:
    FILTER_GROUPS = [
        {"sheet": "FABRIC", "keywords": [r"\bfabric\b"]},
        {"sheet": "TRIM", "keywords": [r"\btrim\b"]},
        {"sheet": "LABELS", "keywords": [r"\bLabels & packaging\b"]},
    ]

    df_detect, df_wide = pdf_to_detect_and_tables_wide(pdf_path, FILTER_GROUPS)

    if df_detect.empty or df_wide.empty:
        return pd.DataFrame(), pd.DataFrame()

    # map page -> style number (top_left_first_line)
    page_to_style = (
        df_detect.drop_duplicates(subset=["page"])
        .set_index("page")["top_left_first_line"]
        .fillna("")
        .astype(str)
        .to_dict()
    )

    out_fabric: list[dict] = []
    out_trimlabels: list[dict] = []

    for (page, table_index), g in df_wide.groupby(["page", "table_index"], sort=True):
        g = g.sort_values("row")
        page_int = int(page)

        meta = {
            "page": page_int,
            "table_index": int(table_index),
            "style_number": page_to_style.get(page_int, ""),
            "matched_groups": str(g["matched_groups"].iloc[0]) if "matched_groups" in g.columns else "",
            "top_right_text": str(g["top_right_text"].iloc[0]) if "top_right_text" in g.columns else "",
        }

        matched_groups = meta["matched_groups"].upper()

        if "FABRIC" in matched_groups:
            out_fabric.extend(parse_one_table_to_fabric_rows(g, meta))
        if ("TRIM" in matched_groups) or ("LABELS" in matched_groups):
            out_trimlabels.extend(parse_one_table_to_trim_rows(g, meta))

    df_fabric = pd.DataFrame(out_fabric)
    df_trimlabels = pd.DataFrame(out_trimlabels)
    return df_fabric, df_trimlabels


# =========================================================
# 8) SQL: create table + fix columns + delete-by-file + insert
# =========================================================
def _safe_ident(name: str) -> str:
    name = str(name).strip()
    name = re.sub(r"[^0-9a-zA-Z_]+", "_", name)
    return name if name else "col_1"


COLUMN_RENAME_MAP = {
    "STYLE#": "STYLE_NO",
    "STYLE_NUMBER": "STYLE_NO",
    "STYLE": "STYLE_NO",

    "ITEM DESCRIPTION": "ITEM_DESCRIPTION",
    "color TRIM": "COLOR_TRIM",
    "date approved": "DATE_APPROVED",
}


def standardize_columns(df: pd.DataFrame) -> pd.DataFrame:
    df = df.copy()
    df.columns = [str(c).strip() for c in df.columns]
    df = df.rename(columns=COLUMN_RENAME_MAP)
    return df


def normalize_df_columns(df: pd.DataFrame) -> pd.DataFrame:
    df = df.copy()
    new_cols = [_safe_ident(c) for c in df.columns]
    seen = {}
    final = []
    for c in new_cols:
        seen[c] = seen.get(c, 0) + 1
        final.append(c if seen[c] == 1 else f"{c}_{seen[c]}")
    df.columns = final
    return df


def ensure_schema(engine: Engine, schema: str) -> None:
    if not re.fullmatch(r"[A-Za-z_][A-Za-z0-9_]*", schema or ""):
        raise ValueError(f"Invalid schema name: {schema!r}")

    with engine.begin() as conn:
        conn.execute(text(f"""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = '{schema}')
            BEGIN
                EXEC('CREATE SCHEMA [{schema}]');
            END
        """))


FABRIC_SCHEMA = {
    "file_name": "NVARCHAR(255) NOT NULL",
    "STYLE_NO": "NVARCHAR(100) NULL",
    "POSITION": "NVARCHAR(200) NULL",
    "COLOR_RAW": "NVARCHAR(100) NULL",
    "COLOR_CODE": "NVARCHAR(20) NULL",
    "COLOR_NAME": "NVARCHAR(200) NULL",
    "page": "INT NULL",
    "matched_groups": "NVARCHAR(200) NULL",
    "top_right_text": "NVARCHAR(500) NULL",
}

TRIM_SCHEMA = {
    "file_name": "NVARCHAR(255) NOT NULL",
    "SUPPLIER": "NVARCHAR(200) NULL",
    "STYLE_NO": "NVARCHAR(100) NULL",
    "description": "NVARCHAR(500) NULL",
    "ITEM_DESCRIPTION": "NVARCHAR(300) NULL",
    "COLOR": "NVARCHAR(1000) NULL", # "A | B | C"
    "COLOR_TRIM": "NVARCHAR(100) NULL",
    "DEL": "NVARCHAR(50) NULL",
    "DATE_APPROVED": "NVARCHAR(50) NULL",
    "Status2": "NVARCHAR(50) NULL",
    "page": "INT NULL",
    "matched_groups": "NVARCHAR(200) NULL",
    "top_right_text": "NVARCHAR(500) NULL",
}


def ensure_table_structure(engine: Engine, full_table_name: str, schema_def: dict, strict_drop_extra: bool = False):
    schema, table = full_table_name.split(".", 1)
    ensure_schema(engine, schema)

    cols_sql = ",\n        ".join([f"[{c}] {t}" for c, t in schema_def.items()])

    with engine.begin() as conn:
        conn.execute(text(f"""
            IF OBJECT_ID('{schema}.{table}', 'U') IS NULL
            BEGIN
                CREATE TABLE {schema}.{table} (
                    {cols_sql}
                );
                CREATE INDEX IX_{table}_file_name ON {schema}.{table} (file_name);
            END
        """))

        conn.execute(text(f"""
            IF COL_LENGTH('{schema}.{table}', 'STYLE_NO') IS NULL
               AND COL_LENGTH('{schema}.{table}', 'STYLE_') IS NOT NULL
            BEGIN
                EXEC sp_rename '{schema}.{table}.STYLE_', 'STYLE_NO', 'COLUMN';
            END
        """))

        for col, coltype in schema_def.items():
            conn.execute(text(f"""
                IF COL_LENGTH('{schema}.{table}', '{col}') IS NULL
                BEGIN
                    ALTER TABLE {schema}.{table} ADD [{col}] {coltype};
                END
            """))

        if strict_drop_extra:
            rows = conn.execute(text(f"""
                SELECT c.name
                FROM sys.columns c
                JOIN sys.objects o ON c.object_id = o.object_id
                JOIN sys.schemas s ON o.schema_id = s.schema_id
                WHERE o.type='U' AND o.name = :table AND s.name = :schema
            """), {"table": table, "schema": schema}).fetchall()

            existing_cols = {r[0] for r in rows}
            wanted_cols = set(schema_def.keys())
            extras = sorted(existing_cols - wanted_cols)

            for c in extras:
                conn.execute(text(f"ALTER TABLE {schema}.{table} DROP COLUMN [{c}];"))


def delete_then_append_by_file(
    engine: Engine,
    df: pd.DataFrame,
    full_table_name: str,
    pdf_path: str,
    file_col: str = "file_name",
) -> None:
    schema, table = full_table_name.split(".", 1)
    file_name = os.path.basename(pdf_path)

    df = pd.DataFrame() if df is None else df.copy()

    # 1) standardize column names
    df = standardize_columns(df)

    # 2) ensure schema and table structure
    if full_table_name.lower().endswith(".fabric"):
        ensure_table_structure(engine, full_table_name, FABRIC_SCHEMA, strict_drop_extra=False)
    else:
        ensure_table_structure(engine, full_table_name, TRIM_SCHEMA, strict_drop_extra=False)

    # 3) normalize column names (safe SQL col names)
    df = normalize_df_columns(df)

    # 4) ensure file_name
    df[file_col] = file_name

    # 5) delete by file_name then insert
    with engine.begin() as conn:
        conn.execute(
            text(f"DELETE FROM {schema}.{table} WHERE [{file_col}] = :fn"),
            {"fn": file_name},
        )

    if not df.empty:
        df.to_sql(
            name=table,
            con=engine,
            schema=schema,
            if_exists="append",
            index=False,
            method="multi",
            chunksize=2000,
        )


def load_pdf_to_sql(
    engine: Engine,
    pdf_path: str,
    fabric_table: str = "dbo.Fabric",
    trimlabels_table: str = "dbo.TrimAndLabels",
    file_col: str = "file_name",
) -> tuple[pd.DataFrame, pd.DataFrame]:
    df_fabric, df_trimlabels = build_fabric_and_trimlabels_from_pdf(pdf_path)

    delete_then_append_by_file(engine, df_fabric, fabric_table, pdf_path, file_col=file_col)
    delete_then_append_by_file(engine, df_trimlabels, trimlabels_table, pdf_path, file_col=file_col)

    return df_fabric, df_trimlabels


# =========================================================
# 9) RUN: scan new PDFs not in SQL then load
# =========================================================
def get_loaded_file_names(engine: Engine, fabric_table="dbo.Fabric", trim_table="dbo.TrimAndLabels", file_col="file_name") -> set[str]:
    f_schema, f_table = fabric_table.split(".", 1)
    t_schema, t_table = trim_table.split(".", 1)

    sql = text(f"""
        SELECT DISTINCT [{file_col}] AS fn FROM {f_schema}.{f_table}
        UNION
        SELECT DISTINCT [{file_col}] AS fn FROM {t_schema}.{t_table}
    """)

    with engine.begin() as conn:
        rows = conn.execute(sql).fetchall()

    return {r[0] for r in rows if r[0]}


def scan_new_pdfs_not_in_sql(
    engine: Engine,
    folder_path: str,
    fabric_table="dbo.Fabric",
    trim_table="dbo.TrimAndLabels",
    file_col="file_name",
    recursive: bool = False,
) -> list[str]:
    loaded = get_loaded_file_names(engine, fabric_table, trim_table, file_col)

    folder = Path(folder_path)
    pdf_paths = folder.rglob("*.pdf") if recursive else folder.glob("*.pdf")

    new_files: list[str] = []
    for p in pdf_paths:
        fn = p.name
        if fn not in loaded:
            new_files.append(str(p))

    return sorted(new_files)


def load_new_pdfs_in_folder(
    engine: Engine,
    folder_path: str,
    recursive: bool = False,
    fabric_table="dbo.Fabric",
    trim_table="dbo.TrimAndLabels",
    file_col="file_name",
):
    new_pdfs = scan_new_pdfs_not_in_sql(
        engine,
        folder_path,
        fabric_table=fabric_table,
        trim_table=trim_table,
        file_col=file_col,
        recursive=recursive,
    )

    print(f"Found {len(new_pdfs)} new PDF(s) to load.")
    for pdf_path in new_pdfs:
        print("Loading:", pdf_path)
        load_pdf_to_sql(
            engine,
            pdf_path,
            fabric_table=fabric_table,
            trimlabels_table=trim_table,
            file_col=file_col,
        )

    return new_pdfs


if __name__ == "__main__":
    import argparse

    ap = argparse.ArgumentParser()
    ap.add_argument("--input-dir", required=True, help="Folder contains uploaded PDFs (from API)")
    ap.add_argument("--recursive", action="store_true")
    args = ap.parse_args()

    CONFIG_PATH = r"C:\ServerPassword.json"

    server, database, user, password = read_config(CONFIG_PATH)
    engine = create_engine_sqlserver(server, database, user, password)

    folder = Path(args.input_dir)
    pdf_paths = folder.rglob("*.pdf") if args.recursive else folder.glob("*.pdf")
    pdf_paths = sorted([str(p) for p in pdf_paths])

    print(f"Found {len(pdf_paths)} PDF(s) in input-dir to load (REPLACE mode).")
    for pdf_path in pdf_paths:
        print("Loading (delete+append):", pdf_path)
        load_pdf_to_sql(
            engine,
            pdf_path,
            fabric_table="dbo.Fabric",
            trimlabels_table="dbo.TrimAndLabels",
            file_col="file_name",
        )