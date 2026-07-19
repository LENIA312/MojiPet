#!/bin/bash
# Archives and exports a signed, installable .ipa from the Xcode project
# exported by Unity (BuildScript.BuildIos). Uses free-account Development
# signing, so no Apple Developer Program membership is required. No physical
# device needs to be connected for this step -- it only requires that the
# target device's UDID was already registered with the team at least once
# (via a manual Xcode Run to that device). See ci/install_ios.sh to push the
# resulting .ipa onto a connected device.
#
# Usage: ci/build_ios.sh <xcode-project-dir> <team-id>

set -euo pipefail

XCODE_PROJECT_DIR="${1:?xcode project dir required}"
TEAM_ID="${2:?team id required}"

BUILD_DIR="${XCODE_PROJECT_DIR}/build"
ARCHIVE_PATH="${BUILD_DIR}/Mojipet.xcarchive"
EXPORT_PATH="${BUILD_DIR}/export"
EXPORT_OPTIONS_PLIST="${BUILD_DIR}/ExportOptions.plist"

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

mkdir -p "$BUILD_DIR"

cat > "$EXPORT_OPTIONS_PLIST" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>method</key>
    <string>development</string>
    <key>teamID</key>
    <string>${TEAM_ID}</string>
    <key>signingStyle</key>
    <string>automatic</string>
</dict>
</plist>
PLIST

echo "Archiving $PROJECT_FILE (scheme: $SCHEME)"

xcodebuild \
  -allowProvisioningUpdates \
  -project "$PROJECT_FILE" \
  -scheme "$SCHEME" \
  -configuration Release \
  -destination "generic/platform=iOS" \
  -archivePath "$ARCHIVE_PATH" \
  archive

echo "Exporting signed .ipa"

xcodebuild \
  -exportArchive \
  -allowProvisioningUpdates \
  -archivePath "$ARCHIVE_PATH" \
  -exportPath "$EXPORT_PATH" \
  -exportOptionsPlist "$EXPORT_OPTIONS_PLIST"

echo "Done. Exported to: $EXPORT_PATH"
