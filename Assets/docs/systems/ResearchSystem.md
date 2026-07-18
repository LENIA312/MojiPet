# ResearchSystem

Version: 1.0

---

# 概要

ResearchSystemは、文字ペットが新しい単語を理解するための研究を管理するシステムである。

本システムは以下を担当する。

- 研究開始
- 研究進行
- 放置進行
- 研究完了
- 未知語理解
- 研究キュー管理
- 研究時間計算

UIはResearchSystemを通して研究状態を取得する。

---

# 責務

担当する機能

- 研究開始
- 研究中データ保持
- 研究時間計算
- 放置進行
- 完了判定
- 単語理解通知

担当しない機能

- 辞書検索
- 経験値付与
- ペット育成
- アイテム購入
- セーブ

---

# 依存関係

依存先

ResearchRepository

MasterRepository

DictionarySystem

PetSystem

EventBus

依存される

HomeView

ResearchView

IdleSystem

TutorialSystem

---

# データ

管理対象

ResearchData

```

ResearchData

```
CharacterId

WordId

ProgressTime

RequiredTime

Status

StartTime

FinishTime
```

---

# CharacterId

研究を行う文字ペットID。

---

# WordId

研究対象単語ID。

DictionaryMasterと一致する。

---

# ProgressTime

現在の研究進行時間。

単位

秒

---

# RequiredTime

研究完了まで必要時間。

MasterDataから取得する。

---

# Status

研究状態。

Idle

研究なし

Researching

研究中

Completed

研究完了

---

# StartTime

研究開始UTC時刻。

---

# FinishTime

研究完了予定UTC時刻。

---

# 公開API

## StartResearch

```csharp
void StartResearch(
    int characterId,
    int wordId)
```

指定した文字ペットで研究開始。

処理

研究中確認

↓

研究生成

↓

開始時刻設定

↓

完了予定設定

↓

保存

↓

イベント送信

---

## CancelResearch

```csharp
void CancelResearch(
    int characterId)
```

研究中止。

進行度は失われる。

完了済み研究は中止できない。

---

## GetResearch

```csharp
ResearchData GetResearch(
    int characterId)
```

研究情報取得。

存在しない場合

nullではなく例外を返す。

---

## GetAllResearch

```csharp
IReadOnlyList<ResearchData>
```

全研究情報取得。

---

## UpdateResearch

```csharp
void UpdateResearch(
    TimeSpan elapsed)
```

経過時間を加算する。

完了判定を行う。

---

## CompleteResearch

```csharp
void CompleteResearch(
    int characterId)
```

研究完了処理。

単語理解イベント送信。

DictionarySystemへ通知。

---

## GetRemainingTime

```csharp
TimeSpan GetRemainingTime(
    int characterId)
```

残り時間取得。

研究していない場合

Zeroを返す。

---

## GetProgressRate

```csharp
float GetProgressRate(
    int characterId)
```

0〜1で返却。

UI表示専用。

---

# 研究開始

研究開始条件

・ペット取得済み

・研究中ではない

・対象単語が未理解

・必要施設解放済み

すべて満たした場合のみ開始。

---

# 研究時間

基本時間はDictionaryMasterで管理。

例

```
あめ

30秒

ありがとう

10分

国際連合教育科学文化機関

8時間
```

研究時間は単語難易度に応じて決定する。

---

# 研究速度倍率

研究時間へ以下倍率を適用。

ペットレベル

施設

満腹度

レアエサ

イベント効果

最終倍率は乗算。

---

# レベル補正

例

Lv1

1.0

Lv10

1.2

Lv20

1.5

Lv50

2.5

倍率はGameBalanceMasterで管理する。

---

# 満腹補正

満腹

1.0

普通

1.0

空腹

0.8

飢餓

0.5

PetSystemから取得する。

---

# 施設補正

研究施設レベルに応じて研究速度を補正する。

倍率はFacilitySystemから取得する。

例

```
研究施設 Lv1

1.00

研究施設 Lv5

1.20

研究施設 Lv10

1.50
```

---

# レアエサ補正

レアエサ使用中は対象カテゴリのみ研究速度を上昇させる。

例

```
ひらめきのはっぱ

↓

植物カテゴリ

研究速度

×1.50
```

対象外カテゴリには適用しない。

---

# 研究進行

研究はリアルタイムで進行する。

毎フレーム更新は行わない。

一定間隔で経過時間を加算する。

例

```
1秒毎
```

---

## UpdateResearch

処理順

```
経過時間取得

↓

研究中確認

↓

倍率取得

↓

Progress加算

↓

完了判定

↓

必要ならCompleteResearch()

↓

Save
```

---

# 研究完了

ProgressTime

>=

RequiredTime

になった場合

研究完了とする。

---

## CompleteResearch

処理

```
研究完了

↓

DictionarySystemへ通知

↓

Word理解

↓

PetSystemへ経験値通知

↓

Status変更

↓

Save

↓

Event送信
```

研究完了後は自動で次の研究は開始しない。

---

# 放置進行

IdleSystemから経過時間を受け取る。

処理

```
経過時間

↓

倍率計算

↓

Progress加算

↓

完了判定

↓

保存
```

---

## 放置上限

GameBalanceMasterで管理する。

例

```
24時間
```

24時間以上放置した場合は24時間として扱う。

---

# 同時研究

Version1では

同時研究数

```
1
```

固定。

研究施設強化による増加は実装しない。

---

# イベント

ResearchSystemは以下イベントを送信する。

---

## OnResearchStarted

発火条件

研究開始

通知内容

```
CharacterId

WordId
```

利用先

・ResearchView

・HomeView

---

## OnResearchProgress

発火条件

進行更新

通知内容

```
CharacterId

ProgressRate
```

利用先

・ProgressBar

---

## OnResearchCompleted

発火条件

研究完了

通知内容

```
CharacterId

WordId
```

利用先

・DictionarySystem

・PetSystem

・HomeView

---

## OnResearchCanceled

発火条件

研究中止

通知内容

```
CharacterId
```

利用先

ResearchView

---

# 内部処理

## StartResearch

```
入力

↓

ペット確認

↓

研究中確認

↓

Dictionary確認

↓

ResearchData生成

↓

開始時刻設定

↓

完了予定時刻計算

↓

Save

↓

Event
```

---

## UpdateResearch

```
経過時間取得

↓

倍率取得

↓

Progress更新

↓

完了判定

↓

Save
```

---

## CompleteResearch

```
Dictionary登録

↓

PetSystem通知

↓

Status更新

↓

Save

↓

Event
```

---

# 状態遷移

```
Idle

↓

Researching

↓

Completed

↓

Idle
```

Completed状態は通知完了後

Idleへ戻る。

---

# 単語理解通知

研究完了後

DictionarySystemへ

```
UnlockWord(wordId)
```

を通知する。

ResearchSystemは辞書を直接更新しない。

---

# PetSystem連携

研究完了時

```
PetSystem.AddExperience()
```

を呼び出す。

経験値量はWordSystemが算出した値を使用する。

---

# DictionarySystem連携

理解済み判定

↓

単語登録

↓

図鑑更新

↓

収集率更新

ResearchSystemは登録成功のみ確認する。

---

# エラー処理

ResearchSystemは異常系を明確に検出し、呼び出し元へ通知する。

エラーを無視して処理を継続してはならない。

---

## CharacterId不存在

発生条件

存在しないCharacterIdを指定した。

動作

```
ArgumentException
```

を送出する。

---

## WordId不存在

発生条件

DictionaryMasterに存在しないWordId。

動作

```
ArgumentException
```

を送出する。

---

## 未取得ペット

発生条件

Unlocked=false

動作

研究開始不可。

```
InvalidOperationException
```

を送出する。

---

## 研究中に再度開始

発生条件

Status == Researching

動作

開始処理を行わない。

```
InvalidOperationException
```

を送出する。

---

## 理解済み単語

発生条件

DictionarySystemで理解済み。

動作

研究開始しない。

```
InvalidOperationException
```

を送出する。

---

## Cancel対象なし

研究中でない場合。

処理

何もしない。

イベント送信なし。

---

## CompleteResearch対象なし

研究対象が存在しない。

処理

例外。

```
InvalidOperationException
```

---

# セーブ

以下のタイミングで保存する。

・研究開始

・研究中止

・研究完了

・放置計算終了

・アプリ終了

・バックグラウンド遷移

---

# ロード

ロード時

ResearchRepositoryからResearchData取得。

ロード中はイベント送信しない。

ロード完了後

```
OnResearchProgress
```

のみ送信する。

---

# 他Systemとの連携

## PetSystem

取得

```
GetResearchSpeed()
```

利用

研究速度倍率。

---

完了

```
AddExperience()
```

通知。

---

## DictionarySystem

利用

```
IsUnlocked()

UnlockWord()
```

---

## IdleSystem

起動時

```
ElapsedTime
```

受け取り。

UpdateResearch()

を実行する。

---

## FacilitySystem

利用

```
GetResearchMultiplier()
```

施設倍率取得。

---

## ItemSystem

利用

```
GetResearchItemMultiplier()
```

アイテム補正取得。

---

## WordSystem

利用

```
GetWordCategory()

GetWordDifficulty()

GetWordBaseTime()
```

---

# パフォーマンス

Update()禁止。

毎フレーム研究進行計算は禁止。

経過時間更新は一定間隔でまとめて実行する。

LINQ禁止。

foreachによるGCAlloc禁止。

研究データ検索はDictionaryまたは配列を使用する。

---

# ログ

Development Buildのみ。

---

研究開始

```
[Research]

Start

CharacterId=12

WordId=3014
```

---

研究完了

```
[Research]

Completed

CharacterId=12

WordId=3014
```

---

研究中止

```
[Research]

Cancel

CharacterId=12
```

---

リリースビルドではログ出力しない。

---

# デバッグ機能

DebugMenuからのみ利用可能。

・即時完了

・研究時間+1時間

・研究時間+24時間

・研究開始

・研究中止

・全研究完了

・ランダム研究開始

---

# 内部Repository

ResearchRepositoryはResearchDataのみ管理する。

Dictionary更新は禁止。

Pet更新は禁止。

---

# 排他制御

Version1ではシングルスレッドのみ対応。

ResearchData更新中は再入を禁止する。

同時更新が発生した場合は後続処理を破棄する。

---

# Time管理

時間はすべてUTCで管理する。

```
DateTime.UtcNow
```

または

```
TimeUtility.CurrentUtc
```

を使用する。

LocalTimeは禁止。

---

# Version互換

SaveVersion変更時

ResearchDataに追加項目が存在しない場合は初期値を設定する。

データ欠損でクラッシュしてはならない。

---

# Public API 一覧

| API | 概要 |
|------|------|
| StartResearch() | 新しい研究を開始する |
| CancelResearch() | 研究を中止する |
| GetResearch() | 指定した文字ペットの研究情報を取得する |
| GetAllResearch() | 全研究情報を取得する |
| UpdateResearch() | 経過時間を反映する |
| CompleteResearch() | 研究完了処理を行う |
| GetRemainingTime() | 残り研究時間を取得する |
| GetProgressRate() | 研究進捗率を取得する |
| IsResearching() | 研究中か判定する |
| CanStartResearch() | 研究開始可能か判定する |

---

# 内部API

外部公開しない。

```
CreateResearch()

CalculateResearchSpeed()

CalculateRequiredTime()

UpdateProgress()

CheckCompleted()

RaiseEvent()

Save()

```

---

# シーケンス

## 研究開始

```
HomeView

↓

ResearchView

↓

ResearchSystem

↓

Dictionary確認

↓

Pet確認

↓

Research生成

↓

Save

↓

Event

↓

UI更新
```

---

## 研究更新

```
Timer

↓

ResearchSystem

↓

倍率取得

↓

Progress更新

↓

Complete判定

↓

Save

↓

UI更新
```

---

## 放置進行

```
Game起動

↓

IdleSystem

↓

ResearchSystem

↓

経過時間反映

↓

研究完了判定

↓

Save

↓

Home表示
```

---

## 研究完了

```
ResearchSystem

↓

DictionarySystem

↓

Word登録

↓

PetSystem

↓

経験値付与

↓

Event

↓

UI更新
```

---

# データ更新タイミング

| 項目 | 更新タイミング |
|------|----------------|
| ProgressTime | 経過時間更新時 |
| RequiredTime | 研究開始時 |
| Status | 開始・完了・中止 |
| StartTime | 研究開始時 |
| FinishTime | 研究開始時 |
| WordId | 研究開始時 |

---

# テストケース

## 研究開始

- 未取得ペットでは開始できない
- 理解済み単語では開始できない
- 研究中は再開始できない
- 正常にResearchDataが生成される

---

## 研究進行

- ProgressTimeが増加する
- 倍率が正しく適用される
- 満腹度補正が反映される
- 施設補正が反映される
- レアエサ補正が反映される

---

## 放置

- オフライン時間が反映される
- 放置上限を超えない
- 研究完了する
- Saveされる

---

## 完了

- DictionarySystemへ通知される
- PetSystemへ経験値通知される
- StatusがCompletedになる
- Eventが送信される

---

## 中止

- Progressが破棄される
- Eventが送信される
- Saveされる

---

## セーブ

- 研究開始後保存される
- 完了後保存される
- 中止後保存される
- 放置後保存される

---

## ロード

- ProgressTimeが復元される
- Statusが復元される
- FinishTimeが復元される
- 継続研究が正常に再開される

---

# 受け入れ条件

以下をすべて満たした場合、本Systemは完成とする。

・研究を開始できる

・研究が進行する

・放置中も研究が進行する

・研究速度補正が反映される

・研究を完了できる

・単語を理解できる

・経験値が付与される

・イベント通知が行われる

・SaveDataへ反映される

・ロード後も研究状態が維持される

・クラッシュしない

---

# パフォーマンス要件

- Update()を使用しない
- 毎フレームGCAllocを発生させない
- LINQを使用しない
- 同時研究数が増加しても60FPSを維持する
- 放置24時間分の研究計算が100ms以内で完了する

---

# 実装チェックリスト

## データ

- [ ] ResearchData
- [ ] ResearchRepository
- [ ] Master参照

---

## API

- [ ] StartResearch
- [ ] CancelResearch
- [ ] GetResearch
- [ ] GetAllResearch
- [ ] UpdateResearch
- [ ] CompleteResearch
- [ ] GetRemainingTime
- [ ] GetProgressRate

---

## イベント

- [ ] OnResearchStarted
- [ ] OnResearchProgress
- [ ] OnResearchCompleted
- [ ] OnResearchCanceled

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

ResearchSystemは「未知の言葉を理解する過程」を管理する唯一のシステムである。

研究対象の選択、進行、完了まではResearchSystemが担当するが、単語の所有状態はDictionarySystem、経験値付与はPetSystemが担当する。

他SystemはResearchDataを直接変更してはならない。

状態変更は必ずResearchSystemを経由して行うことで、イベント通知・保存・進行管理の一貫性を保証する。

---