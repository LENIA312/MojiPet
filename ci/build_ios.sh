#!/bin/bash
# Archives the Xcode project exported by Unity (BuildScript.BuildIos) using
# free-account Development signing, so no Apple Developer Program membership
# is required. No physical device needs to be connected for this step -- it
# only requires that the target device's UDID was already registered with
# the team at least once (via a manual Xcode Run to that device).
#
# Note: `xcodebuild -exportArchive` (producing a portable .ipa) reliably
# fails for free Personal Teams on the command line (IDEDistributionMethodManager
# "Unknown Distribution Error" / "exportOptionsPlist error for key method").
# This is a known CLI limitation, not a config bug -- so we stop at the
# .xcarchive and install straight from the .app bundled inside it via
# ci/install_ios.sh instead of exporting an .ipa.
#
# Usage: ci/build_ios.sh <xcode-project-dir> <team-id>

set -euo pipefail

XCODE_PROJECT_DIR="${1:?xcode project dir required}"
TEAM_ID="${2:?team id required}"

BUILD_DIR="${XCODE_PROJECT_DIR}/build"
ARCHIVE_PATH="${BUILD_DIR}/Mojipet.xcarchive"

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

echo "Archiving $PROJECT_FILE (scheme: $SCHEME, team: $TEAM_ID)"

xcodebuild \
  -allowProvisioningUpdates \
  -project "$PROJECT_FILE" \
  -scheme "$SCHEME" \
  -configuration Release \
  -destination "generic/platform=iOS" \
  -archivePath "$ARCHIVE_PATH" \
  DEVELOPMENT_TEAM="$TEAM_ID" \
  archive

echo "Done. Archive at: $ARCHIVE_PATH"
