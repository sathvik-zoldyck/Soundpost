<div align="center">

<img src="assets/logo.svg" alt="Soundpost" width="120" />

# Soundpost

### ひとつの中心に、すべての音を。

**Windows オーディオのための単一コンソール。出力デバイスを切り替え、あらゆるアプリをミックスし、そして音を「見る」——すべてをひとつの場所で。ローカル優先、アカウント不要、テレメトリなし。**

[![CI](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml/badge.svg)](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml)
[![License: GPLv3](https://img.shields.io/badge/license-GPLv3-blue.svg)](LICENSE)
[![Windows 10 / 11](https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4?logo=windows&logoColor=white)](#入手)
[![.NET 9](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet&logoColor=white)](#)
[![Stars](https://img.shields.io/github/stars/sathvik-zoldyck/Soundpost?style=social)](https://github.com/sathvik-zoldyck/Soundpost/stargazers)

**[ロードマップ](ROADMAP.md) · [アーキテクチャ](ARCHITECTURE.md) · [プラグイン SDK](PLUGIN_SDK.md) · [貢献する](CONTRIBUTING.md) · [ディスカッション](../../discussions)**

[English](README.md) · [简体中文](README.zh.md) · [Español](README.es.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · **日本語** · [Русский](README.ru.md) · [한국어](README.ko.md)

<br/>

<img src="assets/media/dashboard.png" alt="Soundpost のダッシュボード：マスター音量ダイヤル、再生デバイス切り替え、アプリ別ミキサー、ライブ出力メーター" width="880" />

</div>

---

Windows では、音量フライアウト、デバイスメニュー、サウンドコントロールパネル、そして互いに連携しないサードパーティ製ツールに、オーディオがばらばらに散らばっています——しかもどれもあなたの意図を覚えていません。**Soundpost はその欠けていた中心です。** あらゆるデバイスとアプリがひとつのコンソールに集まり、切り替え・ミックス・ルーティングを行えば、音は正しい出力へ届きます。ただ聴きたいときは、ビジュアライザーが再生中の音を「観る価値のあるもの」に変えます。

## 機能

- **即時デバイス切り替え。** 既定の出力・入力をワンクリックで変更。
- **アプリ別ミキサー。** 音を出すすべてのアプリに音量・ミュート・ライブメーター。
- **ライブ出力メーター。** マスターとアプリ別のピークメーター、本格的なバリスティクス付き。
- **ビジュアライザー。** 7 つのライブスタイル——Ribbon、Aurora、Spectrum、Radial、Oscilloscope、Cymatics、カスタム画像モード——が 60 fps で音に反応。感度・スムージング・グロー・パレットを調整可能。
- **全画面オーバーレイ。** ビジュアライザーを音楽動画の上に重ねて表示。背景はソリッド・減光・完全透過から選べます。
- **クイックパネル。** 会議中のちょっとした操作を、フルコンソールを開かずにトレイのコンパクトなフライアウトから。
- **4 つのテーマ。** Indigo、Black & Red、Rich Gold、Cherry Blossom——設定からライブで切り替え。
- **ローカルでプライベート。** アカウントなし、クラウドなし、テレメトリなし。すべてはあなたのマシンの中に。

## スクリーンショット

<div align="center">

<img src="assets/media/themes.png" alt="4 つのテーマの Soundpost：Indigo、Black and Red、Rich Gold、Cherry Blossom" width="880" />

<sub><b>4 つのテーマをライブ切り替え。</b> Indigo、Black &amp; Red、Rich Gold、Cherry Blossom。</sub>

<br/><br/>

<img src="assets/media/quick-panel.png" alt="マスター音量・出力切り替え・アプリ別コントロールを備えたクイックパネルのトレイフライアウト" width="320" />

<sub><b>クイックパネル。</b> マスター音量、出力切り替え、アプリ別ミュート——トレイから直接。</sub>

</div>

## 入手

Soundpost は **Windows 10 と 11** 向けです。公開後は [Releases](../../releases) からビルドを入手するか、ソースからビルドします。

```bash
git clone https://github.com/sathvik-zoldyck/Soundpost.git
cd Soundpost
dotnet run --project src/Soundpost.App
```

[.NET 9 SDK](https://dotnet.microsoft.com/download) が必要です。リリースワークフローは単一ファイルの自己完結型 `Soundpost.exe`（.NET のインストール不要）を生成します。

## 仕組み

あらゆるアプリとデバイスがひとつの中心に集まり、ルーティング・ミックス・自動化を行えば、正しい出力へ届きます。単一の Core Audio 層が Windows の COM API を包み込み、アプリの他の部分がそれらに直接触れないようにしています。これによりコンソールは軽快さを保ち、オーディオ処理は分離されテスト可能になります。

## 拡張する

Soundpost は拡張されることを前提に作られています。

- **ビジュアライザー。** スタイルは `IVisualizerRenderer` を実装する 1 つのクラスです——[visualizers/](visualizers/) を参照。書いて登録すれば、スタイルバーに現れます。
- **テーマ。** パレットは自己完結型の辞書で、新しいテーマは新しいファイルとスウォッチだけです。
- **プラグイン。** イベント駆動のプラグイン面はロードマップにあります——[PLUGIN_SDK.md](PLUGIN_SDK.md) を参照。

## ロードマップ

現在提供中：デバイス切り替え、アプリ別ミキサーとメーター、ビジュアライザー、トレイとクイックパネル、永続化、テーマ。次は：シーンとプロファイル、自動化層、アプリ別ルーティング、平易な言葉での診断。全体計画は [ROADMAP.md](ROADMAP.md) にあります。

## 貢献する

新しいビジュアライザーからバグ修正まで、貢献を歓迎します。[CONTRIBUTING.md](CONTRIBUTING.md) から始め、[issue](../../issues) を開くか、[ディスカッション](../../discussions) で挨拶してください。Soundpost が役に立ったら、スターを付けると他の人が見つけやすくなります。

## ライセンス

[GPLv3](LICENSE)。フリーかつオープンソースのソフトウェアです。
