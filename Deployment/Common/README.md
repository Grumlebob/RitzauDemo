# Deployment Common

Shared deployment behavior lives here only when it is required by the supported LocalSingleNode target. Inventory, firewall, Compose and bootstrap stay in `Deployment/LocalSingleNode`.

- `release.yml` and the [release contract](ReleaseContract.md) own artifact names, provenance and target release checks.
- [SSH host trust](SshTrust.md) owns manual host-fingerprint verification.
- The [public tunnel helper](Scripts/setup-public-tunnel.sh) is integrated by [LocalSingleNode public setup](../LocalSingleNode/PublicSetup.md).
- `Scripts/` holds shared release, toolchain, settings, lock, runner and cleanup helpers. Target guides own their invocation steps.
- `pin_release_payload.py` validates exact GHCR release bytes before storing a repository-scoped local pin. LocalSingleNode recovers by explicit pin key without Actions or GHCR access; its locked maintenance owner protects the current and previous release, retains the newest three other pins per profile, and caps unprotected bytes at 1 GiB.
- `Scripts/registry-inventory.py` measures payload packages without downloading layers or changing registry state. It verifies the configured package's repository linkage and visibility, manifest/config hashes and lengths, and the exact CI attempt behind each payload. Pass `--repository owner/repository --username user --package ghcr.io/owner/app-release-payload=private` with an existing `GITHUB_TOKEN` or `GH_TOKEN` in the environment. Unknown ownership, aliases, or incomplete descriptor graphs stop the inventory; a storage report does not classify releases as disposable or authorize deletion.
- Runner residue cleanup requires `--runner-root` or `--app-name`; it never infers target settings.
- A reviewed config-only bootstrap without CI provenance may be preserved with `registry-inventory.py --retain-manifest ghcr.io/owner/app-release-payload@sha256:...`. This exact digest must exist in a linked configured package, contain no layers or child manifests, and fit within 64 KiB including config. It is counted separately and always protected; it gains no CI identity or release admission. Unlisted or missing references still stop inventory and deletion.
- `ansible/roles/` holds reusable host building blocks; target-specific roles stay with their target.
- `caddy/Caddyfile` imports each app-owned site. The product keeps deployed observability disabled.

Shared Ansible roles use optional guards instead of assuming one target's topology.
