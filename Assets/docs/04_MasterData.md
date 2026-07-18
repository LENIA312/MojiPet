# 04_MasterData.md

Version: 1.0

---

# 概要

本ドキュメントではゲーム内で利用するマスターデータを定義する。

すべてのマスターは ScriptableObject として生成し、
CSVからインポート可能な構造とする。

ゲームロジック内で数値を直接記述してはならない。

---

# Master一覧

|Master|用途|
|-------|------|
|WordMaster|単語辞書|
|PetMaster|文字情報|
|ExpMaster|レベルテーブル|
|ItemMaster|アイテム|
|FacilityMaster|施設|
|ShopMaster|ショップ|
|ResearchMaster|研究速度|
|GameBalanceMaster|ゲーム全体の定数|

---

# WordMaster

## 説明

ゲーム内で利用する辞書。

## Key

Id

## Fields

|Name|Type|説明|
|----|----|----|
|Id|int|ID|
|Word|string|表示文字列|
|Reading|string|ひらがな|
|Category|enum|カテゴリ|
|Difficulty|int|難易度|

---

# PetMaster

## 説明

各文字の初期情報。

46件存在する。

## Key

Character

## Fields

|Name|Type|
|----|----|
|Character|string|
|DisplayName|string|
|InitialLevel|int|
|BaseProduction|int|
|BaseResearchSpeed|float|

例

あ

BaseProduction

1

BaseResearchSpeed

1.0

---

# ExpMaster

## 説明

レベルテーブル

## Key

Level

## Fields

|Name|Type|
|----|----|
|Level|int|
|RequiredExp|int|

例

Lv1

0

Lv2

20

Lv3

60

Lv4

120

---

# ItemMaster

## 説明

アイテム定義

## Key

Id

## Fields

|Name|Type|
|----|----|
|Id|int|
|Name|string|
|Description|string|
|ItemType|enum|
|Price|int|
|Value|float|

---

ItemType

Food

ResearchBoost

Seed

Special

---

例

ことばの実

Food

100

---

ひらめきのしずく

ResearchBoost

500

---

ことばのたね

Seed

5000

---

# FacilityMaster

## 説明

施設データ

## Fields

|Name|Type|
|----|----|
|Id|int|
|FacilityType|enum|
|Level|int|
|UpgradeCost|int|
|EffectValue|float|

---

FacilityType

Research

Library

Garden

---

# ShopMaster

## 説明

ショップ販売データ

## Fields

|Name|Type|
|----|----|
|Id|int|
|ItemId|int|
|Price|int|
|UnlockCondition|string|

---

価格変更は

ShopMasterのみで行う。

---

# ResearchMaster

## 説明

研究時間計算

## Fields

|Name|Type|
|----|----|
|Difficulty|int|
|RequiredSeconds|int|

例

1

30秒

---

2

60秒

---

3

180秒

---

# GameBalanceMaster

## 説明

ゲーム全体で利用する定数

## Fields

|Name|Type|
|----|----|
|MaxOfflineHours|int|
|MaxPetLevel|int|
|MaxFood|int|
|InitialSeedCount|int|
|DefaultResearchSlots|int|

---

初期値

MaxOfflineHours

8

MaxPetLevel

100

InitialSeedCount

1

---

# Enum一覧

## Category

OTHER

PLANT

ANIMAL

PLACE

FOOD

OBJECT

VERB

ADJECTIVE

PERSON

---

## ItemType

Food

ResearchBoost

Seed

Special

---

## FacilityType

Research

Library

Garden

---

# CSV管理

CSVは

MasterData/

配下に配置する。

```
MasterData/

WordMaster.csv

PetMaster.csv

ExpMaster.csv

ItemMaster.csv

FacilityMaster.csv

ShopMaster.csv

ResearchMaster.csv

GameBalanceMaster.csv
```

---

# Import

Unityメニュー

```
Tools

↓

Import MasterData
```

を実行すると

CSV

↓

ScriptableObject

へ変換する。

---

# 更新ルール

マスターの編集は

ScriptableObjectではなく

CSVを編集する。

ScriptableObjectは生成物とする。

---

# 禁止事項

ゲームコード内へ

価格

経験値

研究時間

などの数値を直接書いてはならない。

すべてマスターから取得する。