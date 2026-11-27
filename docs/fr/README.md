# TraceDiag

> Языки: [English](../en/README.md) · [中文](../cn/README.md) · [Français](../fr/README.md) ·
> [Deutsch](../de/README.md) · [Русский](../ru/README.md) · [日本語](../jp/README.md)

Utilitaire de diagnostic de la route vers un hôte distant. Il effectue le traçage ICMP,
résout les noms des nœuds, mesure les pertes et vérifie la robustesse de la route vis-à-vis de
différentes tailles de paquet. Les chaînes canoniques se trouvent dans `Strings.cs`, tandis que
les traductions sont dans les fichiers JSON du dossier `localizations` situé à côté du fichier
exécutable. La logique métier est répartie en petits modules statiques.

La logique commune réside dans `TraceDiag.Core` (`netstandard2.0`) et ne dépend pas de
l'interface utilisateur. Deux hôtes sont construits par-dessus : WinForms (`net48`) et la
console (`net48` et `net10.0`). La console permet d'utiliser le diagnostic sur Windows Server
2008 R2 sans interface graphique et sur Linux (Ubuntu 22.04+).

## Fonctionnalités

- Traçage de la route : TTL de `1` à `255` (maximum autorisé pour un TTL IP), arrêt à
  l'atteinte de la cible ou à la limite de sauts.
- La recherche d'hôtes est confiée à un modèle distinct `HostDiscovery` : les TTL sont
  interrogés séquentiellement, jusqu'à `3` passages, paquet de `64` octets, délai d'attente de
  `5` s. L'indicateur `DontFragment` est activé lorsque le mode « Ne pas fragmenter » est
  activé. Les TTL sans réponse reçoivent le statut `aucune réponse` et sont ignorés lors de la
  vérification.
- Le délai en cas de perte de paquets est configuré par le champ « Délai sur perte, ms » à
  droite, avec une valeur par défaut de `1000` ms. Les réponses sont traitées sans attente ;
  après le premier paquet perdu, toutes les vérifications suivantes de ce nœud sont
  effectuées avec un délai afin d'éviter les pertes erronées dues à la protection du matériel
  contre les inondations ICMP. La valeur `0` désactive le délai.
- Résolution DNS inverse pour chaque nœud détecté, avec protection contre une réponse lente.
- Mode de taille de paquet :
  - la case **« Taille de paquet dynamique »** est cochée — `64 → 128 → … → maximum` ;
  - la case est décochée — seule la taille maximale du champ de saisie est utilisée
    (`64–65500` octets).
  - toutes les tailles sont d'abord vérifiées sur le nœud courant, puis la file passe au
    nœud suivant ;
  - lors d'un changement de taille, les compteurs du nœud et les échantillons RTT sont
    conservés et continuent de s'accumuler.
- La case **« Ne pas fragmenter (DF) »** est décochée par défaut. Lorsqu'elle est cochée, la
  taille maximale du paquet est limitée à `1024` octets, la taille dynamique est désactivée
  et verrouillée, et la découverte de la route, la vérification principale et la surveillance
  utilisent `DontFragment`.
- Avant la vérification principale et une fois au démarrage de la surveillance, un
  précontrôle DF est effectué : une sonde par nœud avec les tailles
  `64 → 128 → 256 → 1024 → 1400` octets. Toutes les réponses et les erreurs sont consignées
  dans la console et le journal à titre d'information complémentaire ; le tableau et les
  diagnostics ne changent pas.
- Les sondes sont créées par des tâches distinctes avec un déclencheur `DO_NOT_FRAGMENT` ou
  un déclencheur ordinaire. L'algorithme de vérification finale et l'algorithme de
  surveillance résident dans des contrôleurs indépendants.
- Nombre de tentatives configurable par nœud : `1–1000`, `100` par défaut — pour le rapport
  à fournir au fournisseur et la mesure du pourcentage de pertes.
- Les cases sont regroupées dans la partie gauche du panneau de saisie sur une seule ligne ;
  la colonne de droite est plus étroite et le panneau lui-même occupe moins de hauteur.
- La case **« Mode de surveillance continue »** : la taille du paquet est lue une seule fois
  dans le champ, le nombre de tentatives n'est pas utilisé. Après la découverte de la route,
  chaque nœud est interrogé par un minuteur distinct une fois par seconde. Les nœuds dont le
  diagnostic est `ICMP désactivé` ne sont pas interrogés ; la limite de taille du paquet
  n'exclut pas un nœud de la surveillance. L'arrêt s'effectue avec le bouton `Stop`.
- En mode surveillance, les données de chaque nœud sont mises à jour dans sa ligne existante ;
  aucune nouvelle ligne n'est créée pour les interrogations répétées.
- L'historique des statistiques est limité à `1 000 000` instantanés, le tampon de sortie à
  `10 000` lignes : en cas de dépassement, les premières entrées et lignes sont supprimées.
  Le journal passe à un nouveau fichier lorsqu'il atteint `4` Mo, le fichier précédent
  restant inchangé. L'écriture du journal et le traitement des statistiques sont effectués
  par des workers distincts, sans occuper les threads d'interrogation ICMP.
- Statistiques précises pour chaque nœud : ping moyen, minimal, maximal et dernier, nombre
  de paquets envoyés/reçus, pourcentage de pertes. La moyenne est calculée à partir de toutes
  les mesures RTT enregistrées, et non de valeurs approximatives.
- À `100%` de pertes, le nœud reçoit le statut `aucune réponse` ; après `10` échecs
  consécutifs, la vérification du nœud s'arrête et il est ignoré pour les tailles de paquet
  suivantes.
- Les pertes sont affichées dans la colonne « État » sous forme de pourcentage, la cause
  figurant dans une colonne distincte « Diagnostic ». Les vérifications répétées des nœuds
  précédents ne sont pas effectuées.
- Langues de l'interface : `En`, `Cn`, `Fr`, `De`, `Ru`, `Jp`. `En` par défaut ; le choix est
  enregistré dans `settings.json`. Dans l'interface graphique, la langue se change via la
  liste du panneau de droite sans redémarrage, dans la console via l'option
  `--language <en|cn|fr|de|ru|jp>`. Les traductions se trouvent dans `localizations/*.json`
  à côté du fichier exécutable et sont modifiables sans recompilation ; les clés inconnues
  restent en anglais. Le journal enregistré dans un fichier est toujours en anglais.
- Le tableau de statistiques est placé au-dessus du journal sur toute la largeur de la
  fenêtre ; entre les deux se trouve un séparateur horizontal redimensionnable par
  glisser-déposer. Sa position est enregistrée séparément des largeurs des colonnes.
  Une colonne distincte « Diagnostic » affiche : `ICMP désactivé`, `Taille ICMP tronquée`,
  `Anti-inondation ICMP` ou `—` en l'absence de signe. Le diagnostic est également consigné
  dans le journal.
- Choix de la taille de police du journal, du délai en cas de perte et bouton `Run`/`Stop`
  de grande taille dans le panneau de droite : pendant une vérification, le bouton arrête
  l'exécution en cours.
- Mode plein écran : `F11` pour activer/désactiver, `Esc` pour quitter.
- Thème sombre par défaut avec un commutateur dans le panneau de gauche ; le choix est
  enregistré dans les paramètres. Les cases restent standards et changent de couleur avec
  le thème ; le tableau utilise la palette du thème et les nœuds non vérifiés apparaissent
  en gris dans les thèmes sombre et clair.
- À la fermeture de la fenêtre, un fichier distinct `ui-state.json` enregistre tous les
  champs de saisie, les cases, les largeurs des colonnes du tableau, la position verticale
  du séparateur (pourcentage de la hauteur du tableau) et l'état de la fenêtre : agrandie
  selon des valeurs en pixels (`x, y, x2, y2`) ou maximisée. Au démarrage, l'état est
  restauré ; un fichier endommagé est supprimé et le programme démarre avec les valeurs
  par défaut.
- L'argument de lancement `reset` (par exemple `TraceDiag.exe reset`) supprime forcément
  l'état de l'interface enregistré avant le chargement.
- Icône propre `cable_24dp.ico`, journalisation dans un fichier et enregistrement des
  paramètres au format JSON.

## Console

```text
TraceDiag.Console run    --host <ip|domain> [--packet-size <n>] [--attempts <n>] [--loss-delay <ms>] [--dynamic] [--df] [--format plain|csv|json] [--language en|cn|fr|de|ru|jp]
TraceDiag.Console monitor --host <ip|domain> [--packet-size <n>] [--loss-delay <ms>] [--df] [--format plain|csv|json] [--language en|cn|fr|de|ru|jp]
TraceDiag.Console reset [--language en|cn|fr|de|ru|jp]
```

- Valeurs par défaut : taille du paquet `1024`, `100` tentatives, délai de `1000` ms.
- `--format plain` — texte, `csv` — lignes CSV, `json` — JSON Lines avec le champ `kind`.
- `--language` localise l'aide, les erreurs, les statuts, les lignes de progression de la
  vérification et le tableau final. Le journal enregistré dans un fichier reste en anglais :
  le texte canonique et les arguments sont séparés de la traduction affichée ; les noms des
  options et les commandes ne sont pas traduits. La console affiche le texte en UTF-8 (avec
  les anciens terminaux Windows, `chcp 65001` peut être nécessaire).
- La progression de la vérification est affichée dans la console ligne par ligne, au moment
  même de l'écriture dans le journal fichier, sans accumulation jusqu'à la fin de la
  vérification. Le fichier est complété par la même ligne via `AsyncLogWriter` : les lignes
  déjà écrites sont donc conservées en cas d'annulation ou de fermeture.
- Une fois la vérification terminée (y compris après `Ctrl+C`), un seul tableau
  récapitulatif (équivalent localisé de `Summary:`) est imprimé en dernier, avec les dernières
  valeurs pour chaque TTL.
- L'application WinForms fonctionne comme auparavant : le tableau est mis à jour directement
  dans l'interface.
- En cas de faute de frappe dans le nom d'une option, la console suggère le nom correct le
  plus proche et affiche la liste complète des options pour comparaison.
- `Ctrl+C` arrête la surveillance ; codes de sortie : `0` succès, `1` erreur, `2` arguments
  invalides, `130` annulation.
- La console utilise les mêmes services, journaux et rotation que l'application WinForms.
- Sous Linux, les sondes ICMP d'un utilisateur ordinaire peuvent nécessiter
  `net.ipv4.ping_group_range` ou la capacité `CAP_NET_RAW`.

## Configuration requise

- WinForms : Windows 7 SP1 / Server 2008 R2 SP1 ou version ultérieure, .NET Framework 4.8.
- Console `net48` : Windows 7 SP1 / Server 2008 R2 SP1 ou version ultérieure,
  .NET Framework 4.8.
- Console `net10.0` : Windows Server 2012+, Ubuntu 22.04/24.04/26.04 et autres systèmes
  pris en charge par .NET 10 ; le paquet Linux est autonome (self-contained), aucun runtime
  distinct n'est nécessaire.
- Pour la compilation : SDK .NET 10 ; pour le MSI : WiX 5 (`dotnet tool restore`), pour le
  deb : WSL avec `dpkg-deb`. L'installation de .NET Framework 4.8 sur les anciennes versions
  de Windows nécessite les droits d'administrateur et un installateur local provenant d'un
  dépôt alternatif (voir ci-dessous).

## Compilation et exécution

```powershell
dotnet build TraceDiag.slnx -c Release
.\TraceDiag\bin\Release\net48\TraceDiag.exe
.\TraceDiag.Console\bin\Release\net10.0\TraceDiag.Console.exe --help
```

## Tests

```powershell
dotnet test TraceDiag.slnx -c Release
```

**252 exécutions de tests** (174 WinForms + 39 tests console sur `net48` et `net10.0`)
couvrent le planificateur de tailles de paquets, les statistiques des nœuds, le mappage des
réponses ICMP, la configuration, la journalisation, la sauvegarde de l'état de l'interface,
la localisation dans les six langues, l'analyse des arguments de la console, les formats de
sortie, les modules de diagnostic et le cycle complet de `TraceDiagnosticsService` avec des
fakes de test pour ICMP, DNS et le journaliseur.

## Versions prêtes à l'emploi

Dernière version : [Version 1.8.4](https://github.com/warriorexclusive-dev/TraceDiag/releases/tag/v1.8.4)

- [TraceDiag.exe](https://github.com/warriorexclusive-dev/TraceDiag/releases/download/v1.8.4/TraceDiag.exe)
- [TraceDiag-install.zip](https://github.com/warriorexclusive-dev/TraceDiag/releases/download/v1.8.4/TraceDiag-install.zip)

Historique des modifications : [ReleaseNotes.md](ReleaseNotes.md) ; les traductions se
trouvent dans `docs/<langue>/`.

## Installation sur un PC vierge

Archives (installation par utilisateur et « décompresser et exécuter ») :

```powershell
cd installer
.\Publish-TraceDiag.ps1
```

Le script publie WinForms et la console `net48` dans `installer\App`, la console `net10.0`
autonome dans `installer\ConsoleLinux` (`linux-x64`) et `installer\ConsoleLinux-arm64`,
copie `localizations\` dans tous les dossiers et crée les archives :

- `installer\TraceDiag-install.zip` — Windows : interface graphique + console `net48` et
  installateur ;
- `installer\TraceDiag-console-linux-x64.tar.gz` — console pour Linux/amd64 ;
- `installer\TraceDiag-console-linux-arm64.tar.gz` — console pour Linux/arm64.

Sous Windows, décompressez la première archive et exécutez `Install-TraceDiag.cmd`.
L'installateur vérifie .NET Framework 4.8 dans le registre. S'il est absent, placez
`dotNetFx48_Full.exe` à côté du script (le script de publication l'ajoutera à l'archive) ou
transmettez `-Framework48InstallerPath` / `-Framework48Url` pour un dépôt alternatif. Le
téléchargement direct depuis le site de Microsoft n'est pas utilisé. L'installateur copie
les fichiers dans `%LOCALAPPDATA%\Programs\TraceDiag` et crée un raccourci dans le menu
« Démarrer ».

Sous Linux, décompressez le tar.gz souhaité et exécutez `bash install-console.sh` ; le script
copie les fichiers autonomes dans `~/.local/share/TraceDiag` et crée un lien symbolique
dans `~/.local/bin`. Aucun runtime .NET distinct n'est à télécharger.

Paquets système (par machine) :

```powershell
cd installer
dotnet tool restore
.\Build-TraceDiag-Packages.ps1
```

Le résultat se trouve dans `installer\dist` : `TraceDiag-<version>-x86.msi`,
`TraceDiag-<version>-x64.msi`, `tracediag_<version>_amd64.deb` et
`tracediag_<version>_arm64.deb`. Le MSI contient l'interface graphique et la console de
l'architecture correspondante, `localizations\` et des raccourcis dans le menu
« Démarrer » ; la présence de .NET Framework 4.8 est vérifiée avant l'installation. Les
paquets deb installent la console dans `/usr/share/tracediag` et créent la commande
`tracediag` via `/usr/bin`. Pour les deb, WSL avec `dpkg-deb` est requis.

Pour plus de détails : [installer/README-Installer.md](../../installer/README-Installer.md).
Les fichiers compilés (`installer\App`, `installer\ConsoleLinux*`, `installer\build`,
`installer\dist`, archives) ne sont pas inclus dans Git.

## Structure du projet

```text
TraceDiag.Core/              netstandard2.0, logique métier indépendante de l'UI
  Models/                    données, tâches de sonde et calcul des statuts des nœuds
  Services/                  ICMP, DNS, configuration, journalisation
  Services/Abstractions/     contrats des contrôleurs, tâches et déclencheurs
  Services/Diagnostics/      route, précontrôle DF, vérification des nœuds et diagnostic des pertes
  Services/Monitoring/       minuteurs indépendants par nœud et lancement de la surveillance
  Services/Logging/          rotation des fichiers et écriture du journal en arrière-plan
  Compatibility/             polyfills pour netstandard2.0 et net48
  Localization/              lecture des traductions JSON et choix de la langue
  Strings.cs                 chaînes canoniques de l'interface et du journal
localizations/               en/cn/fr/de/ru/jp.json — traductions de l'interface
docs/                        en/cn/fr/de/ru/jp — README.md et ReleaseNotes.md en six langues
TraceDiag/                   net48, hôte WinForms
  Ui/                        fabriques de contrôles, mise en page, threads de lancement et politique de taille
TraceDiag.Console/           net48 + net10.0, hôte console
TraceDiag.Tests/             net48, tests WinForms et du noyau
TraceDiag.Console.Tests/     net48 + net10.0, tests de la console et de compatibilité
installer/
  Install-TraceDiag.ps1      installation sur Windows (par utilisateur)
  install-console.sh         installation de la console sur Linux
  Publish-TraceDiag.ps1      création des archives
  Build-TraceDiag-Packages.ps1  création des MSI (x86/x64) et deb (amd64/arm64)
  wix/                       sources MSI
  deb/                       script de création du deb dans WSL
.config/dotnet-tools.json    version figée de WiX 5
```

## Données de l'application

- Windows : `%LOCALAPPDATA%\TraceDiag\` (`settings.json`, `ui-state.json`, `logs\`).
- Linux : `~/.local/share/TraceDiag/` — les mêmes fichiers, créés au premier lancement de la
  console.
- Journaux : `tracediag-yyyyMMdd.log` ; à 4 Mo, les fichiers `tracediag-yyyyMMdd-1.log`,
  `tracediag-yyyyMMdd-2.log` et ainsi de suite sont créés.

## Licence

Apache License 2.0 — voir [LICENSE](../../LICENSE).

Créé par Enaleven 2026 à l'aide de Big Pickle
