# 00_ImplementationProgress.md

本ドキュメントは仕様書ではない。`06_DevelopmentRoadmap.md` の実装進捗を記録するトラッキング用ドキュメントであり、実装が完了したタスクから随時チェックを更新する。

---

## UI配色の統一(2026-07-19)

各Viewがそれぞれ`new Color(0,0,0,0.85f)`等をバラバラに直書きしていたのを、`UI/Components/UiTheme.cs`に集約した。暖色寄りの落ち着いた配色(紫〜珊瑚色、和紙・言霊をイメージ)。

- `UiFactory.CreateText`のデフォルト文字色、`CreateButton`のデフォルト色、`CreateInputField`/`CreateSlider`/`CreateScrollView`の背景色を`UiTheme`参照に変更
- `CreateButton`に`ButtonStyle`(Primary/Secondary/Danger)引数を追加。閉じる・ページ送りは`Secondary`、研究中止は`Danger`、それ以外の主要アクションは既定の`Primary`
- 全Window(図鑑/施設/ショップ/持ち物/文字詳細/研究選択/設定)・ヘッダー・Toastの背景色を`UiTheme.WindowBackground`/`UiTheme.Surface`に統一

レイアウト(余白・配置)や演出(アニメーション)は今回の対象外(見た目・配色のみ)。

---

## プレイ中に進行が止まっていた不具合(2026-07-19対応、重要)

研究完了・満腹度減少・生産の言霊反映が、アプリ**起動時の`IdleSystem.CalculateOfflineProgress()`一度きり**でしか実行されておらず、フォアグラウンドでプレイしている間はずっと止まったままだった(研究が100%表示でも完了処理が走らずToastも出ない、満腹度が減らないためエサが無意味、等)。`PetSystem.md`/`ResearchSystem.md`の「一定間隔で加算する(例:5秒/10秒ごと)」という仕様を見落としていた。

`Core/GameTicker.cs`を新規作成し、5秒間隔で`ResearchSystem.UpdateResearch()`・`PetSystem.UpdateHunger()`・生産の`CurrencySystem.AddMoney()`反映を行うようにした。`GameManager`がBootstrap完了時に開始し、アプリのポーズ/復帰時にも停止・再計算・再開するよう修正。

---

# 既知の問題と対応

## 日本語フォント文字化け(2026-07-19対応)

TMP Essential Resourcesのデフォルトフォント(LiberationSans SDF)は日本語グリフを含まないため、UI上の日本語がすべて文字化けする不具合があった。

`UiFactory.CreateText()`内で`TMP_FontAsset.CreateFontAsset(familyName, styleName)`(AtlasPopulationMode.DynamicOS)を使い、OSにインストール済みの日本語フォント(Yu Gothic UI / Meiryo / MS Gothic / Hiragino Sans等)を候補順に試して動的に割り当てるよう修正した。

**既知の制約**: これはEditor/Windows実機確認用のフォールバックであり、iOS/AndroidのOS標準日本語フォント名は異なる(また端末によって未インストールの場合もある)。実機ビルド・リリース前には、日本語グリフを含む埋め込みフォントアセット(Noto Sans JP等をFont Asset Creatorで事前生成)への差し替えが別途必要。

## WordMasterの文字カバレッジ不足(2026-07-19対応)

Phase1で作成した`WordMaster.csv`は18語のみで、46文字中一部しかカバーしていなかった。「ことばのたね」はランダムに未所持文字を選ぶため、カバーされていない文字を引くと研究できる単語が0件になっていた。

2文字の簡単な単語(難易度1・必要Lv1)を20語追加し、Perlスクリプトで全46文字のカバレッジを検証した。

- **45/46文字をカバー**(あ〜んのうち「を」以外)
- **「を」のみ未カバー**: 助詞としてのみ使われる文字のため、自然な複合名詞の読みにほぼ登場しない。「を」を引いた文字は現状研究できる単語が無い。対応方針(不自然でも単語を用意する/初回引き直しを実装する/「を」を特別扱いする 等)は要相談。

---

# 確定した設計方針(2026-07-19)

- Shop/Item: 消耗品(ことばの実/ひらめきのしずく/ことばのたね)のみ実装。家具配置システムはMVP対象外。
- Facility: `02_GameDesign.md`版(ResearchLab/Library/Garden)を採用。
- SaveData: `systems/SaveSystem.md`版をベースに消耗品用InventoryDataを追加。
- WordMaster: `systems/WordSystem.md`の拡張スキーマ(RequiredLevel/ResearchTimeをCSVで直接管理)。
- `docs/system/PetSystem.md`(重複・破損ファイル)は無視。`docs/systems/PetSystem.md`のみ正とする。

---

# Phase0: プロジェクト基盤

- [x] Addressablesパッケージ追加(`com.unity.addressables`)
- [x] UniTaskパッケージ追加(`Packages/com.cysharp.unitask`, 2.5.10を埋め込みパッケージとして配置。git URL方式は環境依存のgit解決エラーが発生したため不採用)
- [x] Newtonsoft.Jsonパッケージ追加(`com.unity.nuget.newtonsoft-json`)
- [x] asmdef構成作成(Mojipet.Runtime / Mojipet.Editor / Mojipet.Tests.EditMode / Mojipet.Tests.PlayMode)
- [x] EventBus作成(`Scripts/Runtime/Events/EventBus.cs`)
- [x] TimeUtility作成(`Scripts/Runtime/Utilities/TimeUtility.cs`)
- [ ] TextMeshPro Essential Resourcesインポート(UI実装(Phase2以降)までに実施)
- [x] Unity Editorでのパッケージ解決確認(2026-07-19 ユーザー確認済み、エラーなし)

成果物: ゲームが起動する(達成)

---

# Phase1: 基盤システム

- [x] Models作成(SaveData/PetData/DictionaryEntryData/ResearchData/FacilityData/CurrencyData/IdleData/InventoryData/SettingsData + CategoryId/ItemType/FacilityId/ResearchStatus)
- [x] Master ScriptableObject作成(WordMaster/PetMaster/ExpMaster/ItemMaster/FacilityMaster/ShopMaster/ResearchMaster/GameBalanceMaster)
- [x] CSVインポータ作成(`Tools > Import MasterData`)とサンプルCSV(`Assets/MasterData/*.csv`)
- [x] MasterManager作成(Addressables経由でMaster SOをロード)
- [x] SaveSystem作成(Newtonsoft.JsonによるJSON保存/ロード、Validation、Migration土台)
- [x] GameManager作成(Boot→Addressables初期化→MasterManager→SaveSystem→Homeへ遷移)
- [x] Boot/Homeシーン作成、Build Settingsを [0]Boot [1]Home に設定

成果物: ゲームを保存できる(達成、要Editor動作確認)

## 未実施(要Editor手動操作)

- [ ] `Tools > Import MasterData` の実行(CSV→ScriptableObject生成。初回はAddressable Groups未初期化のため `Window > Asset Management > Addressables > Groups` を先に一度開く)
- [ ] Boot→Home遷移の実機/Playモード確認

## Phase1で残した設計上の未確定事項(Phase7で確定させる)

- FacilityMasterの `EffectValue` の意味は施設ごとに異なる想定(ResearchLab=研究速度倍率、Garden=生産倍率、Library=カテゴリ解放の閾値 or 未使用)だが、現状は3施設とも同じダミー計算式(仮データ)。Phase7のFacilitySystem実装時に確定する。
- ItemMaster(ひらめきのしずく等)の効果時間(Duration)フィールドは04_MasterData.mdのItemMasterスキーマに存在しないため未追加。Phase5のItemSystem実装時に必要になった時点で追加する。

---

# Phase2: 育成システム

- [x] CurrencySystem(GetMoney/AddMoney/ConsumeMoney/CanConsume/SetMoney、イベント4種)
- [x] PetSystem(GetPet/GetAllPets/UnlockPet/AddExperience/Feed/UpdateHunger/GetProductionRate/GetResearchSpeed/CalculateProduction/CanLevelUp/IsUnlocked)
- [x] GameManagerへ組み込み(SaveSystem.Load()後にCurrencySystem/PetSystemを生成)

成果物: 文字が育つ(達成、要Editor動作確認)

## Phase2で追加したMasterスキーマ拡張

- `ExpMaster.csv` に `ProductionMultiplier` / `ResearchSpeedMultiplier` 列を追加(レベル別の生産・研究速度倍率。04_MasterData.mdには倍率カーブの格納先が明記されていなかったため、レベル別テーブルであるExpMasterに相乗り)
- `GameBalanceMaster.csv` に `HungerDecayPerHour` / `HungerLowThreshold` / `HungerLowMultiplier` / `HungerStarvingMultiplier` を追加(満腹度減衰・研究/生産速度への影響。同じくPetSystem.mdが要求するが04_MasterData.mdに格納先がなかったため追加)
- 上記追加に伴い `Tools > Import MasterData` の再実行が必要です

## Phase2で保留した項目(Phase4/5で対応)

- `CollectProduction()` / `OnProductionCompleted` イベント: 一定間隔で言霊を確定加算する「収集」の駆動役(タイマー)がまだ存在しないため未実装。Phase4(IdleSystem)で放置計算と合わせて実装する。
- `Feed()` はアイテムの所持数を消費しない(満腹度回復の適用のみ)。所持数チェック・消費はPhase5のItemSystemが`Feed()`を呼び出す前段で行う設計とする。
- FacilityMultiplierは`GetProductionRate`/`GetResearchSpeed`内で1.0固定。Phase7のFacilitySystem実装時に接続する。

---

# Phase3: 研究システム

- [x] WordSystem(単語マスタの検索・キャッシュ専用。イベントは発火しない設計 ※下記参照)
- [x] DictionarySystem(理解済み単語管理、収集率計算)
- [x] ResearchSystem(研究開始/完了/中止、経験値付与)
- [x] GameManagerへ配線(WordSystem→DictionarySystem→ResearchSystemの順で生成)

成果物: 単語を覚える(達成、要Editor動作確認)

## Phase3の設計判断

- **WordSystemはEventBusに依存せず、イベントを発火しない**設計とした。`WordSystem.md`単体では`OnWordSelected`/`OnWordUnlocked`を発火する記述があるが、全Systemのイベントを一元管理する`Events.md`にはWordSystem由来のイベントが一つも定義されておらず、依存関係表でもWordSystemはEventBusに依存しないとされている。Events.mdを正としてWordSystemは状態を持たないクエリ専用システムとした。
- **研究時間はWordMasterEntry.ResearchTimeSeconds(単語ごと)を直接使用し、ResearchMaster.csv(Difficulty→秒数)は現状未使用**。WordMasterスキーマを拡張版(単語ごとにResearchTimeを直接管理)で採用したことと役割が重複するため。ResearchMaster自体は04_MasterData.md記載のMasterなので削除はせず残置。将来的に不要なら整理する。
- 研究の進行はUTC時刻ベース(StartUtc/FinishUtc)で管理し、`UpdateResearch()`は経過時間を受け取らず「現在時刻がFinishUtcを過ぎた研究を完了させる」だけの設計にした(docsのシグネチャは`UpdateResearch(TimeSpan elapsed)`だが、放置中も含めてUTC時刻の差分で自然に進捗するため引数が不要かつ不使用の引数を残すより誠実と判断)。研究速度倍率(レベル・満腹度)は開始時に一度だけ計算してFinishUtcに反映する(以後、満腹度が変化しても所要時間は再計算しない、MVPとしての簡略化)。
- 経験値計算式(`BaseExp × 文字数ボーナス`)を`ResearchSystem`に実装し、`GameBalanceMaster`に`BaseExp`/`LengthBonusMultiplier2〜5Plus`を追加した(WordSystem.mdは経験値計算を担当外と明記しており、ResearchSystem.mdは「WordSystemが算出」と書いているが実際にはどちらの詳細仕様書にも計算式の置き場所がなかったため、詳細な式を持つPetSystem.mdの考え方をResearchSystem内に実装)。

---

---

# Phase4: 放置システム

- [x] IdleSystem(放置時間計算、上限適用、満腹度・研究更新、放置報酬の計算/確定)
- [x] GameManagerへ配線(起動時にCalculateOfflineProgress→ApplyOfflineRewardを実行、終了/バックグラウンド時にSaveLoginTime)

成果物: 放置ゲームとして成立する(達成、要Editor動作確認)

## Phase4の設計判断

- `CalculateOfflineProgress()`は放置報酬額を計算するだけで、まだCurrencySystemには加算しない(`RewardMoney`として保持)。実際にCurrencySystemへ加算するのは`ApplyOfflineReward()`。docsが両者を分けている意図(受け取り演出を挟める設計)を尊重したが、Phase4時点ではUIがまだ無いため、GameManagerが起動時に両方を連続実行している。Phase8で放置報酬ポップアップUIを実装する際は、`ApplyOfflineReward()`の呼び出しをポップアップの「受け取る」ボタン側に移すだけで対応できる。
- 研究の完了判定(`ResearchSystem.UpdateResearch()`)はUTC時刻ベースのため、経過時間を渡す必要がなく、放置時間の長さに関わらず正しく完了判定される。
- 放置計算全体を try/catch で保護し、失敗時は空の結果(経過0・報酬0)にフォールバックしてゲーム起動を継続する(起動のたびに必ず通る処理のため、ここだけは docs 通り明示的な防御を入れた)。

---

# Phase5: ショップ

- [x] ItemSystem(消耗品の所持数管理・使用。Food=満腹度回復、Seed=ランダムな新規文字解放)
- [x] ShopSystem(購入。消耗品なので購入履歴は持たず、何度でも購入可能な設計)
- [x] GameManagerへ配線

成果物: 言霊を消費できる(達成、要Editor動作確認)

## Phase5の設計判断・保留事項

- `ItemSystem.Use()`は`ItemType.Food`(ことばの実→満腹度回復)と`ItemType.Seed`(ことばのたね→未所持文字からランダムで1体解放)のみ実装。`ItemType.ResearchBoost`(ひらめきのしずく)は「効果時間」を持つ一時バフであり、バフの永続化(SaveDataへの保存)やGetResearchSpeedへの反映など設計事項が残っているため、現時点では`NotSupportedException`を投げる(黙って何もしないより安全なため)。将来実装する際の入れ物として`Use()`のswitch文は用意済み。
- ShopSystemは元のShopSystem.md(家具装飾を想定)と異なり、購入履歴(ShopData)を持たない設計にした。消耗品は何度でも購入できるべきという前提のため。
- ことばのたねの「初回のみ引き直し可能」というチュートリアル的UXは未実装(UIが絡む話のためPhase8で検討)。

---

---

# Phase6: 図鑑

- [x] UIをC#コードで動的生成する基盤(`UiFactory`)を作成
- [x] DictionaryPresenter(表示用データ整形)、DictionaryView(図鑑ウィンドウ)を作成
- [x] HomeUIRoot(Canvas/EventSystem/ヘッダー/図鑑ボタンを起動時に生成)をHomeシーンに配置

成果物: 収集ゲームになる(達成、要Editor動作確認・下記の手動対応が必須)

## Phase6の重要な注意点

- **`Window > TextMeshPro > Import TMP Essential Resources` の実行が必須**になりました。未実施だとTextMeshProUGUIのデフォルトフォントが無く、テキストが表示されません(Phase0から保留していた項目です)。
- UIはUnity EditorでCanvas/Prefabとして手作業で組む代わりに、`HomeUIRoot`/`DictionaryView`がPlay時にC#コードで動的生成する方式を採用(ユーザー承認済み)。Editor上でCanvas階層を直接編集することはできず、見た目の調整はコード側(`UiFactory`呼び出しのRectTransformパラメータ)で行う。
- ヘッダーに「言霊」表示と「図鑑」ボタンのみ実装。研究所・図書館・ショップ・設定ボタンはPhase7以降に追加する。
- 図鑑は未理解の単語を「？？？」で表示する(docsに明記はないが収集ゲームとして一般的な表現として採用)。

---

# Phase7: 施設

- [x] FacilitySystem(ResearchLab/Library/Garden。レベル別コスト・効果はFacilityMasterから取得、最大レベルはCSVの収録範囲で自然に決まる)
- [x] PetSystemにFacilitySystemを接続(Garden→生産倍率、ResearchLab→研究速度倍率)
- [x] 施設UI(FacilityPresenter/FacilityView、ヘッダーに「施設」ボタン追加)

成果物: 長期育成要素が完成(達成、要Editor動作確認)

## Phase7の設計判断・保留事項

- **Libraryの「カテゴリ解放」効果は未実装。** 02_GameDesign.mdの「植物図鑑→植物カテゴリ追加」のような記述は、単純なレベル閾値ではなく個別の解放条件(図鑑の種類ごと)を示唆しており、FacilityMasterの単純な「Lv→EffectValue」スキーマにそのまま載せられない。現状Libraryは強化はできる(EffectValueは取得できる)が、WordSystem側のカテゴリ制限には未接続。カテゴリと解放レベルの対応表を別途設計する必要があり、仕様の具体化をお願いしたい。
- FacilityMasterは現状Lv1〜20までのプレースホルダ値(Phase1で生成)。Phase9のバランス調整で拡張・調整する。
- PetSystemのコンストラクタに`FacilitySystem`を追加したため、GameManagerでの生成順序を`CurrencySystem→FacilitySystem→PetSystem`に変更した。

---

# Phase8: UI調整

- [x] 新規ゲーム開始時に「ことばのたね」を1個付与(GameBalanceMaster.InitialSeedCountに接続。従来定義だけで未使用だった値)
- [x] もじの庭(HomeWorldView・PetToken): 所持文字をテキスト表示し、数秒おきにランダムな位置へゆっくり移動(Update不使用、UniTaskのDelayベースで位置更新)。タップで詳細画面
- [x] PetDetailView: レベル・経験値・満腹度・生産量・研究状況を表示。「エサをあげる」「研究する」「研究中止」ボタン
- [x] ResearchSelectView: 対象文字が研究可能な単語一覧を表示し、タップで研究開始
- [x] ShopView: 商品一覧・所持数・購入ボタン
- [x] Toast通知: 研究完了(新しいことば！)、レベルアップ、放置報酬を画面上部に一時表示
- [x] InventoryView(持ち物)を追加: MVP完成条件監査で発覚した欠落を修正。ショップで「ことばのたね」を買っても使うUIが無く、文字を一切取得できなかったため追加。Seedタイプのアイテムのみ「使う」ボタンを表示

成果物: 遊びやすくなる(達成、要Editor動作確認)

## Phase8の設計判断・保留事項

- これでゲームのコアループ(ことばのたねで文字入手→もじの庭で表示→タップして研究→ショップでエサ購入→エサで満腹度回復→施設強化)がUIから一通り操作可能になった。
- 文字の移動はMonoBehaviour.Update()を使わず、UniTaskの`Delay`ループで数秒おきに新しい目標位置を決めて緩やかに移動する方式(docsの「Updateは使用せず、一定間隔で移動先のみ更新する」の解釈)。
- 庭のスクロール(施設拡張に応じて庭が広がる仕様)は未実装。現状は画面内に収まる範囲でのみ移動する。
- 研究所・図書館個別ボタン、設定画面、放置報酬ポップアップ演出、レベルアップ時の「小さく跳ねる」演出、頭上の「！」表示は未実装(Phase9以降または追加polishで対応)。

---

# Phase9: バランス調整

- [ ] GameBalanceMaster等のCSV数値調整(実プレイのフィードバック待ち。AIが根拠なく数値だけ変更するのは避ける)

成果物: 正式リリース可能

## MVP完成条件との照合(06_DevelopmentRoadmap.md)

- [x] ゲーム開始できる
- [x] 文字を取得できる(Phase8後半でInventoryViewを追加して修正。それまでは購入したことばのたねを使う手段がなかった)
- [x] 文字を育成できる
- [x] 単語を登録できる(候補一覧からの選択、読み仮名の自由入力の両方に対応、2026-07-19)
- [x] 研究できる
- [x] 放置報酬を受け取れる(Toast通知あり)
- [x] ショップが利用できる
- [x] 図鑑が動作する
- [x] 施設を強化できる
- [x] セーブ・ロードできる

上記10項目のうち、AIによる机上レビューでは全て満たしている。ただし実機(Unity Editor)での動作確認は未実施のため、実際に満たされているかは要確認。

## 未実装機能を追加実装(2026-07-19)

保留していた4機能をすべて実装した。

### 図書館のカテゴリ解放
`CategoryMaster.csv`(カテゴリ→必要図書館レベル)を新設。`ResearchSelectPresenter.GetCandidates()`/`TryStartResearchByReading()`が、候補の単語カテゴリを図書館レベルと照合してフィルタする。
現在WordMasterで使用中のカテゴリ(OTHER/PLANT/ANIMAL/FOOD/PLACE/OBJECT)はすべて必要Lv0〜1に設定し、図書館初期Lv1で解放済みの状態にしてあるため、既存の遊び心地は変えていない。未使用のVERB/ADJECTIVE/PERSONカテゴリはLv2〜4で段階解放される(将来の単語データ追加時に効いてくる)。

### ひらめきのしずく(ResearchBoost)
`ItemMaster.csv`に`DurationSeconds`列を追加。使用すると`PetSystem`にグローバルな研究速度バフ(倍率・期限UTC)がかかり、`SaveData`に永続化される(アプリを閉じても切れるまで有効)。「持ち物」画面から直接使用可能。
**重要な制約**: 研究時間は`StartResearch()`時点の速度で確定する設計のため、**このバフは使用後に新しく開始する研究にのみ効果があり、進行中の研究には遡って適用されない**。

### 単語の自由入力登録
研究選択画面(`ResearchSelectView`)に読み仮名入力欄を追加(`UiFactory.CreateInputField`を新設)。候補一覧に出ない単語でも、読みが一致し条件(所持文字・レベル・カテゴリ解放・重複)を満たせば直接研究開始できる。`WordSystem.FindByReading()`を追加。

### 設定画面
`SettingsView`を追加(`UiFactory.CreateSlider`を新設)。BGM/SE音量スライダーと画質切替(`QualitySettings`に連動)。音声再生の仕組み自体はまだ無いため、音量は値の保存のみで実際の音は鳴らない。

### 副次的な修正
- ヘッダーに5つ目のボタン(設定)を追加するにあたり、手動座標配置だと横幅が足りず重なるリスクがあったため`HorizontalLayoutGroup`による自動整列に変更(2段組みヘッダー)。
- ヘッダー背景パネルの`raycastTarget`を無効化。ヘッダーが伸びたことで、下に隠れたもじの庭の文字がタップできなくなる問題を予防。

## 辞書を本番用データに差し替え(2026-07-19)

WordMaster.csvを、サンプル38語からJMDict(EDRDG、CC BY-SA 4.0)由来の19,866語へ全面差し替えた(ユーザー指示: 段階導入ではなく最初から本番規模で生成)。

**データ取得元**: `scriptin/jmdict-simplified`の`jmdict-eng-common`版(GitHub Releasesから取得、common語22,617エントリ)。
**生成方法**: Node.jsスクリプトで(1)代表読み仮名をカタカナ→ひらがな変換、(2)ひらがな2〜20文字・読み重複除去でフィルタ、(3)`misc`タグ(arch/obs/rare/vulg/derog/sens/X)を含む語義を除外、(4)`field`/品詞タグからCategoryを自動分類(PERSONのみ約60語を読みキュレーションで補完。JMDictに人物名詞の分類タグが無いため)、(5)読み文字数から`Difficulty`/`RequiredLevel`/`ResearchTimeSeconds`を算出。詳細は`docs/IMPLEMENTED_SPEC.md`3.1節。

**確認済み**:
- 46文字全てカバー(「を」を含む単語も収録、旧来の既知の未対応事項を解消)
- `RequiredLevel`最大33、`ResearchTimeSeconds`最大8,670秒(約2.4時間) — いずれも`MaxPetLevel`(100)の範囲内
- CSVに要エスケープ文字(カンマ・ダブルクォート)を含む行は無し

**波及効果**: VERB(5,004語)/ADJECTIVE(2,794語)/PERSON(63語)に初めて実データが入ったため、`CategoryMaster`による図書館レベル別カテゴリ解放(既存機能、これまでは実質不使用)が実際にプレイに影響するようになった。図書館Lv1の初期状態ではこの3カテゴリの単語は研究候補に出ない。

**未対応**: JMDict/jmdict-simplifiedはCC BY-SA 4.0のためクレジット表記が必要だが、ゲーム内にライセンス表示の仕組みが無い。別途対応が必要。

**要作業**: Unity Editorで`Tools > Import MasterData`を実行してCSVをScriptableObjectへ反映すること。

## 手書き文字作成機能を追加(2026-07-19)

新しい文字がことばのたねで生まれた時、プレイヤー自身がその文字を手書きで描けるようにした(愛着形成が目的、認識・採点は無し)。

- `HandwritingSystem`(新規): 描いた絵を`Application.persistentDataPath/Handwriting/{characterId}.png`にPNGとして保存(SaveData本体には含めない。JSON肥大化を避けるため)。`PetData.HasHandwriting`で有無だけ管理
- `DrawingCanvas`(新規UIコンポーネント): `RawImage`+`Texture2D`へドラッグでブラシ描画するだけの単純な自由描画キャンバス。ストローク認識・採点は無し
- `HandwritingView`(新規): 背景に薄くその文字のグリフをガイド表示しつつ描ける全画面モーダル。「消す」「できた！」ボタンのみ
- `OnPetUnlocked`発火時に`HomeUIRoot`が自動で`HandwritingView`を開く。`PetDetailView`に「描き直す」ボタンも追加し、後からいつでも描き直せる
- `PetToken`(庭の文字トークン)は、手書きが保存されていればプレーンテキストの代わりにその手書き画像を表示。`OnHandwritingSaved`イベントを購読し、描き直した際にその場で表示を更新

**未対応**: 図鑑画面(`DictionaryView`)ではまだプレーンテキスト表示のまま(要望があれば追加対応)。

## 研究を完全自動化・手動選択UIを撤去(2026-07-19)

ユーザー要望「研究は自分で勝手に行うように、こちらから研究する言葉を選ぶのではなく」に対応。手動選択を残すか完全自動かを確認したところ「基本は自動で」との回答だったため、手動選択UIを完全に撤去した。

- `ResearchSelectView`/`ResearchSelectPresenter`を削除(候補一覧タップ選択・読み仮名自由入力検索の両方を含む)
- `ResearchSystem`に`FacilitySystem`依存を追加(旧`ResearchSelectPresenter`が持っていた図書館カテゴリ解放判定ロジックを`ResearchSystem`側に統合)
- `ResearchSystem.AutoStartResearchForIdleCharacters()`(新規): 研究中でなく満腹度>0の解放済みキャラ全員について、候補(所持文字・レベル・カテゴリ解放・重複除外を満たすもの)からランダムに1つ選び自動着手。`UpdateResearch()`の末尾(GameTickerから5秒おき、および`IdleSystem`のオフライン進行計算経由)で毎回呼ばれる
- 追加要望「空腹状態(満腹度0)だと研究ができない」にも対応: `CanStartResearch`/`StartResearch`は満腹度0以下を拒否し、`UpdateResearch()`は研究中のキャラの満腹度が0以下になった時点でその研究を`CancelResearch`する(他の満腹度効果と異なり、生産/研究速度のような緩やかな乗率ではなく即時中断)
- `PetDetailView`から「研究する」「研究中止」ボタンを削除。「エサをあげる」「描き直す」「閉じる」のみに。研究状況は引き続き表示され、非研究中は満腹度に応じて「つぎの研究をさがしています…」または「お腹が空いていて研究できません」を表示

## 頭上ステータスアイコンを追加(2026-07-19)

もじの庭のキャラ頭上に、状態を表す絵文字アイコンをバッジ表示(優先度: 満腹度0=🍖 > 研究中=✍ > 通常時は非表示)。

**フォントの技術的な壁**: Noto Sans JP(日本語本文用)には絵文字グリフが存在せず、そのままだと🍖/✍がtofu表示になる。カラー絵文字フォントはTMPの通常SDFレンダリングでは描画できないため、代わりに**モノクロ版のNoto Emoji**(SDFレンダリング可能)を追加で埋め込み、日本語フォントアセットの`fallbackFontAssetTable`に設定した。これによりNoto Sans JPに無い文字(絵文字)は自動的にNoto Emoji側から探される。

- `Assets/Fonts/NotoEmoji-Regular.ttf`(OFL)を追加
- `FontAssetGenerator.cs`を拡張し、日本語フォントアセットに加えて絵文字フォントアセットも生成、前者のフォールバックに後者を設定するように変更(`Tools > Generate Japanese Font Asset`は変わらず1回の実行でどちらも面倒を見る)
- `✍️`(VS16付き)ではなく`✍`(素のU+270D)を使用。バリエーションセレクタ自体はNoto Emojiに可視グリフが無く、変な四角が出るリスクを避けるため
- `PetToken`に状態アイコンを追加。`OnPetFed`/`OnResearchStarted`/`OnResearchCompleted`/`OnResearchCanceled`イベントで即時更新に加え、満腹度は減衰し続けて専用イベントが無いため5秒おきのポーリングでも更新

**要作業**: Unity Editorで`Tools > Generate Japanese Font Asset`を再実行してNoto Emojiフォールバックを反映すること。

## 図鑑を50件ページング化(2026-07-19)

図鑑を開くと約2万語ぶんのUI行を一度に生成しており、開くのに「えぐい時間」がかかる状態だった。`DictionaryPresenter`を、全件を毎回`DictionaryRowData`化する方式から**現在ページの50件だけをその都度切り出す**方式に変更(`GetPageCount()`/`GetRows(page)`)。`DictionaryView`に「＜前へ」「次へ＞」ボタンとページ数表示(例: 1 / 398)を追加。

## 図鑑に五十音行タブを追加(2026-07-20)

50件ページングに加えて、あ行・か行…のようにタブで絞り込めるようにした。

- `DictionaryPresenter.RowLabels`: `すべて/あ/か/さ/た/な/は/ま/や/ら/わ`の11タブ
- 読みの1文字目で行を判定(`RowMembers`)。濁音(が行→か行、ざ行→さ行等)・半濁音(ぱ行→は行)・小書き文字(ぁぃぅぇぉ→あ行、っ→た行、ゃゅょ→や行、ゎ→わ行)は対応する基本行にまとめて分類する、辞書の五十音順ソートと同じ考え方
- タブ切り替え時はページを0にリセット。選択中のタブボタンは`interactable=false`にして押せない=選択中、と視覚的に示す簡易実装
- フィルタ後の件数に応じて`GetPageCount(rowIndex)`も連動

## 残っている既知の未対応

- 庭のスクロール拡張、レベルアップ演出、研究完了時の頭上「！」表示等の見た目の作り込み
- ことばのたね初回引き直しのチュートリアルUX
- JMDict由来データのクレジット表記(ライセンス表示画面)が未実装
