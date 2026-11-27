# TraceDiag

> Языки: [English](../en/README.md) · [中文](../cn/README.md) · [Français](../fr/README.md) ·
> [Deutsch](../de/README.md) · [Русский](../ru/README.md) · [日本語](../jp/README.md)

A utility for diagnosing the route to a remote host. It collects ICMP traces, resolves node
names, measures loss, and checks how stable the route is across different packet sizes.
Canonical strings live in `Strings.cs`, and translations live in the JSON files of the
`localizations` folder next to the executable. Business logic is split into small static
modules.

Shared logic lives in `TraceDiag.Core` (`netstandard2.0`) and does not depend on the UI. Two
hosts are built on top of it: WinForms (`net48`) and the console (`net48` and `net10.0`). The
console makes it possible to use diagnostics on Windows Server 2008 R2 without a graphical
interface and on Linux (Ubuntu 22.04+).

## Features

- Route tracing: TTL from `1` to `255` (the maximum allowed for IP TTL), stopping when the
  destination is reached or the hop limit is reached.
- Host discovery is moved to a separate `HostDiscovery` model: TTLs are probed sequentially,
  up to `3` passes, a `64`-byte packet, a `5` s timeout. The `DontFragment` flag is turned on
  when the "Don't fragment" mode is enabled. TTLs with no response get the `no response`
  status and are skipped during checking.
- The delay on packet loss is configured by the "Loss delay, ms" field on the right,
  `1000` ms by default. Responses are processed without waiting; after the first missed
  packet, all subsequent checks of that node are performed with a delay so as not to cause
  false losses due to hardware protection against ICMP flood. A value of `0` disables the
  delay.
- Reverse DNS for every discovered node, with protection against slow responses.
- Packet size mode:
  - the **"Dynamic packet size"** checkbox is enabled — `64 → 128 → … → maximum`;
  - the checkbox is disabled — only the maximum size from the input field (`64–65500` bytes).
  - all sizes are checked on the current node first, then the queue moves on to the next one;
  - when the size changes, the node's counters and RTT samples are preserved and continue to
    accumulate.
- The **"Don't fragment (DF)"** checkbox is off by default. When enabled, the maximum packet
  size is limited to `1024` bytes, dynamic sizing is cleared and locked, and route discovery,
  the main check, and monitoring use `DontFragment`.
- Before the main check and once when monitoring starts, a DF pre-check is performed:
  one probe per node with sizes `64 → 128 → 256 → 1024 → 1400` bytes. All responses and
  errors go to the console and the log as additional information; the table and diagnoses do
  not change.
- Probes are created as separate jobs with a `DO_NOT_FRAGMENT` trigger or a regular trigger.
  The final-check algorithm and the monitoring algorithm live in independent controllers.
- Configurable number of attempts per node: `1–1000`, `100` by default — for a provider report
  and measuring the loss percentage.
- The checkboxes are placed in the left part of the input panel in a single row; the right
  column is narrower, and the panel itself takes up less height.
- The **"Continuous monitoring mode"** checkbox: the packet size is taken from the field once,
  and the number of attempts is not used. After route discovery, each node is polled by its own
  timer once per second. Nodes with the `ICMP disabled` diagnosis are not polled, and a packet
  size limit does not exclude a node from monitoring. Stopping is done with the `Stop` button.
- In monitoring mode, each node's data is updated in its existing row; no new rows are created
  for repeated polls.
- Statistics history is limited to `1 000 000` snapshots, and the output buffer to `10 000`
  lines: on overflow, the earliest entries and lines are removed. The log switches to a new
  file when it reaches `4` MB; the previous file remains unchanged. Log writing and statistics
  processing run in separate workers, without occupying ICMP polling threads.
- Accurate statistics for each node: average, minimum, maximum, and last ping, number of
  packets sent/received, and loss percentage. The average is calculated from all saved RTT
  measurements, not from approximate values.
- At `100%` loss, a node gets the `no response` status; after `10` consecutive failures,
  checking of the node stops, and it is skipped at subsequent packet sizes.
- Losses are shown in the "State" column as a percentage, and the reason in a separate
  "Diagnosis" column. Repeated checks of previous nodes are not performed.
- Interface languages: `En`, `Cn`, `Fr`, `De`, `Ru`, `Jp`. The default is `En`; the choice is
  stored in `settings.json`. In the GUI, the language is changed with a list in the right panel
  without a restart; in the console, with the `--language <en|cn|fr|de|ru|jp>` option.
  Translations live in `localizations/*.json` next to the exe and can be edited without
  rebuilding; unknown keys remain in English. The file log is always in English.
- The statistics table sits above the log and spans the full window width, with a draggable
  horizontal divider between them. Its position is saved separately from the column widths.
  A separate "Diagnosis" column shows `ICMP disabled`, `ICMP size truncation`,
  `ICMP anti-flood`, or `—` if there are no signs. The diagnosis is also written to the log.
- Selection of the log font size, the loss delay, and a large `Run`/`Stop` button in the right
  panel: during a check, the button stops the run.
- Full-screen mode: `F11` to turn on/off, `Esc` to exit.
- Dark theme by default with a toggle in the left panel; the choice is saved in the settings.
  Checkboxes remain standard and change colors along with the theme; the table uses the
  theme palette, and nodes that cannot be checked are gray in both dark and light themes.
- When the window is closed, a separate `ui-state.json` file saves all input fields,
  checkboxes, table column widths, the divider's vertical position (percentage of table
  height), and window state: expanded by pixels (`x, y, x2, y2`) or maximized. On startup the
  state is restored; a damaged file is deleted and the program starts with default values.
- The `reset` startup key (for example, `TraceDiag.exe reset`) forcibly removes saved UI
  state before loading.
- A custom `cable_24dp.ico` icon, file logging, and settings saved in JSON.

## Console

```text
TraceDiag.Console run    --host <ip|domain> [--packet-size <n>] [--attempts <n>] [--loss-delay <ms>] [--dynamic] [--df] [--format plain|csv|json] [--language en|cn|fr|de|ru|jp]
TraceDiag.Console monitor --host <ip|domain> [--packet-size <n>] [--loss-delay <ms>] [--df] [--format plain|csv|json] [--language en|cn|fr|de|ru|jp]
TraceDiag.Console reset [--language en|cn|fr|de|ru|jp]
```

- Default values: packet size `1024`, `100` attempts, delay `1000` ms.
- `--format plain` — text, `csv` — CSV lines, `json` — JSON Lines with a `kind` field.
- `--language` localizes help, errors, statuses, progress lines, and the final table. The file
  log remains in English: canonical text and arguments are separated from the displayed
  translation; option names and commands are not translated. The console outputs text in
  UTF-8 (older Windows terminals may require `chcp 65001`).
- Progress is printed to the console line by line, immediately as it is written to the file
  log, without accumulating until the end of the check. The file is appended with the same
  line via `AsyncLogWriter`, so lines already written are preserved on cancellation or close.
- After completion (including after `Ctrl+C`), one final table (the localized equivalent of
  `Summary:`) is printed last, with the latest values for each TTL.
- The WinForms application works as before: the table is updated directly in the UI.
- If an option name is misspelled, the console suggests the nearest valid name and prints the
  full list of options for comparison.
- `Ctrl+C` stops monitoring; exit codes: `0` success, `1` error, `2` invalid arguments,
  `130` cancellation.
- The console uses the same services, logs, and rotation as the WinForms application.
- On Linux, ICMP from a regular user may require `net.ipv4.ping_group_range` or the
  `CAP_NET_RAW` capability.

## Requirements

- WinForms: Windows 7 SP1 / Server 2008 R2 SP1 or newer, .NET Framework 4.8.
- Console `net48`: Windows 7 SP1 / Server 2008 R2 SP1 or newer, .NET Framework 4.8.
- Console `net10.0`: Windows Server 2012+, Ubuntu 22.04/24.04/26.04, and other supported
  .NET 10 systems; the Linux package is self-contained, no separate Runtime is needed.
- For building: .NET 10 SDK; for MSI — WiX 5 (`dotnet tool restore`), for deb — WSL with
  `dpkg-deb`. Installing .NET Framework 4.8 on legacy Windows requires administrator rights
  and a local installer from an alternative repository (see below).

## Build and Run

```powershell
dotnet build TraceDiag.slnx -c Release
.\TraceDiag\bin\Release\net48\TraceDiag.exe
.\TraceDiag.Console\bin\Release\net10.0\TraceDiag.Console.exe --help
```

## Tests

```powershell
dotnet test TraceDiag.slnx -c Release
```

**252 test runs** (174 WinForms + 39 console on `net48` and `net10.0`) cover the packet size
scheduler, node statistics, ICMP response mapping, configuration, logging, UI state
persistence, localization in all six languages, console argument parsing, output formats,
diagnostic modules, and the full `TraceDiagnosticsService` cycle with test fakes for
ICMP, DNS, and the logger.

## Prebuilt Builds

Latest release: [Release 1.8.4 version](https://github.com/warriorexclusive-dev/TraceDiag/releases/tag/v1.8.4)

- [TraceDiag.exe](https://github.com/warriorexclusive-dev/TraceDiag/releases/download/v1.8.4/TraceDiag.exe)
- [TraceDiag-install.zip](https://github.com/warriorexclusive-dev/TraceDiag/releases/download/v1.8.4/TraceDiag-install.zip)

Changelog: [ReleaseNotes.md](ReleaseNotes.md); translations are in `docs/<language>/`.

## Installation on a Clean PC

Archives (per-user installation and "extract and run"):

```powershell
cd installer
.\Publish-TraceDiag.ps1
```

The script publishes WinForms and the `net48` console to `installer\App`, the self-contained
`net10.0` console to `installer\ConsoleLinux` (`linux-x64`) and `installer\ConsoleLinux-arm64`,
copies `localizations\` to all folders, and builds archives:

- `installer\TraceDiag-install.zip` — Windows: GUI + `net48` console and installer;
- `installer\TraceDiag-console-linux-x64.tar.gz` — console for Linux/amd64;
- `installer\TraceDiag-console-linux-arm64.tar.gz` — console for Linux/arm64.

On Windows, unpack the first archive and run `Install-TraceDiag.cmd`. The installer checks
.NET Framework 4.8 in the registry. If it is missing, put `dotNetFx48_Full.exe` next to the
script (the publishing script will add it to the archive) or pass
`-Framework48InstallerPath` / `-Framework48Url` for an alternative repository. Direct download
from the Microsoft website is not used. The installer copies files to
`%LOCALAPPDATA%\Programs\TraceDiag` and creates a shortcut in the "Start" menu.

On Linux, unpack the required tar.gz and run `bash install-console.sh`; the script copies the
self-contained files to `~/.local/share/TraceDiag` and creates a symbolic link in
`~/.local/bin`. There is no need to download a separate .NET Runtime.

System packages (per-machine):

```powershell
cd installer
dotnet tool restore
.\Build-TraceDiag-Packages.ps1
```

The result is in `installer\dist`: `TraceDiag-<version>-x86.msi`, `TraceDiag-<version>-x64.msi`,
`tracediag_<version>_amd64.deb` and `tracediag_<version>_arm64.deb`. The MSI contains the GUI
and console of the corresponding bitness, `localizations\`, and "Start" menu shortcuts;
.NET Framework 4.8 is checked before installation. The deb packages install the console to
`/usr/share/tracediag` and create the `tracediag` command via `/usr/bin`. For deb, WSL with
`dpkg-deb` is required.

Details: [installer/README-Installer.md](../../installer/README-Installer.md).
Compiled files (`installer\App`, `installer\ConsoleLinux*`, `installer\build`,
`installer\dist`, archives) are not included in Git.

## Project Structure

```text
TraceDiag.Core/              netstandard2.0, business logic independent of UI
  Models/                    data, probe jobs, and node status calculation
  Services/                  ICMP, DNS, configuration, logging
  Services/Abstractions/     contracts for controllers, jobs, and triggers
  Services/Diagnostics/      route, DF pre-check, node checks and loss diagnosis
  Services/Monitoring/       independent node timers and monitoring startup
  Services/Logging/          file rotation and background log writing
  Compatibility/             polyfills for netstandard2.0 and net48
  Localization/              reading JSON translations and selecting the language
  Strings.cs                 canonical user-facing and log strings
localizations/               en/cn/fr/de/ru/jp.json — UI translations
docs/                        en/cn/fr/de/ru/jp — README.md and ReleaseNotes.md in 6 languages
TraceDiag/                   net48, WinForms host
  Ui/                        control factories, layout, startup flows and size policy
TraceDiag.Console/           net48 + net10.0, console host
TraceDiag.Tests/             net48, WinForms and core tests
TraceDiag.Console.Tests/     net48 + net10.0, console and compatibility tests
installer/
  Install-TraceDiag.ps1      installation on Windows (per-user)
  install-console.sh         console installation on Linux
  Publish-TraceDiag.ps1      building archives
  Build-TraceDiag-Packages.ps1  building MSI (x86/x64) and deb (amd64/arm64)
  wix/                       MSI sources
  deb/                       script for building deb in WSL
.config/dotnet-tools.json    pinned WiX 5 version
```

## Application Data

- Windows: `%LOCALAPPDATA%\TraceDiag\` (`settings.json`, `ui-state.json`, `logs\`).
- Linux: `~/.local/share/TraceDiag/` — the same files, created on first console launch.
- Logs: `tracediag-yyyyMMdd.log`; when it reaches 4 MB, `tracediag-yyyyMMdd-1.log`,
  `tracediag-yyyyMMdd-2.log`, and so on are created.

## License

Apache License 2.0 — see [LICENSE](../../LICENSE).

Created by Enaleven 2026 with using Big Pickle
