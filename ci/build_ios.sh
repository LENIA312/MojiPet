#!/bin/bash
# Builds the Xcode project exported by Unity (BuildScript.BuildIos) and installs
# it onto a physically connected/paired device using free-account Development
# signing. No Apple Developer Program membership or App Store Connect access
# is required, but the device must already be trusted and paired in Xcode
# (Window > Devices and Simulators) at least once beforehand.
#
# Usage: ci/build_ios.sh <xcode-project-dir> <device-udid>

set -euo pipefail

XCODE_PROJECT_DIR="${1:?xcode project dir required}"
DEVICE_UDID="${2:?device udid required}"
DERIVED_DATA_DIR="${XCODE_PROJECT_DIR}/DerivedData"

cd "$XCODE_PROJECT_DIR"

PROJECT_FILE=$(find . -maxdepth 1 -name "*.xcodeproj" | head -n 1)
if [ -z "$PROJECT_FILE" ]; then
  echo "No .xcodeproj found in $XCODE_PROJECT_DIR" >&2
  exit 1
fi

SCHEME=$(xcodebuild -list -project "$PROJECT_FILE" | awk '/Schemes:/{flag=1; next} flag && NF{print; exit}' | xargs)
if [ -z "$SCHEME" ]; then
  echo "No scheme found in $PROJECT_FILE" >&2
  exit 1
fi

echo "Building $PROJECT_FILE (scheme: $SCHEME) for device $DEVICE_UDID"

xcodebuild \
  -allowProvisioningUpdates \
  -project "$PROJECT_FILE" \
  -scheme "$SCHEME" \
  -configuration Release \
  -destination "id=$DEVICE_UDID" \
  -derivedDataPath "$DERIVED_DATA_DIR" \
  build

APP_PATH=$(find "$DERIVED_DATA_DIR/Build/Products" -maxdepth 2 -name "*.app" | head -n 1)
if [ -z "$APP_PATH" ]; then
  echo "Built .app not found under $DERIVED_DATA_DIR/Build/Products" >&2
  exit 1
fi

echo "Installing $APP_PATH to device $DEVICE_UDID"
xcrun devicectl device install app --device "$DEVICE_UDID" "$APP_PATH"

echo "Done."
