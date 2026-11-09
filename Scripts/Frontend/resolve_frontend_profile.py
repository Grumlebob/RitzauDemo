#!/usr/bin/env python3
"""Resolve the product's one supported frontend and release acceptance profile."""

from __future__ import annotations

import argparse
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

SUPPORTED_PROFILES = ("BlazorAuto",)
DEFAULT_FRONTEND_PROFILE = SUPPORTED_PROFILES[0]
ACCEPTANCE_PROFILE_BY_FRONTEND = {"BlazorAuto": "BlazorAutoV1"}
LEGACY_INSTANCE_ACCEPTANCE_PROFILE_BY_FRONTEND = {
    # Existing shared-host React instances keep their acceptance contract during this product cleanup.
    "React": "ReactPublicV1",
}


def _validate_profile(raw_value: str, source: str) -> str:
    profile = raw_value.strip()
    if not profile:
        raise ValueError(f"{source} is empty; expected exactly {DEFAULT_FRONTEND_PROFILE}")
    if profile not in SUPPORTED_PROFILES:
        raise ValueError(f"{source} must be exactly {DEFAULT_FRONTEND_PROFILE} after trimming whitespace")
    return profile


def resolve_frontend_profile(override: str | None = None) -> str:
    """Return the single supported profile, optionally checking a build override."""
    if override is None:
        return DEFAULT_FRONTEND_PROFILE
    return _validate_profile(override, "profile override")


def _git_show(repository_root: Path, source_sha: str, path: str) -> str:
    result = subprocess.run(
        ["git", "show", f"{source_sha}:{path}"],
        cwd=repository_root,
        check=False,
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    if result.returncode != 0:
        raise ValueError(f"source commit {source_sha} does not contain {path}")
    return result.stdout


def frontend_profile_from_properties(properties: str) -> str | None:
    """Read an explicit profile property; return None for historical file-backed builds."""
    try:
        document = ET.fromstring(properties)
    except ET.ParseError as exc:
        raise ValueError("Directory.Build.props is invalid XML") from exc

    profile_nodes = [node for node in document.iter() if node.tag.rsplit("}", 1)[-1] == "FrontendProfile"]
    values = [node.text.strip() for node in profile_nodes if node.text and node.text.strip()]
    if len(values) > 1:
        raise ValueError("Directory.Build.props has an ambiguous frontend profile")
    if not values or "$(" in values[0]:
        return None
    return _validate_profile(values[0], "Directory.Build.props frontend profile")


def frontend_profile_for_source(repository_root: Path | str, source_sha: str) -> str:
    """Read the profile from an exact source commit, including legacy release commits."""
    if not re.fullmatch(r"[0-9a-f]{40}", source_sha):
        raise ValueError("source SHA must be a full lowercase 40-character commit SHA")

    root = Path(repository_root)
    properties = _git_show(root, source_sha, "Directory.Build.props")
    try:
        explicit_profile = frontend_profile_from_properties(properties)
    except ValueError as exc:
        raise ValueError(f"source commit {source_sha} has invalid Directory.Build.props") from exc
    if explicit_profile:
        return explicit_profile

    # Previously published releases read this value from a tracked file. Keep
    # those immutable release SHAs deployable for recovery and rollback.
    legacy_profile = _git_show(root, source_sha, "frontend-profile.txt")
    return _validate_profile(legacy_profile, f"frontend profile in {source_sha}")


def acceptance_profile_for_frontend(frontend_profile: str) -> str:
    """Return the trusted acceptance contract paired with a frontend profile."""
    try:
        return ACCEPTANCE_PROFILE_BY_FRONTEND[frontend_profile]
    except KeyError as exc:
        raise ValueError(f"unsupported frontend profile for acceptance: {frontend_profile!r}") from exc


def acceptance_profile_for_legacy_instance(frontend_profile: str) -> str:
    """Resolve legacy acceptance contracts used by existing independent deployment instances."""
    if frontend_profile in LEGACY_INSTANCE_ACCEPTANCE_PROFILE_BY_FRONTEND:
        return LEGACY_INSTANCE_ACCEPTANCE_PROFILE_BY_FRONTEND[frontend_profile]
    return acceptance_profile_for_frontend(frontend_profile)


def main(argv: list[str] | None = None) -> int:
    repository_root = Path(__file__).resolve().parents[2]
    parser = argparse.ArgumentParser(description=__doc__)
    selection = parser.add_mutually_exclusive_group()
    selection.add_argument("--override", help="explicit build-validation override")
    selection.add_argument("--source-sha", help="resolve the profile recorded in this exact source commit")
    parser.add_argument(
        "--acceptance-profile",
        action="store_true",
        help="emit the trusted acceptance profile paired with the resolved frontend profile",
    )
    args = parser.parse_args(argv)

    try:
        profile = (
            frontend_profile_for_source(repository_root, args.source_sha)
            if args.source_sha
            else resolve_frontend_profile(args.override)
        )
    except ValueError as exc:
        print(f"Frontend profile error: {exc}", file=sys.stderr)
        return 2

    print(acceptance_profile_for_frontend(profile) if args.acceptance_profile else profile)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
