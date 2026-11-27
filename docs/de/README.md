# TraceDiag

> Языки: [English](../en/README.md) · [中文](../cn/README.md) · [Français](../fr/README.md) ·
> [Deutsch](../de/README.md) · [Русский](../ru/README.md) · [日本語](../jp/README.md)

Ein Hilfsprogramm zur Diagnose der Route zu einem entfernten Host. Es führt eine ICMP-Traceroute durch, löst Knotennamen auf, misst Verluste und prüft die Stabilität der Route bei unterschiedlichen Paketgrößen. Die kanonischen Zeichenfolgen befinden sich in `Strings.cs`, die Übersetzungen in den JSON-Dateien des Ordners `localizations` neben der ausführbaren Datei. Die Geschäftslogik ist in kleine statische Module aufgeteilt.

Die gemeinsame Logik liegt in `TraceDiag.Core` (`netstandard2.0`) und ist unabhängig von der UI. Darauf aufbauend arbeiten zwei Hosts: WinForms (`net48`) und die Konsole (`net48` und `net10.0`). Die Konsole ermöglicht die Diagnose auf Windows Server 2008 R2 ohne grafische Benutzeroberfläche sowie unter Linux (Ubuntu 22.04+).

## Funktionen

- Routeverfolgung: TTL von `1` bis `255` (Maximum für IP-TTL), Stopp beim Erreichen des Ziels oder beim Erreichen des Hop-Limits.
- Die Host-Erkennung ist in das eigene Modell `HostDiscovery` ausgelagert: Die TTLs werden nacheinander abgefragt, bis zu `3` Durchläufe, Paket `64` Byte, Timeout `5` s. Das Flag `DontFragment` wird aktiviert, wenn der Modus „Nicht fragmentieren" eingeschaltet ist. TTLs ohne Antwort erhalten den Status `keine Antwort` und werden bei der Prüfung übersprungen.
- Die Verzögerung bei Paketverlust wird über das Feld „Verzögerung bei Verlust, ms" rechts eingestellt, Standard `1000` ms. Antworten werden ohne Wartezeit verarbeitet; nach dem ersten ausbleibenden Paket erfolgen alle weiteren Prüfungen dieses Knotens mit Verzögerung, um keine falschen Verluste durch den ICMP-Flood-Schutz der Hardware zu verursachen. Der Wert `0` deaktiviert die Verzögerung.
- Reverse DNS für jeden erkannten Knoten mit Schutz vor langsamen Antworten.
- Modus für die Paketgröße:
  - Kontrollkästchen **„Dynamische Paketgröße"** aktiviert — `64 → 128 → … → Maximum`;
  - Kontrollkästchen deaktiviert — nur die maximale Größe aus dem Eingabefeld (`64–65500` Byte).
  - Alle Größen werden zuerst am aktuellen Knoten geprüft, danach rückt die Warteschlange zum nächsten vor;
  - Beim Wechsel der Größe bleiben die Zähler des Knotens und die RTT-Stichproben erhalten und werden weiter fortgeschrieben.
- Das Kontrollkästchen **„Nicht fragmentieren (DF)"** ist standardmäßig deaktiviert. Wenn es aktiviert ist, wird die maximale Paketgröße auf `1024` Byte begrenzt, die dynamische Größe wird abgewählt und gesperrt, und Routenerkennung, Hauptprüfung und Überwachung verwenden `DontFragment`.
- Vor der Hauptprüfung und einmal beim Start der Überwachung wird eine DF-Vorprüfung durchgeführt: eine Sonde pro Knoten mit den Größen `64 → 128 → 256 → 1024 → 1400` Byte. Alle Antworten und Fehler erscheinen in der Konsole und im Log als zusätzliche Informationen; Tabelle und Diagnosen bleiben unverändert.
- Sonden werden als eigene Aufgaben mit dem Trigger `DO_NOT_FRAGMENT` oder einem normalen Trigger erstellt. Der Algorithmus der Abschlussprüfung und der Algorithmus der Überwachung liegen in unabhängigen Controllern.
- Konfigurierbare Anzahl von Versuchen pro Knoten: `1–1000`, Standard `100` — für den Bericht an den Anbieter und die Messung des Verlustprozentsatzes.
- Die Kontrollkästchen sind in einer einzigen Zeile im linken Teil des Eingabebereichs angeordnet; die rechte Spalte ist schmaler und der Bereich selbst nimmt weniger Höhe ein.
- Kontrollkästchen **„Modus für dauerhafte Überwachung"**: Die Paketgröße wird einmalig aus dem Feld übernommen, die Anzahl der Versuche wird nicht verwendet. Nach der Routenerkennung wird jeder Knoten durch einen eigenen Timer einmal pro Sekunde abgefragt. Knoten mit der Diagnose `ICMP deaktiviert` werden nicht abgefragt; eine Einschränkung der Paketgröße schließt einen Knoten nicht von der Überwachung aus. Gestoppt wird mit der Schaltfläche `Stop`.
- Im Überwachungsmodus werden die Daten jedes Knotens in seiner bestehenden Zeile aktualisiert; für erneute Abfragen werden keine neuen Zeilen erstellt.
- Die Statistikhistorie ist auf `1 000 000` Momentaufnahmen begrenzt, der Ausgabepuffer auf `10 000` Zeilen: Bei Überlauf werden die ersten Einträge und Zeilen entfernt. Das Log wechselt beim Erreichen von `4` MB in eine neue Datei; die vorherige Datei bleibt unverändert. Logschreiben und Statistikverarbeitung laufen in eigenen Workern und belegen keine Threads der ICMP-Abfrage.
- Exakte Statistik für jeden Knoten: durchschnittlicher, minimaler, maximaler und letzter Ping, Anzahl gesendeter/empfangener Pakete, Verlustprozentsatz. Der Durchschnitt wird aus allen gespeicherten RTT-Messungen berechnet, nicht aus Näherungswerten.
- Bei `100%` Verlust erhält der Knoten den Status `keine Antwort`; nach `10` aufeinanderfolgenden Fehlschlägen wird die Prüfung des Knotens beendet und bei den folgenden Paketgrößen wird er übersprungen.
- Verluste werden in der Spalte „Status" als Prozentsatz angezeigt, die Ursache in einer eigenen Spalte „Diagnose". Erneute Prüfungen vorheriger Knoten werden nicht durchgeführt.
- Oberflächensprachen: `En`, `Cn`, `Fr`, `De`, `Ru`, `Jp`. Standard ist `En`; die Auswahl wird in `settings.json` gespeichert. In der GUI wird die Sprache über eine Liste im rechten Bereich ohne Neustart geändert, in der Konsole über die Option `--language <en|cn|fr|de|ru|jp>`. Die Übersetzungen liegen in `localizations/*.json` neben der exe und können ohne Neuerstellung bearbeitet werden; unbekannte Schlüssel bleiben englisch. Das Dateilog ist immer englisch.
- Die Statistiktabelle über dem Log reicht über die gesamte Fensterbreite; dazwischen befindet sich ein ziehbarer horizontaler Trenner. Seine Position wird getrennt von den Spaltenbreiten gespeichert. Eine eigene Spalte „Diagnose" zeigt `ICMP deaktiviert`, `ICMP-Größenbegrenzung`, `ICMP-Anti-Flood` oder `—`, wenn keine Anzeichen vorliegen. Die Diagnose wird auch ins Log geschrieben.
- Auswahl der Logschriftgröße, der Verzögerung bei Verlust und eine große Schaltfläche `Run`/`Stop` im rechten Bereich: Während der Prüfung stoppt die Schaltfläche den laufenden Durchlauf.
- Vollbildmodus: `F11` — ein-/ausschalten, `Esc` — beenden.
- Dunkles Design als Standard mit einem Umschalter im linken Bereich; die Auswahl wird in den Einstellungen gespeichert. Die Kontrollkästchen bleiben Standardkontrollkästchen und ändern ihre Farben zusammen mit dem Design; die Tabelle verwendet die Palette des Designs, und nicht prüfbare Knoten sind im dunklen und im hellen Design grau.
- Beim Schließen des Fensters speichert eine separate Datei `ui-state.json` alle Eingabefelder, Kontrollkästchen, Spaltenbreiten der Tabelle, die vertikale Position des Trenners (Prozent der Tabellenhöhe) sowie den Fensterzustand: pixelgenau wiederhergestellt (`x, y, x2, y2`) oder maximized. Beim Start wird der Zustand wiederhergestellt; eine beschädigte Datei wird gelöscht und das Programm startet mit den Standardwerten.
- Der Startparameter `reset` (zum Beispiel `TraceDiag.exe reset`) entfernt den gespeicherten Oberflächenzustand beim Start zwangsweise.
- Ein eigenes Symbol `cable_24dp.ico`, Dateilogging und das Speichern der Einstellungen in JSON.

## Konsole

```text
TraceDiag.Console run    --host <ip|domain> [--packet-size <n>] [--attempts <n>] [--loss-delay <ms>] [--dynamic] [--df] [--format plain|csv|json] [--language en|cn|fr|de|ru|jp]
TraceDiag.Console monitor --host <ip|domain> [--packet-size <n>] [--loss-delay <ms>] [--df] [--format plain|csv|json] [--language en|cn|fr|de|ru|jp]
TraceDiag.Console reset [--language en|cn|fr|de|ru|jp]
```

- Standardwerte: Paketgröße `1024`, `100` Versuche, Verzögerung `1000` ms.
- `--format plain` — Text, `csv` — CSV-Zeilen, `json` — JSON Lines mit dem Feld `kind`.
- `--language` lokalisiert die Hilfe, Fehler, Statusmeldungen, Zeilen des Prüfungsverlaufs und die Abschlusstabelle. Das Dateilog bleibt englisch: Der kanonische Text und die Argumente sind vom angezeigten Übersetzungstext getrennt; Optionsnamen und Befehle werden nicht übersetzt. Die Konsole gibt Text in UTF-8 aus (in älteren Windows-Terminals kann `chcp 65001` erforderlich sein).
- Der Prüfungsverlauf wird zeilenweise in der Konsole ausgegeben, unmittelbar beim Schreiben in das Dateilog, ohne bis zum Ende der Prüfung gesammelt zu werden. Die Datei wird durch `AsyncLogWriter` um dieselbe Zeile ergänzt, sodass bereits geschriebene Zeilen bei Abbruch oder Schließen erhalten bleiben.
- Nach Abschluss (auch nach `Ctrl+C`) wird zuletzt eine einzige Abschlusstabelle (die lokalisierte Entsprechung von `Summary:`) mit den letzten Werten für jede TTL ausgegeben.
- Die WinForms-Anwendung funktioniert wie zuvor: Die Tabelle wird direkt in der Oberfläche aktualisiert.
- Bei einem Tippfehler im Optionsnamen schlägt die Konsole den nächstliegenden gültigen Namen vor und gibt die vollständige Optionsliste zum Vergleich aus.
- `Ctrl+C` stoppt die Überwachung; Exit-Codes: `0` Erfolg, `1` Fehler, `2` ungültige Argumente, `130` Abbruch.
- Die Konsole verwendet dieselben Dienste, Logs und Rotationen wie die WinForms-Anwendung.
- Unter Linux kann für ICMP durch einen normalen Benutzer `net.ipv4.ping_group_range` oder die Capability `CAP_NET_RAW` erforderlich sein.

## Anforderungen

- WinForms: Windows 7 SP1 / Server 2008 R2 SP1 oder neuer, .NET Framework 4.8.
- Konsole `net48`: Windows 7 SP1 / Server 2008 R2 SP1 oder neuer, .NET Framework 4.8.
- Konsole `net10.0`: Windows Server 2012+, Ubuntu 22.04/24.04/26.04 und andere unterstützte .NET-10-Systeme; das Linux-Paket ist eigenständig (self-contained), eine separate Runtime ist nicht erforderlich.
- Für den Build: .NET 10 SDK; für MSI: WiX 5 (`dotnet tool restore`), für deb: WSL mit `dpkg-deb`. Die Installation von .NET Framework 4.8 auf älteren Windows-Versionen erfordert Administratorrechte und einen lokalen Installer aus einem alternativen Repository (siehe unten).

## Erstellen und Ausführen

```powershell
dotnet build TraceDiag.slnx -c Release
.\TraceDiag\bin\Release\net48\TraceDiag.exe
.\TraceDiag.Console\bin\Release\net10.0\TraceDiag.Console.exe --help
```

## Tests

```powershell
dotnet test TraceDiag.slnx -c Release
```

**252 Testläufe** (174 WinForms + 39 Konsolentests unter `net48` und `net10.0`) decken den Planer für Paketgrößen, die Knotenstatistik, die Zuordnung von ICMP-Antworten, die Konfiguration, das Logging, das Speichern des Oberflächenzustands, die Lokalisierung in allen sechs Sprachen, die Auswertung von Konsolenargumenten, Ausgabeformate, Diagnosemodule und den vollständigen Zyklus von `TraceDiagnosticsService` mit Test-Dummies für ICMP, DNS und Logger ab.

## Vorgefertigte Builds

Letzte Veröffentlichung: [Release 1.8.4 version](https://github.com/warriorexclusive-dev/TraceDiag/releases/tag/v1.8.4)

- [TraceDiag.exe](https://github.com/warriorexclusive-dev/TraceDiag/releases/download/v1.8.4/TraceDiag.exe)
- [TraceDiag-install.zip](https://github.com/warriorexclusive-dev/TraceDiag/releases/download/v1.8.4/TraceDiag-install.zip)

Änderungsverlauf: [ReleaseNotes.md](ReleaseNotes.md); Übersetzungen befinden sich in `docs/<Sprache>/`.

## Installation auf einem frischen PC

Archive (per-user-Installation und „entpacken und ausführen"):

```powershell
cd installer
.\Publish-TraceDiag.ps1
```

Das Skript veröffentlicht WinForms und die Konsole `net48` in `installer\App`, die eigenständige Konsole `net10.0` in `installer\ConsoleLinux` (`linux-x64`) und `installer\ConsoleLinux-arm64`, kopiert `localizations\` in alle Ordner und erstellt die Archive:

- `installer\TraceDiag-install.zip` — Windows: GUI + Konsole `net48` und Installer;
- `installer\TraceDiag-console-linux-x64.tar.gz` — Konsole für Linux/amd64;
- `installer\TraceDiag-console-linux-arm64.tar.gz` — Konsole für Linux/arm64.

Unter Windows entpacken Sie das erste Archiv und starten `Install-TraceDiag.cmd`. Der Installer prüft .NET Framework 4.8 in der Registry. Falls es fehlt, legen Sie `dotNetFx48_Full.exe` neben das Skript (das Veröffentlichungsskript fügt es dem Archiv hinzu) oder übergeben Sie `-Framework48InstallerPath` / `-Framework48Url` für ein alternatives Repository. Ein direkter Download von der Microsoft-Website wird nicht verwendet. Der Installer kopiert die Dateien nach `%LOCALAPPDATA%\Programs\TraceDiag` und erstellt eine Verknüpfung im Menü „Start".

Unter Linux entpacken Sie das gewünschte tar.gz und starten `bash install-console.sh`; das Skript kopiert die eigenständigen Dateien nach `~/.local/share/TraceDiag` und erstellt einen symbolischen Link in `~/.local/bin`. Eine separate .NET Runtime muss nicht heruntergeladen werden.

Systempakete (per-machine):

```powershell
cd installer
dotnet tool restore
.\Build-TraceDiag-Packages.ps1
```

Das Ergebnis liegt in `installer\dist`: `TraceDiag-<Version>-x86.msi`, `TraceDiag-<Version>-x64.msi`, `tracediag_<Version>_amd64.deb` und `tracediag_<Version>_arm64.deb`. Das MSI enthält GUI und Konsole der entsprechenden Architektur, `localizations\` und Verknüpfungen im Menü „Start"; vor der Installation wird .NET Framework 4.8 geprüft. Die deb-Pakete installieren die Konsole nach `/usr/share/tracediag` und stellen den Befehl `tracediag` über `/usr/bin` bereit. Für deb wird WSL mit `dpkg-deb` benötigt.

Details: [installer/README-Installer.md](../../installer/README-Installer.md). Kompilierte Dateien (`installer\App`, `installer\ConsoleLinux*`, `installer\build`, `installer\dist`, Archive) werden nicht in Git aufgenommen.

## Projektstruktur

```text
TraceDiag.Core/              netstandard2.0, Geschäftslogik ohne Bindung an die UI
  Models/                    Daten, Sondierungsaufgaben und Berechnung der Knotenstatus
  Services/                  ICMP, DNS, Konfiguration, Logging
  Services/Abstractions/     Verträge für Controller, Aufgaben und Trigger
  Services/Diagnostics/      Route, DF-Vorprüfung, Knotenprüfung und Diagnose von Verlusten
  Services/Monitoring/       unabhängige Knoten-Timer und Start der Überwachung
  Services/Logging/          Dateirotation und Hintergrundschreiben des Logs
  Compatibility/             Polyfills für netstandard2.0 und net48
  Localization/              Lesen der JSON-Übersetzungen und Sprachauswahl
  Strings.cs                 kanonische Benutzer- und Log-Zeichenfolgen
localizations/               en/cn/fr/de/ru/jp.json — Übersetzungen der Oberfläche
docs/                        en/cn/fr/de/ru/jp — README.md und ReleaseNotes.md in 6 Sprachen
TraceDiag/                   net48, WinForms-Host
  Ui/                        Fabriken für Steuerelemente, Layout, Start-Threads und Größenrichtlinie
TraceDiag.Console/           net48 + net10.0, Konsolen-Host
TraceDiag.Tests/             net48, Tests für WinForms und Core
TraceDiag.Console.Tests/     net48 + net10.0, Tests für Konsole und Kompatibilität
installer/
  Install-TraceDiag.ps1      Installation unter Windows (pro Benutzer)
  install-console.sh         Installation der Konsole unter Linux
  Publish-TraceDiag.ps1      Erstellung der Archive
  Build-TraceDiag-Packages.ps1  Erstellung von MSI (x86/x64) und deb (amd64/arm64)
  wix/                       MSI-Quellen
  deb/                       Skript für den deb-Build in WSL
.config/dotnet-tools.json    festgelegte Version von WiX 5
```

## Anwendungsdaten

- Windows: `%LOCALAPPDATA%\TraceDiag\` (`settings.json`, `ui-state.json`, `logs\`).
- Linux: `~/.local/share/TraceDiag/` — dieselben Dateien, sie werden beim ersten Start der Konsole erstellt.
- Logs: `tracediag-yyyyMMdd.log`; beim Erreichen von 4 MB werden `tracediag-yyyyMMdd-1.log`, `tracediag-yyyyMMdd-2.log` und so weiter erstellt.

## Lizenz

Apache License 2.0 — siehe [LICENSE](../../LICENSE).

Erstellt von Enaleven 2026 unter Verwendung von Big Pickle
