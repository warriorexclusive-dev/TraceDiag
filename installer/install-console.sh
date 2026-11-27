#!/usr/bin/env bash
set -euo pipefail

SOURCE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
INSTALL_DIR="${INSTALL_DIR:-$HOME/.local/share/TraceDiag}"
BIN_DIR="${BIN_DIR:-$HOME/.local/bin}"

mkdir -p "$INSTALL_DIR" "$BIN_DIR"
cp -a "$SOURCE_DIR"/. "$INSTALL_DIR"/
chmod +x "$INSTALL_DIR/TraceDiag.Console"
ln -sf "$INSTALL_DIR/TraceDiag.Console" "$BIN_DIR/TraceDiag.Console"

echo "Пакет self-contained: отдельный .NET Runtime не требуется."
echo "TraceDiag console installed to: $INSTALL_DIR"
echo "Run: $BIN_DIR/TraceDiag.Console --help"
