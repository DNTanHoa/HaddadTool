import os
import io
import json
import urllib
import mimetypes
from typing import List, Dict, Tuple

from sqlalchemy import create_engine, text
from sqlalchemy.engine import Engine

from minio import Minio
from minio.error import S3Error


# =========================
# CONFIG
# =========================

CONFIG_PATH = r"C:\ServerPassword.json"

# Folder nguồn (public / share / nơi bạn đang để hình)
SOURCE_DIR = r"\\125.212.216.10\LeadingstarSMB\Public\BU1\HADDAD\ERP - product type"

CUSTOMER_ID = "HA"

# --- S3 / MinIO destination ---
S3_BUCKET = "fileserver-lk"
S3_PREFIX = "productset-images/"
S3_ENDPOINT = "objstore1584api.superdata.vn"
S3_ACCESS_KEY = "pkR4fkf3OMTRjLjseA0k"
S3_SECRET_KEY = "GHxm0g6s87MoLmx2eGlni1x22gkh9FUVOAUJfPt7"
S3_SECURE = True

# Public link prefix to store in SQL (path-style)
# Result in SQL: https://objstore1584api.data.vn/fileserver-lk/productset-images/<filename>
_BASE = f"https://{S3_ENDPOINT}" if S3_SECURE else f"http://{S3_ENDPOINT}"
BASE_IMAGE_URL = f"{_BASE}/{S3_BUCKET}/{S3_PREFIX}"

VALID_EXTS = {".png", ".jpg", ".jpeg", ".webp"}
EXT_PRIORITY = {".png": 0, ".jpg": 1, ".jpeg": 2, ".webp": 3}


# =========================
# DB
# =========================

def read_db_profile(path: str) -> dict:
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)["profiles"]["prod"]


def create_engine_sqlserver(server: str, database: str, user: str, password: str) -> Engine:
    conn_str = (
        "DRIVER=ODBC Driver 17 for SQL Server;"
        f"SERVER={server};DATABASE={database};UID={user};PWD={password}"
    )
    return create_engine(f"mssql+pyodbc:///?odbc_connect={urllib.parse.quote_plus(conn_str)}")


# =========================
# Files
# =========================

def list_images(dir_path: str) -> List[str]:
    out = []
    for name in os.listdir(dir_path):
        p = os.path.join(dir_path, name)
        if not os.path.isfile(p):
            continue
        ext = os.path.splitext(name)[1].lower()
        if ext in VALID_EXTS:
            out.append(p)
    return out


def style_from_filename(file_path: str) -> str:
    return os.path.splitext(os.path.basename(file_path))[0].strip()


def pick_best_file_per_style(img_paths: List[str]) -> Dict[str, str]:
    """
    Nếu 1 style có nhiều file khác đuôi, ưu tiên png rồi jpg...
    """
    best: Dict[str, str] = {}
    for p in img_paths:
        style = style_from_filename(p)
        if not style:
            continue
        ext = os.path.splitext(p)[1].lower()
        if style not in best:
            best[style] = p
        else:
            old_ext = os.path.splitext(best[style])[1].lower()
            if EXT_PRIORITY.get(ext, 99) < EXT_PRIORITY.get(old_ext, 99):
                best[style] = p
    return best


# =========================
# DB: only missing ImageUrl
# =========================

def get_missing_productsets(engine: Engine, customer_id: str, styles: List[str]) -> Dict[str, int]:
    """
    style -> ProductSet.Id
    chỉ lấy rows có ImageUrl NULL/empty
    """
    # unique styles
    uniq = []
    seen = set()
    for s in styles:
        s = (s or "").strip()
        if not s:
            continue
        k = s.lower()
        if k in seen:
            continue
        seen.add(k)
        uniq.append(s)

    if not uniq:
        return {}

    params = {"CustomerId": customer_id}
    placeholders = []
    for i, st in enumerate(uniq):
        key = f"s{i}"
        params[key] = st
        placeholders.append(f":{key}")

    sql = text(f"""
        SELECT CustomerStyle, Id
        FROM ERPv2_Production.dbo.ProductSet
        WHERE CustomerId = :CustomerId
          AND CustomerStyle IN ({", ".join(placeholders)})
          AND (ImageUrl IS NULL OR LTRIM(RTRIM(ImageUrl)) = '')
    """)

    out: Dict[str, int] = {}
    with engine.connect() as conn:
        for row in conn.execute(sql, params).fetchall():
            out[str(row.CustomerStyle)] = int(row.Id)
    return out


def update_imageurl_if_empty(engine: Engine, customer_id: str, style: str, filename: str, use_utc: bool = True) -> int:
    """
    Update đúng 1 row (nếu ImageUrl đang rỗng)
    """
    base = BASE_IMAGE_URL if BASE_IMAGE_URL.endswith("/") else (BASE_IMAGE_URL + "/")
    new_url = base + filename
    now_fn = "DATEADD(HOUR, 7, SYSUTCDATETIME())" if use_utc else "GETDATE()"

    sql = text(f"""
        UPDATE ERPv2_Production.dbo.ProductSet
        SET ImageUrl = :ImageUrl,
            LastUpdatedAt = {now_fn}
        WHERE CustomerId = :CustomerId
          AND CustomerStyle = :CustomerStyle
          AND (ImageUrl IS NULL OR LTRIM(RTRIM(ImageUrl)) = '')
    """)
    with engine.begin() as conn:
        res = conn.execute(sql, {"ImageUrl": new_url, "CustomerId": customer_id, "CustomerStyle": style})
        return int(res.rowcount or 0)


# =========================
# S3 / MinIO
# =========================

def ensure_prefix_marker(client: Minio, bucket: str, prefix: str) -> None:
    """
    Optional: create a 0-byte object 'prefix/' so it shows as a folder in some UIs.
    Safe to call multiple times.
    """
    if not prefix.endswith("/"):
        prefix += "/"
    try:
        client.stat_object(bucket, prefix)
    except S3Error:
        client.put_object(bucket, prefix, data=io.BytesIO(b""), length=0)


def upload_if_missing(client: Minio, bucket: str, prefix: str, src_path: str) -> Tuple[bool, str]:
    """
    Upload file to S3 under prefix if object doesn't exist.
    Return: (uploaded?, object_name)
    """
    if not prefix.endswith("/"):
        prefix += "/"

    filename = os.path.basename(src_path)
    object_name = prefix + filename

    # If exists, skip upload
    try:
        client.stat_object(bucket, object_name)
        return False, object_name
    except S3Error:
        pass

    content_type, _ = mimetypes.guess_type(src_path)
    content_type = content_type or "application/octet-stream"

    client.fput_object(
        bucket_name=bucket,
        object_name=object_name,
        file_path=src_path,
        content_type=content_type,
    )
    return True, object_name


# =========================
# MAIN
# =========================

if __name__ == "__main__":
    # 0) sanity
    if not os.path.isdir(SOURCE_DIR):
        raise RuntimeError(f"SOURCE_DIR not found: {SOURCE_DIR}")

    # 1) DB connect
    db = read_db_profile(CONFIG_PATH)
    engine = create_engine_sqlserver(db["server"], db["database"], db["user"], db["password"])

    # 2) S3 client
    s3 = Minio(
        endpoint=S3_ENDPOINT,
        access_key=S3_ACCESS_KEY,
        secret_key=S3_SECRET_KEY,
        secure=S3_SECURE,
    )

    # Bucket must exist (keep behavior strict; don't create automatically)
    if not s3.bucket_exists(S3_BUCKET):
        raise RuntimeError(f"S3 bucket does not exist: {S3_BUCKET}")

    # Optional "folder" marker
    ensure_prefix_marker(s3, S3_BUCKET, S3_PREFIX)

    # 3) Scan + chọn file tốt nhất theo style
    img_paths = list_images(SOURCE_DIR)
    best_by_style = pick_best_file_per_style(img_paths)

    if not best_by_style:
        print("No images found.")
        raise SystemExit(0)

    # 4) Chỉ lấy các ProductSet đang thiếu ImageUrl
    styles = list(best_by_style.keys())
    missing_map = get_missing_productsets(engine, CUSTOMER_ID, styles)

    if not missing_map:
        print("Nothing to do: no ProductSet missing ImageUrl for these styles.")
        raise SystemExit(0)

    # 5) Với mỗi style thiếu ImageUrl: upload (nếu chưa có) + update DB
    to_process = {style: best_by_style[style] for style in missing_map.keys() if style in best_by_style}

    uploaded = 0
    upload_skipped_exists = 0
    db_updated = 0
    db_skipped = 0
    failed = 0

    for style, src_path in to_process.items():
        filename = os.path.basename(src_path)

        try:
            did_upload, object_name = upload_if_missing(s3, S3_BUCKET, S3_PREFIX, src_path)
            if did_upload:
                uploaded += 1
            else:
                upload_skipped_exists += 1

            # Update DB (chỉ update nếu ImageUrl đang rỗng)
            u = update_imageurl_if_empty(engine, CUSTOMER_ID, style, filename, use_utc=True)
            if u > 0:
                db_updated += u
                print(f"[OK] style={style} file={filename} uploaded={did_upload} s3_key={object_name} db_updated={u}")
            else:
                db_skipped += 1
                print(f"[SKIP-DB] style={style} file={filename} uploaded={did_upload} (ImageUrl already set?)")

        except Exception as ex:
            failed += 1
            print(f"[FAIL] style={style} file={filename} error={ex}")

    print("\n=== SUMMARY ===")
    print("Missing styles to process:", len(to_process))
    print("Uploaded new files:", uploaded)
    print("Upload skipped (already exists):", upload_skipped_exists)
    print("DB updated:", db_updated)
    print("DB skipped:", db_skipped)
    print("Failed:", failed)
    print("\nPublic URL prefix used for SQL:")
    print(BASE_IMAGE_URL)
