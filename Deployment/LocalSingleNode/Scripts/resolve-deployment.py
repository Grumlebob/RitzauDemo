#!/usr/bin/env python3
"""Validate the one LocalSingleNode app and bind it to a published release."""

from __future__ import annotations

import json
import os
from pathlib import Path
import stat
import sys

LIB = Path(__file__).resolve().parent / "lib"
sys.path.insert(0, str(LIB))
from ls_settings import ETC, ROOT, TARGET, settings

sys.path.insert(0, str(ROOT / "Scripts/Frontend"))
from resolve_frontend_profile import acceptance_profile_for_frontend

sys.path.insert(0, str(ROOT / "Deployment/Common/Scripts/Component/lib"))
from release_settings import load_release_settings


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError("duplicate ownership marker key")
        result[key] = value
    return result


def write_private(path: Path, content: str) -> None:
    descriptor = os.open(path, os.O_WRONLY | os.O_CREAT | os.O_EXCL | os.O_NOFOLLOW, 0o600)
    with os.fdopen(descriptor, "w", encoding="utf-8") as output:
        output.write(content)


def resolve_temporary_directory() -> Path:
    runner_temp = Path(os.environ["RUNNER_TEMP"]).resolve()
    requested = Path(os.environ.get("DEPLOYMENT_TEMP_DIR", str(runner_temp)))
    if not requested.is_absolute() or requested.is_symlink():
        raise ValueError("deployment temporary directory must be an absolute, non-symlink path")
    requested.mkdir(mode=0o700, exist_ok=True)
    resolved = requested.resolve(strict=True)
    if os.path.commonpath((str(runner_temp), str(resolved))) != str(runner_temp):
        raise ValueError("deployment temporary directory must remain inside RUNNER_TEMP")
    if resolved != runner_temp:
        os.chmod(resolved, 0o700)
    return resolved


def verify_owned(config: dict, source: str, etc: Path = ETC) -> None:
    marker = etc / f"localsinglenode/apps/{config['app_name']}.json"
    for path in (marker.parent, marker):
        identity = path.lstat()
        if stat.S_ISLNK(identity.st_mode) or identity.st_uid != 0 or identity.st_mode & 0o022:
            raise ValueError("application ownership marker must be protected and root-owned")
    if not marker.is_file():
        raise ValueError("application ownership marker must be a regular file")
    recorded = json.loads(marker.read_text(encoding="utf-8"), object_pairs_hook=unique_object)
    if (not isinstance(recorded, dict) or not isinstance(recorded.get("source_repo_url"), str)
            or recorded["source_repo_url"].casefold() != source.casefold()
            or recorded.get("deploy_root") != config["deploy_root"]
            or recorded.get("backup_root") != config["backup_root"]):
        raise ValueError("application ownership marker does not match this repository")


def resolve(profile: str, acceptance: str, release: dict, config: dict, temporary: Path) -> dict[str, str]:
    if acceptance_profile_for_frontend(profile) != acceptance:
        raise ValueError("frontend and acceptance profiles disagree")
    app_image = release["app_image"]
    variables_path = temporary / "deployment-vars.json"
    write_private(variables_path, json.dumps({**config, "app_image": app_image}, sort_keys=True) + "\n")
    return {
        "APP_NAME": config["app_name"],
        "APP_IMAGE": app_image,
        "DEPLOY_ROOT": config["deploy_root"],
        "MIGRATION_BUNDLE_NAME": release["migration_bundle_name"],
        "MIGRATION_ARTIFACT_NAME": release["migration_artifact_name"],
        "FRONTEND_PROFILE": profile,
        "ACCEPTANCE_PROFILE": acceptance,
        "DEPLOYMENT_VARS_FILE": str(variables_path),
    }


def main() -> int:
    try:
        config = settings(TARGET / "inventory/group_vars/all.yml")
        release = load_release_settings(ROOT / "Deployment/Common/release.yml")
        values = resolve(
            os.environ["FRONTEND_PROFILE"], os.environ["ACCEPTANCE_PROFILE"], release, config,
            resolve_temporary_directory(),
        )
        source = f"{os.environ['GITHUB_SERVER_URL']}/{os.environ['GITHUB_REPOSITORY']}"
        verify_owned(config, source)
        with Path(os.environ["GITHUB_ENV"]).open("a", encoding="utf-8") as output:
            for key, value in values.items():
                output.write(f"{key}={value}\n")
        print("Primary app and release identities validated.")
        return 0
    except (ValueError, OSError, KeyError, TypeError, AttributeError, SyntaxError):
        print("Invalid deployment settings or unsafe output; no configuration was printed.", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
