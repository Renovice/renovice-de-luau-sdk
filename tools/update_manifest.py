#!/usr/bin/env python3
"""Write or verify SHA-256 hashes for the canonical bytes staged in Git."""

from __future__ import annotations

import argparse
import hashlib
from pathlib import Path
import subprocess
import sys


MANIFEST_NAME = "MANIFEST.sha256"


def run_git(repo: Path, *arguments: str) -> bytes:
    result = subprocess.run(
        ["git", "-C", str(repo), *arguments],
        check=False,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
    )
    if result.returncode != 0:
        message = result.stderr.decode("utf-8", "replace").strip()
        raise RuntimeError(f"git {' '.join(arguments)} failed: {message}")
    return result.stdout


def index_entries(repo: Path) -> list[tuple[bytes, str]]:
    raw = run_git(repo, "ls-files", "--stage", "-z")
    entries: list[tuple[bytes, str]] = []
    for record in raw.split(b"\0"):
        if not record:
            continue
        metadata, raw_path = record.split(b"\t", 1)
        _mode, object_id, stage = metadata.split(b" ", 2)
        if stage != b"0":
            raise RuntimeError(
                f"unmerged index entry: {raw_path.decode('utf-8', 'replace')}"
            )
        path = raw_path.decode("utf-8", "surrogateescape")
        if path == MANIFEST_NAME:
            continue
        if "\n" in path or "\r" in path:
            raise RuntimeError(f"manifest cannot represent newline in path: {path!r}")
        entries.append((object_id, path))
    entries.sort(key=lambda entry: entry[1].encode("utf-8", "surrogateescape"))
    return entries


def read_blobs(repo: Path, entries: list[tuple[bytes, str]]) -> list[tuple[str, str]]:
    process = subprocess.Popen(
        ["git", "-C", str(repo), "cat-file", "--batch"],
        stdin=subprocess.PIPE,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
    )
    assert process.stdin is not None
    assert process.stdout is not None
    hashes: list[tuple[str, str]] = []
    try:
        for object_id, path in entries:
            process.stdin.write(object_id + b"\n")
            process.stdin.flush()
            header = process.stdout.readline().rstrip(b"\n")
            fields = header.split(b" ")
            if len(fields) != 3 or fields[1] != b"blob":
                raise RuntimeError(
                    f"unexpected git cat-file response for {path}: "
                    f"{header.decode('utf-8', 'replace')}"
                )
            size = int(fields[2])
            blob = process.stdout.read(size)
            separator = process.stdout.read(1)
            if len(blob) != size or separator != b"\n":
                raise RuntimeError(f"truncated git blob while hashing {path}")
            hashes.append((hashlib.sha256(blob).hexdigest().upper(), path))
    finally:
        process.stdin.close()
        return_code = process.wait()
    if return_code != 0:
        assert process.stderr is not None
        message = process.stderr.read().decode("utf-8", "replace").strip()
        raise RuntimeError(f"git cat-file --batch failed: {message}")
    return hashes


def render(repo: Path) -> bytes:
    lines = [f"{digest}  {path}" for digest, path in read_blobs(repo, index_entries(repo))]
    return ("\n".join(lines) + "\n").encode("utf-8", "surrogateescape")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    operation = parser.add_mutually_exclusive_group(required=True)
    operation.add_argument("--write", action="store_true", help="write MANIFEST.sha256")
    operation.add_argument("--verify", action="store_true", help="verify MANIFEST.sha256")
    parser.add_argument("--repo", type=Path, default=Path(__file__).resolve().parent.parent)
    args = parser.parse_args()

    repo = args.repo.resolve()
    manifest = repo / MANIFEST_NAME
    expected = render(repo)
    if args.write:
        manifest.write_bytes(expected)
        print(f"MANIFEST WRITE PASS files={expected.count(bytes([10]))} path={manifest}")
        return 0
    if not manifest.is_file():
        print(f"MANIFEST VERIFY FAIL missing={manifest}", file=sys.stderr)
        return 1
    actual = manifest.read_bytes()
    if actual != expected:
        print(
            "MANIFEST VERIFY FAIL tracked index content differs; "
            "stage intentional changes and run tools/update_manifest.py --write",
            file=sys.stderr,
        )
        return 1
    print(f"MANIFEST VERIFY PASS files={expected.count(bytes([10]))} path={manifest}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (OSError, RuntimeError, ValueError) as error:
        print(f"MANIFEST ERROR {error}", file=sys.stderr)
        raise SystemExit(1)
