import shutil
import subprocess
from pathlib import Path
from typing import List

from config import PYTHON_EXE, PDF_TO_SQL_SCRIPT, SQL_TO_SHEET_SCRIPT


def run_cmd_and_log(cmd: List[str], log_file: Path) -> int:
    log_file.parent.mkdir(parents=True, exist_ok=True)
    with log_file.open("a", encoding="utf-8", errors="replace") as f:
        f.write("\n=== RUN ===\n")
        f.write("CMD: " + " ".join(cmd) + "\n")

        proc = subprocess.Popen(
            cmd,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
            encoding="utf-8",
            errors="replace",
        )

        assert proc.stdout is not None
        for line in proc.stdout:
            f.write(line)
            f.flush()

        return proc.wait()


def safe_rmtree(p: Path) -> None:
    """Delete a folder safely (ignore errors)."""
    try:
        if p.exists():
            shutil.rmtree(p, ignore_errors=True)
    except Exception:
        pass


def run_pipeline(input_dir: Path, log_file: Path, cleanup_input_dir: bool = True) -> int:
    """
    Run:
      1) PDF -> SQL
      2) SQL -> Sheet

    Then cleanup input_dir (delete uploaded PDFs) if cleanup_input_dir=True.
    """
    try:
        # Step1 PDF -> SQL (delete+append by file already inside your script)
        c1 = run_cmd_and_log(
            [PYTHON_EXE, PDF_TO_SQL_SCRIPT, "--input-dir", str(input_dir)],
            log_file,
        )
        if c1 != 0:
            return c1

        # Step2 SQL -> Sheet (pass input-dir so it knows which files to replace)
        c2 = run_cmd_and_log(
            [PYTHON_EXE, SQL_TO_SHEET_SCRIPT, "--input-dir", str(input_dir)],
            log_file,
        )
        return c2

    finally:
        # ✅ ALWAYS cleanup uploaded PDFs folder after processing (success/fail)
        if cleanup_input_dir:
            # safe_rmtree(input_dir)
            pass