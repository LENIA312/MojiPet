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