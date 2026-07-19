# iOS CI (Jenkins on MacBook) — セットアップ手順

**目的**: Apple Developer Program(有料)登録なしで、無料Apple ID(Personal Team)による開発署名で実機に自動インストールする。Firebase App Distributionは使わない(Ad Hoc配布は無料アカウントでは不可のため)。

**構成**: Jenkins(MacBook上) → Unity batchmodeでXcodeプロジェクトをエクスポート(`BuildScript.BuildIos`) → `xcodebuild archive`で**手動署名**済み`.xcarchive`を作成(`ci/build_ios.sh`、**実機接続は不要**) → `DEVICE_UDID`が指定されている場合のみ、アーカイブ内の`.app`を`xcrun devicectl`でペアリング済み実機に直接インストール(`ci/install_ios.sh`、**このステップだけ実機接続が必要**)。

**注意1**: 当初`xcodebuild -exportArchive`で携帯可能な`.ipa`を書き出す方式を試みたが、無料のPersonal Teamではコマンドライン経由の`-exportArchive`が`IDEDistributionMethodManager ... Unknown Distribution Error`で失敗することが判明した(既知のXcode CLI制限、設定ミスではない)。そのため`.ipa`書き出しは行わず、`.xcarchive`内の`Products/Applications/*.app`を直接実機にインストールする方式に変更した。

**注意2**: 当初は自動署名(`-allowProvisioningUpdates`)を使っていたが、`security find-identity -v -p codesigning`で証明書自体はターミナルから見えているにもかかわらず、Jenkins経由の`xcodebuild`だけ`error: No Account for Team`で失敗し続けた。これはXcode.app GUIでサインインしたアカウント情報がCLI実行コンテキストから見えないという既知のクセで、キーチェーンのロック(LaunchDaemon問題)とは別物。回避策として**手動署名**(`CODE_SIGN_STYLE=Manual`)に切り替え、手順5の手動Runで既に生成済みのプロビジョニングプロファイルのUUIDを直接指定する方式にした。アカウント参照が不要になるため、この問題を完全に回避できる。

`DEVICE_UDID`を空のままビルドすれば、実機を繋がずにアーカイブの作成までできる。UDIDのデバイスは事前に一度Xcodeから手動Runして、プロビジョニングプロファイルに登録済みである必要がある(下記手順5)。

関連ファイル: `Jenkinsfile`(リポジトリ直下)、`ci/build_ios.sh`(ビルド・署名・アーカイブ作成)、`ci/install_ios.sh`(実機インストールのみ)、`Assets/Scripts/Editor/CI/BuildScript.cs`。

## 一度だけ行う手動セットアップ

1. **Unity Hub**: `iOS Build Support`モジュールを導入済みか確認(未導入ならUnity Hub > Installs > 6000.4.9f1 > モジュール追加)。
2. **Xcodeに無料Apple IDを追加**: Xcode > Settings > Accounts > `+` でApple IDを追加。追加後、そのアカウントを選択して「Manage Certificates」を開き、`+` → `Apple Development` 証明書を作成。
3. **Team IDを控える**: 同じAccounts画面でアカウント選択 → 「Manage Certificates」の下、または各Teamの詳細に10桁の英数字のTeam IDが表示される。これを`Jenkinsfile`実行時のパラメータ`TEAM_ID`に渡す。
4. **実機を接続・信頼**: iPhone/iPadをUSB(またはWi-Fi)でMacBookに接続 → デバイス側で「このコンピュータを信頼」→ Xcode > Window > Devices and Simulators で認識されることを確認し、UDIDを控える(右クリック→Copy Identifier、または一覧に表示される値)。
5. **初回は手動ビルドで信頼関係を確立**: Xcodeで一度、その実機をターゲットにRun(⌘R)してみる(Bundle Identifierは`com.lenia.mojipet`、SigningのTeamは手順3のTeamを選択、Automatically manage signingはON)。初回はプロビジョニングプロファイル作成とデバイス登録が自動で走る。またアプリ初回起動時、デバイス側で Settings > General > VPN & Device Management からデベロッパ証明書を「信頼」する必要がある。さらに、iOS 16以降は端末側の Settings > プライバシーとセキュリティ > デベロッパモード をONにする必要がある(再起動が必要)。
6. **生成されたプロビジョニングプロファイルのUUIDを控える**: 手順5の後、以下のコマンドでMac内に生成されたプロファイルを探す。
   ```bash
   find ~/Library -iname "*.mobileprovision" 2>/dev/null
   ```
   `UserData/Provisioning Profiles/`配下に見つかったファイルから、以下でUUIDと対象Bundle IDを確認する。
   ```bash
   security cms -D -i "<見つかったファイルパス>" > /tmp/profile.plist
   /usr/libexec/PlistBuddy -c "Print :Name" /tmp/profile.plist
   /usr/libexec/PlistBuddy -c "Print :Entitlements:application-identifier" /tmp/profile.plist
   /usr/libexec/PlistBuddy -c "Print :UUID" /tmp/profile.plist
   ```
   `application-identifier`が`<何か>.com.lenia.mojipet`になっているものを選び、その`UUID`を`Jenkinsfile`実行時のパラメータ`PROVISIONING_PROFILE_UUID`に渡す。**新しいデバイスを追加した時や、プロファイルが失効した時は、この手順5〜6をやり直してUUIDを更新する必要がある**(下記「既知の制約」参照)。
7. **Jenkinsの実行ユーザーに注意**: JenkinsをLaunchDaemonとして(ログインしていないバックグラウンドサービスとして)動かしていると、ログインキーチェーンにアクセスできず署名時にコード署名エラーになることがある。Jenkinsは**ログイン中のユーザーセッションで**動かすこと。ただし証明書自体が見えていても`No Account for Team`が出ることがあり(注意2参照)、そちらは手動署名で回避済み。

## Jenkinsジョブの設定

- Pipeline定義: リポジトリ直下の`Jenkinsfile`を使う(Pipeline script from SCM)。
- ビルド実行時にパラメータ`TEAM_ID`(手順3)、`PROVISIONING_PROFILE_UUID`(手順6)を入力する。`DEVICE_UDID`(手順4)は**任意**――空ならアーカイブ作成までで止まり、実機は不要。実機にインストールしたい時だけ、実機を接続した状態でUDIDを入力してビルドする。
- `Jenkinsfile`内の`UNITY_PATH`はUnity 6000.4.9f1のインストールパス想定。実際のMacBookでのインストール場所が違えば書き換える。

## 既知の制約

- **プロビジョニングプロファイルは7日で失効**(無料アカウントの制限)。手動署名方式のため自動更新はされない。失効したら手順5〜6をもう一度手動で行い、新しいUUIDを`PROVISIONING_PROFILE_UUID`パラメータに入れ直す必要がある。
- 配布はMacBookから直接インストールする方式のみ。`.ipa`書き出し自体が無料アカウントのCLIでは通らないため、他の人にリンクを送って各自インストールしてもらう、という配布はできない(必要になったらApple Developer Program登録 + Firebase App Distributionの導線に切り替える)。
- 無料アカウントは同時に登録できるデバイス数・App ID数に制限がある。
