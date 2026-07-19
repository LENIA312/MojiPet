#!/bin/bash
# Installs the .app bundled inside an already-built .xcarchive (from
# ci/build_ios.sh) onto a connected, paired iOS device. Only this step
# requires the device to be physically reachable (USB or paired Wi-Fi) --
# archiving/signing does not.
#
# Usage: ci/install_ios.sh <xcarchive-path> <device-udid>

set -euo pipefail

ARCHIVE_PATH="${1:?xcarchive path required}"
DEVICE_UDID="${2:?device udid required}"

APP_PATH=$(find "$ARCHIVE_PATH/Products/Applications" -maxdepth 1 -name "*.app" | head -n 1)
if [ -z "$APP_PATH" ]; then
  echo "No .app found under $ARCHIVE_PATH/Products/Applications" >&2
  exit 1
fi

echo "Installing $APP_PATH to device $DEVICE_UDID"
xcrun devicectl device install app --device "$DEVICE_UDID" "$APP_PATH"

echo "Done."
