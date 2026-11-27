# TraceDiag

> Языки: [English](../en/README.md) · [中文](../cn/README.md) · [Français](../fr/README.md) ·
> [Deutsch](../de/README.md) · [Русский](../ru/README.md) · [日本語](../jp/README.md)

リモートホストへのルートを診断するユーティリティです。ICMP トレースルートを収集し、ノード名を解決し、損失を測定し、さまざまなパケットサイズに対するルートの安定性を確認します。カノニカルな文字列は `Strings.cs` にあり、翻訳は実行ファイルの隣にある `localizations` フォルダーの JSON ファイルにあります。ビジネスロジックは小さな静的モジュールに分割されています。

共通ロジックは `TraceDiag.Core` (`netstandard2.0`) にあり、UI に依存しません。その上で 2 つのホストが動作します: WinForms (`net48`) とコンソール (`net48` および `net10.0`) です。コンソールを使用すると、グラフィカルインターフェイスのない Windows Server 2008 R2 や Linux (Ubuntu 22.04 以降) で診断を利用できます。

## 機能

- ルートトレース: TTL は `1` から `255` まで (IP TTL で許可される最大値)、宛先に到達した時点またはホップ数上限に達した時点で停止します。
- ホスト検出は独立したモデル `HostDiscovery` に分離されています: TTL は順番に問い合わせられ、最大 `3` パス、パケットは `64` バイト、タイムアウトは `5` 秒です。「フラグメントしない」モードが有効な場合、フラグ `DontFragment` がオンになります。応答のない TTL はステータス `応答なし` となり、検査時にスキップされます。
- パケット損失時の遅延は右側の「損失時遅延、ms」フィールドで設定し、既定値は `1000` ミリ秒です。応答は待機せずに処理されます。最初のパケット欠落以降、このノードの後続のすべての検査は、機器の ICMP フラッドに対する保護による誤った損失を防ぐために遅延を伴って実行されます。値 `0` は遅延を無効にします。
- 検出された各ノードに対して、応答が遅い場合に備えた保護機能付きの逆引き DNS を実行します。
- パケットサイズモード:
  - **「動的パケットサイズ」** チェックボックスがオン — `64 → 128 → … → 最大値`;
  - チェックボックスがオフ — 入力フィールドの最大サイズのみ (`64–65500` バイト)。
  - すべてのサイズが最初に現在のノードで検査され、その後キューが次のノードに進みます。
  - サイズを変更してもノードのカウンターと RTT サンプルは保持され、引き続き蓄積されます。
- **「フラグメントしない (DF)」** チェックボックスは既定でオフです。オンにすると、最大パケットサイズが `1024` バイトに制限され、動的サイズのチェックは外されて無効化され、ルート検出、本検査、モニタリングで `DontFragment` が使用されます。
- 本検査の前とモニタリング開始時に 1 回、DF 事前検査が実行されます: 各ノードに対して `64 → 128 → 256 → 1024 → 1400` バイトのプローブを 1 つずつ送信します。すべての応答とエラーは追加情報としてコンソールとログに出力されます。テーブルと診断は変更されません。
- プローブは、`DO_NOT_FRAGMENT` トリガーまたは通常のトリガーを持つ個別のジョブとして作成されます。最終検査アルゴリズムとモニタリングアルゴリズムは独立したコントローラーにあります。
- ノードごとの試行回数は設定可能で、`1–1000`、既定値は `100` です。プロバイダーへの報告と損失率の測定に使用します。
- チェックボックスは入力パネルの左側に 1 行で配置されています。右側の列は狭くなり、パネル自体の高さも小さくなりました。
- **「常時モニタリングモード」** チェックボックス: パケットサイズはフィールドから 1 回だけ取得され、試行回数は使用されません。ルート検出後、各ノードは個別のタイマーで 1 秒ごとに問い合わせられます。診断が `ICMP 無効` のノードは問い合わせられません。パケットサイズの制限があってもノードはモニタリングから除外されません。停止は `Stop` ボタンで行います。
- モニタリングモードでは、各ノードのデータは既存の行内で更新され、再問い合わせ用に新しい行は作成されません。
- 統計履歴は `1 000 000` スナップショットに、出力バッファーは `10 000` 行に制限されています。超過すると古いエントリと行が削除されます。ログは `4` MB に達すると新しいファイルに切り替わり、以前のファイルは変更されません。ログの書き込みと統計処理は別々のワーカーで実行され、ICMP 問い合わせのスレッドを占有しません。
- 各ノードの正確な統計: 平均、最小、最大、最新の ping、送信/受信パケット数、損失率です。平均値は近似値ではなく、保存されているすべての RTT 測定値から算出されます。
- 損失率が `100%` の場合、ノードはステータス `応答なし` となります。`10` 回連続で失敗するとそのノードの検査は停止し、以降のパケットサイズではスキップされます。
- 損失は「状態」列に割合として表示され、その理由は別の「診断」列に表示されます。前のノードに対する再検査は実行されません。
- インターフェイスの言語: `En`, `Cn`, `Fr`, `De`, `Ru`, `Jp`。既定値は `En` で、選択は `settings.json` に保存されます。GUI では右側のパネルのリストから再起動なしで言語を変更できます。コンソールでは `--language <en|cn|fr|de|ru|jp>` オプションで指定します。翻訳は実行ファイルの隣にある `localizations/*.json` にあり、再ビルドなしで編集できます。未知のキーは英語のままになります。ファイルログは常に英語です。
- 統計テーブルはログの上にウィンドウ幅いっぱいで配置され、その間にはドラッグ可能な水平スプリッターがあります。位置は列幅とは別に保存されます。別の「診断」列には、`ICMP 無効`、`ICMP サイズ制限`、`ICMP フラッド対策`、または兆候がない場合は `—` が表示されます。診断はログにも書き込まれます。
- 右側のパネルには、ログのフォントサイズと損失時遅延の選択、および大きな `Run`/`Stop` ボタンがあります。検査中はこのボタンで実行を停止できます。
- 全画面モード: `F11` でオン/オフ、`Esc` で終了します。
- 既定はダークテーマで、左側のパネルに切り替えスイッチがあります。選択は設定に保存されます。チェックボックスは標準のままで、テーマに合わせて色が変わります。テーブルはテーマのパレットを使用し、検査不可能なノードはダークテーマとライトテーマの両方で灰色に表示されます。
- ウィンドウを閉じると、別ファイル `ui-state.json` にすべての入力フィールド、チェックボックス、テーブルの列幅、スプリッターの垂直位置 (テーブル高さの割合)、およびウィンドウの状態 (ピクセル単位の展開 (`x, y, x2, y2`) または maximized) が保存されます。起動時に状態が復元されます。ファイルが破損している場合は削除され、プログラムは既定値で起動します。
- 起動キー `reset` (例: `TraceDiag.exe reset`) は、読み込み前に保存されたインターフェイスの状態を強制的に削除します。
- 独自アイコン `cable_24dp.ico`、ファイルログ、JSON への設定の保存を備えています。

## コンソール

```text
TraceDiag.Console run    --host <ip|domain> [--packet-size <n>] [--attempts <n>] [--loss-delay <ms>] [--dynamic] [--df] [--format plain|csv|json] [--language en|cn|fr|de|ru|jp]
TraceDiag.Console monitor --host <ip|domain> [--packet-size <n>] [--loss-delay <ms>] [--df] [--format plain|csv|json] [--language en|cn|fr|de|ru|jp]
TraceDiag.Console reset [--language en|cn|fr|de|ru|jp]
```

- 既定値: パケットサイズ `1024`、試行回数 `100`、遅延 `1000` ミリ秒。
- `--format plain` はテキスト、`csv` は CSV 行、`json` は `kind` フィールドを持つ JSON Lines です。
- `--language` はヘルプ、エラー、ステータス、検査の進行状況を示す文字列、および最終テーブルをローカライズします。ファイルログは英語のままです。カノニカルなテキストと引数は表示用の翻訳から分離されており、オプション名とコマンドは翻訳されません。コンソールは UTF-8 でテキストを出力します (古い Windows ターミナルでは `chcp 65001` が必要になる場合があります)。
- 検査の進行状況は、検査終了まで溜め込むことなく、ファイルログへの書き込みと同時に 1 行ずつコンソールに出力されます。同じ行が `AsyncLogWriter` 経由でファイルにも追記されるため、キャンセルや終了時に既に書き込まれた行は保持されます。
- 完了後 (`Ctrl+C` による場合も含む) には、各 TTL の最新値を示す最終テーブル (ローカライズされた `Summary:` に相当するもの) が最後に 1 つ出力されます。
- WinForms アプリケーションはこれまでどおり動作し、テーブルはインターフェイス内で直接更新されます。
- オプション名に誤字がある場合、コンソールは最も近い正しい名前を提案し、比較用にオプションの一覧全体を出力します。
- `Ctrl+C` はモニタリングを停止します。終了コード: `0` は成功、`1` はエラー、`2` は引数エラー、`130` はキャンセルです。
- コンソールは WinForms アプリケーションと同じサービス、ログ、ローテーションを使用します。
- Linux では、一般ユーザーが ICMP を使用するために `net.ipv4.ping_group_range` またはケーパビリティ `CAP_NET_RAW` が必要になる場合があります。

## 要件

- WinForms: Windows 7 SP1 / Server 2008 R2 SP1 以降、.NET Framework 4.8。
- コンソール `net48`: Windows 7 SP1 / Server 2008 R2 SP1 以降、.NET Framework 4.8。
- コンソール `net10.0`: Windows Server 2012 以降、Ubuntu 22.04/24.04/26.04 およびその他の対応 .NET 10 システム。Linux パッケージは自己完結型 (self-contained) で、別途 Runtime は不要です。
- ビルドには .NET 10 SDK、MSI には WiX 5 (`dotnet tool restore`)、deb には `dpkg-deb` を備えた WSL が必要です。レガシー Windows への .NET Framework 4.8 のインストールには管理者権限と、代替リポジトリからのローカルインストーラーが必要です (下記を参照)。

## ビルドと実行

```powershell
dotnet build TraceDiag.slnx -c Release
.\TraceDiag\bin\Release\net48\TraceDiag.exe
.\TraceDiag.Console\bin\Release\net10.0\TraceDiag.Console.exe --help
```

## テスト

```powershell
dotnet test TraceDiag.slnx -c Release
```

**252 回のテスト実行** (174 WinForms + `net48` と `net10.0` で各 39 件のコンソール) は、パケットサイズのスケジューラー、ノード統計、ICMP 応答のマッピング、構成、ログ、インターフェイス状態の保存、6 言語すべてのローカライゼーション、コンソール引数の解析、出力形式、診断モジュール、および ICMP、DNS、ロガーのテスト用フェイクを用いた `TraceDiagnosticsService` の完全なライフサイクルをカバーしています。

## ビルド済みパッケージ

最新リリース: [Release 1.8.4 version](https://github.com/warriorexclusive-dev/TraceDiag/releases/tag/v1.8.4)

- [TraceDiag.exe](https://github.com/warriorexclusive-dev/TraceDiag/releases/download/v1.8.4/TraceDiag.exe)
- [TraceDiag-install.zip](https://github.com/warriorexclusive-dev/TraceDiag/releases/download/v1.8.4/TraceDiag-install.zip)

変更履歴: [ReleaseNotes.md](ReleaseNotes.md); 翻訳は `docs/<言語>/` にあります。

## クリーンな PC へのインストール

アーカイブ (ユーザー単位のインストールと「展開して実行」):

```powershell
cd installer
.\Publish-TraceDiag.ps1
```

スクリプトは WinForms とコンソール `net48` を `installer\App` に、自己完結型コンソール `net10.0` を `installer\ConsoleLinux` (`linux-x64`) と `installer\ConsoleLinux-arm64` に発行し、`localizations\` をすべてのフォルダーにコピーして、次のアーカイブを作成します:

- `installer\TraceDiag-install.zip` — Windows: GUI + コンソール `net48` とインストーラー;
- `installer\TraceDiag-console-linux-x64.tar.gz` — Linux/amd64 用コンソール;
- `installer\TraceDiag-console-linux-arm64.tar.gz` — Linux/arm64 用コンソール。

Windows では最初のアーカイブを展開し、`Install-TraceDiag.cmd` を実行します。インストーラーはレジストリで .NET Framework 4.8 を確認します。見つからない場合は、`dotNetFx48_Full.exe` をスクリプトの隣に配置する (発行スクリプトがアーカイブに追加します) か、代替リポジトリ用に `-Framework48InstallerPath` / `-Framework48Url` を渡してください。Microsoft サイトからの直接ダウンロードは行いません。インストーラーはファイルを `%LOCALAPPDATA%\Programs\TraceDiag` にコピーし、「スタート」メニューにショートカットを作成します。

Linux では必要な tar.gz を展開し、`bash install-console.sh` を実行します。スクリプトは自己完結型ファイルを `~/.local/share/TraceDiag` にコピーし、`~/.local/bin` にシンボリックリンクを作成します。別途 .NET Runtime をダウンロードする必要はありません。

システムパッケージ (マシン単位):

```powershell
cd installer
dotnet tool restore
.\Build-TraceDiag-Packages.ps1
```

生成物は `installer\dist` にあります: `TraceDiag-<バージョン>-x86.msi`、`TraceDiag-<バージョン>-x64.msi`、`tracediag_<バージョン>_amd64.deb`、`tracediag_<バージョン>_arm64.deb`。MSI には対応するビット数の GUI とコンソール、`localizations\`、および「スタート」メニューのショートカットが含まれます。インストール前に .NET Framework 4.8 が確認されます。deb パッケージはコンソールを `/usr/share/tracediag` にインストールし、`/usr/bin` 経由で `tracediag` コマンドを作成します。deb には `dpkg-deb` を備えた WSL が必要です。

詳細: [installer/README-Installer.md](../../installer/README-Installer.md)。
コンパイル済みファイル (`installer\App`、`installer\ConsoleLinux*`、`installer\build`、`installer\dist`、アーカイブ) は Git には含まれません。

## プロジェクト構造

```text
TraceDiag.Core/              netstandard2.0、UI に依存しないビジネスロジック
  Models/                    データ、プローブジョブ、ノード状態の計算
  Services/                  ICMP、DNS、構成、ログ記録
  Services/Abstractions/     コントローラー、ジョブ、トリガーの契約
  Services/Diagnostics/      ルート、DF 事前チェック、ノード検査、損失診断
  Services/Monitoring/       ノードごとの独立タイマーと監視の起動
  Services/Logging/          ファイルのローテーションとバックグラウンド書き込み
  Compatibility/             netstandard2.0 と net48 用のポリフィル
  Localization/              JSON 翻訳の読み込みと言語選択
  Strings.cs                 カノニカルなユーザー文字列とログ文字列
localizations/               en/cn/fr/de/ru/jp.json — UI の翻訳
docs/                        en/cn/fr/de/ru/jp — 6 言語の README.md と ReleaseNotes.md
TraceDiag/                   net48、WinForms ホスト
  Ui/                        コントロールのファクトリ、レイアウト、起動スレッド、サイズ方針
TraceDiag.Console/           net48 + net10.0、コンソールホスト
TraceDiag.Tests/             net48、WinForms とコアのテスト
TraceDiag.Console.Tests/     net48 + net10.0、コンソールと互換性のテスト
installer/
  Install-TraceDiag.ps1      Windows へのインストール (per-user)
  install-console.sh         Linux へのコンソールインストール
  Publish-TraceDiag.ps1      アーカイブのビルド
  Build-TraceDiag-Packages.ps1  MSI (x86/x64) と deb (amd64/arm64) のビルド
  wix/                       MSI のソース
  deb/                       WSL で deb をビルドするスクリプト
.config/dotnet-tools.json    固定された WiX 5 のバージョン
```

## アプリケーションデータ

- Windows: `%LOCALAPPDATA%\TraceDiag\` (`settings.json`、`ui-state.json`、`logs\`)。
- Linux: `~/.local/share/TraceDiag/` — 同じファイルで、コンソールの初回起動時に作成されます。
- ログ: `tracediag-yyyyMMdd.log`。4 MB に達すると `tracediag-yyyyMMdd-1.log`、`tracediag-yyyyMMdd-2.log` などが作成されます。

## ライセンス

Apache License 2.0 — [LICENSE](../../LICENSE) を参照してください。

2026 年に Enaleven が Big Pickle を使用して作成
