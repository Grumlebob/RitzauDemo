# LocalSingleNode operations

Scoped image maintenance requires verified current/recovery pins and pending deployment targets under the shared lock. It preserves their application image digests and tags. Missing protection prevents image deletion. Offline recovery checks restore the exact payload and inspect its cached application image digest without a registry pull; a missing cached image fails the drill.

Use this guide for an already prepared native node. Workflow dispatches, reboot, restore and app removal require operator authorization. Use [AgentSetup](AgentSetup.md) for authorized first setup or resume and [PublicSetup](PublicSetup.md) for tunnel and HTTPS changes. Commands that say “on the node” require its repository checkout.

## Release and rollback

From GitHub Actions, dispatch **CD - Deploy LocalSingleNode** from `main`. Require a successful publishing CI run for the exact selected SHA and record the workflow run ID before watching. If watching stops, inspect the same run; never dispatch another run to replace a watcher.

- Leave `target_sha` empty only when the dispatched `main` commit has a published release, or supply a full publishing SHA on `main`. A `[skip ci]` documentation commit has no release; see [source selection](../Common/ReleaseContract.md#select-a-published-source).
- Set `run_migrations=true` for the first deployment or a schema update. Use `false` when the selected release does not need a migration.
- All workflow operations target the configured primary app.

The deployment verifies the release manifest, immutable image digest, GHCR migration payload and acceptance profile before applying migrations. From the node's repository checkout, run the independent LAN check when needed:

```bash
bash ./Deployment/LocalSingleNode/Scripts/acceptance-check.sh
```

For an app rollback before a migration, dispatch the prior verified `main` SHA with `run_migrations=false`; that SHA still needs its successful publishing run. After a migration, prefer a forward fix. Restoring an older backup loses later writes, and rolling back the app image does not roll back database changes. See [the release contract](../Common/ReleaseContract.md).

### Failure on the first deployment

The first deployment has no previously accepted app release to roll back to. Keep the failed deployment run and its available release evidence for diagnosis. Preserve this app's containers, volumes, and protected backups until recovery is understood. Do not treat an unpublished seed commit as a recovery release, delete the database volume, or run broad Docker cleanup.

Inspect the same workflow run and use `doctor.sh --json` plus the independent acceptance check to identify the failure. If the app must stop serving, use only an app-scoped supported operation after verifying ownership; keep its database volume and backups. Recover forward from a corrected, successfully published `main` SHA, selecting migrations from the actual database state. Preserve all neighboring applications, routes, and data.

## Diagnose a running node

`doctor.sh --json` is read-only. Run it from the node's repository checkout:

```bash
bash ./Deployment/LocalSingleNode/Scripts/doctor.sh --json
```

Use its reported checks to identify what needs attention. Run `acceptance-check.sh` to verify LAN readiness and the configured HTTP acceptance profile. Use [Readiness](Readiness.md) for a pre-setup candidate check; it does not bootstrap, back up, deploy, restart or repair the node.

## Runtime resource budgets

The RitzauDemo Compose stack starts with caps of 1.5 CPUs, 512 MiB and 256 PIDs for web; 1 CPU, 768 MiB and 256 PIDs for PostgreSQL; and 0.25 CPUs, 128 MiB and 128 PIDs for Redis. Redis retains append-only persistence, caps its dataset at 64 MiB, and uses `noeviction` so a full cache does not silently discard existing entries. Loaded acceptance must confirm these initial limits leave headroom for existing applications and one admitted build; inspect the effective Docker limits and host pressure during that run.

## Maintenance

The **LocalSingleNode maintenance** workflow runs against the primary app and uses the shared deployment lock. `backup_now` defaults to true; it creates a fresh protected backup and verifies an isolated restore. Keep `acceptance_only=false` for normal maintenance. Host maintenance stays manual; release artifact retention runs daily at 03:17 UTC and also runs on manual dispatch. Set `artifact_only=true` to skip app maintenance. Manual GitHub artifact deletion stays in preview mode unless both `apply_artifacts=true` and the repository variable `ACTIONS_ARTIFACT_RETENTION_ENABLED` is exactly `true`; scheduled deletion requires that variable alone. Leave the variable unset until current accepted/recovery pins and live operations are verified. The workflow also prunes expired runner-local CI reports.

Use these options only when needed:

- `acceptance_only=true` runs HTTP acceptance without routine maintenance.
- `restart_app=true` requires `acceptance_only=true`; it restarts this app's web container and checks persistence.
- `restart_connector=true` also requires `acceptance_only=true` and checks this app's public connector. Keep it separate from an app restart.
- `reboot_check=true` reboots after the whole workflow succeeds.

Public tunnel ownership, credential rotation and removal belong to [PublicSetup](PublicSetup.md).

### Setup and maintenance source

Setup and maintenance use [verify-operation-source.py](../Common/Scripts/verify-operation-source.py). They pin trusted `main` with full history. When ordinary documentation descendants skipped CI, the gate can use the nearest successful main CI only after proving the operation code is unchanged. Source/configuration changes, disguised Markdown in implementation directories, incomplete history, or an actual failed/pending run stop the operation. This exception validates operation code; it never selects release artifacts or relaxes CD's exact-source checks.

## Deployment lock recovery

Single-node workflows run on this node's self-hosted `deploy` runner. They use the shared `/tmp/localcluster-deploy.lockdir` to serialize app changes across repositories. The wrapper releases the lock after its command exits; it does not reclaim a lock left by an interrupted command.

If a workflow times out waiting for the lock, inspect it from the node's repository checkout:

```bash
sudo -u deploy bash ./Deployment/Common/Scripts/release-deploy-lock.sh --inspect
```

Confirm the owner process is gone and no `ansible-playbook` process runs as `deploy`. Release only the exact token printed by inspection:

```bash
sudo -u deploy bash ./Deployment/Common/Scripts/release-deploy-lock.sh --release --token <exact-token>
```

If the owner is on another host, confirm no deployment is active there and add `--owner-host-checked`. If the lock contains another app's metadata, stop and use that app's recovery tool. Never remove the lock by hand.

## Backups and recovery

The node creates a protected PostgreSQL backup nightly. Maintenance also creates and verifies a fresh backup by default. Each backup contains the database dump, `secrets.yml` (including the certificate passphrase), and `data-protection.pem` (the matching encrypted certificate/private key). Keep these files together in protected off-host storage. Verify the host key through [Controller SSH](ControllerSsh.md) before copying.

To verify the newest dump, run this from the node checkout as an account with Docker access:

```bash
bash ./Deployment/LocalSingleNode/Scripts/verify-backup.sh
```

To verify a specific dump, add `--file <backup-root>/<backup-file>.dump`. Verification restores it into an isolated, network-disabled temporary PostgreSQL container and removes only that container. This verifies database restore; retain the matching certificate and passphrase for app recovery.

Copy the configured backup directory to another machine or encrypted storage. Substitute the actual install user, node address, configured `backup_root`, and protected destination:

```bash
scp -r <install-user>@<node-ip>:<backup-root> <protected-destination>
```

Manual restore requires an operator decision. It creates a new `<database>_restore_<suffix>` database; it refuses the live or an existing target and does not switch application connections. Use the app's exact slug for confirmation and replace hyphens with underscores in the database name:

```bash
APP_NAME="your-app-slug"
DATABASE_NAME="${APP_NAME//-/_}"
sudo bash ./Deployment/LocalSingleNode/Scripts/verify-backup.sh \
  --restore-into "${DATABASE_NAME}_restore_recovery" \
  --confirm-restore "$APP_NAME"
```

Keep the complete target database name at 63 characters or fewer. The suffix must use lowercase letters, digits or underscores.

Add `--file <backup-root>/<backup-file>.dump` before the confirmation option to select a specific owned dump.

For recovery on another node, restore `secrets.yml` to `/etc/<app-name>/secrets.yml` with owner `deploy` and mode `0600`, and restore `data-protection.pem` to `/etc/<app-name>/data-protection/certificate.pem` with owner `root`, group `deploy`, and mode `0440` before starting the app. Keep the same app name and Data Protection application discriminator. Database-only restore cannot decrypt stored keys, authentication cookies, or encrypted mail jobs.

Validate recovered data before planning a separate reviewed cutover. A failed partial replacement is left for operator inspection.

## Remove an application

Removal requires an explicit operator decision. Copy a verified backup off the node and record the exact app resources first. Disable this app's backup timer. Remove a runner only if it is dedicated to the removed app and no other app needs it.

Stop only this app's Compose project:

```bash
sudo docker compose --project-directory <deploy-root> --env-file <deploy-root>/.compose.env -p <app-name> down
```

Never add `--volumes`. Keep its PostgreSQL, Redis and app-storage volumes until the operator identifies the exact owned volume, verifies the off-host backup and removes that volume manually. Remove only this app's Caddy site; validate the shared Caddy configuration before reloading it. Retain protected backups and secrets until their removal is separately authorized. Do not uninstall shared host services or alter another app.
