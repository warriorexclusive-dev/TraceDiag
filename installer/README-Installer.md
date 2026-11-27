# Установка TraceDiag

Пакет содержит WinForms-приложение (`net48`), консоль для Windows (`net48`) и
self-contained консоль для Linux (`net10.0`, `linux-x64`). Загрузка с сайта Microsoft
не используется.

## 1. Подготовка архивов на машине разработчика

Нужен .NET 10 SDK:

```powershell
.\Publish-TraceDiag.ps1
```

Скрипт публикует файлы в `App\` и `ConsoleLinux\`, копирует в обе папки каталог
`localizations\` и собирает рядом с собой:

- `TraceDiag-install.zip` — Windows: GUI + консоль `net48`;
- `TraceDiag-console-linux-x64.tar.gz` — self-contained консоль для Linux/amd64;
- `TraceDiag-console-linux-arm64.tar.gz` — self-contained консоль для Linux/arm64.

Если положить `dotNetFx48_Full.exe` в эту папку, скрипт включит его в Windows-архив.

## 2. Системные пакеты

Скрипт `Build-TraceDiag-Packages.ps1` собирает четыре пакета в `installer\dist`:

```powershell
dotnet tool restore
powershell -ExecutionPolicy Bypass -File .\Build-TraceDiag-Packages.ps1
```

- `TraceDiag-<версия>-x86.msi` и `TraceDiag-<версия>-x64.msi` — Windows, per-machine,
  GUI и консоль соответствующей разрядности, папка `localizations`, ярлыки в меню «Пуск».
  Перед установкой проверяется .NET Framework 4.8 (`Release >= 528040`); скачивание с
  сайта Microsoft не выполняется. Установка: `msiexec /i TraceDiag-1.9.11-x64.msi /qn`.
- `tracediag_<версия>_amd64.deb` и `tracediag_<версия>_arm64.deb` — Linux, self-contained
  консоль `net10.0` в `/usr/share/tracediag` и символическая ссылка `/usr/bin/tracediag`.
  Установка: `sudo dpkg -i tracediag_1.9.11_amd64.deb`. Для сборки deb нужна WSL с
  `dpkg-deb`; ключи `-SkipPublish` и `-SkipDeb` позволяют пересобрать только часть.

## 3. Установка на Windows из архива

1. Скопируйте `TraceDiag-install.zip` на целевой ПК и распакуйте.
2. Запустите `Install-TraceDiag.cmd`.

Установщик:

- проверяет .NET Framework 4.8 через реестр (`Release >= 528040`);
- если .NET Framework не найден — требует прав администратора и ищет установщик
  `dotNetFx48_Full.exe` рядом со скриптом или в `App\`; можно задать другой путь через
  `-Framework48InstallerPath` или URL альтернативного репозитория через `-Framework48Url`;
- запускает найденный установщик тихо (перезагрузка может потребоваться);
- копирует `TraceDiag.exe`, `TraceDiag.Console.exe` и папку `localizations` в
  `%LOCALAPPDATA%\Programs\TraceDiag`;
- создаёт ярлык в меню «Пуск» (и на рабочем столе с `-DesktopShortcut`).

Файлы `localizations\*.json` можно править после установки: приложение читает их при
запуске, пересборка не нужна. Ключ — каноничный английский текст, поле `language` — код
языка (`en`, `cn`, `fr`, `de`, `ru`, `jp`).

## 4. Установка консоли на Linux из архива

Отдельный .NET Runtime не нужен: архив содержит self-contained сборку `net10.0`.

```bash
tar -xzf TraceDiag-console-linux-x64.tar.gz -C /tmp/tracediag
# на arm64: tar -xzf TraceDiag-console-linux-arm64.tar.gz -C /tmp/tracediag
bash /tmp/tracediag/install-console.sh
export PATH="$HOME/.local/bin:$PATH"
TraceDiag.Console --help
```

Скрипт копирует файлы в `~/.local/share/TraceDiag` и создаёт ссылку в `~/.local/bin`.
Папки можно переопределить переменными `INSTALL_DIR` и `BIN_DIR`.

Для ICMP от обычного пользователя может понадобиться диапазон
`net.ipv4.ping_group_range` или capability `CAP_NET_RAW`. В минимальных образах без
`libicu` используйте `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1` — приложение форматирует
все значения через `InvariantCulture`.

## Дополнительные параметры Windows-установщика

```powershell
powershell -ExecutionPolicy Bypass -File .\Install-TraceDiag.ps1 -DesktopShortcut
powershell -ExecutionPolicy Bypass -File .\Install-TraceDiag.ps1 -InstallDir "D:\Tools\TraceDiag" -NoLaunch
```

- `-InstallDir` — другая папка установки.
- `-AppSource` — папка с опубликованными файлами (по умолчанию `.\App`).
- `-Framework48InstallerPath` — локальный `dotNetFx48_Full.exe` из альтернативного источника.
- `-Framework48Url` — URL альтернативного репозитория, если установщик не лежит локально.
- `-NoShortcut` — не создавать ярлыки.
- `-NoLaunch` — не запускать приложение после установки.
- `-DesktopShortcut` — добавить ярлык на рабочий стол.
