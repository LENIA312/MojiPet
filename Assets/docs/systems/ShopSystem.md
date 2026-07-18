# ShopSystem

Version: 1.0

---

# 概要

ShopSystemは「もじぺっと」におけるショップ機能を管理するシステムである。

プレイヤーは所持している言霊を消費してアイテムを購入できる。

購入可能な商品はMasterDataで定義される。

ShopSystemは商品の表示順・購入可否・購入処理・購入履歴を管理する。

---

# 責務

担当する機能

・商品一覧取得

・商品購入

・購入可否判定

・購入履歴管理

・ショップイベント通知

担当しない機能

・通貨管理

・施設強化

・セーブ処理

・UI生成

・演出

---

# 依存関係

依存先

ShopMaster

CurrencySystem

InventorySystem

SaveSystem

EventBus

依存される

ShopView

TutorialSystem

AchievementSystem

GameManager

---

# データ

管理対象

```
ShopData
```

---

## ShopData

```
PurchasedItems
```

---

## PurchasedItems

購入済み商品の一覧。

型

```
List<int>
```

保持する値

```
ItemId
```

重複は許可しない。

---

# 商品データ

ShopMasterで管理する。

```
ItemId

ItemType

Price

Name

Description

Icon

IsEnabled

SortOrder
```

---

## ItemType

Version1

```
Decoration

Consumable

Special
```

将来追加可能。

---

# 公開API

## GetItems

```csharp
IReadOnlyList<ShopItemMaster>
```

購入可能商品一覧を取得する。

---

## Purchase

```csharp
bool Purchase(
    int itemId)
```

商品購入。

成功時

```
true
```

失敗時

```
false
```

---

## CanPurchase

```csharp
bool CanPurchase(
    int itemId)
```

購入可能判定。

---

## IsPurchased

```csharp
bool IsPurchased(
    int itemId)
```

購入済み判定。

---

## GetPurchasedItems

```csharp
IReadOnlyList<int>
```

購入履歴取得。

---

# 購入条件

以下を満たす必要がある。

・商品が存在する

・販売中

・購入済みではない（限定商品の場合）

・所持言霊が足りる

条件を満たさない場合

購入できない。

---

# 購入処理

```
Purchase

↓

CanPurchase

↓

CurrencySystem.ConsumeMoney()

↓

InventorySystem.AddItem()

↓

PurchasedItems追加

↓

Save

↓

Event
```

---

# 商品表示

商品は

```
SortOrder
```

昇順で表示する。

非公開商品

```
IsEnabled=false
```

は表示しない。

---

# 商品価格

価格はShopMasterで管理する。

ShopSystemは価格計算を行わない。

例

```
Item001

100
```

```
Item002

500
```

```
Item003

5000
```

---

# Version1対象

実装対象

・通常購入

・一度だけ購入可能な商品

・価格固定

対象外

・セール

・時間限定販売

・広告視聴購入

・ランダムショップ

・ガチャ

---

---

# 商品状態

各商品は以下のいずれかの状態を持つ。

```
Locked

Available

Purchased
```

---

## Locked

条件を満たしていない。

表示しない、または「？？？」として表示する。

Version1では使用しない。

---

## Available

購入可能。

価格を表示する。

購入ボタンを有効化する。

---

## Purchased

購入済み。

再購入不可の商品は「購入済み」と表示する。

消耗品はVersion1では対象外。

---

# 商品カテゴリ

Version1では以下のカテゴリを使用する。

---

## Decoration

ホーム画面へ配置できる装飾。

例

```
植木鉢

本棚

時計

机
```

---

## Consumable

将来実装。

Version1では未使用。

---

## Special

ゲーム進行で解放される特別なアイテム。

Version1では未使用。

---

# イベント

ShopSystemは以下イベントを送信する。

---

## OnItemPurchased

発火条件

商品購入成功。

通知内容

```
ItemId
```

利用先

HomeView

InventorySystem

AchievementSystem

TutorialSystem

---

## OnPurchaseFailed

発火条件

購入失敗。

通知内容

```
ItemId

Reason
```

利用先

ShopView

---

## OnShopUpdated

発火条件

商品一覧更新。

通知内容

なし。

利用先

ShopView

---

# 内部処理

## Purchase

```
CanPurchase()

↓

CurrencySystem.ConsumeMoney()

↓

InventorySystem.AddItem()

↓

PurchasedItems追加

↓

Save()

↓

OnItemPurchased
```

---

## CanPurchase

```
Item存在確認

↓

販売中判定

↓

購入済み判定

↓

所持金判定

↓

true / false
```

---

## IsPurchased

```
PurchasedItems検索

↓

true / false
```

検索はHashSetでO(1)。

---

## GetItems

```
ShopMaster取得

↓

IsEnabled判定

↓

SortOrder順

↓

返却
```

---

# 他Systemとの連携

## CurrencySystem

利用API

```
CanConsume()

ConsumeMoney()

GetMoney()
```

---

## InventorySystem

利用API

```
AddItem()
```

購入したアイテムを付与する。

---

## SaveSystem

保存対象

```
ShopData
```

---

## ShopView

利用API

```
GetItems()

CanPurchase()

Purchase()
```

---

## AchievementSystem

購入実績更新。

---

# 状態遷移

```
Available

↓

Purchase()

↓

Purchased
```

再購入可能商品の場合は

```
Available
```

のまま状態を維持する。

Version1ではすべて再購入不可。

---

# 表示ルール

商品一覧は

```
SortOrder
```

昇順。

価格表示

```
100

500

2,500

10,000
```

購入済み商品は

```
購入済み
```

と表示する。

価格表示は行わない。

---

# ランタイムキャッシュ

起動時に

```
Dictionary<int, ShopItemMaster>

HashSet<int>
```

を生成する。

商品検索・購入済み判定は

O(1)。

---

# MasterData

ShopMasterには以下を保持する。

```
ItemId

Name

Description

Price

ItemType

Icon

SortOrder

IsEnabled
```

ShopSystemはMasterDataを書き換えてはならない。

---

---

# エラー処理

ShopSystemは購入処理の整合性を保証する。

購入失敗時はアイテム・通貨・購入履歴のいずれも変更してはならない。

---

## ItemId不存在

発生条件

存在しないItemIdを指定した。

動作

```
ArgumentException
```

を送出する。

---

## 非公開商品

発生条件

```
IsEnabled = false
```

動作

購入不可。

```
false
```

を返す。

---

## 購入済み

発生条件

再購入不可の商品を購入しようとした。

動作

```
false
```

を返す。

イベントは送信しない。

---

## 通貨不足

発生条件

```
CurrencySystem.CanConsume()
```

が

```
false
```

を返した。

動作

購入処理を中断する。

```
OnPurchaseFailed
```

を送信する。

---

## Inventory追加失敗

発生条件

```
InventorySystem.AddItem()
```

が失敗した。

動作

購入処理全体を失敗とする。

通貨は消費しない。

購入履歴も追加しない。

---

## MasterData異常

発生条件

価格が負数。

```
Price < 0
```

動作

MasterDataエラー。

ロード失敗。

---

# セーブ

以下タイミングで保存する。

・商品購入成功

・オートセーブ

・アプリ終了

保存はSaveSystem経由で実行する。

---

# ロード

ゲーム起動時に

```
PurchasedItems
```

を読み込む。

存在しない場合は

空の一覧を生成する。

---

# パフォーマンス

商品一覧は起動時にキャッシュする。

```
Dictionary<int, ShopItemMaster>
```

購入済み一覧は

```
HashSet<int>
```

で保持する。

Update()は禁止。

LINQ禁止。

毎フレームGCAlloc禁止。

商品取得は

O(1)。

購入済み判定は

O(1)。

---

# ログ

Development Buildのみ。

---

購入成功

```
[Shop]

Purchase

Item=1001

Price=500
```

---

購入失敗

```
[Shop]

PurchaseFailed

Item=1001

Reason=Money
```

---

ロード

```
[Shop]

Load

Purchased=12
```

---

異常

```
[Shop]

InvalidItemId

9999
```

---

Release Buildではログを出力しない。

---

# デバッグ機能

DebugMenu限定。

・全商品購入

・全商品未購入

・商品購入状態リセット

・任意商品購入

・ショップ更新

・購入履歴表示

---

# Repository

ShopRepositoryは作成しない。

ShopDataはSaveSystemで管理する。

ShopMasterのみ商品定義を保持する。

---

# Version互換

新しい商品追加時は

ShopMasterへ追加するだけで対応可能。

既存セーブデータでは

新商品は未購入状態で開始する。

削除された商品IDはロード時に無視する。

---

# データ整合性

購入履歴に存在するItemIdは

必ずShopMasterに存在すること。

存在しないItemIdはロード時に除外する。

---

---

# Public API 一覧

| API | 概要 |
|------|------|
| GetItems() | 購入可能な商品一覧を取得する |
| Purchase() | 商品を購入する |
| CanPurchase() | 商品を購入可能か判定する |
| IsPurchased() | 商品が購入済みか判定する |
| GetPurchasedItems() | 購入済み商品の一覧を取得する |

---

# 内部API

外部公開しない。

```
Initialize()

LoadPurchasedItems()

BuildItemCache()

BuildPurchasedCache()

ValidateItem()

RaiseEvent()

Save()
```

---

# シーケンス

## 商品購入

```
ShopView

↓

ShopSystem.Purchase()

↓

CanPurchase()

↓

CurrencySystem.ConsumeMoney()

↓

InventorySystem.AddItem()

↓

PurchasedItems追加

↓

SaveSystem.Save()

↓

OnItemPurchased

↓

UI更新
```

---

## ショップ表示

```
ShopView

↓

ShopSystem.GetItems()

↓

ShopMaster取得

↓

SortOrder順に並び替え

↓

一覧表示
```

---

## ゲーム起動

```
GameManager

↓

SaveSystem.Load()

↓

ShopSystem.Initialize()

↓

購入履歴読込

↓

HashSet生成

↓

ショップ利用可能
```

---

# データ更新タイミング

| 項目 | 更新タイミング |
|------|----------------|
| PurchasedItems | 商品購入時 |
| ShopData | Save実行時 |
| 商品一覧 | 起動時 |
| 購入済みキャッシュ | 起動時・購入時 |

---

# テストケース

## 商品一覧

- 商品表示順
- 非公開商品の非表示
- 商品情報取得

---

## 購入

- 通常購入
- 所持金不足
- 購入済み商品の再購入
- 存在しない商品
- 非公開商品の購入

---

## 通貨

- 購入時に言霊が減る
- 購入失敗時に減らない

---

## Inventory

- アイテム付与
- 付与失敗時のロールバック

---

## Save

- 購入履歴保存
- ロード後も購入済み維持

---

## キャッシュ

- HashSet生成
- 商品キャッシュ生成
- O(1)検索

---

## Debug

- 全商品購入
- リセット
- 任意商品購入

---

# 受け入れ条件

以下をすべて満たした場合、本Systemは完成とする。

・商品一覧を取得できる

・商品を購入できる

・購入済み判定ができる

・所持金不足時に購入できない

・再購入不可商品を再購入できない

・購入時にInventoryへアイテムが追加される

・購入履歴が保存される

・ロード後も購入状態が維持される

・イベント通知が行われる

・Version更新で商品追加に対応できる

---

# パフォーマンス要件

- Update()を使用しない
- LINQを使用しない
- 毎フレームGCAllocを発生させない
- 商品検索は O(1)
- 購入済み判定は O(1)
- 商品一覧は起動時にキャッシュ生成する

---

# 実装チェックリスト

## データ

- [ ] ShopData
- [ ] ShopMaster
- [ ] 商品キャッシュ
- [ ] PurchasedItemsキャッシュ

---

## API

- [ ] GetItems
- [ ] Purchase
- [ ] CanPurchase
- [ ] IsPurchased
- [ ] GetPurchasedItems

---

## イベント

- [ ] OnItemPurchased
- [ ] OnPurchaseFailed
- [ ] OnShopUpdated

---

## セーブ

- [ ] Save
- [ ] Load
- [ ] AutoSave対応

---

## テスト

- [ ] UnitTest
- [ ] IntegrationTest
- [ ] SaveLoadTest
- [ ] PerformanceTest

---

# 備考

ShopSystemは「もじぺっと」におけるショップ機能を管理する唯一のシステムである。

商品情報は `ShopMaster` に定義し、購入履歴のみを `ShopData` として保存する。購入処理では必ず `CurrencySystem` を経由して言霊を消費し、`InventorySystem` を経由してアイテムを付与する。

Version1では固定価格・単発購入のみを対象とし、セール・期間限定商品・広告視聴・ガチャ・ランダムショップなどの機能は対象外とする。

新しい商品は `ShopMaster` に追加するだけで利用可能となり、既存セーブデータとの互換性は購入履歴の差分管理によって維持される。

---