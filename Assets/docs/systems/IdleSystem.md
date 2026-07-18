# IdleSystem

Version: 1.0

---

# 概要

IdleSystemは「もじぺっと」における放置時間の計算を管理するシステムである。

本システムは以下を担当する。

- オフライン時間計算
- 放置報酬計算
- 言霊生産
- 研究進行
- 満腹度減少
- 放置時間制限
- ログイン時処理

ゲーム終了中の進行はすべて本Systemを経由して処理する。

---

# 責務

担当する機能

- 放置時間取得
- 放置上限適用
- 放置報酬計算
- PetSystem更新
- ResearchSystem更新
- 最終ログイン時刻更新

担当しない機能

- セーブデータ保持
- UI表示
- 単語判定
- アイテム管理
- レベルアップ

---

# 依存関係

依存先

SaveSystem

PetSystem

ResearchSystem

FacilitySystem

EventBus

依存される

GameManager

HomeView

SplashScene

---

# データ

管理対象

IdleData

```

IdleData

```
LastLoginUtc

CurrentLoginUtc

ElapsedTime

RewardMoney

```

---

# LastLoginUtc

前回終了時刻。

UTCで保存する。

---

# CurrentLoginUtc

今回起動時刻。

UTC取得。

---

# ElapsedTime

放置時間。

CurrentLoginUtc

-

LastLoginUtc

で算出する。

---

# RewardMoney

今回獲得する言霊。

表示専用。

---

# 公開API

## CalculateOfflineProgress

```csharp
void CalculateOfflineProgress()
```

起動時に実行する。

処理

現在時刻取得

↓

経過時間算出

↓

放置上限適用

↓

各System更新

↓

保存

↓

イベント送信

---

## GetOfflineTime

```csharp
TimeSpan GetOfflineTime()
```

放置時間取得。

---

## ApplyOfflineReward

```csharp
void ApplyOfflineReward()
```

放置報酬を適用する。

処理

言霊加算

↓

研究進行

↓

満腹度更新

↓

保存

---

## SaveLoginTime

```csharp
void SaveLoginTime()
```

現在UTCを保存する。

アプリ終了時に実行する。

---

## GetRewardMoney

```csharp
long GetRewardMoney()
```

今回獲得した言霊を返す。

---

# 放置時間計算

```
ElapsedTime

=

CurrentLoginUtc

-

LastLoginUtc
```

UTC以外は禁止。

LocalTimeは禁止。

---

# 放置上限

GameBalanceMasterで管理する。

初期値

```
24時間
```

例

```
放置

30時間

↓

計算

24時間
```

---

# 起動処理

ゲーム起動

↓

Save読込

↓

CalculateOfflineProgress()

↓

Home表示

---

# 終了処理

アプリ終了

↓

SaveLoginTime()

↓

Save

↓

終了

---

# 放置対象

Version1では以下のみ対象。

・言霊生産

・研究進行

・満腹度減少

以下は対象外。

・ショップ更新

・イベント更新

・ログインボーナス

・期間限定イベント

---

# 言霊生産

PetSystemから全取得済み文字ペットを取得する。

各ペットについて

```
ProductionRate

×

ElapsedTime
```

を計算する。

全ペットの結果を合算し

RewardMoneyへ設定する。

---

# 満腹度更新

PetSystemへ

```
UpdateHunger(
ElapsedTime)
```

を通知する。

IdleSystemは満腹度を保持しない。

---

# 研究進行

ResearchSystemへ経過時間を通知する。

```
UpdateResearch(
    ElapsedTime)
```

ResearchSystemは受け取った時間を使用して研究進行を行う。

IdleSystemは研究内容を保持しない。

---

# 放置報酬

## 概要

放置時間中に生産された言霊を計算する。

放置報酬はゲーム起動時に一括で付与する。

---

## 計算式

```
RewardMoney

=

Σ(
ProductionRate
×

ElapsedTime
)
```

ProductionRateはPetSystemから取得する。

---

## 小数処理

小数点以下は切り捨てる。

例

```
120.95

↓

120
```

---

## 負数

RewardMoneyは負数にならない。

異常値が発生した場合

```
0
```

へ補正する。

---

# 放置報酬適用

処理順

```
Reward計算

↓

Currency追加

↓

Research更新

↓

満腹度更新

↓

Save

↓

Event
```

---

# イベント

IdleSystemは以下イベントを送信する。

---

## OnOfflineCalculated

発火条件

放置時間計算完了。

通知内容

```
ElapsedTime

RewardMoney
```

利用先

・HomeView

・RewardPopup

---

## OnOfflineRewardApplied

発火条件

放置報酬付与完了。

通知内容

```
RewardMoney
```

利用先

・CurrencyView

---

## OnOfflineSkipped

発火条件

放置時間

0秒。

通知内容

なし。

利用先

HomeView

---

# 内部処理

## CalculateOfflineProgress

```
現在UTC取得

↓

前回UTC取得

↓

ElapsedTime計算

↓

上限適用

↓

Pet更新

↓

Research更新

↓

Reward計算

↓

Save

↓

Event
```

---

## ApplyOfflineReward

```
Currency追加

↓

Pet更新

↓

Research更新

↓

Save

↓

Event
```

---

## SaveLoginTime

```
現在UTC取得

↓

SaveData更新

↓

保存
```

---

# 状態遷移

```
未起動

↓

起動

↓

放置時間計算

↓

放置報酬適用

↓

通常プレイ

↓

終了

↓

未起動
```

---

# 時刻取得

Version1では

```
DateTime.UtcNow
```

または

```
TimeUtility.CurrentUtc
```

のみ使用する。

---

# 放置時間0秒

ElapsedTimeが

```
0
```

以下の場合

放置計算は行わない。

```
OnOfflineSkipped
```

のみ送信する。

---

# 放置時間異常

ElapsedTimeが負数の場合

```
0
```

として扱う。

例外は発生させない。

ログのみ出力する。

---

# RewardPopup

IdleSystemはPopupを生成しない。

HomeViewが

```
OnOfflineRewardApplied
```

を受信して表示する。

---

# FacilitySystem連携

Facility倍率はPetSystem経由で適用される。

IdleSystemは倍率を保持しない。

---

# Currency連携

RewardMoneyはCurrencySystemへ通知する。

IdleSystemは通貨総数を保持しない。

---

# Save連携

放置計算終了後

必ずSaveSystemへ保存要求を送信する。

途中で異常終了した場合でも

次回起動時に二重取得しないよう

LastLoginUtcは保存完了後に更新する。

---

# エラー処理

IdleSystemは放置処理中の異常を検知し、安全に処理を終了する。

放置処理の失敗によってゲームを継続不能にしてはならない。

---

## SaveData不存在

発生条件

SaveDataが存在しない。

動作

初回起動として扱う。

```
LastLoginUtc
=
CurrentLoginUtc
```

Rewardは0。

---

## LastLoginUtc未設定

初回起動と同じ扱い。

Rewardは発生しない。

---

## CurrentLoginUtc取得失敗

発生条件

現在時刻取得失敗。

動作

放置計算を行わない。

ログ出力のみ。

---

## ElapsedTime負数

発生条件

端末時刻変更など。

動作

```
ElapsedTime = 0
```

へ補正。

Reward発生なし。

---

## 放置上限超過

ElapsedTimeが上限を超えた場合

上限時間へ補正する。

例外は発生させない。

---

## PetSystem更新失敗

発生条件

PetSystem例外。

動作

放置処理中断。

Rewardは適用しない。

ログ出力。

---

## ResearchSystem更新失敗

動作

研究進行のみ中止。

言霊報酬は破棄しない。

ログ出力。

---

# セーブ

以下で保存する。

・放置計算終了

・放置報酬付与後

・終了時

・バックグラウンド遷移

---

# ロード

ゲーム起動時

SaveSystemから

```
LastLoginUtc
```

取得。

その後

CalculateOfflineProgress()

を実行する。

---

# 他Systemとの連携

## SaveSystem

利用

```
Load()

Save()

```

---

## PetSystem

利用

```
UpdateHunger()

GetProductionRate()

GetAllPets()
```

---

## ResearchSystem

利用

```
UpdateResearch()
```

---

## CurrencySystem

利用

```
AddMoney()
```

---

## FacilitySystem

直接利用しない。

倍率はPetSystemが適用する。

---

# パフォーマンス

放置計算はゲーム起動時のみ実行する。

Update()は禁止。

毎フレーム計算は禁止。

LINQ禁止。

foreachによるGCAlloc禁止。

取得済みペット数46体を想定し

放置24時間分の計算が100ms以内で終了すること。

---

# ログ

Development Buildのみ。

---

放置開始

```
[Idle]

Calculate

Elapsed=03:21:15
```

---

放置報酬

```
[Idle]

Reward

Money=12540
```

---

放置終了

```
[Idle]

Complete
```

---

異常

```
[Idle]

NegativeTime

Elapsed=-05:00
```

---

Release Buildではログ出力しない。

---

# デバッグ機能

DebugMenu限定。

・放置5分

・放置30分

・放置1時間

・放置6時間

・放置24時間

・放置48時間（上限確認用）

・Rewardリセット

・LastLoginUtc変更

---

# 内部Repository

IdleSystemはRepositoryを持たない。

永続化はSaveSystemへ委譲する。

---

# 時間管理

Version1ではUTCのみ使用。

LocalTime禁止。

端末時刻変更による異常値は補正する。

---

# 再入禁止

CalculateOfflineProgress()

実行中は再実行してはならない。

多重実行要求は無視する。

---

# Version互換

SaveVersion更新時

LastLoginUtcが存在しない場合は

CurrentUtcを代入する。

クラッシュしてはならない。

---

# Public API 一覧

| API | 概要 |
|------|------|
| CalculateOfflineProgress() | 放置時間を計算し、各システムへ反映する |
| GetOfflineTime() | 放置時間を取得する |
| ApplyOfflineReward() | 放置報酬を付与する |
| SaveLoginTime() | 現在時刻を保存する |
| GetRewardMoney() | 今回獲得した言霊を取得する |
| HasOfflineReward() | 放置報酬が存在するか判定する |

---

# 内部API

外部公開しない。

```
CalculateElapsedTime()

ClampOfflineTime()

CalculateReward()

ApplyPetProgress()

ApplyResearchProgress()

RaiseEvent()

Save()
```

---

# シーケンス

## アプリ起動

```
Application

↓

GameManager

↓

SaveSystem.Load()

↓

IdleSystem.CalculateOfflineProgress()

↓

PetSystem.UpdateHunger()

↓

ResearchSystem.UpdateResearch()

↓

CurrencySystem.AddMoney()

↓

HomeScene
```

---

## アプリ終了

```
Application

↓

IdleSystem.SaveLoginTime()

↓

SaveSystem.Save()

↓

Application終了
```

---

## 放置報酬付与

```
IdleSystem

↓

Reward計算

↓

CurrencySystem

↓

Save

↓

Event

↓

Popup表示
```

---

# データ更新タイミング

| 項目 | 更新タイミング |
|------|----------------|
| LastLoginUtc | アプリ終了時 |
| CurrentLoginUtc | アプリ起動時 |
| ElapsedTime | 起動時 |
| RewardMoney | 放置計算完了時 |

---

# テストケース

## 初回起動

- SaveDataが存在しない
- Rewardが0
- クラッシュしない

---

## 通常放置

- 5分
- 30分
- 1時間
- 6時間
- 24時間

それぞれ正常に報酬が計算される。

---

## 上限

- 24時間を超えない
- 48時間放置しても24時間として処理される

---

## 時刻異常

- LastLoginUtc > CurrentLoginUtc
- Rewardが0
- クラッシュしない

---

## PetSystem連携

- 満腹度が減少する
- 生産量が反映される

---

## ResearchSystem連携

- Progressが進む
- 完了する
- 経験値が付与される

---

## Save

- LastLoginUtc保存
- Reward二重取得なし

---

## Load

- LastLoginUtc復元
- 初回起動判定正常

---

# 受け入れ条件

以下をすべて満たした場合、本Systemは完成とする。

・放置時間を計算できる

・放置時間上限が適用される

・言霊を獲得できる

・研究が進行する

・満腹度が減少する

・放置報酬が一度だけ付与される

・SaveDataへ反映される

・ロード後も正常に動作する

・端末時間変更でクラッシュしない

・イベント通知が行われる

---

# パフォーマンス要件

- Update()を使用しない
- 毎フレームGCAllocを発生させない
- LINQを使用しない
- 取得済み46文字で放置24時間計算が100ms以内
- 起動時のみ放置計算を実行する

---

# 実装チェックリスト

## データ

- [ ] IdleData
- [ ] SaveData参照
- [ ] UTC管理

---

## API

- [ ] CalculateOfflineProgress
- [ ] GetOfflineTime
- [ ] ApplyOfflineReward
- [ ] SaveLoginTime
- [ ] GetRewardMoney
- [ ] HasOfflineReward

---

## イベント

- [ ] OnOfflineCalculated
- [ ] OnOfflineRewardApplied
- [ ] OnOfflineSkipped

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
- [ ] TimeManipulationTest

---

# 備考

IdleSystemは「もじぺっと」の放置進行を管理する唯一のシステムである。

ゲーム終了中に発生した進行はすべてIdleSystemが計算し、PetSystem・ResearchSystem・CurrencySystemへ反映する。

IdleSystemは各システムの内部状態を直接変更してはならない。

状態変更は必ず公開APIを経由して行うことで、放置進行・イベント通知・セーブデータの整合性を保証する。

Version 1では放置対象を「言霊生産」「研究進行」「満腹度減少」の3要素に限定し、その他のゲーム要素は放置計算の対象外とする。

---