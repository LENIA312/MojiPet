# Events

Version: 1.0

---

# 概要

本ドキュメントは「もじぺっと」で使用するイベント一覧を定義する。

イベントはSystem間の疎結合を目的として使用する。

System同士は可能な限りイベント経由で通知を行い、直接参照を避ける。

---

# 基本ルール

イベント名は必ず

```
On～
```

で開始する。

例

```
OnMoneyChanged
```

イベントは過去状態を保持しない。

購読開始以前に送信されたイベントは受信しない。

---

# EventBus

全イベントは

```
EventBus
```

経由で送受信する。

System同士が直接イベントを保持してはならない。

---

# イベント一覧

---

## CurrencySystem

### OnMoneyAdded

送信元

```
CurrencySystem
```

引数

```
long AddedAmount

long CurrentMoney
```

発火

```
AddMoney()
```

利用先

```
HomeView

AchievementSystem
```

---

### OnMoneyConsumed

引数

```
long ConsumedAmount

long CurrentMoney
```

利用先

```
ShopView

FacilityView
```

---

### OnMoneyChanged

引数

```
long CurrentMoney
```

利用先

```
HUD

HeaderView

HomeView
```

---

### OnMoneyInsufficient

引数

```
long RequiredMoney

long CurrentMoney
```

利用先

```
ShopView

FacilityView
```

---

# ResearchSystem

### OnResearchStarted

引数

```
WordId
```

---

### OnResearchCompleted

引数

```
WordId
```

利用先

```
DictionarySystem

PetSystem

HomeView
```

---

# DictionarySystem

### OnWordUnlocked

引数

```
WordId
```

利用先

```
PetSystem

AchievementSystem

HomeView
```

---

### OnDictionaryUpdated

引数

なし

利用先

```
DictionaryView
```

---

# PetSystem

### OnPetCreated

引数

```
PetId
```

---

### OnPetLevelUp

引数

```
PetId

Level
```

利用先

```
HomeView

AchievementSystem
```

---

### OnPetStateChanged

引数

```
PetId
```

利用先

```
HomeView
```

---

# FacilitySystem

### OnFacilityLevelUp

引数

```
FacilityId

Level
```

利用先

```
HomeView

TutorialSystem
```

---

### OnFacilityUnlocked

引数

```
FacilityId
```

---

# InventorySystem

### OnItemAdded

引数

```
ItemId
```

利用先

```
HomeSystem

AchievementSystem
```

---

### OnItemRemoved

引数

```
ItemId
```

---

### OnItemPlaced

引数

```
ItemId

Vector2 Position
```

利用先

```
HomeSystem
```

---

### OnItemUnplaced

引数

```
ItemId
```

---

### OnInventoryUpdated

引数

なし

利用先

```
InventoryView
```

---

# ShopSystem

### OnItemPurchased

引数

```
ItemId
```

利用先

```
InventorySystem

AchievementSystem
```

---

### OnPurchaseFailed

引数

```
ItemId

PurchaseFailureReason
```

---

### OnShopUpdated

引数

なし

---

# SaveSystem

### OnSaveLoaded

引数

```
SaveData
```

---

### OnSaveCompleted

引数

```
DateTime LastSaveUtc
```

---

### OnNewGameCreated

引数

なし

---

### OnMigrationCompleted

引数

```
OldVersion

NewVersion
```

---

# IdleSystem

### OnIdleRewardCalculated

引数

```
long Reward
```

---

### OnIdleRewardClaimed

引数

```
long Reward
```

---

# 共通イベント

### OnGameStarted

送信元

```
GameManager
```

---

### OnGamePaused

---

### OnGameResumed

---

### OnApplicationQuit

---

### OnSceneLoaded

引数

```
SceneName
```

---

# イベント命名規則

イベント名

```
On + 名詞 + 動詞
```

例

```
OnMoneyChanged

OnPetLevelUp

OnResearchCompleted
```

禁止

```
MoneyChanged

ChangeMoney

MoneyEvent
```

---

# 実装ルール

- イベントは過度に細分化しない
- 同じ意味のイベントを複数作らない
- EventBus経由で通知する
- UnityEventはSystem間通知に使用しない
- EventBusから送信されるイベントは同期実行とする
- イベント受信側は送信元の実装を前提にしない

---

# 受け入れ条件

- 全Systemイベントが本ドキュメントに定義されている
- イベント名が統一されている
- 引数が明確に定義されている
- 発火元・利用先が明記されている
- AIがイベントの流れを追跡できる

---