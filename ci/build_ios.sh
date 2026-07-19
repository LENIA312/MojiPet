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
# Note: automatic signing (`-allowProvisioningUpdates`) also fails headlessly
# for free Personal Teams with "No Account for Team" even though the signing
# certificate is present in the keychain -- xcodebuild's account lookup
# doesn't see the Xcode.app GUI's signed-in account from this context. So we
# sign manually instead, referencing the provisioning profile UUID that was
# already generated on disk by the one-time manual Xcode Run (see
# docs/ios_ci_setup.md step 5). If a new device is added later, redo that
# manual Run and pass its new profile UUID here.
#
# Usage: ci/build_ios.sh <xcode-project-dir> <team-id> <provisioning-profile-uuid>

set -euo pipefail

XCODE_PROJECT_DIR="${1:?xcode project dir required}"
TEAM_ID="${2:?team id required}"
PROVISIONING_PROFILE_UUID="${3:?provisioning profile uuid required}"

BUILD_DIR="${XCODE_PROJECT_DIR}/build"
ARCHIVE_PATH="${BUILD_DIR}/Mojipet.xcarchive"

cd "$XCODE_PROJECT_DIR"

PROJECT_FILE=$(find . -maxdepth 1 -name "*.xcodeproj" | head -n 1)
if [ -z "$PROJECT_FILE" ]; then
  echo "No .xcodeproj found in $XCODE_PROJECT_DIR" >&2
  exit 1
fi

# Unity's exported project has multiple schemes (e.g. an app scheme plus a
# GameAssembly library scheme); picking blindly can grab a non-app one, whose
# archive has no Products/Applications/*.app. Prefer the actual app scheme
# ("Unity-iPhone" in every Unity version so far), falling back to the first
# listed scheme only if that name isn't present.
SCHEME_LIST=$(xcodebuild -list -project "$PROJECT_FILE" | awk '/Schemes:/{flag=1; next} flag { if (NF==0) exit; gsub(/^[ \t]+|[ \t]+$/, ""); print }')
if [ -z "$SCHEME_LIST" ]; then
  echo "No schemes found in $PROJECT_FILE" >&2
  exit 1
fi

echo "Available schemes: $(echo "$SCHEME_LIST" | tr '\n' ',')"

if echo "$SCHEME_LIST" | grep -qx "Unity-iPhone"; then
  SCHEME="Unity-iPhone"
else
  SCHEME=$(echo "$SCHEME_LIST" | head -n 1)
  echo "Warning: 'Unity-iPhone' scheme not found, falling back to '$SCHEME'" >&2
fi

mkdir -p "$BUILD_DIR"

echo "Archiving $PROJECT_FILE (scheme: $SCHEME, team: $TEAM_ID, profile: $PROVISIONING_PROFILE_UUID)"

xcodebuild \
  -project "$PROJECT_FILE" \
  -scheme "$SCHEME" \
  -configuration Release \
  -destination "generic/platform=iOS" \
  -archivePath "$ARCHIVE_PATH" \
  CODE_SIGN_STYLE=Manual \
  CODE_SIGN_IDENTITY="Apple Development" \
  PROVISIONING_PROFILE_SPECIFIER="$PROVISIONING_PROFILE_UUID" \
  DEVELOPMENT_TEAM="$TEAM_ID" \
  archive

if [ -z "$(find "$ARCHIVE_PATH/Products/Applications" -maxdepth 1 -name "*.app" 2>/dev/null | head -n 1)" ]; then
  echo "Archive succeeded but no .app found under $ARCHIVE_PATH/Products/Applications -- wrong scheme was likely archived." >&2
  exit 1
fi

echo "Done. Archive at: $ARCHIVE_PATH"
