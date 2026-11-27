# Release Notes

> Языки: [English](../en/ReleaseNotes.md) · [中文](../cn/ReleaseNotes.md) · [Français](../fr/ReleaseNotes.md) ·
> [Deutsch](../de/ReleaseNotes.md) · [Русский](../ru/ReleaseNotes.md) · [日本語](../jp/ReleaseNotes.md)

Änderungsverlauf von TraceDiag. Fertige Builds werden auf den Seiten [Releases](https://github.com/warriorexclusive-dev/TraceDiag/releases) veröffentlicht.

## 1.9.11

- Icons 16/32 px (Standard 32), Deep-Logs der Routenvorprüfung, Route und Ziel in der Statistiktabelle; ein interner Deadline wird nicht mehr als Benutzerabbruch gemeldet.

## 1.9.10

- Sprachauswahl in den Einstellungen, Ausrichtung des Remote-Host-Labels und Laden der mitgelieferten dns-servers.json korrigiert.

## 1.9.9

- Neues Hauptfenster: Host, Play/Stop/Einstellungen-Icons, Tabelle, Ausgabe und Status; alle anderen Einstellungen in einem separaten Fenster. Adapter über RAW-Socket, DNS-Editor, Optionen --adapter, --no-asn und --dns-file.

## 1.9.8

- System-DNS der Netzwerkkarte ist überall zwingend, auch für IPv6; alternative Resolver standardmäßig aktiv, abschaltbar mit --no-alt-dns oder im GUI. Fenstertitel folgt dem Theme.

## 1.9.7

- Konsole fragt ohne Argumente nach dem Host und meldet ungültige Eingaben; Kommandozeilen-Schlüssel in einem gemeinsamen Modul.

## 1.9.6

- Asynchrone Vorprüfung alternativer Routen: dns-servers.json, 3000 ms pro Abfrage, 150 s Gesamtlimit, 4 Flow-Signaturen (8 im Deep-Modus), Fallback bei Fehlern.

## 1.9.5

- ASN über Team-Cymru-DNS-TXT und eigene Spalte; Pool-Inhaber standardmäßig aus RIPEstat, abschaltbar mit --no-ripe. Externe Quellen werden für private Ziele deaktiviert.

## 1.9.4

- Erste ASN-Abfrage und eigene Spalte für das autonome System.

## 1.9.3

- Erweiterte automatische Diagnosen: MTU Black Hole, asymmetrische Route, ICMP-Blockierung durch Zwischenknoten, Port-/NAT-Filterung.

## 1.9.2

- Mehrere Diagnosen in einer Zeile: Paketkürzung, ICMP-Anti-Flood und ECMP-Balancing.

## 1.9.1

- Erkennung von Routenwechsel und ECMP anhand unterschiedlicher IP-Antwortquellen innerhalb eines Hops.

## 1.9.0

- Die Oberflächensprachen `En`, `Cn`, `Fr`, `De`, `Ru`, `Jp` wurden hinzugefügt; Standard ist `En`. Die Auswahl wird in `settings.json` gespeichert, in der GUI ohne Neustart geändert und in der Konsole über die Option `--language <en|cn|fr|de|ru|jp>` festgelegt. Die Übersetzungen werden in `localizations/*.json` neben der ausführbaren Datei gespeichert und erfordern keinen Neubuild. Alle sichtbaren Zeichenfolgen der GUI und der Konsole sind lokalisiert, einschließlich des Prüfungsverlaufs und der Abschlusstabelle; das Dateilog bleibt durch die Trennung von kanonischem Text und angezeigtem Übersetzungstext englisch.
- In der Windows-GUI wurde zwischen Tabelle und Log ein ziehbarer Trenner hinzugefügt: Die Höhe der Bereiche wird mit der Maus geändert, die Position wird in `ui-state.json` als Prozentsatz (mit Bereichsbegrenzung) gespeichert und beim Start wiederhergestellt.
- Die Geschäftslogik wurde nach `TraceDiag.Core` (`netstandard2.0`) verlagert; der WinForms-Host wurde auf .NET Framework 4.8 umgestellt und läuft unter Windows 7 SP1 / Server 2008 R2 SP1.
- Der Konsolen-Host `TraceDiag.Console` wurde für `net48` (Windows Server 2008 R2 ohne GUI) und `net10.0` (Ubuntu 22.04+ und Windows Server 2012+) hinzugefügt. Die Befehle `run`, `monitor` und `reset`, die Ausgabeformate `plain`, `csv` und `json`, das Stoppen per `Ctrl+C`. Der Prüfungsverlauf wird zeilenweise in der Konsole ausgegeben, gleichzeitig mit der zeilenweisen Aufzeichnung im Dateilog über `AsyncLogWriter`; nach Abschluss wird zuletzt eine einzige Abschlusstabelle `Summary:` gedruckt. Das Verhalten von WinForms wurde nicht geändert.
- Unbekannte Optionen erhalten einen Vorschlag für den nächstliegenden Namen und die vollständige Optionsliste; alle Benutzer- und Log-Zeichenfolgen der Konsole wurden in `Strings.cs` aufgenommen; die Konsole nutzt die Dienste, das Dateilog und die Rotation des Kerns wieder.
- Für ältere Framework-Ziele wurden Polyfills und API-Ersätze hinzugefügt (`PeriodicTimer`, `TickCount64`, `Math.Clamp`, `IReadOnlySet`, `ArgumentNullException.ThrowIfNull`, `Dns.GetHostAddressesAsync` mit Token).
- Der Windows-Installer prüft .NET Framework 4.8 in der Registry; falls es fehlt, erfordert er Administratorrechte und verwendet einen Offline-Installer aus einer lokalen Datei oder einem alternativen Repository (`-Framework48InstallerPath` / `-Framework48Url`); ein Download von der Microsoft-Website wird nicht eingesetzt. Die Linux-Konsole wird als eigenständiges tar.gz mit `install-console.sh` geliefert; eine separate Runtime ist nicht erforderlich.
- Systempakete wurden hinzugefügt: MSI für Windows x86 und x64 (mit Prüfung von .NET Framework 4.8 und Verknüpfungen im Menü „Start") sowie deb für Linux amd64 und arm64 (eigenständige Konsole und der Befehl `tracediag`). Der Build erfolgt über `installer\Build-TraceDiag-Packages.ps1`. Die Archive wurden um eine Konsolenvariante für `linux-arm64` ergänzt.
- README und Release Notes wurden in `En`, `Cn`, `Fr`, `De`, `Ru`, `Jp` übersetzt und liegen als eigene Dateien in `docs/<Sprache>/`.
- Eine GUI für Linux ist als separate Erweiterung geplant: Die Core-Architektur ist darauf vorbereitet, einen zweiten UI-Adapter ohne Änderung der Geschäftslogik anzuschließen.

## 1.8.4

- Das Kontrollkästchen „Nicht fragmentieren (DF)" wurde hinzugefügt, standardmäßig deaktiviert. Bei Aktivierung wird die Paketgröße auf `1024` Byte begrenzt, die dynamische Größe wird abgewählt und gesperrt, und alle Sonden der Hauptprüfung und der Überwachung verwenden das Flag `DontFragment`.
- Die maximale ICMP-Paketgröße im Eingabefeld wurde auf den sicheren Wert `65500` Byte erhöht; die Beschriftung des Feldes wird aus denselben Grenzen gebildet wie die Eingabe selbst.
- Vor der Hauptprüfung und einmal beim Start der Überwachung wird eine DF-Vorprüfung durchgeführt: eine Sonde für jeden gefundenen Knoten bei den Größen `64 → 128 → 256 → 1024 → 1400`. Erfolge und Fehler werden in Konsole und Dateilog geschrieben, ohne Zähler und Diagnosen der Tabelle zu verändern.
- Sonden der normalen Prüfung, der Überwachung und der Routenerkennung werden als eigene Aufgaben mit Trigger erstellt; die Prüf- und Überwachungsalgorithmen wurden in unabhängige Controller ausgelagert.

## 1.8.3

- Die Kontrollkästchen sind wieder Standardkontrollkästchen: Die eigene Darstellung wurde zusammen mit der Textüberlagerung entfernt.
- Die Farben der Standardkontrollkästchen ändern sich über die Palette zusammen mit dem Design.
- Inaktive Knoten werden im dunklen und im hellen Design grau angezeigt.

## 1.8.2

- Im dunklen Design werden Kontrollkästchen und Tabelle mit der Designpalette gezeichnet: aktiver Text weiß, Hintergrund und Linien dunkel.
- Nicht prüfbare Knoten mit der Diagnose `ICMP deaktiviert` werden mit grauem Text hervorgehoben.
- Deaktivierte Steuerelemente verwenden ebenfalls Grau statt Schwarz.

## 1.8.1

- Die Kontrollkästchen wurden in den linken Teil des Eingabebereichs verschoben und in einer Zeile angeordnet.
- Die rechte Spalte wurde schmaler und der obere Bereich nimmt weniger Höhe ein.

## 1.8.0

- Die Ausrichtung der Beschriftung „Anzahl der Versuche (1-1000)" relativ zum Eingabefeld wurde im normalen Fenster, bei der Maximierung und im Vollbildmodus korrigiert.
- Das Kontrollkästchen „Modus für dauerhafte Überwachung" wurde hinzugefügt. Die Paketgröße wird einmalig aus dem Feld übernommen, die Anzahl der Versuche wird nicht verwendet. Nach der Routenerkennung wird jeder erreichbare Knoten durch einen eigenen Timer einmal pro Sekunde abgefragt; Knoten mit der Diagnose `ICMP deaktiviert` werden ausgeschlossen, Knoten mit einer Einschränkung der Paketgröße werden weiterhin abgefragt.
- Die Schaltfläche `Stop` stoppt die Überwachung; der Zustand des Kontrollkästchens wird in `ui-state.json` gespeichert.
- Im Überwachungsmodus aktualisieren erneute Abfragen die Zeile des Knotens, ohne neue Zeilen hinzuzufügen.
- Die Statistikhistorie ist auf 1 000 000 Momentaufnahmen begrenzt, der Ausgabepuffer auf 10 000 Zeilen. Bei Überschreitung der Limits werden die ältesten Einträge und Zeilen entfernt.
- Das Dateilog wechselt beim Erreichen von 4 MB in eine neue Datei; die vorherige Datei wird nicht verändert. Logschreiben und Statistikverarbeitung erfolgen in eigenen Workern, ohne die Abfrage der Sockets zu blockieren.

## 1.7.0

- Die zusätzliche Verlustprüfung durch erneute ICMP-Anfragen an den vorherigen und den aktuellen Knoten wurde entfernt.
- Die Spalte „Status" zeigt den Verlustprozentsatz aus der Hauptprüfung, die Ursache des Verlusts bleibt in einer eigenen Spalte „Diagnose".
- Falsche Statusmeldungen wie „beschädigt: Verlust zwischen Knoten" und „Verlust auch am vorherigen Knoten" wurden ausgeschlossen, wenn große ICMP-Pakete einen Knoten nicht passieren.

## 1.6.0

- Beim Schließen des Fensters werden alle Eingabefelder, Kontrollkästchen, Spaltenbreiten der Tabelle und der Fensterzustand gespeichert: Größe und Position in Pixeln oder maximized.
- Beim Start wird der Oberflächenzustand aus einer separaten Datei `ui-state.json` wiederhergestellt. Eine beschädigte Datei wird gelöscht und das Programm startet mit Standardwerten.
- Der Startparameter `reset` entfernt den gespeicherten Oberflächenzustand beim Start zwangsweise.

## 1.5.0

- In der Knotenstatistiktabelle wurde die Spalte „Diagnose" hinzugefügt.
- Ein Knoten erhält die Diagnose `ICMP deaktiviert`, wenn er auf keine Paketgröße antwortet oder bei der Routenerkennung übersprungen wurde.
- Die Diagnose `ICMP-Größenbegrenzung` wird vergeben, wenn größere Paketgrößen keine Antwort mehr liefern oder mehr Verluste verursachen als kleinere Größen beim selben Knoten.
- Die Diagnose `ICMP-Anti-Flood` wird bei teilweisen Paketverlusten ohne Anzeichen einer Größenbegrenzung vergeben. Die Diagnosen werden ins Log geschrieben.

## 1.4.1

- Beim Wechsel zur nächsten Paketgröße werden die Zähler gesendeter und empfangener Pakete, der Verlustprozentsatz und die RTT-Stichproben des Knotens nicht mehr zurückgesetzt.
- Durchschnittlicher, minimaler und maximaler Ping werden aus allen für den Knoten gesammelten Messungen berechnet, nicht nur aus der letzten Paketgröße.

## 1.4.0

- Die Schaltfläche `Run` zeigt während der Prüfung `Stop` an und beendet den laufenden Durchlauf.
- Die Paketgrößen werden knotweise durchlaufen: zuerst alle Werte für einen Knoten (`64 → 128 → 256 → 512 → 1024`), dann der nächste Knoten.
- Das Verzögerungsfeld wurde in „Verzögerung bei Verlust, ms" umbenannt. Die Verzögerung wird nur nach einem ausbleibenden Paket angewendet; Antworten werden ohne Wartezeit verarbeitet.
- Nach dem ersten Verlust gilt die Verzögerung für alle weiteren Prüfungen dieses Knotens, einschließlich der Verlustkontrolle. Der Wert `0` deaktiviert die Verzögerung.

## 1.3.0

- Das Feld „ICMP-Verzögerung, ms" wurde im rechten Bereich hinzugefügt; der Standardwert beträgt `1000` ms.
- Die Verzögerung wird nach jeder ICMP-Antwort bei der Routenerkennung, den Hauptprüfungen des Knotens und der Verlustkontrolle angewendet. Dies verringert die Zahl falscher Verluste bei Hardware mit ICMP-Flood-Schutz.
- Der Wert `0` deaktiviert die Verzögerung; die Obergrenze des Feldes beträgt `60000` ms.
- Die Farbe der Kontrollkästchen wird nun durch die Designpalette für das dunkle und das helle Design auf dieselbe Weise festgelegt.

## 1.2.1

- Ein Fehler wurde behoben, nach dem ein erneuter Start mit einem gültigen Knoten nicht funktionierte, wenn der vorherige Versuch in der Phase der Eingabeprüfung geendet hatte.
- Unspezifische Adressen (`0.0.0.0`, `::`) werden nicht mehr als Zielknoten akzeptiert.
- Die Verlustprüfung sendet keine ICMP-Anfragen mehr an Dummy-Knoten ohne Antwort: Für den Vergleich wird der vorherige Knoten verwendet, der geantwortet hat.

## 1.2.0

- Ein dunkles Design mit einem Umschalter im rechten Bereich wurde hinzugefügt; standardmäßig ist das dunkle Design aktiviert.
- Die Designauswahl wird in `settings.json` gespeichert und beim Start wiederhergestellt.
- Als Anwendungssymbol wird `cable_24dp.ico` verwendet.

## 1.1.2

- Die Standardanzahl der Versuche wurde von `4` auf `100` erhöht.

## 1.1.1

- Die Host-Erkennung wurde in das eigene Modell `HostDiscovery` ausgelagert.
- Die Suche erfolgt nacheinander nach TTL: bis zu `3` Durchläufe mit einem Intervall von `1` s, Paket `64` Byte, Flag `DontFragment` und Timeout `5` s.
- TTLs ohne Antwort werden mit dem Status `keine Antwort` in die Tabelle aufgenommen und als zu überspringende markiert; DNS und Hauptprüfung lassen solche Einträge unberührt.

## 1.1.0

- Das Tracing-Limit wurde auf `255` Hops erhöht.
- Die überhöhten Zähler in der Tabelle wurden korrigiert: Kontrollprüfungen der Verluste werden nicht mehr zu den gesendeten und empfangenen Paketen gezählt.
- Ein Installer für die .NET 10 Desktop Runtime und ein distributionsfähiges Paket für einen frischen PC wurden hinzugefügt.
