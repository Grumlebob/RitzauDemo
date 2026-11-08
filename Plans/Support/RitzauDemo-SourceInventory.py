"""Inventory tracked template dependencies without changing the source repository."""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess


def git(root: Path, *arguments: str) -> str:
    return subprocess.check_output(
        ["git", "-C", str(root), *arguments], text=True, encoding="utf-8"
    ).strip()


def snapshot(root: Path) -> dict:
    top = Path(git(root, "rev-parse", "--show-toplevel")).resolve()
    if top != root:
        raise ValueError("Pass the repository root, not a child directory")
    if git(root, "status", "--porcelain", "--untracked-files=no"):
        raise ValueError("Tracked source edits exist; do not record them as the clean baseline")
    tracked = subprocess.check_output(
        ["git", "-C", str(root), "ls-files", "-z"]
    ).decode("utf-8").split("\0")
    groups = {
        "react_consumers_outside_client": [],
        "removed_target_consumers_outside_targets": [],
        "release_label_consumers": [],
        "bootstrap_and_admission": [],
        "migration_files": [],
    }
    hashes = {}
    for name in sorted(filter(None, tracked)):
        path = root / name
        if not path.is_file():
            continue
        raw = path.read_bytes()
        if b"\0" in raw or len(raw) > 3_000_000:
            continue
        try:
            content = raw.decode("utf-8-sig")
        except UnicodeDecodeError:
            continue
        content = content.replace("\r\n", "\n")
        operational = not name.startswith(("Plans/", "docs/", "GrumPlates.React/"))
        if operational and re.search(r"React|final-react|ADDITIONAL_FRONTEND_PROFILES", content):
            groups["react_consumers_outside_client"].append(name)
        if operational and not name.startswith(("Deployment/LocalCluster/", "Deployment/Cloud/")):
            if re.search(r"Deployment/LocalCluster|Deployment/Cloud|\.\./LocalCluster", content):
                groups["removed_target_consumers_outside_targets"].append(name)
        if operational and "com.grumplates." in content:
            groups["release_label_consumers"].append(name)
        if name in {
            ".github/workflows/ci.yml", ".github/workflows/cd-localsinglenode.yml",
            "Deployment/Common/Scripts/with-node-browser-lock.sh",
            "Deployment/Common/Scripts/Component/with-deploy-lock.sh",
            "Deployment/LocalSingleNode/Scripts/lib/setup_status.py",
            "Deployment/LocalSingleNode/Scripts/lib/setup_next.py",
            "Deployment/LocalSingleNode/Scripts/lib/bootstrap.py",
            "Deployment/LocalSingleNode/Scripts/lib/runner.py",
            "Deployment/LocalSingleNode/ansible/playbooks/PrepareSingleNode.yml",
        }:
            groups["bootstrap_and_admission"].append(name)
        if name.startswith("GrumPlates/Infrastructure/Persistence/Migrations/"):
            groups["migration_files"].append(name)
        if any(name in entries for entries in groups.values()):
            hashes[name] = hashlib.sha256(content.encode("utf-8")).hexdigest()
    return {
        "schema_version": 1,
        "source_repository": "Grumlebob/GrumPlates",
        "source_sha": git(root, "rev-parse", "HEAD"),
        "hash_format": "SHA-256 of UTF-8 text, BOM removed, CRLF normalized to LF",
        "groups": groups,
        "source_hashes": hashes,
    }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    parser.add_argument("--output", type=Path)
    parser.add_argument("--verify", type=Path)
    arguments = parser.parse_args()
    if bool(arguments.output) == bool(arguments.verify):
        parser.error("Choose exactly one of --output or --verify")
    result = snapshot(arguments.source.resolve())
    if arguments.verify:
        expected = json.loads(arguments.verify.read_text(encoding="utf-8"))
        if result != expected:
            raise ValueError("Source drift detected. Review changes and refresh the execution packet")
        print("Source baseline and dependency inventory match")
        return
    destination = arguments.output.resolve()
    if destination.is_relative_to(arguments.source.resolve()):
        raise ValueError("Write the planning inventory outside the source repository")
    if destination.exists():
        raise ValueError("Output already exists; inspect it before selecting a new output path")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"source_sha": result["source_sha"], "groups": {
        name: len(paths) for name, paths in result["groups"].items()
    }}, indent=2))


if __name__ == "__main__":
    main()
