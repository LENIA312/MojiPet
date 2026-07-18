# SceneSpecification

Version: 1.0

---

# 概要

本ドキュメントは「もじぺっと」のシーン構成と遷移を定義する。

Version1ではシーン数を最小限に抑え、ゲーム全体を1つのメインシーンで構成する。

---

# シーン構成

```
Boot
    ↓
Home
```

Version1ではこの2シーンのみ使用する。

---

# Boot

## 役割

ゲーム起動専用シーン。

ユーザーはBootシーンを操作しない。

---

## 責務

・アプリ初期化

・設定読込

・SaveData読込

・MasterData初期化

・Addressables初期化

・EventBus生成

・GameManager生成

・各System生成

・Homeシーン読込

---

## 配置オブジェクト

```
Bootstrap

GameManager

EventBus

SaveSystem

AudioListener
```

UIは配置しない。

---

# Home

## 役割

ゲーム本編。

Version1ではすべてのゲームプレイをこのシーンで行う。

---

## 責務

・ホーム表示

・家具表示

・文字表示

・研究

・辞書

・ショップ

・設定

・放置報酬

・チュートリアル

---

## 画面構成

```
Canvas

HomeRoot

Header

World

Windows

Popup

Toast
```

---

# World

ホーム画面。

自由スクロール可能。

表示するもの

```
文字

家具

背景

エフェクト
```

Worldは固定UIではない。

---

# Header

固定UI。

表示

```
言霊

メニュー

設定
```

スクロールしない。

---

# Windows

モーダル画面。

```
Dictionary

Research

Shop

Settings
```

同時に複数開かない。

---

# Popup

確認ダイアログ。

例

```
購入確認

リセット確認
```

---

# Toast

短時間表示。

例

```
研究完了

家具取得

レベルアップ
```

---

# シーン遷移

Version1

```
Boot

↓

Home
```

Homeから別シーンへ遷移しない。

---

# ローディング

Bootのみ。

Homeではローディング画面を表示しない。

---

# シーンロード方式

Boot

```
Single
```

Home

```
Single
```

AdditiveはVersion1では使用しない。

---

# シーン初期化

Boot

```
MasterData

↓

Save

↓

Systems

↓

Home
```

---

Home

```
UI生成

↓

ホーム生成

↓

イベント購読

↓

ゲーム開始
```

---

# 終了処理

アプリ終了時

```
AutoSave()

↓

Save()

↓

終了
```

---

# シーン依存

Bootのみが

```
GameManager
```

を生成する。

Homeでは生成してはならない。

---

# Addressables

Homeで使用する

Prefab

Sprite

Audio

はAddressables経由でロードする。

---

# DontDestroyOnLoad

以下のみ許可。

```
GameManager

EventBus

AudioManager
```

その他は禁止。

---

# シーン命名

```
Boot

Home
```

PascalCase。

番号は付けない。

---

# Build Settings

```
0 Boot

1 Home
```

Bootを最初に配置する。

---

# Version2以降

追加候補

```
Title

Tutorial

Debug

Benchmark
```

Version1では追加しない。

---

# 受け入れ条件

・BootからHomeへ遷移できる

・Homeのみでゲームを遊べる

・シーン遷移時にSaveDataが維持される

・DontDestroyOnLoadが最小限である

・Addressablesでリソースをロードできる

・AIがシーン構成を迷わない

---