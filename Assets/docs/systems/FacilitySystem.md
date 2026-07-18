# FacilitySystem

Version: 1.0

---

# 概要

FacilitySystemは「もじぺっと」に登場する施設を管理するシステムである。

施設はプレイヤーが放置で獲得した「言霊」を消費して強化できる。

各施設はゲーム全体へ永続的なボーナスを与える。

Version1では以下3施設を実装する。

・研究施設
・勉強部屋
・図書館

---

# 責務

担当する機能

・施設レベル管理

・施設強化

・施設効果取得

・強化費用計算

・施設状態保存

担当しない機能

・通貨管理

・研究進行

・辞書管理

・放置処理

・UI生成

---

# 依存関係

依存先

FacilityMaster

CurrencySystem

SaveSystem

EventBus

依存される

ResearchSystem

PetSystem

IdleSystem

HomeView

ShopSystem

---

# データ

管理対象

FacilityData

```

FacilityData

```
FacilityId

Level
```

---

# FacilityId

施設ID。

Version1

```
ResearchLab

StudyRoom

Library
```

---

# Level

施設Lv

最小

```
1
```

最大

```
100
```

GameBalanceMasterで変更可能。

---

# 公開API

## UpgradeFacility

```csharp
bool UpgradeFacility(
    FacilityId id)
```

施設を強化する。

成功時

true

失敗時

false

---

## GetLevel

```csharp
int GetLevel(
    FacilityId id)
```

現在Lv取得。

---

## GetUpgradeCost

```csharp
long GetUpgradeCost(
    FacilityId id)
```

次Lv費用取得。

---

## GetEffectValue

```csharp
float GetEffectValue(
    FacilityId id)
```

現在効果取得。

---

## CanUpgrade

```csharp
bool CanUpgrade(
    FacilityId id)
```

強化可能判定。

---

## GetAllFacilities

```csharp
IReadOnlyList<FacilityData>
```

全施設取得。

---

# 施設一覧

## 研究施設

効果

研究速度上昇。

例

```
Lv1

100%
```

```
Lv10

120%
```

```
Lv50

180%
```

```
Lv100

250%
```

倍率計算はResearchSystemが行う。

FacilitySystemは倍率のみ返す。

---

## 勉強部屋

効果

文字ペット獲得経験値倍率。

例

```
Lv1

100%
```

```
Lv20

130%
```

```
Lv50

180%
```

```
Lv100

250%
```

倍率計算はPetSystemが行う。

---

## 図書館

効果

放置中の言霊生産倍率。

例

```
Lv1

100%
```

```
Lv20

125%
```

```
Lv50

170%
```

```
Lv100

220%
```

倍率計算はIdleSystemではなくPetSystem経由で適用する。

---

# 強化

施設はLvを1ずつ上げる。

一括強化はVersion1では実装しない。

---

# 強化条件

以下を満たす必要がある。

・必要言霊所持

・Lv100未満

条件を満たさない場合

強化できない。

---

# 強化処理

```
Upgrade

↓

CanUpgrade

↓

Currency消費

↓

Level++

↓

Save

↓

Event
```

---

# 強化費用

費用はFacilityMasterで管理する。

例

```
Lv1→2

100
```

```
Lv2→3

150
```

```
Lv50→51

15000
```

FacilitySystemは計算式を持たない。

MasterDataのみ参照する。

---

---

# 施設効果

各施設はゲーム全体に永続効果を与える。

FacilitySystemは効果値のみを管理し、実際の計算は各Systemが行う。

---

## 研究施設

対象

ResearchSystem

効果

研究速度倍率。

計算例

```
ResearchTime

=

BaseTime

/

ResearchSpeedMultiplier
```

倍率取得

```
GetEffectValue(
    ResearchLab)
```

---

## 勉強部屋

対象

PetSystem

効果

獲得経験値倍率。

計算例

```
Exp

=

BaseExp

×

StudyRoomMultiplier
```

倍率取得

```
GetEffectValue(
    StudyRoom)
```

---

## 図書館

対象

PetSystem

効果

言霊生産倍率。

計算例

```
Production

=

BaseProduction

×

LibraryMultiplier
```

IdleSystemは最終的な生産量のみ取得する。

---

# 効果取得

倍率は

```
float
```

で返す。

例

```
1.00

1.15

1.50

2.00
```

UI表示時のみ

```
100%

115%

150%
```

へ変換する。

---

# 最大レベル

```
Level

=

100
```

到達時

CanUpgrade()は

```
false
```

を返す。

費用は取得できない。

---

# イベント

FacilitySystemは以下イベントを送信する。

---

## OnFacilityUpgraded

発火条件

施設強化成功。

通知内容

```
FacilityId

Level
```

利用先

HomeView

FacilityView

TutorialSystem

---

## OnFacilityMaxLevel

発火条件

Lv100到達。

通知内容

```
FacilityId
```

利用先

AchievementSystem

---

## OnFacilityLoaded

発火条件

ロード完了。

通知内容

なし。

利用先

HomeView

---

# 内部処理

## UpgradeFacility

```
CanUpgrade

↓

Currency確認

↓

Currency消費

↓

Level++

↓

Save

↓

Event
```

---

## GetEffectValue

```
FacilityMaster検索

↓

Level取得

↓

倍率返却
```

---

## CanUpgrade

```
MaxLv判定

↓

Cost取得

↓

Currency判定

↓

true

false
```

---

## GetUpgradeCost

```
Master検索

↓

Cost返却
```

---

# 状態遷移

```
Lv1

↓

Lv2

↓

Lv3

↓

・・・

↓

Lv100
```

施設レベルは減少しない。

Version1ではリセット機能は存在しない。

---

# 効果適用

FacilitySystemは倍率を返却するのみ。

各Systemが倍率を適用する。

---

ResearchSystem

```
ResearchSpeedMultiplier
```

---

PetSystem

```
ExpMultiplier
```

```
ProductionMultiplier
```

---

IdleSystem

FacilitySystemを直接参照しない。

PetSystem経由で最終生産量を取得する。

---

# ホーム画面

施設はホーム画面内に配置される。

プレイヤーが施設を選択すると

FacilityViewを表示する。

FacilitySystemは座標情報を保持しない。

---

# ランタイムキャッシュ

起動時に

```
Dictionary<FacilityId, FacilityData>
```

を生成する。

GetLevel()

GetEffectValue()

は

O(1)で取得できる。

---

# MasterData

FacilityMasterには以下を保持する。

```
FacilityId

Level

UpgradeCost

EffectValue
```

FacilitySystemは

MasterDataを書き換えてはならない。

---

# エラー処理

FacilitySystemは施設データの整合性を保証する。

異常が発生した場合でも、セーブデータの破損を防ぐことを優先する。

---

## FacilityId不存在

発生条件

存在しないFacilityIdを指定した。

動作

```
ArgumentException
```

を送出する。

---

## FacilityData不存在

発生条件

SaveDataに施設データが存在しない。

動作

初期データを生成する。

```
Level = 1
```

で初期化する。

---

## MasterData不存在

発生条件

FacilityMasterに対象施設が存在しない。

動作

```
InvalidDataException
```

を送出する。

ゲーム起動を中止する。

---

## 最大レベル

発生条件

```
Level >= MaxLevel
```

動作

```
UpgradeFacility()
```

は

```
false
```

を返す。

イベントは送信しない。

---

## 通貨不足

発生条件

強化費用より所持言霊が少ない。

動作

```
false
```

を返す。

施設レベルは変更しない。

---

## Cost異常

発生条件

```
UpgradeCost < 0
```

動作

MasterDataエラー。

ロード失敗。

---

## Effect異常

発生条件

```
EffectValue <= 0
```

動作

MasterDataエラー。

ロード失敗。

---

# セーブ

以下タイミングで保存する。

・施設強化

・オートセーブ

・アプリ終了

---

# ロード

ゲーム起動時

FacilityDataを読み込む。

不足施設がある場合

初期データを生成する。

---

# 他Systemとの連携

## CurrencySystem

利用

```
GetMoney()

ConsumeMoney()
```

---

## ResearchSystem

利用

```
GetEffectValue(
ResearchLab)
```

---

## PetSystem

利用

```
GetEffectValue(
StudyRoom)

GetEffectValue(
Library)
```

---

## SaveSystem

利用

```
Save()

Load()
```

---

## HomeView

通知

```
OnFacilityUpgraded
```

---

## AchievementSystem

通知

```
OnFacilityMaxLevel
```

---

# パフォーマンス

施設数はVersion1で3件固定。

検索は

```
Dictionary<FacilityId, FacilityData>
```

を利用する。

すべて

O(1)。

Update()は禁止。

LINQ禁止。

毎フレームGCAlloc禁止。

---

# ログ

Development Buildのみ。

---

施設強化

```
[Facility]

Upgrade

ResearchLab

Lv12
```

---

最大レベル

```
[Facility]

MaxLevel

Library
```

---

ロード

```
[Facility]

Load

Count=3
```

---

異常

```
[Facility]

InvalidFacilityId

999
```

---

Release Buildではログを出力しない。

---

# デバッグ機能

DebugMenu限定。

・研究施設Lv+1

・勉強部屋Lv+1

・図書館Lv+1

・全施設Lv100

・全施設Lv1

・費用無視強化

・施設再生成

・現在倍率表示

---

# Repository

FacilityRepositoryは使用しない。

FacilityDataはSaveSystem経由で管理する。

MasterDataはFacilityMasterのみ保持する。

---

# キャッシュ

起動時に生成する。

```
Dictionary<
FacilityId,
FacilityData>
```

MasterDataも

```
Dictionary<
FacilityId,
FacilityLevelData[]>
```

として保持し、

Lvごとの効果・費用を高速取得する。

---

# Version互換

Version更新で施設追加時は

新しいFacilityDataを自動生成する。

既存施設レベルは維持する。

施設削除は行わない。

# Public API 一覧

| API | 概要 |
|------|------|
| UpgradeFacility() | 施設を1レベル強化する |
| GetLevel() | 現在レベルを取得する |
| GetUpgradeCost() | 次レベルへの強化費用を取得する |
| GetEffectValue() | 現在の施設効果を取得する |
| CanUpgrade() | 強化可能か判定する |
| GetAllFacilities() | 全施設情報を取得する |

---

# 内部API

外部公開しない。

```
LoadFacilityData()

CreateDefaultFacilityData()

BuildCache()

ValidateMaster()

ValidateSaveData()

RaiseEvent()

Save()
```

---

# シーケンス

## 施設強化

```
FacilityView

↓

FacilitySystem

↓

CanUpgrade()

↓

CurrencySystem.ConsumeMoney()

↓

Level++

↓

SaveSystem.Save()

↓

OnFacilityUpgraded

↓

UI更新
```

---

## ゲーム起動

```
GameManager

↓

SaveSystem.Load()

↓

FacilitySystem

↓

不足施設生成

↓

キャッシュ生成

↓

OnFacilityLoaded
```

---

## 効果取得

```
ResearchSystem

↓

FacilitySystem

↓

FacilityMaster

↓

倍率返却
```

同様に PetSystem も FacilitySystem を参照して倍率を取得する。

---

# データ更新タイミング

| 項目 | 更新タイミング |
|------|----------------|
| FacilityData | 起動時ロード |
| FacilityLevel | 強化時 |
| キャッシュ | 起動時生成 |
| EffectValue | 取得時 |

---

# テストケース

## 初期状態

- 全施設Lv1
- 強化可能
- 効果100%

---

## 強化

- Lv1→2
- Lv50→51
- Lv99→100
- Lv100で強化不可

---

## 通貨

- 所持金不足
- 所持金ぴったり
- 大量所持

---

## 効果

- 研究施設倍率
- 勉強部屋倍率
- 図書館倍率
- Lv変化による倍率更新

---

## ロード

- 初回起動
- 通常ロード
- Save不足データ
- 新施設追加

---

## キャッシュ

- 起動時生成
- 再生成
- O(1)取得

---

## Save

- 強化後保存
- AutoSave
- アプリ終了保存

---

# 受け入れ条件

以下をすべて満たした場合、本Systemは完成とする。

・施設レベルを取得できる

・施設を強化できる

・通貨不足時に強化できない

・Lv100以上にならない

・効果倍率を取得できる

・研究施設倍率がResearchSystemへ反映される

・勉強部屋倍率がPetSystemへ反映される

・図書館倍率が言霊生産へ反映される

・ロード後も施設Lvが保持される

・イベント通知が行われる

・Version更新で施設追加に対応できる

---

# パフォーマンス要件

- Update()を使用しない
- LINQを使用しない
- 毎フレームGCAllocを発生させない
- Facility取得は O(1)
- 効果取得は O(1)
- 起動時のみキャッシュ生成

---

# 実装チェックリスト

## データ

- [ ] FacilityData
- [ ] FacilityMaster
- [ ] キャッシュ生成

---

## API

- [ ] UpgradeFacility
- [ ] GetLevel
- [ ] GetUpgradeCost
- [ ] GetEffectValue
- [ ] CanUpgrade
- [ ] GetAllFacilities

---

## イベント

- [ ] OnFacilityUpgraded
- [ ] OnFacilityMaxLevel
- [ ] OnFacilityLoaded

---

## セーブ

- [ ] Save
- [ ] Load
- [ ] AutoSave

---

## テスト

- [ ] UnitTest
- [ ] IntegrationTest
- [ ] SaveLoadTest
- [ ] PerformanceTest

---

# 備考

FacilitySystemは「もじぺっと」の永続的な成長要素を管理する唯一のシステムである。

Version1では以下の3施設のみを実装する。

- 研究施設（研究速度上昇）
- 勉強部屋（文字ペット獲得経験値上昇）
- 図書館（言霊生産倍率上昇）

FacilitySystemは倍率・レベル・強化費用のみを管理し、実際の計算はResearchSystem・PetSystemが担当する。

新しい施設を追加する場合は `FacilityMaster` に定義を追加し、`FacilityId` を拡張することで対応できる。既存システムへの影響を最小限に抑えられる設計とする。

Version1では施設の売却・ダウングレード・一括強化・時間制限効果は実装しない。

---