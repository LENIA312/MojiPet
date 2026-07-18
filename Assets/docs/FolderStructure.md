# FolderStructure

Version: 1.0

---

# 概要

本ドキュメントは「もじぺっと」のUnityプロジェクトにおけるフォルダ構成を定義する。

目的は以下の通り。

・責務の分離
・保守性向上
・AI実装時の配置統一
・アセット管理の標準化

---

# 基本構成

Assets/

```
Assets
├── _Project
│
├── Art
│
├── Audio
│
├── Fonts
│
├── Materials
│
├── Prefabs
│
├── Scenes
│
├── Settings
│
├── Shaders
│
├── Sprites
│
├── StreamingAssets
│
└── Scripts
```

---

# Scripts

```
Scripts
├── Runtime
├── Editor
└── Tests
```

EditorコードはRuntimeへ依存してはならない。

---

# Runtime

```
Runtime
├── Core
├── Systems
├── Managers
├── Models
├── Master
├── Save
├── Events
├── UI
├── Utilities
└── Extensions
```

---

# Core

ゲーム全体の基盤。

```
GameManager

SceneLoader

Bootstrap

ApplicationLifetime

ServiceLocator
```

---

# Systems

ゲームロジック。

```
CurrencySystem

PetSystem

ResearchSystem

DictionarySystem

WordSystem

FacilitySystem

InventorySystem

ShopSystem

IdleSystem

SaveSystem
```

1システム1フォルダを推奨する。

例

```
Systems/
    Currency/
    Pet/
    Shop/
```

---

# Managers

画面単位の管理。

例

```
HomeManager

ResearchManager

ShopManager
```

ManagerはUI制御のみ担当する。

---

# Models

Runtimeデータ。

例

```
PetData

CurrencyData

InventoryData

SaveData
```

ModelはUnity APIへ依存しない。

---

# Master

ScriptableObject。

```
WordMaster

PetMaster

ShopMaster

FacilityMaster
```

MasterDataは読み取り専用。

---

# Save

保存関連。

```
SaveRepository

Migration

Serializer
```

---

# Events

EventBus。

```
GameEvents

UIEvents

SystemEvents
```

イベント名は `On` で始める。

例

```
OnMoneyChanged

OnResearchCompleted
```

---

# UI

```
UI
├── Views
├── Presenters
├── Components
└── Windows
```

Viewは表示のみ担当する。

ゲームロジックを書いてはならない。

---

# Utilities

共通Utility。

```
TimeUtility

MathUtility

RandomUtility

FileUtility
```

---

# Extensions

拡張メソッド。

```
StringExtensions

TransformExtensions

ColorExtensions
```

---

# Editor

Unity Editor専用コード。

```
CustomInspector

Tools

MenuItems

Importer
```

Runtimeコードへ依存しない。

---

# Tests

```
EditMode

PlayMode
```

Unity Test Frameworkを使用する。

---

# 命名規則

フォルダ

```
PascalCase
```

例

```
Systems

Managers

Utilities
```

---

# 禁止事項

・Resourcesフォルダへの追加（Version1では使用しない）

・Scripts直下へのクラス追加

・EditorコードをRuntimeへ配置

・循環参照

・RuntimeからEditor参照

---

# Addressables

Version1ではAddressablesを使用する。

Prefab・Sprite・AudioはAddressablesで管理する。

Resources.Loadは禁止。

---

# 受け入れ条件

・全クラスが適切なフォルダへ配置される

・RuntimeとEditorが分離されている

・Systemごとの責務が分離されている

・AIが配置先を迷わない構成になっている

---