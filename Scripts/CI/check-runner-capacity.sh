#!/usr/bin/env bash
set -euo pipefail

minimum_free_mib="${CI_MIN_FREE_MIB:-20480}"
minimum_free_inode_percent="${CI_MIN_FREE_INODE_PERCENT:-5}"
[[ "$minimum_free_mib" =~ ^[0-9]+$ && "$minimum_free_inode_percent" =~ ^[0-9]+$ ]] || {
  echo "CI capacity thresholds must be non-negative integers." >&2
  exit 2
}
(( minimum_free_inode_percent <= 100 )) || {
  echo "CI_MIN_FREE_INODE_PERCENT cannot exceed 100." >&2
  exit 2
}

workspace="${GITHUB_WORKSPACE:-$PWD}"
runner_temp="${RUNNER_TEMP:-/tmp}"
docker_root="$(docker info --format '{{.DockerRootDir}}')" || {
  echo "Cannot identify the Docker data root; refusing to guess." >&2
  exit 1
}
[[ -n "$docker_root" && -d "$docker_root" ]] || {
  echo "Docker returned an invalid data root: $docker_root" >&2
  exit 1
}

declare -A seen=()
status=0
for candidate in "$workspace" "$runner_temp" "$docker_root" /opt; do
  [[ -e "$candidate" ]] || continue
  path="$(readlink -f "$candidate")"
  [[ -n "${seen[$path]:-}" ]] && continue
  seen[$path]=1

  if ! free_mib="$(df -Pm "$path" | awk 'NR == 2 {print $4}')" \
    || ! inode_counts="$(df -Pi "$path" | awk 'NR == 2 {print $2, $4}')"; then
    echo "Cannot query byte/inode capacity for $path; refusing to guess." >&2
    exit 1
  fi
  read -r total_inodes free_inodes <<< "$inode_counts"
  if [[ ! "$free_mib" =~ ^[0-9]+$ || ! "$total_inodes" =~ ^[0-9]+$ || ! "$free_inodes" =~ ^[0-9]+$ ]] \
    || (( total_inodes == 0 || free_inodes > total_inodes )); then
    echo "Cannot establish byte/inode capacity for $path; refusing to guess." >&2
    exit 1
  fi
  free_inode_percent=$((free_inodes * 100 / total_inodes))
  printf '%s capacity: free_mib=%s free_inode_percent=%s\n' "$path" "$free_mib" "$free_inode_percent"
  if (( free_mib < minimum_free_mib || free_inode_percent < minimum_free_inode_percent )); then
    echo "Required reserve: ${minimum_free_mib}MiB and ${minimum_free_inode_percent}% free inodes on $path." >&2
    echo "Review the single-node maintenance and scoped cleanup procedure before retrying CI." >&2
    status=2
  fi
done

exit "$status"
