# SaveDataSpecification.md

Version 1.0

---

# 概要

本ドキュメントはプレイヤー毎に保存されるデータを定義する。

保存データはJSON形式で管理する。

MasterDataは保存しない。

IDのみ保存する。

---

# SaveData

ゲーム全体

```csharp
GameSaveData
```

```
GameSaveData

PlayerData

PetData[]

ResearchData[]

DictionaryData

InventoryData

FacilityData

SettingData
```

---

# PlayerData

```
PlayerData
```

|項目|型|
|-------|------|
|Kotodama|long|
|CreateTime|DateTime|
|LastLoginTime|DateTime|

---

Kotodama

所持通貨

---

CreateTime

ゲーム開始日時

---

LastLoginTime

放置時間計算に利用

---

# PetData

1文字につき1件。

46件まで。

```
PetData
```

|項目|型|
|-------|------|
|Character|string|
|Level|int|
|Exp|int|
|Hunger|float|
|Unlocked|bool|

---

Character

あ

い

う

など。

---

Unlocked

取得済みか。

---

# DictionaryData

```
DictionaryData
```

保存内容

理解済み単語ID

のみ。

```
List<int>
```

で管理する。

---

例

```
1

5

18

205

900
```

---

# ResearchData

研究中単語

```
ResearchData
```

|項目|型|
|-------|------|
|WordId|int|
|Character|string|
|StartTime|DateTime|
|FinishTime|DateTime|

---

ゲーム起動時

FinishTime

を超えていれば

研究完了。

---

# InventoryData

```
InventoryData
```

Dictionary

```
ItemId

↓

Count
```

で保存。

---

例

```
Food

25

```

```
Seed

3
```

---

# FacilityData

```
FacilityData
```

|項目|型|
|-------|------|
|ResearchLevel|int|
|LibraryLevel|int|
|GardenLevel|int|

---

# SettingData

```
SettingData
```

|項目|型|
|-------|------|
|BGM|float|
|SE|float|
|Language|string|
|Notification|bool|

---

# 保存タイミング

保存するタイミング

・研究完了

・アイテム使用

・施設強化

・レベルアップ

・ショップ購入

・アプリ終了

・バックグラウンド遷移

---

# 放置処理

起動時

```
現在時間

−

LastLoginTime
```

↓

放置時間算出

↓

研究進行

↓

言霊加算

↓

満腹度減少

↓

保存

---

# 保存しないデータ

以下は保存禁止。

WordMaster

ItemMaster

FacilityMaster

ExpMaster

ResearchMaster

GameBalanceMaster

これらはScriptableObjectから取得する。

---

# JSON例

```json
{
    "kotodama":1250,
    "pets":[
        {
            "character":"あ",
            "level":8,
            "exp":120,
            "hunger":80,
            "unlocked":true
        }
    ]
}
```

---

# Version

SaveDataVersion

を保持する。

将来のアップデートで

Migrationを可能にする。

---

# 設計方針

SaveDataは

「プレイヤーの状態」

のみを保存する。

ゲームバランスに関わる数値は

一切保存しない。

すべてMasterDataから取得する。