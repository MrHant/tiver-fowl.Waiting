#!/usr/bin/env bash
# Installs go-task (the `task` command-line task runner) into /usr/local/bin.
# Runs from postCreateCommand; safe to re-run — skips work if already installed.
set -euo pipefail

if command -v task > /dev/null; then
  echo "task already installed: $(task --version)"
else
  echo "Installing go-task..."
  curl -fsSL https://taskfile.dev/install.sh | sudo sh -s -- -b /usr/local/bin
fi
