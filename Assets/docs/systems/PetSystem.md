# PetSystem

Version: 1.0

---

# 概要

PetSystemは「もじぺっと」における文字ペットの育成を管理するシステムである。

本システムは以下を担当する。

- レベル
- 経験値
- 満腹度
- 言霊生産
- 研究速度補正
- ペット状態取得
- ペット解放

UIは本Systemを通してのみ文字ペットへアクセスする。

---

# 責務

PetSystemが担当する機能

- 文字ペット生成
- ペット取得
- ペット育成
- 経験値付与
- レベルアップ
- 満腹度更新
- 放置中の生産量計算
- 言霊生産速度計算

担当しない機能

- 単語判定
- 辞書検索
- アイテム購入
- セーブ
- 画面更新

---

# 依存関係

依存先

PetRepository

MasterRepository

EventBus

依存される

HomeView

PetDetailView

ResearchSystem

IdleSystem

ShopSystem

---

# データ

管理対象

PetData

```

PetData

```
CharacterId

Level

Exp

Hunger

Unlocked

```

---

# CharacterId

ひらがなを識別するID。

例

```
1 = あ

2 = い

3 = う

...

46 = ん
```

MasterDataと一致する。

---

# Level

現在レベル。

最小

1

最大

GameBalanceMasterで定義。

---

# Exp

現在経験値。

整数。

負数にならない。

---

# Hunger

満腹度。

0〜100

100

満腹

0

空腹

---

# Unlocked

取得済み。

false

未取得

true

育成可能

---

# 公開API

## GetPet

```csharp
PetData GetPet(int characterId)
```

指定文字を取得する。

存在しない場合はnullを返さない。

例外を発生させる。

---

## GetAllPets

```csharp
IReadOnlyList<PetData> GetAllPets()
```

取得済み文字を返す。

未取得は含めない。

---

## UnlockPet

```csharp
void UnlockPet(int characterId)
```

新しい文字を解放する。

処理

Unlocked=true

↓

初期レベル設定

↓

イベント送信

```
OnPetUnlocked
```

重複解放は禁止。

---

## AddExperience

```csharp
void AddExperience(
    int characterId,
    int amount)
```

経験値を加算する。

amount

1以上。

負数禁止。

経験値加算後

必要経験値以上なら

レベルアップを行う。

---

## Feed

```csharp
void Feed(
    int characterId,
    ItemType item)
```

エサを与える。

処理

アイテム消費

↓

満腹度増加

↓

効果時間設定

↓

イベント送信

```
OnPetFed
```

---

## GetProductionRate

```csharp
long GetProductionRate(
    int characterId)
```

1秒あたりの言霊生産量を返す。

計算式

```
BaseProduction

×

LevelMultiplier

×

HungerMultiplier

×

FacilityBonus
```

戻り値は整数。

---

## UpdateHunger

```csharp
void UpdateHunger(
    TimeSpan elapsed)
```

経過時間に応じて満腹度を減少させる。

満腹度は0未満にならない。

---

## GetResearchSpeed

```csharp
float GetResearchSpeed(
    int characterId)
```

研究速度倍率。

例

```
1.0

通常

0.7

空腹

1.3

レアエサ使用中
```

---

# レベルアップ

レベルアップ条件

```
Exp

>=

RequiredExp
```

RequiredExpは

ExpMasterより取得。

処理順

経験値確認

↓

レベル+1

↓

余剰EXP保持

↓

イベント通知

↓

自動保存

---

# レベルアップイベント

送信イベント

```
OnPetLevelUp
```

通知内容

CharacterId

OldLevel

NewLevel

```

---

# 経験値システム

## 基本ルール

文字ペットは「新しく理解した単語」に含まれていた場合のみ経験値を獲得する。

同一単語から経験値を複数回取得することはできない。

例

単語

あめ

↓

「あ」「め」

経験値獲得

再び「あめ」を登録

↓

経験値なし

---

## 経験値配分

基本経験値

```
1文字につき

BaseExp
```

BaseExpはGameBalanceMasterで管理する。

例

```
BaseExp = 10
```

---

## 長い単語ボーナス

文字数に応じて補正を行う。

例

```
2文字

×1.0

3文字

×1.1

4文字

×1.2

5文字以上

×1.3
```

倍率はMasterDataで管理する。

---

## カテゴリボーナス

レアエサ使用中のみ適用。

例

```
植物

×

ひらめきのはっぱ

↓

経験値1.5倍
```

対象カテゴリ以外は通常倍率。

---

## レベルアップ処理

経験値加算後

```
while(CurrentExp >= RequiredExp)
```

を繰り返す。

1回で複数レベルアップ可能。

余剰経験値は保持する。

---

# 満腹度

## 概要

満腹度は文字ペットの体調を表す。

放置時間経過により徐々に減少する。

---

## 初期値

```
100
```

---

## 最小

```
0
```

---

## 最大

```
100
```

---

## 減少速度

MasterDataで管理する。

例

```
1時間

↓

5減少
```

---

## 状態

### 満腹

```
80〜100
```

効果

研究速度100%

生産速度100%

---

### 普通

```
40〜79
```

効果

変化なし。

---

### 空腹

```
1〜39
```

効果

研究速度80%

生産速度80%

---

### 飢餓

```
0
```

効果

研究速度50%

生産速度50%

レベルダウンや死亡などのペナルティは発生しない。

---

# エサ

## 通常エサ

効果

満腹度回復

例

```
+30
```

---

## レアエサ

通常効果

満腹度回復

追加効果

一定時間カテゴリ補正を付与する。

例

```
ひらめきのしずく

↓

未知語理解速度

+50%
```

---

```
ひらめきのはっぱ

↓

植物カテゴリ

経験値

+50%
```

---

```
ひらめきのつち

↓

地名カテゴリ

経験値

+50%
```

---

効果時間はItemMasterで定義する。

---

# 言霊生産

## 概要

すべての取得済み文字ペットは時間経過に応じて言霊を生産する。

ゲーム起動中・放置中の両方で生産される。

---

## 基本式

```
Production

=

BaseProduction

×

LevelMultiplier

×

HungerMultiplier

×

FacilityMultiplier
```

---

## BaseProduction

レベル1時の基礎値。

MasterDataで管理する。

---

## LevelMultiplier

例

```
Lv1

1.0

Lv10

2.0

Lv20

3.5

Lv50

7.0
```

計算式はGameBalanceMasterで定義する。

---

## HungerMultiplier

```
満腹

1.0

普通

1.0

空腹

0.8

飢餓

0.5
```

---

## FacilityMultiplier

施設強化による倍率。

例

```
研究所Lv5

↓

1.25倍
```

---

## 生産タイミング

リアルタイムでは毎秒加算しない。

一定間隔でまとめて加算する。

例

```
10秒ごと
```

加算後

```
OnMoneyChanged
```

イベントを送信する。

---

# 放置処理

アプリ起動時

```
現在時刻

-

LastLoginTime
```

を計算する。

↓

経過時間取得

↓

満腹度減少

↓

言霊生産

↓

研究進行

↓

保存

---

## 放置上限

MasterDataで定義する。

例

```
24時間
```

24時間を超える場合は24時間として扱う。

---

# 研究速度補正

文字ペットは研究中のみ速度補正を持つ。

基本倍率

```
1.0
```

空腹時

```
0.8
```

飢餓

```
0.5
```

レアエサ

```
+カテゴリ倍率
```

複数倍率は乗算で計算する。

---

# イベント

PetSystemはゲーム内の各システムへイベントを通知する。

UIはイベントを監視して画面更新を行う。

---

## OnPetUnlocked

発火条件

新しい文字ペット取得

通知内容

```
CharacterId
```

利用先

- HomeView
- DictionarySystem
- TutorialSystem

---

## OnPetLevelUp

発火条件

レベルアップ

通知内容

```
CharacterId

OldLevel

NewLevel
```

利用先

- HomeView
- PetDetailView
- EffectSystem
- SaveSystem

---

## OnPetFed

発火条件

エサ使用

通知内容

```
CharacterId

ItemId

OldHunger

NewHunger
```

利用先

- HomeView
- PetDetailView

---

## OnPetUpdated

発火条件

PetData更新

通知内容

```
CharacterId
```

利用先

全UI

---

## OnProductionCompleted

発火条件

言霊加算

通知内容

```
Amount

TotalMoney
```

利用先

- CurrencyView
- ShopView

---

# 内部処理

## UnlockPet

処理順

```
入力

↓

取得済み確認

↓

Master存在確認

↓

PetData生成

↓

Unlocked=true

↓

Level=1

↓

Exp=0

↓

Hunger=100

↓

Save

↓

Event送信
```

---

## AddExperience

```
入力

↓

文字存在確認

↓

Exp加算

↓

必要経験値取得

↓

LevelUp判定

↓

イベント

↓

Save
```

---

## Feed

```
入力

↓

アイテム所持確認

↓

アイテム消費

↓

満腹度加算

↓

100でClamp

↓

特殊効果付与

↓

Save

↓

Event
```

---

## OfflineProduction

```
経過時間取得

↓

満腹度更新

↓

倍率計算

↓

言霊計算

↓

研究進行

↓

Save
```

---

# 状態遷移

## ペット

```
未取得

↓

取得

↓

育成中

↓

レベルアップ

↓

育成中
```

ペットを失うことはない。

---

## 満腹度

```
満腹

↓

普通

↓

空腹

↓

飢餓

↓

エサ

↓

満腹
```

死亡状態は存在しない。

---

# エラー処理

## CharacterId不存在

例外発生

```
ArgumentException
```

---

## Exp負数

例外発生

```
ArgumentOutOfRangeException
```

---

## 未取得文字へ経験値

無視しない。

例外発生。

開発時に問題へ気付きやすくする。

---

## エサ不足

戻り値

```
false
```

イベント送信なし。

---

## レベル上限

最大レベル到達時

経験値加算のみ停止。

レベルアップイベントは発火しない。

---

# セーブ

以下のタイミングで保存する。

・レベルアップ

・エサ使用

・新規取得

・放置報酬受取

・アプリ終了

・バックグラウンド遷移

---

# ロード

ロード時

PetRepositoryから全PetData取得。

ロード中はイベント送信しない。

ロード完了後

```
OnPetUpdated
```

のみ送信する。

---

# 他Systemとの連携

## WordSystem

新単語理解

↓

PetSystem.AddExperience()

---

## IdleSystem

放置時間計算

↓

PetSystem.UpdateHunger()

↓

PetSystem.CalculateProduction()

---

## ShopSystem

エサ購入

↓

Inventory追加

↓

PetSystemは関与しない

---

## ItemSystem

アイテム使用

↓

PetSystem.Feed()

---

## FacilitySystem

施設レベル取得

↓

Production倍率計算

---

## DictionarySystem

文字取得通知のみ受け取る。

単語登録は担当しない。

---

# パフォーマンス

毎フレーム処理は禁止。

満腹度は必要時のみ更新する。

言霊は一定間隔でまとめて加算する。

LINQの利用は禁止。

毎回List生成は禁止。

GCAllocを発生させない。

---

# ログ出力

開発時のみ。

```
[PetSystem]

LevelUp

CharacterId=12

Lv 9 -> 10
```

```
[PetSystem]

Feed

CharacterId=3

Food=NormalFood
```

リリースビルドでは出力しない。

---

# デバッグ機能

開発ビルドのみ。

以下の機能を提供する。

・全ペット解放

・経験値追加

・レベル変更

・満腹度変更

・言霊追加

・レアエサ効果付与

これらはDebugMenu経由のみ利用可能とする。

---

# Public API 一覧

|API|概要|
|------|---------------------------|
|GetPet()|指定した文字ペットを取得|
|GetAllPets()|取得済み文字一覧を取得|
|UnlockPet()|新しい文字ペットを解放|
|AddExperience()|経験値を付与|
|Feed()|エサを与える|
|UpdateHunger()|満腹度更新|
|CalculateProduction()|言霊生産量計算|
|CollectProduction()|生産済み言霊受取|
|GetResearchSpeed()|研究速度倍率取得|
|GetProductionRate()|現在の生産速度取得|
|IsUnlocked()|取得済み判定|
|CanLevelUp()|レベルアップ可能判定|

---

# 内部API

外部公開しない。

```
LevelUp()

ApplyFoodEffect()

CalculateHungerMultiplier()

CalculateLevelMultiplier()

ClampHunger()

RaiseEvent()

Save()

```

---

# シーケンス

## 単語理解

```
WordSystem

↓

DictionarySystem

↓

PetSystem.AddExperience()

↓

LevelUp判定

↓

Save

↓

Event

↓

UI更新
```

---

## エサ

```
HomeView

↓

ItemSystem

↓

PetSystem

↓

Hunger更新

↓

Save

↓

Event

↓

UI更新
```

---

## 放置

```
Game起動

↓

IdleSystem

↓

PetSystem

↓

満腹度更新

↓

生産量計算

↓

言霊加算

↓

Save

↓

Home表示
```

---

# レベルアップフロー

```
EXP取得

↓

必要経験値確認

↓

Level++

↓

余剰EXP保持

↓

生産倍率更新

↓

研究倍率更新

↓

Event

↓

Save
```

---

# データ更新タイミング

|項目|更新タイミング|
|------|----------------|
|Level|レベルアップ|
|Exp|経験値取得|
|Hunger|時間経過・エサ|
|Unlocked|取得時のみ|
|Production|計算時|

---

# テストケース

## ペット取得

- 未取得状態では取得できない
- 解放後は取得できる
- 重複解放できない

---

## レベル

- Lv1から開始する
- 経験値加算でレベルアップする
- 一度に複数レベルアップできる
- 最大レベルを超えない

---

## 経験値

- 負数を受け付けない
- 0を受け付けない
- 正常に加算される
- 余剰EXPが保持される

---

## 満腹度

- 初期100
- 100を超えない
- 0未満にならない
- 時間経過で減少する
- エサで回復する

---

## 生産

- レベルで増加する
- 空腹で減少する
- 施設倍率が反映される
- 放置でも計算される

---

## レアエサ

- 対象カテゴリのみ倍率が適用される
- 効果時間終了後に解除される
- 重複ルールが正しく動作する

---

## セーブ

- レベルアップ後保存される
- エサ後保存される
- 放置後保存される

---

## ロード

- 前回状態が復元される
- 満腹度が維持される
- レベルが維持される
- 経験値が維持される

---

# 受け入れ条件

以下をすべて満たした場合、本Systemは完成とする。

・文字ペットを取得できる

・育成できる

・レベルアップする

・満腹度が変化する

・言霊を生産する

・放置計算が正しい

・イベント通知が行われる

・SaveDataへ反映される

・ロードで復元される

・クラッシュしない

---

# パフォーマンス要件

- Update()を使用しない
- 毎フレームGCAllocを発生させない
- LINQを使用しない
- 全ペット同時計算でも60FPSを維持する
- 放置24時間分の計算が100ms以内に完了すること

---

# 実装チェックリスト

## データ

- [ ] PetData
- [ ] PetRepository
- [ ] Master参照

---

## API

- [ ] GetPet
- [ ] GetAllPets
- [ ] UnlockPet
- [ ] AddExperience
- [ ] Feed
- [ ] UpdateHunger
- [ ] CalculateProduction
- [ ] CollectProduction

---

## イベント

- [ ] OnPetUnlocked
- [ ] OnPetUpdated
- [ ] OnPetLevelUp
- [ ] OnPetFed
- [ ] OnProductionCompleted

---

## セーブ

- [ ] Save
- [ ] Load
- [ ] AutoSave

---

## テスト

- [ ] UnitTest
- [ ] IntegrationTest
- [ ] OfflineTest

---

# 備考

PetSystemは「もじぺっと」の中核システムである。

ゲーム内のすべての文字ペットの状態は本Systemが唯一管理する。

他SystemはPetDataを直接変更してはならない。

状態変更は必ずPetSystemを経由して行う。

これにより、イベント通知・保存・バランス計算の一貫性を保証する。
