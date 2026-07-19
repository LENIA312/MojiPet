# もじぺっと 実装仕様書(as-built)

**最終更新: 2026-07-19**

本ドキュメントは「今のコードが実際に何をするか」を記述したas-built仕様書である。`01_ProjectOverview.md`等の元設計書とは役割が異なる。

- `docs/01〜07_*.md`、`docs/systems/*.md` = **原設計書**(実装前に書かれた意図)
- `docs/00_ImplementationProgress.md` = **進捗ログ**(何をいつ・なぜ実装/変更したかの時系列記録)
- **本ドキュメント(`IMPLEMENTED_SPEC.md`) = 現在のコードの動作を機能網羅的にまとめたリファレンス**。原設計書と食い違う箇所は実装側の挙動を正としてここに記載し、差異は明記する。

対象バージョン: MVP実装完了時点(Phase0〜8 + 追加4機能)。Phase9(バランス調整)は未着手。

---

# 1. 概要

もじぺっとは、46種類のひらがな文字を「もじぺっと」として集め、日本語の単語を教えて育てる放置系収集ゲームである。プレイヤーは文字を入手し、単語を研究させ、レベルを上げ、言霊(通貨)を稼ぎ、施設を強化し、ショップでアイテムを買う。

**プラットフォーム**: Unity 6000.4.9f1(URP)、実機ターゲットはiOS/Android想定(現状Windows Editorでの動作確認のみ)。

**主要ライブラリ**: UniTask(埋め込みパッケージ)、Addressables、TextMeshPro、Newtonsoft.Json、Unity Input System。

**単語データソース**: [JMdict](https://www.edrdg.org/jmdict/j_jmdict.html)(EDRDG、CC BY-SA 4.0)。詳細は3.1節。

---

# 2. アーキテクチャ

## 2.1 レイヤーと依存方向

```
UI (Views / Presenters)
    ↓
Systems (ゲームロジック)
    ↓
Managers (MasterManager) / Save (SaveSystem)
    ↓
Models (データ) / Master (ScriptableObject定義)
```

- **Systems**はコンストラクタインジェクションで依存を受け取る(Service Locator/FindObjectOfTypeは不使用)。
- **UI(View)はロジックを持たない**。Presenterが各Systemを呼び出してデータを整形し、Viewはそれを表示・入力を仲介するだけ。
- 唯一のグローバル参照点は`GameManager.Instance`(DontDestroyOnLoad)。View層はここから必要なSystemを取得する。
- System間の通知は`EventBus`経由(Publish/Subscribe)。同期実行。

## 2.2 起動シーケンス(`GameManager.BootstrapAsync`, Boot→Homeシーン)

```
1. Addressables.InitializeAsync()
2. EventBus 生成
3. MasterManager 生成 → InitializeAsync()(全MasterをAddressables経由でロード)
4. SaveSystem 生成 → Load()(セーブ有:読込/Migration/Validation、セーブ無:NewGame())
5. CurrencySystem 生成
6. FacilitySystem 生成(SaveSystem, MasterManager, CurrencySystem依存)
7. PetSystem 生成(SaveSystem, MasterManager, FacilitySystem依存)
8. ItemSystem 生成(SaveSystem, PetSystem, MasterManager依存)
9. ShopSystem 生成(MasterManager, CurrencySystem, ItemSystem依存)
10. GrantInitialItemsIfNewGame()(新規ゲームなら「ことばのたね」をInitialSeedCount個付与)
11. WordSystem 生成(MasterManagerのみ依存。EventBus不使用)
12. DictionarySystem 生成(SaveSystem, WordSystem依存)
13. ResearchSystem 生成(SaveSystem, MasterManager, WordSystem, PetSystem, DictionarySystem依存)
14. IdleSystem 生成(SaveSystem, PetSystem, ResearchSystem, CurrencySystem, MasterManager依存)
15. IdleSystem.CalculateOfflineProgress() → ApplyOfflineReward()(放置時間計算・報酬付与)
16. GameTicker 生成・Start()(5秒間隔の進行ループ開始)
17. Homeシーンをロード(SceneManager.LoadSceneAsync)
```

Homeシーンロード後、`HomeUIRoot.Start()`がCanvas/UIを動的生成し、`GameManager.Instance`経由で各Systemを参照する。

## 2.3 アプリのポーズ・終了処理

- `OnApplicationQuit`: GameTicker停止 → `IdleSystem.SaveLoginTime()`(現在UTCを保存)
- `OnApplicationPause(true)`(バックグラウンド遷移): GameTicker停止 → `SaveLoginTime()`
- `OnApplicationPause(false)`(復帰): `IdleSystem.CalculateOfflineProgress()` → `ApplyOfflineReward()` → GameTicker再開

## 2.4 フォルダ構成(実際)

```
Assets/
├── Scripts/
│   ├── Runtime/
│   │   ├── Core/        GameManager, GameTicker
│   │   ├── Managers/     MasterManager
│   │   ├── Systems/      Currency/Pet/Item/Shop/Word/Dictionary/Research/Idle/Facility/Save 各System
│   │   ├── Models/       SaveData配下の全データクラス・enum
│   │   ├── Master/       ScriptableObject定義(WordMasterSO等)
│   │   ├── Save/         SaveRepository, SaveDataSerializer
│   │   ├── Events/       EventBus, 各種イベント構造体
│   │   ├── UI/
│   │   │   ├── Components/  UiFactory
│   │   │   ├── Presenters/  各種Presenter
│   │   │   ├── Views/       各種View
│   │   │   └── HomeUIRoot.cs
│   │   └── Utilities/    TimeUtility
│   ├── Editor/
│   │   └── MasterData/   CsvReader, MasterDataImporter
│   └── Tests/            (EditMode/PlayMode、asmdefのみ・テスト未実装)
├── MasterData/            *.csv (9ファイル、Import元)
├── AddressableAssetsData/ Addressables設定
├── AddressableAssets/Master/  Import後のScriptableObjectアセット(9個)
├── Scenes/                Boot.unity, Home.unity
└── docs/                  設計書・進捗ログ・本ドキュメント
```

---

# 3. MasterData(全9種)

CSVは`Assets/MasterData/*.csv`。`Tools > Import MasterData`メニューで`Assets/AddressableAssets/Master/*.asset`に変換される(`MasterDataImporter.cs`)。ゲームコードはScriptableObjectのみを参照し、CSVを直接読まない。

## 3.1 WordMaster(拡張スキーマ)

| 列 | 型 | 説明 |
|---|---|---|
| WordId | int | 一意ID |
| Word | string | 表示文字列 |
| Reading | string | ひらがな読み |
| Difficulty | int | 難易度(数値が大きいほど難しい) |
| Category | enum(CategoryId) | カテゴリ |
| RequiredLevel | int | 研究に必要な文字のレベル |
| ResearchTimeSeconds | int | 基礎研究時間(秒)。**この値がそのまま使われ、ResearchMasterは参照しない** |

`Length`(文字数)と`Characters`(構成文字の配列)はCSVには無く、インポート時に`Reading`から自動生成される。

現在19,866語収録(WordId 1〜19866、2026-07-19に本番データへ差し替え)。46文字全てがいずれかの単語でカバーされている(「を」を含む複合語も収録済み)。

**データソース**: [JMdict](https://www.edrdg.org/jmdict/j_jmdict.html)(EDRDG、CC BY-SA 4.0)の`jmdict-simplified`(scriptin/jmdict-simplified)配布の`jmdict-eng-common`版(common語のみ、22,617エントリ)から自動生成。生成手順:

1. 各エントリの代表読み仮名(`common`優先)を採用し、カタカナはUnicodeコードポイントシフト(U+30A1〜U+30F6 → U+3041〜U+3096)でひらがなに変換
2. ひらがな(+長音記号「ー」)のみ・2〜20文字の読みだけを許可、読みの重複は除去
3. `misc`タグに`arch`(古語)/`obs`(廃語)/`rare`(まれ)/`vulg`(下品)/`derog`(蔑称)/`sens`(センシティブ)/`X`(教育用ソフトウェアには不適切、とJMDict自身が明記)のいずれかを含む語義は除外。全ての語義が除外対象の単語は収録しない
4. `field`タグ(`bot`→PLANT、`zool`/`ornith`/`fish`/`vet`→ANIMAL、`food`→FOOD、`geogr`/`geol`/`place`/`astron`→PLACE)と品詞タグ(動詞活用タグ→VERB、`adj-*`→ADJECTIVE、`n`系→OBJECT、それ以外→OTHER)で`Category`を自動分類。JMDictには人物を指す一般名詞向けの分類タグが無いため、家族・職業・人称代名詞など約60語は読みのキュレーションリストで`PERSON`に上書き
5. `Difficulty = max(1, 読み文字数-1)`、`RequiredLevel = clamp(1, 100, (Difficulty-1)×2+1)`、`ResearchTimeSeconds = 30 × Difficulty²`
6. 表示`Word`は`common`な漢字表記を優先、無ければ最初の漢字表記、無ければ仮名表記(原表記のまま、カタカナ語はカタカナ表示)

カテゴリ内訳: OBJECT 10,954 / VERB 5,004 / ADJECTIVE 2,794 / OTHER 851 / FOOD 134 / PERSON 63 / PLACE 45 / ANIMAL 15 / PLANT 6。最大`RequiredLevel`は33、最大`ResearchTimeSeconds`は8,670秒(約2.4時間)で、いずれも`MaxPetLevel`(100)以内に収まる。

**ライセンス表示について**: JMDictはCC BY-SA 4.0のためクレジット表記が必要だが、**ゲーム内には現状クレジット/ライセンス表示画面が存在しない**。別途対応が必要(未実装)。

## 3.2 PetMaster

| 列 | 型 | 説明 |
|---|---|---|
| CharacterId | int | 1〜46の一意ID |
| Character | string | 実際のひらがな1文字 |
| DisplayName | string | 表示名(現状Characterと同一) |
| InitialLevel | int | 解放時の初期レベル(全て1) |
| BaseProduction | int | 基礎言霊生産量(全て1) |
| BaseResearchSpeed | float | 基礎研究速度倍率(全て1.0) |

46行(あ〜ん、五十音順、CharacterId=1〜46)。全キャラ同一パラメータ(差別化なし)。

## 3.3 ExpMaster

| 列 | 型 | 説明 |
|---|---|---|
| Level | int | レベル |
| RequiredExp | int | そのレベルに到達するための累積必要経験値 |
| ProductionMultiplier | float | そのレベルでの言霊生産倍率 |
| ResearchSpeedMultiplier | float | そのレベルでの研究速度倍率 |

Level 1〜100。`RequiredExp = 10 × Level × (Level-1)`(例: Lv1=0, Lv2=20, Lv3=60, Lv4=120)。`ProductionMultiplier = 1 + 0.122×(Level-1)`、`ResearchSpeedMultiplier = 1 + 0.03×(Level-1)`(概算、Lv50でそれぞれ約6.98倍・約2.47倍)。

## 3.4 ItemMaster

| 列 | 型 | 説明 |
|---|---|---|
| Id | int | 一意ID |
| Name | string | 表示名 |
| Description | string | 説明文 |
| ItemType | enum(ItemType: Food/ResearchBoost/Seed/Special) | 種別 |
| Price | int | 参考価格(実売価格はShopMasterが持つ) |
| Value | float | 効果量(Foodは満腹度回復量、ResearchBoostは速度倍率、Seedは未使用) |
| DurationSeconds | int | 効果継続時間(ResearchBoostのみ使用) |

現在3件:

| Id | Name | ItemType | Price | Value | DurationSeconds |
|---|---|---|---|---|---|
| 1 | ことばの実 | Food | 100 | 30(満腹度+30) | 0 |
| 2 | ひらめきのしずく | ResearchBoost | 500 | 1.5(研究速度×1.5) | 600(10分) |
| 3 | ことばのたね | Seed | 5000 | 0 | 0 |

`ItemType.Special`はマスタ定義はあるが使用箇所なし(未実装)。

## 3.5 FacilityMaster

| 列 | 型 | 説明 |
|---|---|---|
| Id | int | 一意ID |
| FacilityType | enum(FacilityId: ResearchLab/Library/Garden) | 施設種別 |
| Level | int | レベル |
| UpgradeCost | long | このレベルへの強化費用 |
| EffectValue | float | このレベルでの効果倍率 |

3施設 × Lv1〜20 = 60行。`UpgradeCost ≈ 100 × Level^1.6`、`EffectValue = 1.0 + 0.02×Level`(Lv1で1.02倍、Lv20で1.40倍)。**Lv21以上のデータは存在しないため、Lv20が現状の実質最大レベル**(`CanUpgrade`/`GetUpgradeCost`がLv21のマスタ行を見つけられず強化不可判定になる)。

3施設の効果適用先:

- **ResearchLab**: `PetSystem.GetResearchSpeed()`の倍率に乗算
- **Garden**: `PetSystem.GetProductionRate()`の倍率に乗算
- **Library**: `CategoryMaster`の`RequiredLibraryLevel`と比較され、研究候補のカテゴリ解放判定に使用(3.9節参照)

## 3.6 ShopMaster

| 列 | 型 | 説明 |
|---|---|---|
| Id | int | ショップ枠の一意ID |
| ItemId | int | 対応するItemMaster.Id |
| Price | int | 実売価格(ここが実際に使われる) |
| UnlockCondition | string | 未使用(常に空文字列) |

現在3件、ItemMasterの3アイテムに1:1対応(Id=ItemId)。消耗品ショップなので購入履歴は保持せず、所持金があれば何度でも購入可能。

## 3.7 ResearchMaster

| 列 | 型 | 説明 |
|---|---|---|
| Difficulty | int | 難易度 |
| RequiredSeconds | int | 基礎研究時間(秒) |

Difficulty 1〜100、`RequiredSeconds = 30 × Difficulty²`。**現在ResearchSystemはこのマスタを参照していない**(WordMasterの`ResearchTimeSeconds`を直接使用するため)。インポート・データ自体は存在するが実質未使用。

## 3.8 GameBalanceMaster

単一レコード(Field, Valueのキーバリュー形式CSV)。

| Field | 値 | 用途 |
|---|---|---|
| MaxOfflineHours | 8 | 放置報酬の時間上限 |
| MaxPetLevel | 100 | 文字の最大レベル |
| MaxFood | 100 | 満腹度の最大値 |
| InitialSeedCount | 1 | 新規ゲーム時に付与する「ことばのたね」個数 |
| DefaultResearchSlots | 1 | 未使用(同時研究数は1体1件で固定実装、可変にしていない) |
| HungerDecayPerHour | 5 | 満腹度の時間あたり自然減少量 |
| HungerLowThreshold | 40 | この値未満で「空腹」倍率が適用される閾値 |
| HungerLowMultiplier | 0.8 | 空腹時の生産・研究速度倍率 |
| HungerStarvingMultiplier | 0.5 | 満腹度0時の生産・研究速度倍率 |
| BaseExp | 20 | 経験値計算の基礎値 |
| LengthBonusMultiplier2/3/4/5Plus | 1.0/1.1/1.2/1.3 | 単語文字数による経験値ボーナス倍率 |

## 3.9 CategoryMaster(2026-07-19追加)

| 列 | 型 | 説明 |
|---|---|---|
| Category | enum(CategoryId) | カテゴリ |
| RequiredLibraryLevel | int | このカテゴリの単語が研究候補に出るために必要な図書館レベル |

| Category | RequiredLibraryLevel |
|---|---|
| OTHER | 0(常に解放) |
| PLANT / ANIMAL / FOOD / PLACE / OBJECT | 1(初期図書館レベルで解放済み) |
| VERB | 2 |
| ADJECTIVE | 3 |
| PERSON | 4 |

2026-07-19の辞書差し替え以降、VERB(5,004語)/ADJECTIVE(2,794語)/PERSON(63語)にも実データが入ったため、図書館レベルによるカテゴリ解放が実際にゲームプレイへ影響するようになった(図書館Lv1の初期状態ではVERB/ADJECTIVE/PERSONの単語は研究候補に出ない)。

マスタに存在しないカテゴリは「常に解放」扱い(フォールバック)。

---

# 4. SaveData(セーブデータ)

保存先: `Application.persistentDataPath/save.json`(Windows実機では`%USERPROFILE%\AppData\LocalLow\DefaultCompany\mojipet\save.json`)。UTF-8・BOM無し・Newtonsoft.Jsonで`Formatting.None`(整形なし1行)。

## 4.1 ルート構造(`SaveData`クラス)

```json
{
  "Version": 1,
  "LastSaveUtc": "ISO8601 UTC",
  "Currency": { "Money": 0 },
  "Pets": [ { "CharacterId": int, "Level": int, "Exp": int, "Hunger": float, "Unlocked": true } ],
  "Dictionary": [ { "WordId": int, "UnlockedUtc": "ISO8601 UTC" } ],
  "Research": [ { "CharacterId": int, "WordId": int, "Status": "Researching", "StartUtc": "...", "FinishUtc": "..." } ],
  "Facilities": [ { "FacilityId": "ResearchLab|Library|Garden", "Level": int } ],
  "Idle": { "LastLoginUtc": "ISO8601 UTC" },
  "Inventory": { "ItemCounts": { "1": 3, "2": 1 } },
  "Settings": { "BgmVolume": 1.0, "SeVolume": 1.0, "Language": "ja", "Quality": 2 },
  "ResearchBoostExpiryUtc": "ISO8601 UTC",
  "ResearchBoostMultiplier": 1.0
}
```

- `Pets`/`Dictionary`/`Research`/`Facilities`は**該当するものだけ**が要素として存在するリスト(未解放の文字や未理解の単語のレコードは作らない。「存在しない=未所持/未理解」という設計)。
- `Facilities`は初回アクセス時に遅延生成される(`FacilitySystem.GetOrCreateFacilityData`)。
- `ResearchBoostExpiryUtc`が現在時刻より過去なら、バフは効果無し(`PetSystem.IsResearchBoostActive()`が判定)。

## 4.2 Version/Migration

`SaveSystem.CurrentVersion = 1`固定。Migrationの仕組み(`RunMigration`)は用意されているが、Version1からの移行ステップは未実装(将来のVersion2以降で追加する想定の空実装)。

## 4.3 保存タイミング

各Systemの状態変更メソッド(`AddExperience`, `Feed`, `UnlockPet`, `StartResearch`, `CompleteResearch`, `AddMoney`, `ConsumeMoney`, `UpgradeFacility`, `AddItem`, `UseSeed`経由の`UnlockPet`, `ApplyResearchBoost`等)が呼ばれるたびに同期的に`SaveSystem.Save()`が実行される(バッチ化・デバウンスなし)。多重保存防止のガード(`_isSaving`)はあるが、頻度制御はしていない。

## 4.4 ロード時のValidation

`SaveSystem.Load()`は以下を自動補正する: `Currency`がnullまたはMoney<0→0、`Pets`/`Dictionary`/`Research`/`Facilities`がnull→空リスト、`Idle`がnull→現在時刻、`Inventory`/`ItemCounts`がnull→空、`Settings`がnull→デフォルト値。JSON破損時は新規ゲームにフォールバックする。

---

# 5. Systems 詳細

## 5.1 EventBus

`Subscribe<TEvent>(Action<TEvent>)` / `Unsubscribe<TEvent>(Action<TEvent>)` / `Publish<TEvent>(TEvent)`。型ごとに`Dictionary<Type, List<Delegate>>`で管理し、同期的に即時実行する。購読前に発火したイベントは受信されない。

## 5.2 SaveSystem

`Systems/SaveSystem.cs`。責務: JSON保存/読込/新規ゲーム生成/バリデーション。詳細は4章参照。

**公開API**: `Load()`, `NewGame()`, `Save()`, `AutoSave()`(Save()のエイリアス、呼び出し箇所なし), `DeleteSave()`, `Exists()`, `GetVersion()`, `WasNewGame`(プロパティ、直近のLoad/NewGameが新規作成だったか)。

## 5.3 MasterManager

`Managers/MasterManager.cs`。9種のMasterをAddressables経由(`Addressables.LoadAssetAsync<T>("Master/{名前}")`)で非同期ロードし、プロパティとして公開する。CSV読込やインポート処理は一切持たない(それはEditor専用の`MasterDataImporter`の責務)。

## 5.4 CurrencySystem

言霊(long型)の唯一の管理者。`GetMoney()`, `AddMoney(long)`(0以下は例外), `ConsumeMoney(long)`(残高不足ならfalseを返し`OnMoneyInsufficient`発火、成功なら`OnMoneyConsumed`+`OnMoneyChanged`発火), `CanConsume(long)`, `SetMoney(long)`(デバッグ用、UIからは未使用)。オーバーフロー時は`long.MaxValue`にクランプ。

## 5.5 PetSystem

文字ペットの育成を管理する中核System。

**データ**: `SaveData.Pets`(存在するレコード=解放済み)をキャッシュ(`Dictionary<int,PetData>`)。

**API一覧**:

| API | 説明 |
|---|---|
| `GetPet(characterId)` | 存在しなければ`ArgumentException` |
| `GetAllPets()` | 解放済み全員 |
| `IsUnlocked(characterId)` | bool |
| `UnlockPet(characterId)` | 新規解放。Level=PetMaster.InitialLevel(1)、Exp=0、Hunger=100。`OnPetUnlocked`発火 |
| `AddExperience(characterId, amount)` | 経験値付与+複数レベルアップ処理。`OnPetLevelUp`(レベル変化時のみ)+`OnPetUpdated`発火 |
| `Feed(characterId, itemType)` | 満腹度回復。`OnPetFed`発火 |
| `UpdateHunger(elapsed)` | 全員の満腹度を時間経過で減少(`HungerDecayPerHour × 経過時間`) |
| `GetProductionRate(characterId)` | 1秒あたり言霊生産量(下記式) |
| `GetResearchSpeed(characterId)` | 研究速度倍率(下記式) |
| `ApplyResearchBoost(multiplier, duration)` | グローバル研究速度バフを付与、SaveDataに永続化 |
| `IsResearchBoostActive()` / `GetResearchBoostRemaining()` | バフ状態の取得 |
| `CalculateProduction(elapsed)` | 全員分の生産量合計(経過時間×レート) |
| `CanLevelUp(characterId)` | bool |

**生産量の計算式**:
```
GetProductionRate = BaseProduction(PetMaster, 固定1)
                   × ProductionMultiplier(ExpMaster, レベル依存)
                   × HungerMultiplier(下記)
                   × FacilityMultiplier(FacilitySystem.GetEffectValue(Garden))
                   → long にキャストして返す
```

**研究速度の計算式**:
```
GetResearchSpeed = BaseResearchSpeed(PetMaster, 固定1.0)
                  × ResearchSpeedMultiplier(ExpMaster, レベル依存)
                  × HungerMultiplier
                  × FacilityMultiplier(FacilitySystem.GetEffectValue(ResearchLab))
                  × ResearchBoostMultiplier(バフが有効な場合のみ、通常1.0)
```

**満腹度倍率(HungerMultiplier)**: `Hunger <= 0` → `HungerStarvingMultiplier`(0.5)、`Hunger < HungerLowThreshold(40)` → `HungerLowMultiplier`(0.8)、それ以外 → `1.0`。生産・研究速度どちらにも同じ倍率を使う(元設計書のPetSystem.mdは4段階・ResearchSystem.mdは別の数値を使っていたが、実装ではGameBalanceMasterで一本化した2段階しきい値を採用)。

**満腹度の減衰**: `減少量 = HungerDecayPerHour(5) × 経過時間(時間単位)`。0〜100にクランプ。

**エラー処理**: 未解放文字へのアクセスは`ArgumentException`、重複解放は`InvalidOperationException`、経験値0以下は`ArgumentOutOfRangeException`。

## 5.6 ItemSystem

消耗品の所持数管理・使用。

**データ**: `SaveData.Inventory.ItemCounts`(`Dictionary<int,int>`、ItemId→所持数)。

**API**: `GetItemCount(itemId)`, `HasItem(itemId)`, `GetAllItems()`(IReadOnlyDictionary), `AddItem(itemId, count)`(`OnItemAdded`発火), `RemoveItem(itemId, count)`(所持数不足ならfalse、成功で`OnItemRemoved`発火), `Use(itemId, characterId)`。

**`Use()`の分岐**:
- `ItemType.Food` → `PetSystem.Feed(characterId, Food)`
- `ItemType.Seed` → 未所持文字からランダムで1体選び`PetSystem.UnlockPet()`(候補0=全解放済みなら`InvalidOperationException`)
- `ItemType.ResearchBoost` → `PetSystem.ApplyResearchBoost(itemEntry.Value, itemEntry.DurationSeconds)`
- それ以外(`Special`) → `NotSupportedException`

使用成功時は必ず1個消費し`OnItemUsed`発火。

## 5.7 ShopSystem

`GetItems()`(ShopMaster全件), `CanPurchase(shopEntryId)`, `Purchase(shopEntryId)`(所持金確認→`CurrencySystem.ConsumeMoney`→`ItemSystem.AddItem`→`OnItemPurchased`。残高不足なら`OnPurchaseFailed`でfalse)。購入履歴を保持しないため何度でも購入可能。

## 5.8 WordSystem

単語マスタの検索専用。**状態を持たず、EventBusにも依存しない**(イベントを一切発火しない設計。理由は`00_ImplementationProgress.md`のPhase3節を参照)。

起動時にMasterManager.WordMaster.Entriesから4種のキャッシュを構築: WordId→Entry、CharacterId→Word一覧、Category→Word一覧、Reading→Entry。

**API**: `GetWord(wordId)`, `FindByReading(reading)`(見つからなければnull), `GetWords()`, `GetWordsByCharacter(characterId)`, `GetWordsByCategory(category)`, `GetWordsByDifficulty(difficulty)`, `GetResearchTime(wordId)`, `GetDifficulty/GetRequiredLevel/GetCategory(wordId)`, `GetCharacters(wordId)`(構成文字のCharacterId一覧), `ContainsCharacter(wordId, characterId)`, `IsLevelUnlocked(wordId, petLevel)`, `GetCandidateWords(characterId, petLevel, excludedWordIds)`(所持文字・レベル・除外セットで絞り込み。**カテゴリのフィルタはここでは行わない**、`ResearchSystem`が図書館レベルと照合して行う), `SelectRandomWord(candidates)`。

## 5.9 DictionarySystem

理解済み単語(図鑑)の管理。`SaveData.Dictionary`のレコード有無=理解済みかどうか。`HashSet<int>`でO(1)判定。

**API**: `UnlockWord(wordId)`(理解済みなら何もしない。新規なら`Dictionary`に追加、`OnWordUnlocked`+`OnCompletionUpdated`+`OnCategoryCompletionUpdated`を発火), `IsUnlocked(wordId)`, `GetDictionary()`, `GetUnlockedWords()`, `GetLockedWords()`, `GetCompletionRate()`(0〜1)、`GetCategoryCompletionRate(category)`、`GetUnlockedCount()`、`GetTotalWordCount()`。

## 5.10 ResearchSystem

研究の開始・進行・完了を管理。**1文字につき同時1件まで**(`_researchByCharacterId`はCharacterIdをキーとする単一レコード)。

**研究は完全自動(2026-07-19〜)**: プレイヤーが単語を選ぶUIは存在しない。`UpdateResearch()`の末尾で毎回`AutoStartResearchForIdleCharacters()`を呼び、**研究中でなく・満腹度が0より大きい**全ての解放済みキャラについて、そのキャラで研究可能な候補(所持文字・レベル・図書館カテゴリ解放・理解済み/研究中との重複除外を満たすもの)からランダムに1つ選んで自動的に`StartResearch`する。候補が無ければそのキャラは待機する。手動選択画面(旧`ResearchSelectView`/`ResearchSelectPresenter`、読み仮名自由入力検索を含む)は撤去済み。

**満腹度0での挙動**: `CanStartResearch`/`StartResearch`は満腹度0以下のキャラを拒否する。さらに`UpdateResearch()`は、研究中のキャラの満腹度が0以下になった時点でその研究を`CancelResearch`する(進行度は破棄)。満腹度による効果は他システム(生産・研究速度の乗率)と異なり、ここは**時間経過で自然回復せず、給餌で満腹度が0を超えるまで再開しない**。

**進行方式**: UTC時刻ベース。研究開始時に速度倍率を1回だけ計算し、`FinishUtc = StartUtc + (WordMaster.ResearchTimeSeconds / 開始時点の速度倍率)`を確定する。**開始後にレベル・施設・バフが変化しても、進行中の研究の所要時間は再計算されない**(重要な設計上の割り切り。満腹度0のみ例外的にキャンセルとして即時反映される)。

**API**:

| API | 説明 |
|---|---|
| `IsResearching(characterId)` | bool |
| `GetResearch(characterId)` | 存在しなければ`InvalidOperationException` |
| `GetAllResearch()` | 全員分 |
| `CanStartResearch(characterId, wordId)` | 解放済み・非研究中・満腹度>0・未理解・文字を含む・レベル十分の全条件をbool判定(例外を投げない安全版) |
| `StartResearch(characterId, wordId)` | 条件を満たさなければ各種`InvalidOperationException`。成功で`OnResearchStarted` |
| `CancelResearch(characterId)` | 研究中でなければ何もしない。成功で`OnResearchCanceled`(進行度は破棄) |
| `UpdateResearch()` | **引数なし**。満腹度0キャラの研究をキャンセル→FinishUtc超過分を`CompleteResearch`→`AutoStartResearchForIdleCharacters()`で空いているキャラに自動着手、の順で実行。GameTickerから5秒おきに呼ばれる |
| `AutoStartResearchForIdleCharacters()` | 研究中でなく満腹度>0の全キャラに対し、候補からランダムに1つ選び自動着手。`UpdateResearch()`から呼ばれるほか、`IdleSystem`のオフライン進行計算経由でも間接的に呼ばれる |
| `CompleteResearch(characterId)` | `DictionarySystem.UnlockWord()` → 単語の構成文字**全員**(所持しているキャラのみ)に経験値付与 → `OnResearchCompleted` |
| `GetRemainingTime(characterId)` | 研究していなければ`TimeSpan.Zero` |
| `GetProgressRate(characterId)` | 0〜1(FinishUtc/StartUtcからの逆算) |

コンストラクタに`FacilitySystem`が追加された(カテゴリの図書館レベル判定に必要なため)。

**経験値計算**(`CompleteResearch`内、`CalculateExperience`):
```
Exp = BaseExp(GameBalanceMaster, 20) × LengthBonusMultiplier(文字数依存)
LengthBonusMultiplier: 2文字以下=1.0, 3文字=1.1, 4文字=1.2, 5文字以上=1.3
```
この経験値が、単語を構成する**全ての所持文字**に同額付与される(1文字だけでなく単語全体のメンバーに)。

## 5.11 IdleSystem

放置時間の計算と報酬の付与。「計算」と「確定」を分離した2段階設計。

**API**: `CalculateOfflineProgress()`(現在時刻-LastLoginUtcを算出、MaxOfflineHours(8h)でクランプ、`PetSystem.UpdateHunger()`+`ResearchSystem.UpdateResearch()`実行、`RewardMoney`を算出して保持するが**まだCurrencySystemには加算しない**。`OnOfflineCalculated`発火。経過0秒なら`OnOfflineSkipped`のみ), `ApplyOfflineReward()`(保持していた`RewardMoney`を`CurrencySystem.AddMoney()`で確定加算。`OnOfflineRewardApplied`発火), `SaveLoginTime()`(現在UTCを`SaveData.Idle.LastLoginUtc`に保存), `GetOfflineTime()`, `GetRewardMoney()`, `HasOfflineReward()`。

現状GameManagerが起動直後に`CalculateOfflineProgress()`→`ApplyOfflineReward()`を連続実行しているため、UI上は分離の恩恵(受け取り演出待ち)は使っていない。`CalculateOfflineProgress()`全体はtry-catchで保護され、失敗時は経過0・報酬0にフォールバックして起動を継続する。

## 5.12 FacilitySystem

3施設(ResearchLab/Library/Garden)のレベル・強化管理。

**API**: `GetLevel(facilityId)`(未初期化ならLv1で遅延生成), `GetUpgradeCost(facilityId)`(次レベルのマスタが無ければ-1=強化不可), `GetEffectValue(facilityId)`(現レベルのEffectValue。マスタが無ければ1.0), `CanUpgrade(facilityId)`, `UpgradeFacility(facilityId)`(所持金確認→消費→Level++→`OnFacilityUpgraded`。次の次のレベルが存在しなければ`OnFacilityMaxLevel`も発火), `GetAllFacilities()`。

## 5.13 GameTicker(`Core/GameTicker.cs`)

Systemではなく`Core`層の補助クラス。UniTaskの`Delay`ループで**5秒間隔**の進行処理を担当:

```
Tick() {
  経過時間 = 現在UTC - 前回UTC
  PetSystem.UpdateHunger(経過時間)
  ResearchSystem.UpdateResearch()
  production = PetSystem.CalculateProduction(経過時間)
  if (production > 0) CurrencySystem.AddMoney(production)
}
```

`Start()`/`Stop()`はCancellationTokenSourceで制御。`GameManager`がBootstrap完了時に開始し、アプリのポーズ/復帰で停止・再開する。**MonoBehaviour.Update()は使用しない**(docsの「Update禁止」方針に準拠)。

---

# 6. UI仕様

## 6.1 実装方針

Canvas・Button・TextMeshProUGUI等の全UIは**Unity EditorでPrefab/シーンとして手作業配置するのではなく、`UiFactory`を介してC#コードで実行時に動的生成**する(ユーザー承認済みの方針。理由: 開発環境からUnity Editorを直接操作・目視確認できないため)。

## 6.2 UiFactory(`UI/Components/UiFactory.cs`)

| メソッド | 生成するもの |
|---|---|
| `CreateCanvasRoot(name)` | Canvas + CanvasScaler(ScaleWithScreenSize, 参照解像度1080×1920) + GraphicRaycaster |
| `CreateEventSystem()` | EventSystem + `InputSystemUIInputModule`(new Input System用) |
| `StretchFull(rect)` | RectTransformを親いっぱいに広げるユーティリティ |
| `CreatePanel(parent, color)` | 単色Image |
| `CreateText(parent, text, fontSize, alignment)` | TextMeshProUGUI。**日本語フォントを自動適用**(下記6.3) |
| `CreateButton(parent, label, onClick)` | Image+Button+中央寄せラベル |
| `CreateScrollView(parent, out content)` | ScrollRect+Mask+VerticalLayoutGroupのリスト表示領域 |
| `CreateInputField(parent, placeholderText)` | TMP_InputField(プレースホルダ付き) |
| `CreateSlider(parent, min, max, initial, onValueChanged)` | Slider(Fill表示付き) |

## 6.3 日本語フォント対応

TMP Essential Resourcesの既定フォントは日本語グリフを含まないため、`UiFactory`は初回`CreateText`呼び出し時に`TMP_FontAsset.CreateFontAsset(familyName, "Regular", 90)`(`AtlasPopulationMode.DynamicOS`)でOSインストール済みフォントを動的読み込みする。候補: `Yu Gothic UI`→`Yu Gothic`→`Meiryo UI`→`Meiryo`→`MS Gothic`→`Noto Sans CJK JP`→`Noto Sans JP`→`Hiragino Sans`→`Hiragino Kaku Gothic ProN`(先頭から見つかったものを使用、以降キャッシュ)。

**既知の制約**: Windows/Editor向けのフォールバックであり、iOS/Android実機では別対応(埋め込みフォントアセットの事前生成)が必要。

## 6.4 画面構成(`HomeUIRoot.cs`)

Homeシーンにアタッチされた`HomeUIRoot`が`Start()`でCanvas以下を全て動的生成する。

```
Canvas
├── World (HomeWorldView) - もじの庭。所持文字がPetTokenとして表示・徘徊
├── Header (高さ170px, raycastTarget無効)
│   ├── 言霊表示テキスト(上半分)
│   └── ButtonRow (HorizontalLayoutGroupで自動整列、下半分)
│       ├── 図鑑ボタン → DictionaryView
│       ├── 施設ボタン → FacilityView
│       ├── ショップボタン → ShopView
│       ├── 持ち物ボタン → InventoryView
│       └── 設定ボタン → SettingsView
├── Toasts (通知レイヤー)
└── Windows (モーダルウィンドウレイヤー、最前面)
```

起動時に`OnMoneyChanged`/`OnResearchCompleted`/`OnPetLevelUp`を購読し、それぞれ言霊表示更新・「新しいことば！」Toast・「レベルアップ！」Toastを表示する。起動直後、直近の放置時間が0超なら放置報酬Toastも表示する。

## 6.5 もじの庭(`HomeWorldView` + `PetToken`)

- `HomeWorldView`: 所持済み文字ぶん`PetToken`を生成。`OnPetUnlocked`購読で新規解放時に追加生成。
- `PetToken`: 1体の文字を表すTextMeshPro。3〜6秒のランダム待機後、画面内のランダム位置へ1.5秒かけて緩やかに移動する処理をUniTaskループで繰り返す(**MonoBehaviour.Updateは不使用**)。タップで`OpenPetDetail`コールバックが発火。
- 庭のスクロール拡張(施設強化で庭が広がる仕様)は未実装。常に画面内に収まる範囲で移動する。

## 6.6 各ウィンドウ

| ウィンドウ | Presenter | 内容 |
|---|---|---|
| DictionaryView | DictionaryPresenter | 単語一覧(未理解は「？？？」表示)、完成率(X/Y、%) |
| FacilityView | FacilityPresenter | 3施設のLv・効果値・強化コストと強化ボタン。最大Lvは「強化」ボタンを非表示にしテキストのみ |
| ShopView | ShopPresenter | 商品一覧・所持数・価格・購入ボタン(残高不足でinteractable=false) |
| InventoryView | InventoryPresenter | 所持アイテム一覧。SeedとResearchBoostタイプのみ「使う」ボタン表示(Foodは文字詳細から使う設計) |
| PetDetailView | PetDetailPresenter | Lv・経験値・満腹度・生産量・研究状況・研究速度バフ残り時間。「エサをあげる」「描き直す」ボタン(研究は完全自動のため選択・中止ボタンは無い) |
| HandwritingView | ― | 新しい文字が生まれた時に自動で開く手書きキャンバス。背景に薄いガイド文字、「消す」「できた！」ボタン |
| SettingsView | SettingsPresenter | BGM/SE音量スライダー、画質切替(QualitySettingsに連動) |

いずれも背景Panelで画面全体を覆う簡易モーダル(`raycastTarget`は既定のtrueのまま、下にあるWorld/Headerへのクリックを遮断する)。閉じるボタンで`Destroy(gameObject)`。

**ResearchSelectPresenterのカテゴリゲート**: `GetCandidates()`は`WordSystem.GetCandidateWords()`の結果から、さらに`FacilitySystem.GetLevel(Library)`と`CategoryMaster`を照合してカテゴリ未解放の単語を除外する。`TryStartResearchByReading(characterId, reading)`は自由入力検索用で、`(bool Success, string ErrorMessage)`のタプルを返す(理由: 辞書に無い/理解済み/文字を含まない/レベル不足/カテゴリ未解放/他文字が研究中/開始失敗のいずれか)。

## 6.7 Toast(`UI/Views/Toast.cs`)

画面上部に2.5秒間表示され自動で消える通知。`Toast.Show(parent, message)`の静的呼び出しのみ。アニメーション(フェード等)は無く即座に表示・破棄。

---

# 7. イベント一覧

全イベントは`Mojipet.Events`名前空間の`readonly struct`。`EventBus.Publish<T>`で同期発火する。

| System | イベント | ペイロード |
|---|---|---|
| SaveSystem | `OnSaveLoaded` | SaveData |
| | `OnSaveCompleted` | DateTime LastSaveUtc |
| | `OnNewGameCreated` | (なし) |
| | `OnMigrationCompleted` | int OldVersion, int NewVersion |
| CurrencySystem | `OnMoneyAdded` | long AddedAmount, long CurrentMoney |
| | `OnMoneyConsumed` | long ConsumedAmount, long CurrentMoney |
| | `OnMoneyChanged` | long CurrentMoney |
| | `OnMoneyInsufficient` | long RequiredMoney, long CurrentMoney |
| PetSystem | `OnPetUnlocked` | int CharacterId |
| | `OnPetLevelUp` | int CharacterId, int OldLevel, int NewLevel |
| | `OnPetFed` | int CharacterId, ItemType ItemType, float OldHunger, float NewHunger |
| | `OnPetUpdated` | int CharacterId |
| DictionarySystem | `OnWordUnlocked` | int WordId |
| | `OnCompletionUpdated` | float CompletionRate |
| | `OnCategoryCompletionUpdated` | CategoryId Category, float CompletionRate |
| ResearchSystem | `OnResearchStarted` | int CharacterId, int WordId |
| | `OnResearchCompleted` | int CharacterId, int WordId |
| | `OnResearchCanceled` | int CharacterId |
| IdleSystem | `OnOfflineCalculated` | TimeSpan ElapsedTime, long RewardMoney |
| | `OnOfflineRewardApplied` | long RewardMoney |
| | `OnOfflineSkipped` | (なし) |
| ItemSystem | `OnItemAdded` | int ItemId, int NewCount |
| | `OnItemRemoved` | int ItemId, int NewCount |
| | `OnItemUsed` | int ItemId, int CharacterId |
| ShopSystem | `OnItemPurchased` | int ShopEntryId, int ItemId |
| | `OnPurchaseFailed` | int ShopEntryId, string Reason |
| FacilitySystem | `OnFacilityUpgraded` | FacilityId FacilityId, int Level |
| | `OnFacilityMaxLevel` | FacilityId FacilityId |

**WordSystemはイベントを発火しない**(意図的な設計、5.8節参照)。

現在UI側で実際に購読しているのは`OnMoneyChanged`/`OnResearchCompleted`/`OnPetLevelUp`/`OnPetUnlocked`のみ。他のイベントはSystem間の内部通知や将来のUI拡張向けに発火されているが、購読側は未実装。

---

# 8. ゲームプレイフロー(実装済みの一連の操作)

1. 初回起動 → 新規ゲーム作成 → 「ことばのたね」1個が自動付与される
2. 「持ち物」を開き、ことばのたねの「使う」をタップ → ランダムな未所持文字が1体解放される(`OnPetUnlocked`)
3. `OnPetUnlocked`を受けて`HandwritingView`が自動的に開く。ガイド文字をなぞって手書きし「できた！」で保存(スキップも可)
4. もじの庭に文字が出現し、ゆっくり徘徊し始める(手書き済みならその絵を表示)
5. 5秒ごとのGameTickerが進行を判定。満腹度が減り、言霊が生産され、研究完了時刻を過ぎた研究があれば自動完了し、研究中でなく満腹度が残っているキャラは自動で次の単語の研究を開始する(`OnResearchStarted`。プレイヤーが単語を選ぶ操作は無い)
6. 研究完了(`OnResearchCompleted`) → 図鑑に単語登録 → 単語の構成文字全員(所持分)に経験値付与 → レベルアップがあれば`OnPetLevelUp` → 画面上部にToast通知
7. 言霊が貯まったらショップでアイテム購入、または施設(研究所/図書館/もじの庭)を強化
8. ことばの実は文字詳細画面から「エサをあげる」で使用(満腹度回復)。満腹度が0になると研究は自動的に中断され、給餌で満腹度が回復するまで新規研究も始まらない
9. ひらめきのしずくは持ち物から直接使用(以後の新規研究に速度バフ)
10. アプリを閉じて再度開くと、離れていた時間ぶんの満腹度減少・研究進行・言霊生産がまとめて計算され(上限8時間)、Toastで通知される

---

# 9. 元設計書との既知の差異・未実装事項

## 9.1 意図的に元設計と変えた点(承認済みの設計判断)

- Shop/Itemは消耗品のみ(家具装飾システムは実装しない)
- Facilityは`ResearchLab/Library/Garden`構成(`FacilitySystem.md`の`ResearchLab/StudyRoom/Library`案は不採用)
- WordMasterは拡張スキーマ(RequiredLevel/ResearchTimeを単語ごとにCSV直接管理、ResearchMasterは実質未使用)
- SaveDataは`systems/SaveSystem.md`をベースに消耗品用InventoryDataを追加した独自スキーマ
- WordSystemはEventBusに依存せずイベントを発火しない
- 研究進行はUTC時刻ベースで、開始時に速度を確定(以後の満腹度/バフ変化を遡って反映しない。ただし満腹度0によるキャンセルのみ即時反映される例外)
- 研究対象の単語はプレイヤーが選ばず、`ResearchSystem`が候補からランダムに自動選択する(2026-07-19、元設計の手動選択画面から変更)
- 新しい文字が生まれた時、プレイヤーが手書きでその文字を描ける(`HandwritingView`、2026-07-19追加。元設計には無い機能)

## 9.2 未実装

- 単語の完全自由入力での新規追加(WordMasterに無い単語は登録不可。研究対象は`ResearchSystem`が既存データから自動選択する)
- ことばのたね初回引き直しのチュートリアルUX
- もじの庭のスクロール拡張(施設強化で庭が広がる仕様)
- レベルアップ時の「跳ねる」演出、研究完了時の頭上「！」表示等の視覚演出、頭上ステータスアイコン(研究中✍️/空腹🍖等)
- 実際の音声再生(設定画面の音量値は保存されるだけで、鳴らす仕組み自体が無い)
- FacilityMasterのLv21以上のデータ(現状Lv20が事実上の上限)
- Phase9のバランス調整(現在の数値は全て仮のプレースホルダ)

## 9.3 テスト

`Scripts/Tests/EditMode`・`PlayMode`のasmdefは用意されているが、テストコードは1件も書かれていない。
