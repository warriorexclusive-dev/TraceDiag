# TraceDiag

> Языки: [English](../en/README.md) · [中文](../cn/README.md) · [Français](../fr/README.md) ·
> [Deutsch](../de/README.md) · [Русский](../ru/README.md) · [日本語](../jp/README.md)

一款用于诊断到远程主机路由的实用工具。它执行 ICMP 路由跟踪、解析节点名称、测量丢包率，并检查路由对不同数据包大小的稳定性。规范字符串位于 `Strings.cs`，译文则位于可执行文件旁的 `localizations` 文件夹中的 JSON 文件里。业务逻辑被拆分为若干小型静态模块。

通用逻辑位于 `TraceDiag.Core`（`netstandard2.0`）中，不依赖于 UI。在其之上运行着两个宿主：WinForms（`net48`）和控制台（`net48` 与 `net10.0`）。借助控制台，可以在没有图形界面的 Windows Server 2008 R2 上以及在 Linux（Ubuntu 22.04+）上使用诊断功能。

## 功能特性

- 路由跟踪：TTL 从 `1` 到 `255`（IP TTL 允许的最大值），在到达目标或达到跳数限制时停止。
- 主机发现被拆分到独立的 `HostDiscovery` 模型中：按顺序探测 TTL，最多 `3` 轮，数据包为 `64` 字节，超时为 `5` 秒。当启用«不分片»模式时，会开启 `DontFragment` 标志。没有响应的 TTL 会被赋予 `无响应` 状态，并在检查时跳过。
- 丢包延迟通过右侧的«丢包延迟，毫秒»字段设置，默认值为 `1000` 毫秒。响应会被立即处理，不作等待；在第一个数据包丢失之后，对该节点的所有后续检查都会带上延迟，以免因设备针对 ICMP flood 的防护而造成误判丢包。值 `0` 会关闭延迟。
- 对每个发现的节点执行反向 DNS 查询，并防止响应过慢。
- 数据包大小模式：
  - **«动态数据包大小»**复选框已勾选——`64 → 128 → … → 最大值`；
  - 复选框未勾选——只使用输入框中的最大值（`64–65500` 字节）。
  - 所有大小都会先在当前节点上检查，然后队列才转到下一个节点；
  - 切换大小时，节点计数器和 RTT 采样会保留并继续累积。
- **«不分片 (DF)»**复选框默认未勾选。启用后，最大数据包大小被限制为 `1024` 字节，动态大小会被取消并禁用，而路由发现、主检查和监控都会使用 `DontFragment`。
- 在主检查之前以及监控启动时各执行一次 DF 预检查：针对每个节点分别发送大小为 `64 → 128 → 256 → 1024 → 1400` 字节的探测包各一个。所有响应和错误都会作为附加信息输出到控制台和日志；表格和诊断不会改变。
- 探测包由带有 `DO_NOT_FRAGMENT` 触发器或普通触发器的独立任务创建。最终检查算法和监控算法位于相互独立的控制器中。
- 每个节点的可配置尝试次数：`1–1000`，默认 `100`——用于向提供商报告并测量丢包百分比。
- 复选框排列在输入面板左侧的一行中；右列更窄，面板本身占用的高度也更小。
- **«持续监控模式»**复选框：数据包大小只从字段读取一次，不使用尝试次数。发现路由后，每个节点由独立的计时器每秒轮询一次。诊断为 `ICMP 已禁用` 的节点不会被轮询，数据包大小受限也不会将节点排除在监控之外。通过 `Stop` 按钮停止。
- 在监控模式下，每个节点的数据都会更新到其已有的行中，不会为重复轮询创建新行。
- 统计历史记录限制为 `1 000 000` 个快照，输出缓冲区为 `10 000` 行：超出时删除最早的记录和行。日志达到 `4` MB 时会切换到新文件，之前的文件保持不变。日志写入和统计处理在独立的工作线程中进行，不会占用 ICMP 轮询的线程。
- 每个节点的精确统计：平均、最小、最大和最新 ping 值，已发送/已接收数据包数量，丢包百分比。平均值根据所有已保存的 RTT 测量值计算，而非近似值。
- 当丢包率达到 `100%` 时，节点会被赋予 `无响应` 状态；连续 `10` 次失败后，将停止对该节点的检查，并在后续数据包大小中跳过它。
- 丢包率在«状态»列中以百分比显示，原因则显示在单独的«诊断»列中。不会对之前的节点执行重复检查。
- 界面语言：`En`, `Cn`, `Fr`, `De`, `Ru`, `Jp`。默认为 `En`；选择保存在 `settings.json` 中。在 GUI 中可通过右侧面板的下拉列表切换语言，无需重启；在控制台中通过 `--language <en|cn|fr|de|ru|jp>` 选项切换。译文位于可执行文件旁的 `localizations/*.json` 中，可直接编辑而无需重新编译；未知键会保留英文。文件日志始终为英文。
- 统计表格位于日志上方，横跨整个窗口宽度；两者之间有一个可拖动的水平分隔条。其位置与列宽分开保存。单独的«诊断»列会显示：`ICMP 已禁用`、`ICMP 大小限制`、`ICMP 防洪泛`，或在没有迹象时显示 `—`。诊断也会写入日志。
- 在右侧面板中可选择日志字号和丢包延迟，并有一个大号的 `Run`/`Stop` 按钮：检查进行中时，该按钮会停止当前运行。
- 全屏模式：`F11`——开启/关闭，`Esc`——退出。
- 默认使用深色主题，左侧面板中有切换开关；选择会保存在设置中。复选框保持为标准样式，颜色随主题变化；表格使用主题调色板，不可检查的节点在深色和浅色主题下均显示为灰色。
- 关闭窗口时，单独的 `ui-state.json` 文件会保存所有输入字段、复选框、表格列宽、分隔条的垂直位置（表格高度的百分比）以及窗口状态：按像素展开（`x, y, x2, y2`）或 maximized。启动时状态会被恢复；损坏的文件会被删除，程序以默认值启动。
- 启动参数 `reset`（例如 `TraceDiag.exe reset`）会在加载前强制删除已保存的界面状态。
- 自定义图标 `cable_24dp.ico`、文件日志记录以及将设置保存为 JSON。

## 控制台

```text
TraceDiag.Console run    --host <ip|domain> [--packet-size <n>] [--attempts <n>] [--loss-delay <ms>] [--dynamic] [--df] [--format plain|csv|json] [--language en|cn|fr|de|ru|jp]
TraceDiag.Console monitor --host <ip|domain> [--packet-size <n>] [--loss-delay <ms>] [--df] [--format plain|csv|json] [--language en|cn|fr|de|ru|jp]
TraceDiag.Console reset [--language en|cn|fr|de|ru|jp]
```

- 默认值：数据包大小 `1024`，尝试次数 `100`，延迟 `1000` 毫秒。
- `--format plain`——文本，`csv`——CSV 行，`json`——带 `kind` 字段的 JSON Lines。
- `--language` 会本地化帮助、错误、状态、检查进度信息和最终表格。文件日志仍为英文：规范文本及其参数与显示的译文相互分离；选项名称和命令不作翻译。控制台以 UTF-8 输出文本（在较旧的 Windows 终端中可能需要 `chcp 65001`）。
- 检查进度会逐行输出到控制台，并在写入文件日志的同时立即输出，不会一直累积到检查结束。同一行会通过 `AsyncLogWriter` 追加到文件中，因此取消或关闭时已经写入的行仍会保留。
- 完成后（包括 `Ctrl+C` 之后），最后会打印一张汇总表格（`Summary:` 的本地化对应形式），其中显示每个 TTL 的最新值。
- WinForms 应用程序的行为保持不变：表格直接在界面中更新。
- 如果选项名称拼写错误，控制台会提示最接近的正确名称，并打印完整的选项列表以供对照。
- `Ctrl+C` 会停止监控；退出代码：`0` 表示成功，`1` 表示错误，`2` 表示参数无效，`130` 表示取消。
- 控制台使用与 WinForms 应用程序相同的服务、日志和轮换机制。
- 在 Linux 上，普通用户执行 ICMP 可能需要 `net.ipv4.ping_group_range` 或能力 `CAP_NET_RAW`。

## 系统要求

- WinForms：Windows 7 SP1 / Server 2008 R2 SP1 或更高版本，.NET Framework 4.8。
- 控制台 `net48`：Windows 7 SP1 / Server 2008 R2 SP1 或更高版本，.NET Framework 4.8。
- 控制台 `net10.0`：Windows Server 2012+、Ubuntu 22.04/24.04/26.04 以及受 .NET 10 支持的其他系统；Linux 包为 self-contained，无需单独的运行时。
- 构建需要：.NET 10 SDK；MSI 需要 WiX 5（`dotnet tool restore`），deb 需要带有 `dpkg-deb` 的 WSL。在旧版 Windows 上安装 .NET Framework 4.8 需要管理员权限和来自替代仓库的本地安装程序（见下文）。

## 构建与运行

```powershell
dotnet build TraceDiag.slnx -c Release
.\TraceDiag\bin\Release\net48\TraceDiag.exe
.\TraceDiag.Console\bin\Release\net10.0\TraceDiag.Console.exe --help
```

## 测试

```powershell
dotnet test TraceDiag.slnx -c Release
```

**252 次测试运行**（174 次 WinForms + 在 `net48` 和 `net10.0` 上各 39 次控制台测试）覆盖数据包大小调度器、节点统计、ICMP 响应映射、配置、日志记录、界面状态保存、全部六种语言的本地化、控制台参数解析、输出格式、诊断模块，以及使用 ICMP、DNS 和日志器测试替身完成 `TraceDiagnosticsService` 的完整流程。

## 现成构建版本

最新版本：[Release 1.8.4 version](https://github.com/warriorexclusive-dev/TraceDiag/releases/tag/v1.8.4)

- [TraceDiag.exe](https://github.com/warriorexclusive-dev/TraceDiag/releases/download/v1.8.4/TraceDiag.exe)
- [TraceDiag-install.zip](https://github.com/warriorexclusive-dev/TraceDiag/releases/download/v1.8.4/TraceDiag-install.zip)

变更历史：[ReleaseNotes.md](ReleaseNotes.md)；译文位于 `docs/<语言>/`。

## 在全新电脑上安装

压缩包（per-user 安装和«解压即运行»）：

```powershell
cd installer
.\Publish-TraceDiag.ps1
```

该脚本将 WinForms 和 `net48` 控制台发布到 `installer\App`，将 self-contained 的 `net10.0` 控制台发布到 `installer\ConsoleLinux`（`linux-x64`）和 `installer\ConsoleLinux-arm64`，把 `localizations\` 复制到所有文件夹，并打包生成以下压缩包：

- `installer\TraceDiag-install.zip`——Windows：GUI + `net48` 控制台和安装程序；
- `installer\TraceDiag-console-linux-x64.tar.gz`——适用于 Linux/amd64 的控制台；
- `installer\TraceDiag-console-linux-arm64.tar.gz`——适用于 Linux/arm64 的控制台。

在 Windows 上，解压第一个压缩包并运行 `Install-TraceDiag.cmd`。安装程序会通过注册表检查 .NET Framework 4.8。如果未安装，请将 `dotNetFx48_Full.exe` 放在脚本旁边（发布脚本会将其加入压缩包），或传入 `-Framework48InstallerPath` / `-Framework48Url` 指向替代仓库。不会直接从 Microsoft 网站下载。安装程序会将文件复制到 `%LOCALAPPDATA%\Programs\TraceDiag`，并在«开始»菜单中创建快捷方式。

在 Linux 上，解压所需的 tar.gz 并运行 `bash install-console.sh`；脚本会将 self-contained 文件复制到 `~/.local/share/TraceDiag`，并在 `~/.local/bin` 中创建符号链接。无需单独下载 .NET 运行时。

系统级安装包（per-machine）：

```powershell
cd installer
dotnet tool restore
.\Build-TraceDiag-Packages.ps1
```

结果位于 `installer\dist`：`TraceDiag-<版本>-x86.msi`、`TraceDiag-<版本>-x64.msi`、`tracediag_<版本>_amd64.deb` 和 `tracediag_<版本>_arm64.deb`。MSI 包含相应位数的 GUI 和控制台、`localizations\` 以及«开始»菜单中的快捷方式；安装前会检查 .NET Framework 4.8。deb 包会将控制台安装到 `/usr/share/tracediag`，并通过 `/usr/bin` 创建 `tracediag` 命令。构建 deb 需要带有 `dpkg-deb` 的 WSL。

详情：[installer/README-Installer.md](../../installer/README-Installer.md)。已编译的文件（`installer\App`、`installer\ConsoleLinux*`、`installer\build`、`installer\dist`、压缩包）不会纳入 Git。

## 项目结构

```text
TraceDiag.Core/              netstandard2.0，与 UI 无关的业务逻辑
  Models/                    数据、探测任务及节点状态计算
  Services/                  ICMP、DNS、配置、日志记录
  Services/Abstractions/     控制器、任务和触发器的契约
  Services/Diagnostics/      路由、DF 预检查、节点检查及丢包诊断
  Services/Monitoring/       各节点独立计时器与监控启动
  Services/Logging/          文件轮换与后台日志写入
  Compatibility/             面向 netstandard2.0 和 net48 的 polyfill
  Localization/              JSON 译文读取与语言选择
  Strings.cs                 规范的用户界面与日志字符串
localizations/               en/cn/fr/de/ru/jp.json——界面译文
docs/                        en/cn/fr/de/ru/jp——六种语言的 README.md 和 ReleaseNotes.md
TraceDiag/                   net48，WinForms 宿主
  Ui/                        控件工厂、布局、启动流程与大小策略
TraceDiag.Console/           net48 + net10.0，控制台宿主
TraceDiag.Tests/             net48，WinForms 与核心测试
TraceDiag.Console.Tests/     net48 + net10.0，控制台与兼容性测试
installer/
  Install-TraceDiag.ps1      Windows 安装（per-user）
  install-console.sh         Linux 控制台安装
  Publish-TraceDiag.ps1      构建压缩包
  Build-TraceDiag-Packages.ps1  构建 MSI（x86/x64）和 deb（amd64/arm64）
  wix/                       MSI 源文件
  deb/                       在 WSL 中构建 deb 的脚本
.config/dotnet-tools.json    固定版本的 WiX 5
```

## 应用数据

- Windows：`%LOCALAPPDATA%\TraceDiag\`（`settings.json`、`ui-state.json`、`logs\`）。
- Linux：`~/.local/share/TraceDiag/`——相同的文件，在首次启动控制台时创建。
- 日志：`tracediag-yyyyMMdd.log`；达到 4 MB 时会创建 `tracediag-yyyyMMdd-1.log`、`tracediag-yyyyMMdd-2.log`，依此类推。

## 许可证

Apache License 2.0——参见 [LICENSE](../../LICENSE)。

由 Enaleven 于 2026 年使用 Big Pickle 创建
