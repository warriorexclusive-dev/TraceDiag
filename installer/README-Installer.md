# Установка TraceDiag на чистый ПК

Пакет рассчитан на Windows и не требует прав администратора.

## 1. Подготовка архива на машине разработчика

Нужен .NET 10 SDK:

```powershell
.\Publish-TraceDiag.ps1
```

Скрипт публикует приложение в `App\` и собирает `TraceDiag-install.zip` рядом с собой.

## 2. Установка на чистом ПК

1. Скопируйте `TraceDiag-install.zip` на целевой ПК и распакуйте.
2. Запустите `Install-TraceDiag.cmd` (двойным щелчком или из командной строки).

Установщик:
- определяет архитектуру (`x64` / `x86` / `arm64`);
- если .NET 10 Desktop Runtime не найден — скачивает официальный
  `dotnet-install.ps1` с `https://dot.net/v1/dotnet-install.ps1` и ставит рантайм
  в `%LOCALAPPDATA%\Microsoft\dotnet` без прав администратора;
- копирует приложение в `%LOCALAPPDATA%\Programs\TraceDiag`;
- создаёт ярлык в меню «Пуск» (и на рабочем столе, если передать `-DesktopShortcut`).

Для установки нужен интернет только в момент загрузки рантайма. Если рантайм уже
установлен системно, скачивание не выполняется.

## Дополнительные параметры

```powershell
powershell -ExecutionPolicy Bypass -File .\Install-TraceDiag.ps1 -DesktopShortcut
powershell -ExecutionPolicy Bypass -File .\Install-TraceDiag.ps1 -InstallDir "D:\Tools\TraceDiag" -NoLaunch
```

- `-InstallDir` — другая папка установки.
- `-AppSource` — папка с опубликованными файлами (по умолчанию `.\App`).
- `-NoShortcut` — не создавать ярлыки.
- `-NoLaunch` — не запускать приложение после установки.
- `-DesktopShortcut` — добавить ярлык на рабочий стол.
