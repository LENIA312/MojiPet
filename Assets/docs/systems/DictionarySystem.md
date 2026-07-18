# DictionarySystem

Version: 1.0

---

# 概要

DictionarySystemは「もじぺっと」においてプレイヤーが理解した単語を管理するシステムである。

本Systemは以下を担当する。

- 理解済み単語管理
- 辞書登録
- 図鑑進捗管理
- 収集率計算
- カテゴリ収集率計算
- 未理解判定
- 理解済み検索

理解済み状態は本Systemのみが保持する。

---

# 責務

担当する機能

・理解済み単語登録

・理解状態取得

・収集率計算

・カテゴリ収集率計算

・辞書一覧生成

・検索

担当しない機能

・研究進行

・単語Master管理

・経験値

・ショップ

・放置

---

# 依存関係

依存先

WordSystem

DictionaryRepository

EventBus

依存される

ResearchSystem

HomeView

DictionaryView

AchievementSystem

TutorialSystem

---

# データ

管理対象

DictionaryData

```

DictionaryData

```
WordId

Unlocked

UnlockedUtc
```

---

# WordId

WordMasterと一致する。

変更禁止。

---

# Unlocked

理解済みか。

```
true

false
```

---

# UnlockedUtc

理解日時。

UTC保存。

未理解時は

```
null
```

---

# 公開API

## UnlockWord

```csharp
void UnlockWord(
    int wordId)
```

単語を理解済みにする。

---

## IsUnlocked

```csharp
bool IsUnlocked(
    int wordId)
```

理解済み判定。

---

## GetDictionary

```csharp
IReadOnlyList<DictionaryData>
```

辞書一覧取得。

---

## GetUnlockedWords

```csharp
IReadOnlyList<WordData>
```

理解済み一覧取得。

---

## GetLockedWords

```csharp
IReadOnlyList<WordData>
```

未理解一覧取得。

---

## GetCompletionRate

```csharp
float
```

図鑑完成率取得。

0〜1で返す。

---

## GetCategoryCompletionRate

```csharp
float
```

カテゴリ収集率取得。

---

## GetUnlockedCount

```csharp
int
```

理解済み件数取得。

---

## GetTotalWordCount

```csharp
int
```

総単語数取得。

---

# 理解

理解とは

ResearchSystemから

```
UnlockWord()
```

が呼ばれた状態を指す。

理解済み単語は再研究できない。

---

# 登録処理

処理

```
WordId取得

↓

未理解確認

↓

Unlocked=true

↓

UTC保存

↓

Save

↓

Event送信
```

---

# 収集率

```
UnlockedCount

/

TotalWordCount
```

0〜1で保持。

UI表示時のみ

％へ変換する。

---

# カテゴリ収集率

例

```
植物

100件

理解40件

↓

40%
```

カテゴリ毎に個別計算する。

---

# 検索

Version1では以下検索に対応。

・WordId

・カテゴリ

・理解状態

・五十音順

全文検索は実装しない。

---

# 並び順

Version1では

WordMaster順を基本とする。

UI側で

五十音順

カテゴリ順

へ変更可能。

DictionarySystemでは並び替えを保持しない。

---

# 初期状態

新規ゲームでは

すべて

```
Unlocked=false
```

から開始する。

---

# 理解フロー

ResearchSystemから研究完了通知を受け取り、

DictionarySystemが理解済み登録を行う。

処理順

```
ResearchComplete

↓

UnlockWord()

↓

Dictionary更新

↓

収集率更新

↓

Save

↓

Event送信
```

---

# 理解済み判定

```
Unlocked

==

true
```

の場合

理解済み。

再登録は行わない。

---

# 未理解判定

```
Unlocked

==

false
```

または

DictionaryDataが存在しない。

---

# 理解日時

初めて理解した日時のみ保存する。

再登録時に

UnlockedUtcを更新してはならない。

---

# 図鑑完成率更新

UnlockWord実行後

完成率を再計算する。

```
CompletionRate

=

UnlockedCount

/

TotalWordCount
```

結果はRuntime保持する。

---

# カテゴリ完成率更新

対象カテゴリのみ再計算する。

例

```
植物

↓

植物カテゴリ件数取得

↓

理解件数取得

↓

収集率更新
```

他カテゴリは更新しない。

---

# イベント

DictionarySystemは以下イベントを送信する。

---

## OnWordUnlocked

発火条件

単語理解。

通知内容

```
WordId
```

利用先

HomeView

DictionaryView

AchievementSystem

---

## OnCompletionUpdated

発火条件

収集率変更。

通知内容

```
CompletionRate
```

利用先

HomeView

CollectionView

---

## OnCategoryCompletionUpdated

発火条件

カテゴリ収集率変更。

通知内容

```
CategoryId

CompletionRate
```

利用先

DictionaryView

---

# Achievement連携

AchievementSystemへ通知する。

通知内容

```
UnlockedCount

CompletionRate
```

Achievement判定は

AchievementSystem側で行う。

---

# HomeView連携

理解通知を受け取り

「○○を覚えた！」

などの演出を表示する。

DictionarySystemはUI生成を行わない。

---

# 内部処理

## UnlockWord

```
Word存在確認

↓

理解済み確認

↓

Dictionary更新

↓

収集率更新

↓

Save

↓

Event
```

---

## UpdateCompletionRate

```
理解件数取得

↓

総件数取得

↓

計算

↓

保存
```

---

## UpdateCategoryCompletion

```
カテゴリ取得

↓

カテゴリ総数取得

↓

理解件数取得

↓

更新
```

---

## GetUnlockedWords

```
Dictionary検索

↓

Unlocked抽出

↓

WordSystem取得

↓

返却
```

---

## GetLockedWords

```
Dictionary検索

↓

Locked抽出

↓

WordSystem取得

↓

返却
```

---

# 状態遷移

```
Locked

↓

Unlocked
```

Version1では

一度理解した単語を

未理解へ戻す処理は存在しない。

---

# 辞書表示

DictionaryViewは

WordSystemから単語情報を取得し、

DictionarySystemから理解状態を取得して表示する。

Word情報をDictionarySystemで保持してはならない。

---

# ランタイムキャッシュ

起動時に

```
HashSet<int>
```

を生成し、

理解済みWordIdを保持する。

IsUnlocked()は

HashSetを参照して

O(1)で判定する。

---

---

# エラー処理

DictionarySystemは理解状態の整合性を保証する。

異常データを検出した場合は例外を送出する。

---

## WordId不存在

発生条件

存在しないWordIdを指定した。

動作

```
ArgumentException
```

を送出する。

---

## 重複登録

発生条件

既に

```
Unlocked=true
```

の単語へ

再度

```
UnlockWord()
```

を実行した。

動作

何もしない。

イベント送信なし。

例外は発生させない。

---

## DictionaryData不存在

発生条件

DictionaryData生成前。

動作

初回生成する。

---

## WordMaster件数不一致

発生条件

Dictionary件数と

WordMaster件数が一致しない。

動作

不足データを生成する。

不足分は

```
Unlocked=false
```

で初期化する。

---

## Category不存在

発生条件

存在しないカテゴリ。

動作

```
ArgumentException
```

---

# セーブ

以下タイミングで保存する。

・単語理解

・新規DictionaryData生成

・オートセーブ

・アプリ終了

---

# ロード

ゲーム起動時

DictionaryRepositoryから取得する。

取得後

WordMasterとの差分を確認する。

不足データは生成する。

---

# 他Systemとの連携

## ResearchSystem

利用

```
UnlockWord()

IsUnlocked()
```

---

## WordSystem

利用

```
GetWord()

GetWords()

GetWordsByCategory()

GetCategory()
```

---

## AchievementSystem

通知

```
UnlockedCount

CompletionRate
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
OnWordUnlocked

OnCompletionUpdated
```

---

# パフォーマンス

理解済み判定は

```
HashSet<int>
```

で管理する。

WordId検索は

O(1)。

LINQ禁止。

毎フレームGCAlloc禁止。

収集率計算は

理解時のみ実行する。

毎フレーム再計算は禁止。

---

# ログ

Development Buildのみ。

---

理解

```
[Dictionary]

Unlock

WordId=1245
```

---

完成率

```
[Dictionary]

Completion

42.6%
```

---

カテゴリ

```
[Dictionary]

Category

植物

65%
```

---

異常

```
[Dictionary]

DuplicateUnlock

WordId=1245
```

---

Release Buildではログ出力しない。

---

# デバッグ機能

DebugMenu限定。

・単語理解

・全理解

・全未理解

・カテゴリ理解

・完成率再計算

・HashSet再生成

・Dictionary再生成

・件数表示

---

# Repository

DictionaryRepositoryが

DictionaryDataのみ保持する。

Word情報は保持しない。

カテゴリ情報は保持しない。

---

# キャッシュ

起動時に生成する。

```
HashSet<int>

UnlockedWordIds
```

必要に応じて

カテゴリ件数キャッシュも生成する。

```
Dictionary<CategoryId, int>
```

---

# Version互換

WordMaster更新時

新規Wordが追加された場合

DictionaryDataを自動生成する。

既存理解状態は維持する。

削除Wordは

```
IsEnabled=false
```

扱いとする。

---

# 初回起動

新規ゲーム時

WordMaster全件について

DictionaryDataを生成する。

すべて

```
Unlocked=false
```

UTCは

```
null
```

とする。

---

# Public API 一覧

| API | 概要 |
|------|------|
| UnlockWord() | 単語を理解済みにする |
| IsUnlocked() | 理解済みか判定する |
| GetDictionary() | 辞書データ一覧を取得する |
| GetUnlockedWords() | 理解済み単語一覧を取得する |
| GetLockedWords() | 未理解単語一覧を取得する |
| GetCompletionRate() | 図鑑完成率を取得する |
| GetCategoryCompletionRate() | カテゴリ収集率を取得する |
| GetUnlockedCount() | 理解済み件数を取得する |
| GetTotalWordCount() | 総単語数を取得する |

---

# 内部API

外部公開しない。

```
CreateDictionary()

ValidateDictionary()

BuildUnlockedCache()

UpdateCompletionRate()

UpdateCategoryCompletion()

RaiseEvent()

Save()
```

---

# シーケンス

## 研究完了

```
ResearchSystem

↓

DictionarySystem.UnlockWord()

↓

Dictionary更新

↓

収集率更新

↓

Save

↓

Event

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

DictionaryRepository

↓

DictionarySystem

↓

不足データ生成

↓

HashSet生成

↓

起動完了
```

---

## 図鑑表示

```
DictionaryView

↓

DictionarySystem

↓

WordSystem

↓

理解状態取得

↓

一覧生成

↓

表示
```

---

# データ更新タイミング

| 項目 | 更新タイミング |
|------|----------------|
| Unlocked | 単語理解時 |
| UnlockedUtc | 初回理解時 |
| CompletionRate | 単語理解時 |
| CategoryCompletion | 単語理解時 |
| HashSet | 起動時・再生成時 |

---

# テストケース

## 理解

- 未理解単語を理解できる
- 理解済み単語は再登録されない
- UnlockedUtcが保存される

---

## 完成率

- 0%
- 50%
- 100%
- 小数点計算
- Word追加時

---

## カテゴリ

- 植物
- 動物
- 食べ物
- 地名
- その他

各カテゴリで収集率が正しく計算される。

---

## 起動

- 初回起動
- 通常ロード
- Master追加
- Master削除
- 差分生成

---

## 検索

- WordId検索
- 理解済み検索
- 未理解検索
- カテゴリ検索

---

## キャッシュ

- HashSet生成
- HashSet再生成
- O(1)判定

---

## Save

- 理解時保存
- AutoSave
- アプリ終了

---

# 受け入れ条件

以下をすべて満たした場合、本Systemは完成とする。

・単語を理解できる

・理解済み判定ができる

・未理解判定ができる

・図鑑完成率を取得できる

・カテゴリ完成率を取得できる

・理解済み一覧を取得できる

・未理解一覧を取得できる

・SaveDataへ保存される

・ロード後も理解状態が維持される

・WordMaster更新に追従できる

・HashSetによる高速検索が動作する

・イベント通知が行われる

---

# パフォーマンス要件

- Update()を使用しない
- LINQを使用しない
- 毎フレームGCAllocを発生させない
- IsUnlocked() は O(1)
- 起動時のみHashSetを生成する
- CompletionRateは理解時のみ再計算する

---

# 実装チェックリスト

## データ

- [ ] DictionaryData
- [ ] DictionaryRepository
- [ ] HashSet生成
- [ ] CategoryCache生成

---

## API

- [ ] UnlockWord
- [ ] IsUnlocked
- [ ] GetDictionary
- [ ] GetUnlockedWords
- [ ] GetLockedWords
- [ ] GetCompletionRate
- [ ] GetCategoryCompletionRate
- [ ] GetUnlockedCount
- [ ] GetTotalWordCount

---

## イベント

- [ ] OnWordUnlocked
- [ ] OnCompletionUpdated
- [ ] OnCategoryCompletionUpdated

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

DictionarySystemは「もじぺっと」におけるプレイヤーの知識の進捗を管理する唯一のシステムである。

理解済み単語の登録・検索・完成率計算はすべてDictionarySystemが担当する。WordMaster自体はWordSystemが管理し、研究の進行はResearchSystemが管理するため、本Systemはそれらの中間に位置する「理解状態」の管理に専念する。

理解済み判定はHashSetによる高速検索を前提とし、実行中にWordMasterを書き換えてはならない。

新しい単語がWordMasterへ追加された場合は、既存セーブデータを維持したまま不足分のDictionaryDataを自動生成することで、バージョンアップ時の互換性を保証する。

---