# 05_ClassDesign.md

Version: 1.0

---

# 概要

本ドキュメントは、ゲーム全体のクラス構成と責務を定義する。

設計方針は以下とする。

- Single Responsibility Principle を遵守する
- UIとゲームロジックを分離する
- MasterDataとSaveDataを分離する
- Managerクラス同士の依存を最小限にする

---

# システム構成

Game
├── Managers
├── Systems
├── Data
├── UI
├── Save
├── Master
└── Utility

---

# Managers

## GameManager

役割

ゲーム全体の初期化

管理内容

- システム初期化
- シーン初期化
- 起動処理

---

## SaveManager

役割

セーブ・ロード

管理内容

- Save
- Load
- AutoSave

---

## MasterManager

役割

ScriptableObject管理

管理内容

- WordMaster
- ItemMaster
- FacilityMaster
- ExpMaster
- ShopMaster

---

## SceneManager

役割

画面遷移

---

# Systems

## PetSystem

役割

文字育成

管理

- レベル
- EXP
- 満腹度
- 生産量

公開API

AddExp()

Feed()

LevelUp()

GetPet()

---

## WordSystem

役割

単語登録

公開API

RegisterWord()

Exists()

GetWord()

---

## ResearchSystem

役割

研究

公開API

StartResearch()

FinishResearch()

UpdateResearch()

---

## IdleSystem

役割

放置時間計算

公開API

CalculateOffline()

---

## DictionarySystem

役割

図鑑管理

公開API

Unlock()

Contains()

GetWords()

---

## ShopSystem

役割

購入処理

公開API

Buy()

CanBuy()

---

## FacilitySystem

役割

施設管理

公開API

Upgrade()

GetLevel()

---

## ItemSystem

役割

アイテム使用

公開API

Use()

Add()

Remove()

---

# UI

## HomeView

もじの庭

---

## PetDetailView

文字詳細

---

## DictionaryView

図鑑

---

## ShopView

ショップ

---

## LibraryView

図書館

---

## ResearchView

研究所

---

## SettingView

設定

---

# Save

## SaveData

ゲーム全体

---

## PlayerData

プレイヤー情報

---

## PetData

文字データ

---

## InventoryData

アイテム

---

## ResearchData

研究

---

## FacilityData

施設

---

## SettingData

設定

---

# Master

WordMaster

PetMaster

ItemMaster

FacilityMaster

ExpMaster

ResearchMaster

ShopMaster

GameBalanceMaster

---

# Utility

TimeUtility

MathUtility

RandomUtility

CsvImporter

JsonUtility

---

# イベント

以下のイベントを利用する。

OnWordRegistered

OnResearchFinished

OnPetLevelUp

OnItemUsed

OnMoneyChanged

OnFacilityUpgrade

OnSave

OnLoad

---

# 初期化順

GameManager

↓

MasterManager

↓

SaveManager

↓

Systems

↓

UI

---

# シーン構成

Boot

↓

Title

↓

Main

---

Mainシーン内

Canvas

UI

Managers

Systems

のみ配置する。

ゲーム画面切り替えはScene遷移ではなくUI切替で行う。

---

# 更新処理

毎フレームUpdateは禁止。

必要最低限のみ使用する。

時間経過はTimerまたはUniTaskを利用する。

---

# データ参照

MasterData

↓

ReadOnly

SaveData

↓

ReadWrite

ゲームロジックはMasterDataを書き換えてはならない。

---

# 依存関係

UI

↓

System

↓

Manager

↓

Data

依存方向を逆転させない。

---

# 責務

Manager

全体管理

System

ゲームロジック

View

表示

Data

保存

Master

定義

Utility

共通処理