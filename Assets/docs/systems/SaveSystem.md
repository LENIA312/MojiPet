# SaveSystem

Version: 1.0

---

# 概要

SaveSystemは「もじぺっと」のすべてのセーブデータの保存・読込・バージョン管理を担当するシステムである。

本Systemは唯一のセーブデータ管理システムであり、各SystemはSaveSystemを経由してデータを永続化する。

---

# 責務

担当する機能

・セーブ

・ロード

・オートセーブ

・新規ゲーム生成

・Version管理

・SaveData移行

担当しない機能

・ゲームロジック

・UI

・MasterData管理

・ネットワーク通信

---

# 依存関係

依存先

SaveRepository

JsonSerializer

FileSystem

EventBus

依存される

PetSystem

ResearchSystem

DictionarySystem

FacilitySystem

IdleSystem

ShopSystem

CurrencySystem

SettingsSystem

---

# SaveData

管理対象

```
SaveData
```

---

## SaveData

```
Version

LastSaveUtc

CurrencyData

PetData

DictionaryData

ResearchData

FacilityData

IdleData

SettingsData
```

---

# Version

セーブデータVersion。

整数。

例

```
1
```

Versionアップ時はMigrationを実行する。

---

# LastSaveUtc

最後に保存したUTC。

```
2026-07-19T10:30:15Z
```

ISO8601形式。

---

# CurrencyData

所持言霊。

```
Money
```

---

# PetData

文字ペット情報。

```
CharacterId

Level

Exp

Hunger

ProductionRate
```

複数保持する。

---

# DictionaryData

理解済み単語。

```
WordId

Unlocked

UnlockedUtc
```

---

# ResearchData

研究中情報。

```
WordId

StartUtc

FinishUtc
```

---

# FacilityData

施設情報。

```
FacilityId

Level
```

---

# IdleData

放置管理。

```
LastLoginUtc
```

---

# SettingsData

ゲーム設定。

```
BgmVolume

SeVolume

Language

Quality
```

---

# 公開API

## Save

```csharp
void Save()
```

現在状態を保存する。

---

## Load

```csharp
SaveData Load()
```

SaveData取得。

---

## NewGame

```csharp
void NewGame()
```

初期データ生成。

---

## AutoSave

```csharp
void AutoSave()
```

自動保存。

---

## DeleteSave

```csharp
void DeleteSave()
```

セーブ削除。

---

## Exists

```csharp
bool Exists()
```

セーブ存在確認。

---

## GetVersion

```csharp
int GetVersion()
```

保存Version取得。

---

# 保存方式

Version1では

JSON形式を採用する。

文字コード

UTF-8

BOMなし。

---

# 保存場所

Unity

```
Application.persistentDataPath
```

配下。

ファイル名

```
save.json
```

---

# 保存タイミング

以下で保存する。

・単語理解

・施設強化

・ショップ購入

・設定変更

・オートセーブ

・アプリ終了

---

# オートセーブ

デフォルト

60秒毎。

変更可能。

保存中に多重保存は行わない。

---

---

# ロード処理

ゲーム起動時にセーブデータを読み込む。

処理順

```
Exists()

↓

Load()

↓

Version確認

↓

Migration

↓

Validation

↓

各Systemへ配布

↓

OnSaveLoaded
```

セーブデータが存在しない場合は

```
NewGame()
```

を実行する。

---

# 新規ゲーム

初回起動時に実行する。

以下を生成する。

・CurrencyData

・PetData

・DictionaryData

・ResearchData

・FacilityData

・IdleData

・SettingsData

生成後

```
Save()
```

を実行する。

---

# Migration

SaveVersionが現在Versionより古い場合

Migrationを実行する。

例

```
Version1

↓

Version2

↓

Version3
```

順番に適用する。

途中Versionを飛ばしてはならない。

---

# Validation

ロード後

SaveDataを検証する。

確認項目

・Version

・PetData

・DictionaryData

・ResearchData

・FacilityData

・IdleData

・SettingsData

不足データは初期値を生成する。

---

# イベント

SaveSystemは以下イベントを送信する。

---

## OnSaveLoaded

発火条件

ロード完了。

通知内容

```
SaveData
```

利用先

GameManager

---

## OnSaveCompleted

発火条件

保存成功。

通知内容

```
LastSaveUtc
```

利用先

HomeView

DebugMenu

---

## OnNewGameCreated

発火条件

新規ゲーム作成。

通知内容

なし。

利用先

TutorialSystem

---

## OnMigrationCompleted

発火条件

Migration完了。

通知内容

```
OldVersion

NewVersion
```

利用先

DebugMenu

---

# 内部処理

## Save

```
各System取得

↓

SaveData生成

↓

Json変換

↓

ファイル保存

↓

Event
```

---

## Load

```
ファイル読込

↓

Json解析

↓

Version確認

↓

Migration

↓

Validation

↓

返却
```

---

## NewGame

```
初期Data生成

↓

Save

↓

Event
```

---

## AutoSave

```
保存中判定

↓

Save

↓

完了
```

---

## DeleteSave

```
ファイル削除

↓

NewGame
```

---

# 状態遷移

```
未ロード

↓

ロード中

↓

利用可能

↓

保存中

↓

利用可能
```

保存中は再度Saveを開始しない。

---

# 保存対象

Version1で保存するデータ

・通貨

・文字ペット

・辞書

・研究

・施設

・放置

・設定

保存しないデータ

・MasterData

・一時UI状態

・演出

・キャッシュ

---

# JSON

Version1では整形しない。

```
Formatting.None
```

を使用する。

容量削減を優先する。

---

# 圧縮

Version1では圧縮しない。

将来

gzip対応可能。

---

# 暗号化

Version1では暗号化しない。

将来

AES暗号化を追加可能。

---

# バックアップ

Version1ではバックアップファイルを保持しない。

将来

```
save.bak
```

対応可能。

---

---

# エラー処理

SaveSystemはセーブデータの整合性を保証する。

ロードに失敗した場合でも、ゲームが起動不能にならないことを最優先とする。

---

## セーブファイル不存在

発生条件

```
save.json
```

が存在しない。

動作

```
NewGame()
```

を実行する。

例外は送出しない。

---

## JSON解析失敗

発生条件

JSONの破損。

動作

ロード失敗。

エラーログ出力後、

新規ゲームを生成する。

---

## Version不正

発生条件

```
Version <= 0
```

動作

Migration対象外。

新規ゲーム生成。

---

## Migration失敗

発生条件

Migration中に例外。

動作

ロード失敗。

元データは変更しない。

新規ゲーム生成。

---

## 保存失敗

発生条件

ディスク書き込み失敗。

動作

エラーログ出力。

イベント送信なし。

ゲーム続行。

---

## Validation失敗

発生条件

必要データ欠落。

動作

不足データのみ生成する。

可能な限り既存データを維持する。

---

## データ異常

例

```
Money < 0

Level < 1

Exp < 0
```

動作

Validation時に初期値へ補正する。

---

# 他Systemとの連携

## PetSystem

保存

```
PetData
```

---

## DictionarySystem

保存

```
DictionaryData
```

---

## ResearchSystem

保存

```
ResearchData
```

---

## FacilitySystem

保存

```
FacilityData
```

---

## IdleSystem

保存

```
IdleData
```

---

## CurrencySystem

保存

```
CurrencyData
```

---

## SettingsSystem

保存

```
SettingsData
```

---

# パフォーマンス

SaveDataは一度だけ生成する。

JSON変換は保存時のみ実施する。

Update()は禁止。

LINQ禁止。

毎フレームGCAlloc禁止。

オートセーブ中は二重保存を禁止する。

---

# ログ

Development Buildのみ。

---

ロード

```
[Save]

Load

Version=1
```

---

保存

```
[Save]

Save

Success
```

---

Migration

```
[Save]

Migration

1→2
```

---

Validation

```
[Save]

Validation

Completed
```

---

異常

```
[Save]

InvalidJson
```

---

Release Buildではログを出力しない。

---

# デバッグ機能

DebugMenu限定。

・強制Save

・強制Load

・NewGame

・DeleteSave

・SaveVersion表示

・JSON表示

・Validation実行

・Migration実行

---

# Repository

SaveRepositoryが

ファイルI/Oのみ担当する。

SaveSystemはRepository経由で読み書きを行う。

直接ファイルアクセスしてはならない。

---

# Version互換

SaveVersionを保持する。

Version更新時はMigrationを追加する。

古いSaveDataを削除せず、

可能な限り自動移行する。

Migrationは冪等性（同じ処理を複数回実行しても結果が変わらないこと）を満たすこと。

---

# データ整合性

SaveData内の参照は常にWordMaster・FacilityMasterなどのMasterDataと整合していること。

ロード時に参照先が存在しない場合は、

該当データのみ初期化し、他のデータは保持する。

---

---

# Public API 一覧

| API | 概要 |
|------|------|
| Save() | 現在のゲーム状態を保存する |
| Load() | セーブデータを読み込む |
| AutoSave() | オートセーブを実行する |
| NewGame() | 新規ゲームデータを生成する |
| DeleteSave() | セーブデータを削除する |
| Exists() | セーブデータの存在を確認する |
| GetVersion() | セーブデータのバージョンを取得する |

---

# 内部API

外部公開しない。

```
CreateSaveData()

ValidateSaveData()

RunMigration()

Serialize()

Deserialize()

WriteFile()

ReadFile()

RaiseEvent()
```

---

# シーケンス

## ゲーム起動

```
Application

↓

GameManager

↓

SaveSystem.Load()

↓

Validation

↓

Migration

↓

各Systemへデータ配布

↓

OnSaveLoaded

↓

ゲーム開始
```

---

## セーブ

```
各System

↓

SaveSystem

↓

CreateSaveData()

↓

Serialize()

↓

WriteFile()

↓

OnSaveCompleted
```

---

## オートセーブ

```
Timer

↓

AutoSave()

↓

Save()

↓

保存完了
```

保存中の場合は処理をスキップする。

---

## 新規ゲーム

```
NewGame()

↓

初期データ生成

↓

Save()

↓

OnNewGameCreated

↓

ゲーム開始
```

---

# データ更新タイミング

| 項目 | 更新タイミング |
|------|----------------|
| SaveData | Save実行時 |
| LastSaveUtc | Save成功時 |
| Version | Migration完了時 |
| Validation結果 | Load時 |

---

# テストケース

## 新規ゲーム

- セーブファイルなし
- 初期データ生成
- 初回保存成功

---

## セーブ

- 通常保存
- 連続保存
- オートセーブ
- 終了時保存

---

## ロード

- 通常ロード
- セーブなし
- JSON破損
- Validation補正
- Migration実行

---

## Version

- Version一致
- Version更新
- Migration成功
- Migration失敗

---

## Delete

- ファイル削除
- NewGame生成
- 再保存

---

## Validation

- 通貨負数
- 不正レベル
- 欠損データ
- Masterとの差分

---

## Repository

- 読込成功
- 保存成功
- 書込失敗
- 読込失敗

---

# 受け入れ条件

以下をすべて満たした場合、本Systemは完成とする。

・ゲーム状態を保存できる

・ゲーム状態を読み込める

・新規ゲームを生成できる

・オートセーブが動作する

・セーブデータ削除ができる

・Version管理が動作する

・Migrationが実行できる

・Validationで異常データを補正できる

・JSON破損時でもゲームを開始できる

・各Systemへデータを正しく配布できる

・イベント通知が行われる

---

# パフォーマンス要件

- Update()を使用しない
- LINQを使用しない
- 毎フレームGCAllocを発生させない
- SaveData生成は保存時のみ
- JSONシリアライズは保存時のみ
- ロードはゲーム起動時のみ実施
- 保存中の二重Saveを禁止する

---

# 実装チェックリスト

## データ

- [ ] SaveData
- [ ] SaveRepository
- [ ] Version管理
- [ ] Migration実装

---

## API

- [ ] Save
- [ ] Load
- [ ] AutoSave
- [ ] NewGame
- [ ] DeleteSave
- [ ] Exists
- [ ] GetVersion

---

## イベント

- [ ] OnSaveLoaded
- [ ] OnSaveCompleted
- [ ] OnNewGameCreated
- [ ] OnMigrationCompleted

---

## 保存

- [ ] JSON保存
- [ ] JSON読込
- [ ] Validation
- [ ] Migration

---

## テスト

- [ ] UnitTest
- [ ] IntegrationTest
- [ ] SaveLoadTest
- [ ] MigrationTest
- [ ] PerformanceTest

---

# 備考

SaveSystemは「もじぺっと」における唯一の永続化システムであり、すべてのゲームデータの保存・読込・バージョン管理を担当する。

各システム（PetSystem、ResearchSystem、DictionarySystem、FacilitySystem、IdleSystem、CurrencySystemなど）は、自身でファイルアクセスを行わず、必要なデータをSaveSystemへ渡す設計とする。

Version1ではJSON形式によるローカル保存を採用し、圧縮・暗号化・クラウド同期・バックアップファイルは対象外とする。将来的な拡張では、Migrationによる後方互換性を維持しながら、新しいデータ項目を追加できることを前提とする。

Validationはロード時に必ず実行し、不正値や欠損データを可能な限り補正することで、セーブデータの破損によるゲーム進行不能を防ぐ。

---