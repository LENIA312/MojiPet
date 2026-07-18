# 07_AI_IMPLEMENTATION_RULE.md

Version: 1.0

---

# 目的

本ドキュメントはAIによる実装品質を統一するためのルールを定義する。

すべての実装は本ドキュメントを優先して従うこと。

---

# 基本方針

最優先事項

- 保守性
- 可読性
- 拡張性
- シンプルさ

短いコードを書くことより、
理解しやすいコードを書くことを優先する。

---

# 開発環境

Engine

Unity 2022 LTS

Language

C#

IDE

Visual Studio
Rider

---

# 使用ライブラリ

必須

- UniTask
- TextMeshPro
- Addressables

使用可能

- Newtonsoft.Json

追加ライブラリを導入する場合は理由を明確にすること。

---

# 禁止事項

ゲームロジックをMonoBehaviourへ書かない。

Updateを多用しない。

FindObjectOfTypeを使用しない。

Resources.Loadを使用しない。

マジックナンバーを書かない。

MasterDataを書き換えない。

Singletonを乱用しない。

GodClassを作らない。

#regionでコードを隠さない。

コメントでコードを説明しない。
コード自体が説明になる設計を行う。

---

# 推奨事項

小さなクラスを作る。

責務を一つにする。

早期returnを使う。

例外は握り潰さない。

命名を分かりやすくする。

インターフェースを適切に利用する。

データ駆動設計を優先する。

---

# フォルダ構成

Assets/

Scripts/

Scenes/

Prefabs/

MasterData/

Addressables/

StreamingAssets/

Resources/

UI/

---

Scripts

Scripts/

Core/

Managers/

Systems/

Models/

Master/

Save/

UI/

Utility/

Extensions/

---

# 命名規則

クラス

PascalCase

例

GameManager

PetSystem

ResearchSystem

---

変数

camelCase

例

currentLevel

researchTime

---

定数

PascalCase

例

MaxLevel

MaxOfflineHour

---

enum

PascalCase

例

ItemType

FacilityType

---

privateフィールド

先頭に_

例

_currentExp

_petData

---

# ScriptableObject

MasterDataのみ利用する。

ゲーム中の状態は保持しない。

ReadOnlyとして扱う。

---

# SaveData

JSON保存。

MasterDataは保存しない。

IDのみ保存する。

---

# 非同期

Coroutineは禁止。

UniTaskを使用する。

時間待ちは

UniTask.Delay

を使用する。

---

# 時間管理

DateTimeを利用する。

Time.timeには依存しない。

放置時間はUTC基準で計算する。

---

# イベント

イベント駆動を基本とする。

例

OnLevelUp

OnResearchFinished

OnMoneyChanged

OnItemUsed

---

# UI

UIはロジックを持たない。

表示のみ担当する。

ゲームロジックはSystemへ実装する。

---

# シーン

Sceneは最小限。

Boot

Title

Main

のみ。

画面切替はUIのみ。

Scene遷移は極力行わない。

---

# データ取得

MasterManager経由で取得する。

CSVを直接読むコードを書かない。

---

# ログ

Debug.Logは開発時のみ。

リリース時には不要なログを削除する。

---

# エラー処理

nullを許容しない。

入力チェックを行う。

例外はログへ記録する。

---

# コメント

コメントより命名で説明する。

TODOは残さない。

FIXMEは禁止。

---

# パフォーマンス

GCAllocを減らす。

LINQを乱用しない。

毎フレームnewしない。

文字列連結を多用しない。

必要ならObjectPoolを利用する。

---

# テスト

各Systemは単体テスト可能な構造とする。

UI依存を避ける。

---

# AIへの要求

実装前に既存コードを確認すること。

重複クラスを作らないこと。

既存設計を壊さないこと。

責務を追加する場合は既存クラスを肥大化させず、新しいクラスを作成すること。

保守性を優先すること。

---

# 完了条件

以下を満たした時点で実装完了とする。

- コンパイルエラーなし
- Warningなし
- NullReferenceExceptionなし
- セーブ互換性維持
- GameDesign.md準拠
- MasterData.md準拠
- SaveDataSpecification.md準拠

---

# 最終目標

AIが長期間開発を継続しても、
設計が崩壊しないプロジェクトを構築すること。

機能追加ではなく、
保守性と拡張性を最優先とする。