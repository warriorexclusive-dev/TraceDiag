# Release Notes

> Языки: [English](../en/ReleaseNotes.md) · [中文](../cn/ReleaseNotes.md) · [Français](../fr/ReleaseNotes.md) ·
> [Deutsch](../de/ReleaseNotes.md) · [Русский](../ru/ReleaseNotes.md) · [日本語](../jp/ReleaseNotes.md)

The TraceDiag changelog. Prebuilt builds are published on the
[Releases](https://github.com/warriorexclusive-dev/TraceDiag/releases) page.

## 1.9.0

- Added interface languages `En`, `Cn`, `Fr`, `De`, `Ru`, `Jp`; `En` by default. The choice is
  saved in `settings.json`, can be changed in the GUI without a restart, and is set in the
  console with the `--language <en|cn|fr|de|ru|jp>` option. Translations are stored in
  `localizations/*.json` next to the executable and require no rebuild. All visible GUI and
  console strings are localized, including progress and the final table; the file log remains
  in English thanks to the separation of canonical text and the displayed translation.
- A draggable divider has been added between the table and the log in the Windows GUI: panel
  heights change with the mouse, and the position is saved in `ui-state.json` as a percentage
  (with range clamping) and restored on startup.
- Business logic has been moved to `TraceDiag.Core` (`netstandard2.0`); the WinForms host has
  been switched to .NET Framework 4.8 and runs on Windows 7 SP1 / Server 2008 R2 SP1.
- Added the `TraceDiag.Console` console host for `net48` (Windows Server 2008 R2 without a
  GUI) and `net10.0` (Ubuntu 22.04+ and Windows Server 2012+). Commands `run`, `monitor`,
  and `reset`, output formats `plain`, `csv`, and `json`, stopping with `Ctrl+C`. Progress is
  printed to the console line by line at the same time as it is written line by line to the
  file log via `AsyncLogWriter`; after completion, one final `Summary:` table is printed last.
  WinForms behavior is unchanged.
- Unknown options now get a suggestion of the nearest name and a full list of options; all
  user-facing and log strings of the console have been added to `Strings.cs`; the console
  reuses the core's services, file log, and rotation.
- Polyfills and API replacements have been added for legacy framework targets (`PeriodicTimer`,
  `TickCount64`, `Math.Clamp`, `IReadOnlySet`, `ArgumentNullException.ThrowIfNull`,
  `Dns.GetHostAddressesAsync` with a token).
- The Windows installer checks .NET Framework 4.8 in the registry; if it is missing, it
  requires administrator rights and takes an offline installer from a local file or an
  alternative repository (`-Framework48InstallerPath` / `-Framework48Url`); downloading from
  the Microsoft website is not used. The Linux console is shipped as a self-contained tar.gz
  with `install-console.sh`; no separate Runtime is needed.
- Added system packages: MSI for Windows x86 and x64 (with a .NET Framework 4.8 check and
  "Start" menu shortcuts) and deb for Linux amd64 and arm64 (a standalone console and the
  `tracediag` command). Build with `installer\Build-TraceDiag-Packages.ps1`.
  The archives have been supplemented with a console variant for `linux-arm64`.
- The README and Release Notes have been translated into `En`, `Cn`, `Fr`, `De`, `Ru`, `Jp`
  and placed as separate files in `docs/<language>/`.
- A Linux GUI is planned as a separate task: the Core architecture is ready to connect a
  second UI adapter without changing business logic.

## 1.8.4

- Added the "Don't fragment (DF)" checkbox, off by default. When enabled, the packet size is
  limited to `1024` bytes, dynamic sizing is cleared and locked, and all probes of the main
  check and monitoring are sent with the `DontFragment` flag.
- The maximum ICMP packet size in the input field has been increased to the safe value of
  `65500` bytes; the field label is generated from the same bounds as the input itself.
- Before the main check and once when monitoring starts, a DF pre-check is performed:
  one probe for each discovered node at sizes `64 → 128 → 256 → 1024 → 1400`. Successes and
  errors are written to the console and the file log without changing the table's counters
  and diagnoses.
- Probes for regular checks, monitoring, and route discovery are created as separate jobs
  with a trigger; the check and monitoring algorithms have been extracted into independent
  controllers.

## 1.8.3

- Checkboxes are standard again: custom rendering has been removed along with the text
  overlay.
- Colors of standard checkboxes change with the theme via the palette.
- Inactive nodes are shown in gray in both dark and light themes.

## 1.8.2

- In the dark theme, checkboxes and the table are drawn with the theme palette: active text
  is white, the background and lines are dark.
- Nodes that cannot be checked with the `ICMP disabled` diagnosis are highlighted in gray
  text.
- Disabled controls also use gray instead of black.

## 1.8.1

- Checkboxes have been moved to the left part of the input panel and arranged in a single
  row.
- The right column has become narrower, and the top panel takes up less height.

## 1.8.0

- Fixed the alignment of the "Number of attempts (1-1000)" label relative to the input field
  in the normal window, when maximized, and in full-screen mode.
- Added the "Continuous monitoring mode" checkbox. The packet size is taken from the field
  once, and the number of attempts is not used. After route discovery, each available node is
  polled by its own timer once per second; nodes with the `ICMP disabled` diagnosis are
  excluded, while nodes with a packet size limitation continue to be polled.
- The `Stop` button stops monitoring; the checkbox state is saved in `ui-state.json`.
- In monitoring mode, repeated polls update the node's row without adding new rows.
- Statistics history is limited to 1 000 000 snapshots and the output buffer to 10 000 lines.
  When the limits are exceeded, the oldest entries and lines are removed.
- The file log switches to a new file when it reaches 4 MB; the previous file is left
  unchanged. Log writing and statistics processing are performed by separate workers without
  blocking socket polling.

## 1.7.0

- Removed the additional loss check by repeated ICMP requests to the previous and current
  nodes.
- The "State" column shows the loss percentage from the main check, while the cause of the
  loss remains in a separate "Diagnosis" column.
- Eliminated false statuses "damaged: loss between nodes" and "loss also at the previous
  node" when large ICMP packets do not pass through a node.

## 1.6.0

- When the window is closed, all input fields, checkboxes, table column widths, and window
  state are saved: size and position in pixels or maximized.
- On startup, UI state is restored from a separate `ui-state.json` file. A damaged file is
  deleted, and the program starts with default values.
- The `reset` startup key forcibly deletes saved UI state before loading.

## 1.5.0

- Added a "Diagnosis" column to the node statistics table.
- A node gets the `ICMP disabled` diagnosis if it does not respond to any packet size or was
  skipped during route discovery.
- The `ICMP size truncation` diagnosis is assigned when larger packet sizes stop responding
  or produce more loss than smaller sizes at the same node.
- The `ICMP anti-flood` diagnosis is assigned for partial packet loss without signs of size
  limitation. Diagnoses are written to the log.

## 1.4.1

- When moving to the next packet size, the node's sent and received packet counters, loss
  percentage, and RTT samples are no longer reset.
- Average, minimum, and maximum ping are calculated from all accumulated measurements for
  the node, not only from the last packet size.

## 1.4.0

- During a check, the `Run` button displays `Stop` and terminates the current run.
- Packet sizes are iterated node by node: all values for one node first
  (`64 → 128 → 256 → 512 → 1024`), then the next node.
- The delay field has been renamed to "Loss delay, ms". The delay is applied only after a
  missed packet; responses are processed without waiting.
- After the first loss, the delay applies to all subsequent checks of that node, including
  loss monitoring. A value of `0` disables the delay.

## 1.3.0

- Added an "ICMP delay, ms" field in the right panel; the default value is `1000` ms.
- The delay is applied after every ICMP response during route discovery, main node checks,
  and loss monitoring. This reduces false losses on hardware with ICMP flood protection.
- A value of `0` disables the delay; the field's upper bound is `60000` ms.
- Checkbox colors are now set by the theme palette uniformly for dark and light styles.

## 1.2.1

- Fixed a bug where a repeated run with a valid node did not work if the previous attempt
  ended at the input validation stage.
- Unspecified addresses (`0.0.0.0`, `::`) are no longer accepted as a destination node.
- Loss checking no longer sends ICMP requests to non-responsive stub nodes: the previous
  responding node is used for comparison.

## 1.2.0

- Added a dark theme with a toggle in the right panel; the dark theme is enabled by default.
- The theme choice is saved in `settings.json` and restored on startup.
- `cable_24dp.ico` is used as the application icon.

## 1.1.2

- The default number of attempts has been increased from `4` to `100`.

## 1.1.1

- Host discovery has been moved to a separate `HostDiscovery` model.
- Discovery runs sequentially over TTLs: up to `3` passes with a `1` s interval, a `64`-byte
  packet, the `DontFragment` flag, and a `5` s timeout.
- TTLs without a response are added to the table with the `no response` status and marked as
  skipped; DNS and the main check do not touch such entries.

## 1.1.0

- The trace limit has been increased to `255` hops.
- Fixed inflated counters in the table: loss monitoring checks are no longer added to sent
  and received packets.
- Added the .NET 10 Desktop Runtime installer and a distribution build for a clean PC.
