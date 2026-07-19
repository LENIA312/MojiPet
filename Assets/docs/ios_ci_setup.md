# iOS CI (Jenkins on MacBook) — セットアップ手順

**目的**: Apple Developer Program(有料)登録なしで、無料Apple ID(Personal Team)による開発署名で実機に自動インストールする。Firebase App Distributionは使わない(Ad Hoc配布は無料アカウントでは不可のため)。

**構成**: Jenkins(MacBook上) → Unity batchmodeでXcodeプロジェクトをエクスポート(`BuildScript.BuildIos`) → `xcodebuild archive`で開発署名済み`.xcarchive`を作成(`ci/build_ios.sh`、**実機接続は不要**) → `DEVICE_UDID`が指定されている場合のみ、アーカイブ内の`.app`を`xcrun devicectl`でペアリング済み実機に直接インストール(`ci/install_ios.sh`、**このステップだけ実機接続が必要**)。

**注意**: 当初`xcodebuild -exportArchive`で携帯可能な`.ipa`を書き出す方式を試みたが、無料のPersonal Teamではコマンドライン経由の`-exportArchive`が`IDEDistributionMethodManager ... Unknown Distribution Error`で失敗することが判明した(既知のXcode CLI制限、設定ミスではない)。そのため`.ipa`書き出しは行わず、`.xcarchive`内の`Products/Applications/*.app`を直接実機にインストールする方式に変更した。

`DEVICE_UDID`を空のままビルドすれば、実機を繋がずにアーカイブの作成までできる。UDIDのデバイスは事前に一度Xcodeから手動Runして、プロビジョニングプロファイルに登録済みである必要がある(下記手順5)。

関連ファイル: `Jenkinsfile`(リポジトリ直下)、`ci/build_ios.sh`(ビルド・署名・アーカイブ作成)、`ci/install_ios.sh`(実機インストールのみ)、`Assets/Scripts/Editor/CI/BuildScript.cs`。

## 一度だけ行う手動セットアップ

1. **Unity Hub**: `iOS Build Support`モジュールを導入済みか確認(未導入ならUnity Hub > Installs > 6000.4.9f1 > モジュール追加)。
2. **Xcodeに無料Apple IDを追加**: Xcode > Settings > Accounts > `+` でApple IDを追加。追加後、そのアカウントを選択して「Manage Certificates」を開き、`+` → `Apple Development` 証明書を作成。
3. **Team IDを控える**: 同じAccounts画面でアカウント選択 → 「Manage Certificates」の下、または各Teamの詳細に10桁の英数字のTeam IDが表示される。これを`Jenkinsfile`実行時のパラメータ`TEAM_ID`に渡す。
4. **実機を接続・信頼**: iPhone/iPadをUSB(またはWi-Fi)でMacBookに接続 → デバイス側で「このコンピュータを信頼」→ Xcode > Window > Devices and Simulators で認識されることを確認し、UDIDを控える(右クリック→Copy Identifier、または一覧に表示される値)。
5. **初回は手動ビルドで信頼関係を確立**: Xcodeで一度、その実機をターゲットにRun(⌘R)してみる。初回はプロビジョニングプロファイル作成とデバイス登録が自動で走る。またアプリ初回起動時、デバイス側で Settings > General > VPN & Device Management からデベロッパ証明書を「信頼」する必要がある。ここを手動で済ませておくと、以降のJenkins自動ビルドがスムーズになる。
6. **Jenkinsの実行ユーザーに注意(重要)**: JenkinsをLaunchDaemonとして(ログインしていないバックグラウンドサービスとして)動かしていると、ログインキーチェーンにアクセスできず署名時にコード署名エラーになることがある。Jenkinsは**ログイン中のユーザーセッションでLaunchAgentとして**動かすか、`security unlock-keychain`をビルド前に呼ぶ運用にすること。すでに動いている場合、iOSビルドで原因不明の署名エラーが出たらまずここを疑う。

## Jenkinsジョブの設定

- Pipeline定義: リポジトリ直下の`Jenkinsfile`を使う(Pipeline script from SCM)。
- ビルド実行時にパラメータ`TEAM_ID`(手順3で控えたTeam ID)を入力する。`DEVICE_UDID`(手順4で控えたUDID)は**任意**――空ならアーカイブ作成までで止まり、実機は不要。実機にインストールしたい時だけ、実機を接続した状態でUDIDを入力してビルドする。
- `Jenkinsfile`内の`UNITY_PATH`はUnity 6000.4.9f1のインストールパス想定。実際のMacBookでのインストール場所が違えば書き換える。

## 既知の制約

- **プロビジョニングプロファイルは7日で失効**(無料アカウントの制限)。1週間以上ビルドが無いと次回ビルドが失敗することがある。その場合は手順5をもう一度手動で行うか、単純にJenkinsジョブを再実行すれば自動更新されることが多い(`-allowProvisioningUpdates`を指定済み)。
- 配布はMacBookから直接インストールする方式のみ。`.ipa`書き出し自体が無料アカウントのCLIでは通らないため、他の人にリンクを送って各自インストールしてもらう、という配布はできない(必要になったらApple Developer Program登録 + Firebase App Distributionの導線に切り替える)。
- 無料アカウントは同時に登録できるデバイス数・App ID数に制限がある。
