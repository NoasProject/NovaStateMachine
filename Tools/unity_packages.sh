#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/.." && pwd)"

src_dir="$repo_root/NovaStateMachine/src"
dest_dir="$repo_root/Unity/NovaStateMachine_Packages/Runtime"

mkdir -p "$dest_dir"

rsync -a --include='*/' --include='*.cs' --exclude='*' "$src_dir/" "$dest_dir/"
