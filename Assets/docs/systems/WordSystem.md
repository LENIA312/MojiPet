# WordSystem

Version: 1.0

---

# 概要

WordSystemは「もじぺっと」に登場するすべての単語データを管理するシステムである。

本Systemは以下を担当する。

- 単語取得
- 単語検索
- 難易度取得
- カテゴリ取得
- 研究時間取得
- 出現判定
- 文字構成取得

WordSystemはゲーム中で最も大きなMasterDataを扱う。

---

# 責務

担当する機能

・WordMaster管理

・カテゴリ管理

・難易度管理

・研究時間管理

・レベル解放判定

・文字構成取得

担当しない機能

・研究進行

・辞書登録

・経験値計算

・ショップ

・Save

---

# 依存関係

依存先

WordMaster

MasterRepository

依存される

ResearchSystem

DictionarySystem

PetSystem

ShopSystem

TutorialSystem

---

# データ

管理対象

WordMaster

```

WordMaster

```
WordId

Word

Reading

Length

Difficulty

Category

RequiredLevel

ResearchTime

Characters
```

---

# WordId

単語ID

一意。

変更禁止。

---

# Word

表示文字列。

例

```
ありがとう
```

---

# Reading

ひらがな読み。

例

```
ありがとう
```

漢字を含む場合も

Readingはひらがなのみ。

---

# Length

文字数。

例

```
ありがとう

=

5
```

---

# Difficulty

難易度。

整数。

最小

1

最大

100

MasterData管理。

---

# Category

カテゴリID。

例

```
植物

動物

食べ物

地名

人名

ことわざ

四字熟語

カタカナ語

ゲーム

歴史

科学
```

カテゴリ追加可能。

---

# RequiredLevel

研究開始に必要な

文字ペットLv。

例

```
あ

Lv15

↓

ありがとう
```

---

# ResearchTime

研究時間。

秒で保持する。

例

```
30

300

1800

7200
```

---

# Characters

単語に含まれる文字一覧。

例

```
ありがとう

↓

あ

り

が

と

う
```

---

# 公開API

## GetWord

```csharp
WordData GetWord(
    int wordId)
```

Word取得。

存在しない場合

例外。

---

## GetWords

```csharp
IReadOnlyList<WordData>
```

全単語取得。

Master順。

---

## GetWordsByCharacter

```csharp
IReadOnlyList<WordData>
```

指定文字を含む単語一覧取得。

例

```
あ

↓

あめ

ありがとう

あしか

...
```

---

## GetWordsByCategory

```csharp
IReadOnlyList<WordData>
```

カテゴリ検索。

---

## GetWordsByDifficulty

```csharp
IReadOnlyList<WordData>
```

難易度検索。

---

## GetResearchTime

```csharp
TimeSpan
```

研究時間取得。

---

## GetDifficulty

```csharp
int
```

難易度取得。

---

## GetRequiredLevel

```csharp
int
```

必要Lv取得。

---

## GetCharacters

```csharp
IReadOnlyList<CharacterId>
```

文字一覧取得。

---

# 出現ルール

Version1では

文字ペットLvによって

研究候補を制限する。

例

```
Lv1

↓

難易度1〜3
```

```
Lv10

↓

難易度1〜10
```

```
Lv30

↓

難易度1〜30
```

難易度上限は

GameBalanceMasterで管理する。

---

# レベル解放

単語は

```
PetLevel

>=

RequiredLevel
```

で研究候補へ追加される。

解放済みでも

理解済みとは限らない。

---

# 単語カテゴリ

Version1では以下カテゴリを実装する。

・食べ物

・飲み物

・植物

・動物

・虫

・魚

・地名

・人物

・職業

・スポーツ

・乗り物

・建物

・色

・自然

・天気

・学校

・家電

・ゲーム

・ことわざ

・四字熟語

・外来語

・その他

カテゴリはMasterDataのみで追加可能。

---

---

# 単語選出

## 概要

WordSystemは研究候補となる単語を抽選する。

研究候補は文字ペット毎に個別に決定される。

同じ単語を複数回研究することはできない。

---

# 選出条件

候補となる単語は以下すべてを満たす。

・理解済みではない

・研究中ではない

・RequiredLevelを満たす

・対象文字を含む

・無効フラグではない

---

# 抽選アルゴリズム

処理順

```
CharacterId取得

↓

文字を含む単語取得

↓

理解済み除外

↓

研究中除外

↓

Lv条件除外

↓

候補生成

↓

Random選択

↓

返却
```

---

# 候補ゼロ

候補数

```
0
```

の場合

研究開始不可。

イベント送信なし。

例外は発生させない。

---

# 重複防止

理解済み単語は候補にならない。

研究中単語も候補にならない。

同一候補を同時に複数保持しない。

---

# ランダム

Version1

均等抽選。

重み付けなし。

---

# 将来拡張

Version2以降

以下追加可能。

・カテゴリ補正

・レア単語

・イベント単語

・季節単語

Version1では実装しない。

---

# 難易度

Difficultyは

研究時間

経験値

解放Lv

の基準として利用する。

---

## Difficulty例

```
1

あめ

いぬ

ねこ
```

```
10

ありがとう

テレビ

じてんしゃ
```

```
30

しんかんせん

とうきょうと
```

```
60

こくさいれんごう

せかいいさん
```

```
100

最終コンテンツ
```

---

# カテゴリ取得

カテゴリはCategoryMasterと一致する。

例

```
植物

↓

ひまわり

さくら

たんぽぽ
```

---

# レアエサ対応

ItemSystemから

カテゴリ倍率取得。

WordSystemは

対象カテゴリ判定のみ行う。

倍率計算は行わない。

---

# イベント

WordSystemは以下イベントを送信する。

---

## OnWordSelected

発火条件

研究候補決定。

通知内容

```
CharacterId

WordId
```

利用先

ResearchView

---

## OnWordUnlocked

発火条件

研究完了。

通知内容

```
WordId
```

利用先

DictionarySystem

PetSystem

---

## OnWordMasterLoaded

発火条件

Master読込。

通知内容

なし。

利用先

DebugMenu

---

# 内部処理

## GetCandidateWords

```
Character取得

↓

Word検索

↓

Lv判定

↓

研究中除外

↓

理解済み除外

↓

候補返却
```

---

## SelectRandomWord

```
候補数取得

↓

Random生成

↓

Word返却
```

---

## IsUnlocked

```
PetLv

>=

RequiredLevel
```

---

## ContainsCharacter

```
Characters内検索

↓

true

false
```

---

# 状態

WordMaster自体は状態を持たない。

Runtime状態は保持しない。

理解状態はDictionarySystemが管理する。

研究状態はResearchSystemが管理する。

---

# Master読込

ゲーム起動時

WordMasterを全件読込。

以降

再読込は行わない。

---

# キャッシュ

Version1では

WordMasterをメモリ保持する。

Dictionary検索を高速化する。

ロード中のみ生成する。

---

# ソート

MasterData順を基本とする。

UI表示時のみ

五十音順などへ並び替えてもよい。

WordSystem内部では並び替えを行わない。

---

---

# エラー処理

WordSystemはMasterDataの整合性を保証する。

異常データを検出した場合は例外を送出し、MasterDataの修正を促す。

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

## CharacterId不存在

発生条件

存在しない文字IDを指定した。

動作

```
ArgumentException
```

を送出する。

---

## Category不存在

発生条件

CategoryMasterに登録されていないカテゴリ。

動作

```
InvalidDataException
```

を送出する。

---

## RequiredLevel異常

発生条件

```
RequiredLevel < 1
```

または

```
RequiredLevel > MaxLevel
```

動作

MasterDataエラー。

ロード失敗。

---

## ResearchTime異常

発生条件

```
ResearchTime <= 0
```

動作

MasterDataエラー。

ロード失敗。

---

## Reading異常

発生条件

Readingが空文字。

動作

MasterDataエラー。

---

## Characters異常

発生条件

Charactersが空。

動作

MasterDataエラー。

---

## 重複WordId

発生条件

同一WordIdが存在する。

動作

ロード失敗。

---

# セーブ

WordSystemはRuntime状態を持たない。

Save対象なし。

---

# ロード

ゲーム起動時

WordMasterを一度だけ読み込む。

ロード後

```
OnWordMasterLoaded
```

を送信する。

---

# 他Systemとの連携

## ResearchSystem

利用API

```
GetResearchTime()

GetRequiredLevel()

GetDifficulty()

GetCandidateWords()

SelectRandomWord()
```

---

## DictionarySystem

利用API

```
GetWord()

GetWords()

GetCharacters()
```

---

## PetSystem

利用API

```
GetRequiredLevel()
```

---

## ItemSystem

利用API

```
GetCategory()
```

カテゴリ一致判定のみ実施する。

---

## TutorialSystem

利用API

```
GetWordsByDifficulty()
```

初心者向け候補取得。

---

# パフォーマンス

WordMasterは起動時にメモリへ展開する。

ゲーム中のCSV再読込は禁止。

毎フレーム検索は禁止。

LINQ禁止。

GCAlloc禁止。

検索速度向上のため以下キャッシュを生成する。

・WordId → WordData

・CharacterId → Word一覧

・Category → Word一覧

・Difficulty帯 → Word一覧

---

# ログ

Development Buildのみ。

---

Master読込

```
[Word]

Load

Count=120000
```

---

候補生成

```
[Word]

Candidate

Character=あ

Count=523
```

---

ランダム選択

```
[Word]

Select

WordId=14562
```

---

異常

```
[Word]

InvalidCategory

WordId=1521
```

---

Release Buildではログ出力しない。

---

# デバッグ機能

DebugMenu限定。

・WordId検索

・Reading検索

・カテゴリ検索

・候補生成確認

・ランダム抽選

・Master再読込

・全カテゴリ件数表示

・重複チェック実行

---

# Repository

WordRepositoryは使用しない。

WordMasterはMasterRepositoryから取得する。

WordSystemはMasterDataを書き換えてはならない。

---

# キャッシュ生成

ロード時に以下Dictionaryを生成する。

```
Dictionary<int, WordData>

Dictionary<CharacterId, List<WordData>>

Dictionary<CategoryId, List<WordData>>
```

生成後は読み取り専用とする。

---

# Version互換

WordMaster更新時

WordIdは変更禁止。

既存SaveDataとの互換性を維持する。

削除する場合は

```
IsEnabled=false
```

を使用する。

物理削除は禁止。

---

# MasterDataルール

WordMasterはCSV管理する。

UTF-8。

BOMなし。

改行コード

LF。

Excel保存は禁止。

CSV生成ツール経由のみ更新する。

---

# Public API 一覧

| API | 概要 |
|------|------|
| GetWord() | WordIdから単語情報を取得する |
| GetWords() | 全単語を取得する |
| GetWordsByCharacter() | 指定文字を含む単語一覧を取得する |
| GetWordsByCategory() | カテゴリ別単語一覧を取得する |
| GetWordsByDifficulty() | 難易度別単語一覧を取得する |
| GetCandidateWords() | 研究候補となる単語一覧を取得する |
| SelectRandomWord() | 候補からランダムに1件選択する |
| GetResearchTime() | 研究時間を取得する |
| GetDifficulty() | 難易度を取得する |
| GetRequiredLevel() | 必要レベルを取得する |
| GetCategory() | カテゴリを取得する |
| GetCharacters() | 構成文字一覧を取得する |
| ContainsCharacter() | 指定文字を含むか判定する |

---

# 内部API

外部公開しない。

```
LoadMaster()

BuildCache()

ValidateMaster()

FindByWordId()

FilterByCharacter()

FilterByCategory()

FilterByDifficulty()

RaiseEvent()
```

---

# シーケンス

## Master読込

```
GameManager

↓

MasterRepository

↓

WordSystem.LoadMaster()

↓

Validate

↓

BuildCache

↓

Event
```

---

## 研究候補取得

```
ResearchSystem

↓

WordSystem

↓

Character検索

↓

Lv判定

↓

Dictionary確認

↓

候補返却
```

---

## ランダム抽選

```
候補取得

↓

Random

↓

Word決定

↓

ResearchSystem
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

WordSystem参照

↓

表示更新
```

---

# データ更新タイミング

| 項目 | 更新タイミング |
|------|----------------|
| WordMaster | 起動時ロード |
| キャッシュ | 起動時生成 |
| CandidateList | 候補取得時 |
| RandomResult | 抽選時 |

WordSystemはRuntimeデータを保持しない。

---

# テストケース

## Master

- CSV読込成功
- WordId重複なし
- Reading空文字なし
- Category正常
- ResearchTime正常
- RequiredLevel正常

---

## 検索

- WordId検索
- Character検索
- Category検索
- Difficulty検索
- Reading検索

---

## 抽選

- 候補0件
- 候補1件
- 候補複数件
- ランダム選択
- 理解済み除外
- 研究中除外

---

## キャッシュ

- 起動時生成
- 再生成不要
- Master件数一致

---

## パフォーマンス

- 100,000件検索
- Character検索速度
- Category検索速度
- GCAllocなし

---

# 受け入れ条件

以下をすべて満たした場合、本Systemは完成とする。

・WordMasterを正常に読み込める

・WordId検索ができる

・Character検索ができる

・Category検索ができる

・Difficulty検索ができる

・研究候補を生成できる

・ランダム抽選できる

・理解済み単語を除外できる

・研究中単語を除外できる

・カテゴリ取得ができる

・構成文字を取得できる

・CSV異常を検出できる

・起動時のみロードされる

・Runtime中にMasterを書き換えない

---

# パフォーマンス要件

- 起動時のみCSV読込
- Update()を使用しない
- LINQを使用しない
- 毎フレームGCAllocを発生させない
- WordId検索 O(1)
- Character検索はキャッシュを利用する
- Category検索はキャッシュを利用する
- Difficulty検索はキャッシュを利用する

---

# 実装チェックリスト

## データ

- [ ] WordMaster
- [ ] CategoryMaster
- [ ] キャッシュ生成

---

## API

- [ ] GetWord
- [ ] GetWords
- [ ] GetWordsByCharacter
- [ ] GetWordsByCategory
- [ ] GetWordsByDifficulty
- [ ] GetCandidateWords
- [ ] SelectRandomWord
- [ ] GetResearchTime
- [ ] GetDifficulty
- [ ] GetRequiredLevel
- [ ] GetCategory
- [ ] GetCharacters
- [ ] ContainsCharacter

---

## イベント

- [ ] OnWordSelected
- [ ] OnWordUnlocked
- [ ] OnWordMasterLoaded

---

## テスト

- [ ] UnitTest
- [ ] IntegrationTest
- [ ] MasterValidationTest
- [ ] PerformanceTest

---

# 備考

WordSystemは「もじぺっと」に存在するすべての単語情報を管理する唯一のシステムである。

WordSystemはMasterDataのみを保持し、ゲーム中に状態を持たない。研究状況はResearchSystem、理解済み状態はDictionarySystem、レベル条件はPetSystemと連携して判定する。

すべての検索処理は起動時に構築したキャッシュを利用し、高速に実行できることを前提とする。

WordMasterはゲーム全体の基盤データであり、実行中に変更・追加・削除を行ってはならない。すべての更新はMasterData更新フローを通して行う。

---