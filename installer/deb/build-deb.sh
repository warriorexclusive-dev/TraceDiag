#!/usr/bin/env bash
set -euo pipefail

SOURCE_DIR="$1"
VERSION="$2"
ARCH="$3"
OUTPUT="$4"

WORK="$(mktemp -d)"
ROOT="$WORK/tracediag_${VERSION}_${ARCH}"

trap 'rm -rf "$WORK"' EXIT

mkdir -p "$ROOT/DEBIAN" "$ROOT/usr/share/tracediag" "$ROOT/usr/bin"
cp -a "$SOURCE_DIR/." "$ROOT/usr/share/tracediag/"
chmod 0755 "$ROOT/usr/share/tracediag/TraceDiag.Console"
ln -sf /usr/share/tracediag/TraceDiag.Console "$ROOT/usr/bin/tracediag"

cat > "$ROOT/DEBIAN/control" <<EOF
Package: tracediag
Version: $VERSION
Section: utils
Priority: optional
Architecture: $ARCH
Maintainer: Enaleven <tracediag@example.invalid>
Description: TraceDiag ICMP route diagnostics console
 Self-contained .NET 10 console for route tracing, packet loss measurement
 and ICMP packet size checks. No separate .NET runtime is required.
EOF

dpkg-deb --build --root-owner-group "$ROOT" "$OUTPUT"
