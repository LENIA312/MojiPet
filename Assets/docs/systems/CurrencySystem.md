# CurrencySystem

Version: 1.0

---

# 概要

CurrencySystemは「もじぺっと」におけるゲーム内通貨「言霊」の管理を担当するシステムである。

Version1では通貨は1種類のみ実装する。

```
言霊
```

CurrencySystemは所持数の変更・取得・消費・加算・不足判定を管理する唯一のシステムであり、他Systemは通貨を直接変更してはならない。

---

# 責務

担当する機能

・所持言霊管理

・通貨加算

・通貨消費

・所持数取得

・不足判定

・通貨イベント通知

担当しない機能

・ショップ

・施設強化

・放置計算

・研究

・セーブファイル管理

---

# 依存関係

依存先

SaveSystem

EventBus

依存される

IdleSystem

PetSystem

FacilitySystem

ShopSystem

HomeView

TutorialSystem

AchievementSystem

---

# データ

管理対象

CurrencyData

```
CurrencyData
```

---

## CurrencyData

```
Money
```

---

# Money

現在所持している言霊。

型

```
long
```

最小

```
0
```

最大

```
long.MaxValue
```

負数になることはない。

---

# 公開API

## GetMoney

```csharp
long GetMoney()
```

現在の所持言霊を取得する。

---

## AddMoney

```csharp
void AddMoney(
    long amount)
```

言霊を加算する。

---

## ConsumeMoney

```csharp
bool ConsumeMoney(
    long amount)
```

言霊を消費する。

成功時

```
true
```

失敗時

```
false
```

---

## CanConsume

```csharp
bool CanConsume(
    long amount)
```

消費可能判定。

---

## SetMoney

```csharp
void SetMoney(
    long amount)
```

Debug専用。

通常ゲームから呼び出してはならない。

---

# 加算

処理

```
現在所持

+

amount
```

結果を保存する。

amountは

```
1以上
```

であること。

---

# 消費

処理

```
現在所持

-

amount
```

不足する場合

変更しない。

---

# 消費判定

```
Money

>=

amount
```

であれば

消費可能。

---

# 獲得元

Version1

・放置報酬

・文字ペット生産

・デバッグ

将来追加可能

・実績

・イベント

・ログインボーナス

---

# 消費先

Version1

・施設強化

・ショップ

将来追加可能

・ガチャ

・イベント交換

・装飾購入

---

# 通貨仕様

Version1では小数を扱わない。

整数のみ。

表示時のみ

```
1,000

10,000

100,000
```

などの桁区切りを行う。

---

# オーバーフロー

加算時

```
long.MaxValue
```

を超える場合

```
long.MaxValue
```

で固定する。

オーバーフローは発生させない。

---

---

# イベント

CurrencySystemは所持言霊の変化を他Systemへ通知する。

---

## OnMoneyAdded

発火条件

言霊加算成功。

通知内容

```
AddedAmount

CurrentMoney
```

利用先

HomeView

EffectSystem

AchievementSystem

---

## OnMoneyConsumed

発火条件

言霊消費成功。

通知内容

```
ConsumedAmount

CurrentMoney
```

利用先

HomeView

ShopView

FacilityView

---

## OnMoneyChanged

発火条件

所持数変更。

通知内容

```
CurrentMoney
```

利用先

HUD

HeaderView

StatusBar

---

## OnMoneyInsufficient

発火条件

消費失敗。

通知内容

```
RequiredMoney

CurrentMoney
```

利用先

ShopView

FacilityView

TutorialSystem

---

# 内部処理

## AddMoney

```
amount確認

↓

Money += amount

↓

MaxValue判定

↓

Save

↓

Event
```

---

## ConsumeMoney

```
CanConsume()

↓

Money -= amount

↓

Save

↓

Event
```

---

## CanConsume

```
Money

>=

amount
```

---

## SetMoney

```
DebugOnly

↓

Money更新

↓

Save

↓

Event
```

---

# 状態遷移

```
0

↓

加算

↓

所持増加

↓

消費

↓

所持減少
```

負数状態は存在しない。

---

# 通貨変更フロー

## 放置報酬

```
IdleSystem

↓

CurrencySystem.AddMoney()

↓

Save

↓

UI更新
```

---

## 施設強化

```
FacilitySystem

↓

CanConsume()

↓

ConsumeMoney()

↓

施設LvUP
```

---

## ショップ購入

```
ShopSystem

↓

CanConsume()

↓

ConsumeMoney()

↓

アイテム付与
```

---

## デバッグ

```
DebugMenu

↓

SetMoney()

↓

UI更新
```

---

# 他Systemとの連携

## IdleSystem

利用API

```
AddMoney()
```

放置報酬を加算する。

---

## PetSystem

利用API

```
AddMoney()
```

文字ペットの言霊生産結果を反映する。

---

## FacilitySystem

利用API

```
CanConsume()

ConsumeMoney()

GetMoney()
```

---

## ShopSystem

利用API

```
CanConsume()

ConsumeMoney()

GetMoney()
```

---

## SaveSystem

保存対象

```
CurrencyData
```

---

## HomeView

利用API

```
GetMoney()
```

所持数表示。

---

# ランタイムキャッシュ

保持するデータは

```
Money
```

のみ。

DictionaryやListは生成しない。

---

# UI表示

表示形式例

```
0

15

250

1,250

15,800

250,000
```

表示処理はUI側で実装する。

CurrencySystemは数値のみ返却する。

---

# 通貨変更ルール

通貨の変更は必ずCurrencySystem経由で行う。

他Systemが

```
Money++
```

```
Money--
```

を直接行ってはならない。

---

# 将来拡張

Version2以降で追加可能。

・有償通貨

・イベント通貨

・期間限定通貨

Version1では実装しない。

---

---

# エラー処理

CurrencySystemは通貨データの整合性を保証する。

異常値を検出した場合は安全な状態へ補正する。

---

## 負数加算

発生条件

```
amount <= 0
```

動作

```
ArgumentOutOfRangeException
```

を送出する。

---

## 負数消費

発生条件

```
amount <= 0
```

動作

```
ArgumentOutOfRangeException
```

を送出する。

---

## 通貨不足

発生条件

```
Money < amount
```

動作

```
false
```

を返す。

OnMoneyInsufficientを送信する。

所持数は変更しない。

---

## オーバーフロー

発生条件

```
Money + amount
>
long.MaxValue
```

動作

```
Money = long.MaxValue
```

とする。

例外は送出しない。

---

## SaveData異常

発生条件

```
Money < 0
```

動作

ロード時に

```
0
```

へ補正する。

---

# セーブ

以下タイミングで保存する。

・通貨加算

・通貨消費

・Debug変更

・オートセーブ

・アプリ終了

保存はSaveSystem経由で実行する。

---

# ロード

ゲーム起動時に

CurrencyDataを取得する。

存在しない場合は

```
Money = 0
```

で初期化する。

---

# パフォーマンス

保持するデータは

```
long Money
```

のみ。

Update()は禁止。

LINQ禁止。

毎フレームGCAlloc禁止。

GetMoney()は

O(1)。

CanConsume()は

O(1)。

---

# ログ

Development Buildのみ。

---

加算

```
[Currency]

Add

+250

Current=1500
```

---

消費

```
[Currency]

Consume

-500

Current=1000
```

---

不足

```
[Currency]

Insufficient

Need=5000

Current=1200
```

---

ロード

```
[Currency]

Load

Money=25000
```

---

異常

```
[Currency]

InvalidAmount

-100
```

---

Release Buildではログを出力しない。

---

# デバッグ機能

DebugMenu限定。

・+100

・+1,000

・+10,000

・+100,000

・所持金MAX

・所持金0

・任意値設定

・現在所持数表示

---

# Version互換

Version更新で

CurrencyDataへ項目追加された場合は

Migrationで補完する。

Version1では

```
Money
```

のみ保持する。

---

# Repository

CurrencyRepositoryは作成しない。

CurrencyDataは

SaveSystem内で管理する。

CurrencySystemはRuntimeデータのみ保持する。

---

# データ整合性

Moneyは常に

```
0
<=
Money
<=
long.MaxValue
```

を満たす。

異常値を検出した場合は補正する。

---

---

# Public API 一覧

| API | 概要 |
|------|------|
| GetMoney() | 現在の所持言霊を取得する |
| AddMoney() | 言霊を加算する |
| ConsumeMoney() | 言霊を消費する |
| CanConsume() | 指定金額を消費可能か判定する |
| SetMoney() | 所持言霊を設定する（Debug専用） |

---

# 内部API

外部公開しない。

```
ValidateAmount()

ClampMoney()

RaiseEvent()

NotifySave()

LoadCurrency()

Initialize()

```

---

# シーケンス

## 言霊獲得

```
IdleSystem / PetSystem

↓

CurrencySystem.AddMoney()

↓

Money更新

↓

SaveSystem.Save()

↓

OnMoneyAdded

↓

OnMoneyChanged

↓

UI更新
```

---

## 言霊消費

```
FacilitySystem / ShopSystem

↓

CanConsume()

↓

ConsumeMoney()

↓

Money更新

↓

SaveSystem.Save()

↓

OnMoneyConsumed

↓

OnMoneyChanged

↓

UI更新
```

---

## 起動

```
GameManager

↓

SaveSystem.Load()

↓

CurrencySystem.Initialize()

↓

Money読込

↓

OnMoneyChanged
```

---

# データ更新タイミング

| 項目 | 更新タイミング |
|------|----------------|
| Money | 加算・消費・Debug変更 |
| CurrencyData | Save実行時 |
| UI表示 | Money変更時 |

---

# テストケース

## 加算

- +1
- +100
- +10000
- long.MaxValue付近
- 上限超過時のClamp

---

## 消費

- 1消費
- 全額消費
- 所持金不足
- 0消費（異常）
- 負数消費（異常）

---

## 判定

- CanConsume=true
- CanConsume=false
- 所持金0
- 所持金MAX

---

## Save

- Save後も所持金維持
- Load後も所持金維持
- 初回起動時は0

---

## イベント

- OnMoneyAdded
- OnMoneyConsumed
- OnMoneyChanged
- OnMoneyInsufficient

---

## Debug

- SetMoney
- Money=0
- Money=MAX
- 任意値設定

---

# 受け入れ条件

以下をすべて満たした場合、本Systemは完成とする。

・所持言霊を取得できる

・言霊を加算できる

・言霊を消費できる

・不足時に消費できない

・負数にならない

・オーバーフローしない

・所持金変更イベントが送信される

・SaveDataへ保存される

・ロード後も所持金が維持される

・Debugから任意値を設定できる

---

# パフォーマンス要件

- Update()を使用しない
- LINQを使用しない
- 毎フレームGCAllocを発生させない
- GetMoney() は O(1)
- CanConsume() は O(1)
- AddMoney() / ConsumeMoney() は定数時間で処理する

---

# 実装チェックリスト

## データ

- [ ] CurrencyData
- [ ] Runtime Money初期化
- [ ] Validation

---

## API

- [ ] GetMoney
- [ ] AddMoney
- [ ] ConsumeMoney
- [ ] CanConsume
- [ ] SetMoney

---

## イベント

- [ ] OnMoneyAdded
- [ ] OnMoneyConsumed
- [ ] OnMoneyChanged
- [ ] OnMoneyInsufficient

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

CurrencySystemは「もじぺっと」における唯一の通貨管理システムであり、ゲーム内通貨「言霊」の所持数を一元管理する。

すべての通貨増減はCurrencySystemを経由して行い、他システムが所持金を直接変更してはならない。

Version1では単一通貨のみを扱う設計とし、施設強化・ショップ購入・放置報酬・文字ペット生産など、すべての通貨処理は本システムを介して実行される。

将来的に有償通貨やイベント専用通貨を追加する場合は、`CurrencyData` を複数通貨対応へ拡張し、既存APIとの互換性を維持することを前提とする。

---