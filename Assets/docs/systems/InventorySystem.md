# InventorySystem

Version: 1.0

---

# 概要

InventorySystemは「もじぺっと」においてプレイヤーが所有するアイテムを管理するシステムである。

Version1ではホーム画面へ配置可能な家具・装飾アイテムのみを管理対象とする。

InventorySystemはアイテムの取得・所持判定・配置状態・所持一覧を管理する唯一のシステムである。

---

# 責務

担当する機能

・アイテム取得

・アイテム所持管理

・所持一覧取得

・配置状態管理

・配置中判定

・Inventoryイベント通知

担当しない機能

・ショップ

・通貨管理

・ホーム画面UI

・セーブファイル管理

・家具効果計算

---

# 依存関係

依存先

InventoryMaster

SaveSystem

EventBus

依存される

ShopSystem

HomeSystem

HomeView

TutorialSystem

AchievementSystem

---

# データ

管理対象

```
InventoryData
```

---

## InventoryData

```
OwnedItems
```

---

## OwnedItems

プレイヤーが所有しているアイテム一覧。

```
List<InventoryItemData>
```

---

## InventoryItemData

```
ItemId

IsPlaced

Position

Rotation
```

---

# ItemId

InventoryMasterで定義される一意ID。

```
1001

1002

1003
```

---

# IsPlaced

ホームへ配置済みか。

```
true

false
```

---

# Position

ホーム座標。

```
Vector2
```

ホームスクロール領域内座標。

---

# Rotation

家具の向き。

Version1では

```
0
```

固定。

将来拡張用。

---

# 公開API

## AddItem

```csharp
bool AddItem(
    int itemId)
```

アイテム取得。

成功時

```
true
```

失敗時

```
false
```

---

## HasItem

```csharp
bool HasItem(
    int itemId)
```

所持判定。

---

## GetItems

```csharp
IReadOnlyList<InventoryItemData>
```

所持一覧取得。

---

## RemoveItem

```csharp
bool RemoveItem(
    int itemId)
```

Version1ではDebug専用。

通常ゲームでは使用しない。

---

## PlaceItem

```csharp
bool PlaceItem(
    int itemId,
    Vector2 position)
```

ホームへ配置する。

---

## UnplaceItem

```csharp
bool UnplaceItem(
    int itemId)
```

配置解除。

---

## IsPlaced

```csharp
bool IsPlaced(
    int itemId)
```

配置状態取得。

---

# アイテム取得

取得元

Version1

・Shop購入

将来

・イベント

・実績

・ログインボーナス

---

# 配置

配置できるのは

所有済みアイテムのみ。

同じ家具は一度だけ配置できる。

---

# 配置制限

Version1

配置数制限なし。

重なり判定なし。

グリッドなし。

自由配置。

---

# ホームとの関係

ホーム画面では

InventorySystemから

配置済み家具一覧を取得して生成する。

InventorySystemはPrefabを保持しない。

Prefab取得はHomeSystemが行う。

---

---

# イベント

InventorySystemは所持アイテム・配置状態の変更を通知する。

---

## OnItemAdded

発火条件

アイテム取得成功。

通知内容

```
ItemId
```

利用先

HomeSystem

HomeView

AchievementSystem

TutorialSystem

---

## OnItemRemoved

発火条件

アイテム削除。

Version1ではDebug専用。

通知内容

```
ItemId
```

---

## OnItemPlaced

発火条件

ホームへ配置した。

通知内容

```
ItemId

Position
```

利用先

HomeSystem

HomeView

SaveSystem

---

## OnItemUnplaced

発火条件

配置解除。

通知内容

```
ItemId
```

---

## OnInventoryUpdated

発火条件

Inventory変更。

通知内容

なし。

利用先

HomeView

InventoryView

---

# 内部処理

## AddItem

```
HasItem()

↓

InventoryMaster確認

↓

OwnedItems追加

↓

Save()

↓

OnItemAdded

↓

OnInventoryUpdated
```

---

## PlaceItem

```
HasItem()

↓

InventoryItem取得

↓

Position更新

↓

IsPlaced=true

↓

Save()

↓

OnItemPlaced
```

---

## UnplaceItem

```
InventoryItem取得

↓

IsPlaced=false

↓

Save()

↓

OnItemUnplaced
```

---

## RemoveItem

```
配置中なら解除

↓

OwnedItems削除

↓

Save()

↓

OnItemRemoved
```

---

# 他Systemとの連携

## ShopSystem

利用API

```
AddItem()
```

購入後に呼び出す。

---

## HomeSystem

利用API

```
GetItems()

PlaceItem()

UnplaceItem()

IsPlaced()
```

ホーム表示を管理する。

---

## SaveSystem

保存対象

```
InventoryData
```

---

## AchievementSystem

家具取得数を更新する。

---

## TutorialSystem

初回家具取得を監視する。

---

# 配置仕様

配置済みアイテムは

```
IsPlaced=true
```

位置は

```
Position
```

へ保存する。

ホーム再読込時は

保存座標へ生成する。

---

# 配置座標

Version1では

```
Vector2
```

のみ保持する。

Z座標は保持しない。

描画順はHomeSystemが決定する。

---

# ランタイムキャッシュ

起動時に

```
Dictionary<int, InventoryItemData>
```

を生成する。

取得

```
O(1)
```

---

# HomeSystemとの役割分担

InventorySystem

・所持管理

・配置状態管理

・配置座標保存

HomeSystem

・Prefab生成

・ドラッグ

・当たり判定

・描画順

・表示更新

---

# 表示ルール

Inventory一覧は

```
InventoryMaster.SortOrder
```

昇順で表示する。

配置済みアイテムには

```
配置中
```

ラベルを表示する。

Version1では検索・フィルターは実装しない。

---

---

# エラー処理

InventorySystemは所持アイテムおよび配置情報の整合性を保証する。

処理失敗時は所持状態・配置状態を変更してはならない。

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

## 未所持アイテム

発生条件

所持していないアイテムを配置・削除しようとした。

動作

```
false
```

を返す。

状態は変更しない。

---

## 重複取得

発生条件

既に所持しているアイテムを再取得した。

動作

```
false
```

を返す。

Version1では重複所持を許可しない。

---

## 重複配置

発生条件

既に配置済みのアイテムを再度配置した。

動作

```
false
```

を返す。

位置は変更しない。

---

## MasterData異常

発生条件

InventoryMasterに対象ItemIdが存在しない。

動作

```
InvalidDataException
```

を送出する。

ゲーム起動を中止する。

---

# セーブ

以下タイミングで保存する。

・アイテム取得

・アイテム削除

・配置

・配置解除

・オートセーブ

・アプリ終了

保存はSaveSystem経由で行う。

---

# ロード

ゲーム起動時に

```
InventoryData
```

を読み込む。

存在しない場合は

空のInventoryを生成する。

---

# パフォーマンス

起動時に以下のキャッシュを生成する。

```
Dictionary<int, InventoryItemData>
```

Update()は禁止。

LINQ禁止。

毎フレームGCAlloc禁止。

所持判定

```
O(1)
```

配置状態取得

```
O(1)
```

アイテム取得

```
O(1)
```

---

# ログ

Development Buildのみ。

---

取得

```
[Inventory]

Add

Item=1001
```

---

配置

```
[Inventory]

Place

Item=1001

Pos=(12.4,5.8)
```

---

解除

```
[Inventory]

Unplace

Item=1001
```

---

削除

```
[Inventory]

Remove

Item=1001
```

---

異常

```
[Inventory]

InvalidItem

9999
```

---

Release Buildではログを出力しない。

---

# デバッグ機能

DebugMenu限定。

・全家具取得

・Inventory初期化

・全配置解除

・全家具自動配置

・任意家具取得

・任意家具削除

・配置座標リセット

・Inventory一覧表示

---

# Repository

InventoryRepositoryは作成しない。

InventoryDataはSaveSystemで管理する。

InventoryMasterのみアイテム定義を保持する。

---

# Version互換

Version更新で家具追加時は

InventoryMasterへ追加するだけで対応できる。

既存セーブデータでは新家具は未所持状態となる。

削除されたItemIdはロード時に除外する。

---

# データ整合性

InventoryData内のItemIdは必ずInventoryMasterに存在すること。

存在しないItemIdはロード時に削除する。

配置済みアイテムは必ず所持済みであること。

不整合を検出した場合は

```
IsPlaced = false
```

へ補正する。

---

# Public API 一覧

| API | 概要 |
|------|------|
| AddItem(int itemId) | アイテムを取得する |
| RemoveItem(int itemId) | アイテムを削除する（Debug専用） |
| HasItem(int itemId) | 所持判定 |
| GetItems() | 所持アイテム一覧を取得する |
| PlaceItem(int itemId, Vector2 position) | ホームへ配置する |
| UnplaceItem(int itemId) | 配置解除する |
| IsPlaced(int itemId) | 配置状態を取得する |

---

# 内部API

外部公開しない。

```
Initialize()

LoadInventory()

ValidateInventory()

BuildCache()

GetInventoryItem()

RaiseEvent()

Save()
```

---

# シーケンス

## アイテム購入

```
ShopView

↓

ShopSystem.Purchase()

↓

CurrencySystem.ConsumeMoney()

↓

InventorySystem.AddItem()

↓

SaveSystem.Save()

↓

OnItemAdded

↓

HomeView更新
```

---

## 家具配置

```
HomeView

↓

HomeSystem

↓

InventorySystem.PlaceItem()

↓

InventoryData更新

↓

SaveSystem.Save()

↓

OnItemPlaced

↓

Home更新
```

---

## 家具撤去

```
HomeView

↓

HomeSystem

↓

InventorySystem.UnplaceItem()

↓

SaveSystem.Save()

↓

OnItemUnplaced
```

---

## ゲーム起動

```
GameManager

↓

SaveSystem.Load()

↓

InventorySystem.Initialize()

↓

InventoryData読込

↓

キャッシュ生成

↓

HomeSystem生成
```

---

# データ更新タイミング

| 項目 | 更新タイミング |
|------|----------------|
| OwnedItems | アイテム取得・削除時 |
| Position | 配置・移動時 |
| IsPlaced | 配置・撤去時 |
| InventoryData | Save実行時 |

---

# テストケース

## アイテム取得

- 通常取得
- 重複取得
- 存在しないItemId
- Save後の保持

---

## 所持判定

- 所持あり
- 所持なし
- 削除後

---

## 配置

- 初回配置
- 配置済み再配置
- 未所持アイテム配置
- 配置解除
- Save後の座標保持

---

## ロード

- Inventory読込
- 空Inventory
- 不正ItemId除外
- 配置情報復元

---

## イベント

- OnItemAdded
- OnItemRemoved
- OnItemPlaced
- OnItemUnplaced
- OnInventoryUpdated

---

## Debug

- 全取得
- 全削除
- 自動配置
- 座標リセット

---

# 受け入れ条件

以下をすべて満たした場合、本Systemは完成とする。

・アイテムを取得できる

・所持判定ができる

・所持一覧を取得できる

・家具を配置できる

・配置解除できる

・配置座標が保存される

・ロード後も配置状態が維持される

・イベント通知が行われる

・InventoryDataへ保存される

・重複取得が発生しない

---

# パフォーマンス要件

- Update()を使用しない
- LINQを使用しない
- 毎フレームGCAllocを発生させない
- アイテム検索は O(1)
- 配置状態取得は O(1)
- キャッシュは起動時のみ生成する

---

# 実装チェックリスト

## データ

- [ ] InventoryData
- [ ] InventoryItemData
- [ ] InventoryMaster
- [ ] ランタイムキャッシュ

---

## API

- [ ] AddItem
- [ ] RemoveItem
- [ ] HasItem
- [ ] GetItems
- [ ] PlaceItem
- [ ] UnplaceItem
- [ ] IsPlaced

---

## イベント

- [ ] OnItemAdded
- [ ] OnItemRemoved
- [ ] OnItemPlaced
- [ ] OnItemUnplaced
- [ ] OnInventoryUpdated

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

InventorySystemは「もじぺっと」におけるアイテム所有情報とホーム配置情報を一元管理する唯一のシステムである。

Version1では家具・装飾アイテムのみを対象とし、各アイテムは一度だけ取得できる。配置情報（`Position`・`IsPlaced`）は `InventoryData` として保存され、ゲーム再起動後もホーム画面を復元できる。

HomeSystemはInventorySystemから配置済みアイテムを取得してPrefabを生成する責務のみを持ち、InventorySystemは表示やドラッグ操作を担当しない。

将来的に家具の回転・拡大縮小・スタック・複数所持・倉庫機能などを追加する場合も、本システムを拡張して対応することを前提とする。

---