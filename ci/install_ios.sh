#!/bin/bash
# Installs an already-exported .ipa (from ci/build_ios.sh) onto a connected,
# paired iOS device. Only this step requires the device to be physically
# reachable (USB or paired Wi-Fi) -- building/signing does not.
#
# Usage: ci/install_ios.sh <export-dir> <device-udid>

set -euo pipefail

EXPORT_PATH="${1:?export dir required}"
DEVICE_UDID="${2:?device udid required}"

APP_PATH=$(find "$EXPORT_PATH" -maxdepth 1 -name "*.app" | head -n 1)

if [ -z "$APP_PATH" ]; then
  IPA_PATH=$(find "$EXPORT_PATH" -maxdepth 1 -name "*.ipa" | head -n 1)
  if [ -z "$IPA_PATH" ]; then
    echo "No .app or .ipa found in $EXPORT_PATH" >&2
    exit 1
  fi

  TMP_DIR=$(mktemp -d)
  unzip -q "$IPA_PATH" -d "$TMP_DIR"
  APP_PATH=$(find "$TMP_DIR/Payload" -maxdepth 1 -name "*.app" | head -n 1)
  if [ -z "$APP_PATH" ]; then
    echo "No .app found inside $IPA_PATH" >&2
    exit 1
  fi
fi

echo "Installing $APP_PATH to device $DEVICE_UDID"
xcrun devicectl device install app --device "$DEVICE_UDID" "$APP_PATH"

echo "Done."
