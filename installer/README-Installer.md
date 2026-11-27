# Установка TraceDiag

Пакет содержит WinForms-приложение (`net48`), консоль для Windows (`net48`) и
self-contained консоль для Linux (`net10.0`, `linux-x64`). Загрузка с сайта Microsoft
не используется.

## 1. Подготовка архивов на машине разработчика

Нужен .NET 10 SDK:

```powershell
.\Publish-TraceDiag.ps1
```

Скрипт публикует файлы в `App\` и `ConsoleLinux\` и собирает рядом с собой:

- `TraceDiag-install.zip` — Windows: GUI + консоль `net48`;
- `TraceDiag-console-linux-x64.tar.gz` — self-contained консоль для Linux.

Если положить `dotNetFx48_Full.exe` в эту папку, скрипт включит его в Windows-архив.

## 2. Установка на Windows

1. Скопируйте `TraceDiag-install.zip` на целевой ПК и распакуйте.
2. Запустите `Install-TraceDiag.cmd`.

Установщик:

- проверяет .NET Framework 4.8 через реестр (`Release >= 528040`);
- если .NET Framework не найден — требует прав администратора и ищет установщик
  `dotNetFx48_Full.exe` рядом со скриптом или в `App\`; можно задать другой путь через
  `-Framework48InstallerPath` или URL альтернативного репозитория через `-Framework48Url`;
- запускает найденный установщик тихо (перезагрузка может потребоваться);
- копирует `TraceDiag.exe` и `TraceDiag.Console.exe` в `%LOCALAPPDATA%\Programs\TraceDiag`;
- создаёт ярлык в меню «Пуск» (и на рабочем столе с `-DesktopShortcut`).

## 3. Установка консоли на Linux

Отдельный .NET Runtime не нужен: архив содержит self-contained сборку `net10.0`.

```bash
tar -xzf TraceDiag-console-linux-x64.tar.gz -C /tmp/tracediag
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
