#!/usr/bin/env bash
# Installs the coding-agent CLIs: Claude Code, OpenAI Codex, OpenCode, and herdr.
# Runs from postCreateCommand; safe to re-run — skips anything already installed.
set -euo pipefail

if command -v claude > /dev/null; then
  echo "claude already installed: $(claude --version)"
else
  echo "Installing Claude Code..."
  curl -fsSL https://claude.ai/install.sh | bash
fi

export CODEX_NON_INTERACTIVE=1
if command -v codex > /dev/null; then
  echo "codex already installed: $(codex --version)"
else
  echo "Installing OpenAI Codex CLI..."
  curl -fsSL https://chatgpt.com/codex/install.sh | sh
fi

if command -v opencode > /dev/null; then
  echo "opencode already installed: $(opencode --version)"
else
  echo "Installing OpenCode..."
  curl -fsSL https://opencode.ai/install | bash
fi

if command -v herdr > /dev/null; then
  echo "herdr already installed: $(herdr --version)"
else
  echo "Installing herdr..."
  curl -fsSL https://herdr.dev/install.sh | sh
fi
