# Release Notes

> Языки: [English](../en/ReleaseNotes.md) · [中文](../cn/ReleaseNotes.md) · [Français](../fr/ReleaseNotes.md) ·
> [Deutsch](../de/ReleaseNotes.md) · [Русский](../ru/ReleaseNotes.md) · [日本語](../jp/ReleaseNotes.md)

Historique des modifications de TraceDiag. Les versions compilées sont publiées sur la page
[Releases](https://github.com/warriorexclusive-dev/TraceDiag/releases).

## 1.9.11

- Ajout des tailles d'icônes 16/32 px (32 par défaut), des journaux détaillés du précontrôle, de la route et de la cible dans le tableau ; un délai interne n'est plus signalé comme une annulation utilisateur.

## 1.9.10

- Correction de la langue dans les paramètres, de l'alignement du libellé de l'hôte distant et du chargement du dns-servers.json fourni.

## 1.9.9

- Nouvelle fenêtre principale : hôte, icônes lecture/arrêt/paramètres, tableau, sortie et statut ; les autres réglages dans une fenêtre séparée. Adaptateur via socket RAW, éditeur DNS, options --adapter, --no-asn et --dns-file.

## 1.9.8

- Le DNS système de la carte réseau est obligatoire partout, y compris IPv6 ; les résolveurs alternatifs sont actifs par défaut et désactivables par --no-alt-dns ou l'interface. La barre de titre suit le thème.

## 1.9.7

- La console demande un hôte sans arguments et signale une saisie invalide ; les clés de ligne de commande sont regroupées dans un module commun.

## 1.9.6

- Précontrôle asynchrone des itinéraires alternatifs : dns-servers.json, 3000 ms par requête, délai global 150 s, 4 signatures de flux (8 en mode détaillé), repli en cas d'échec.

## 1.9.5

- ASN via TXT DNS Team Cymru et colonne dédiée ; propriétaire du bloc depuis RIPEstat par défaut, désactivable avec --no-ripe. Les sources externes sont désactivées pour les cibles privées.

## 1.9.4

- Première version de la recherche ASN et d'une colonne autonome.

## 1.9.3

- Diagnostics automatiques étendus : trou noir MTU, asymétrie de route, blocage ICMP par les routeurs intermédiaires, filtrage par port/NAT.

## 1.9.2

- Plusieurs diagnostics sur une même ligne : troncature de paquet, anti-inondation ICMP et équilibrage ECMP.

## 1.9.1

- Détection des changements de route et de l'équilibrage ECMP selon les différentes sources IP des réponses d'un même saut.

## 1.9.0

- Ajout des langues d'interface `En`, `Cn`, `Fr`, `De`, `Ru`, `Jp` ; `En` par défaut.
  Le choix est enregistré dans `settings.json`, modifiable dans l'interface graphique sans
  redémarrage et défini dans la console par l'option
  `--language <en|cn|fr|de|ru|jp>`. Les traductions sont stockées dans `localizations/*.json`
  à côté du fichier exécutable et ne nécessitent pas de recompilation. Toutes les chaînes
  visibles de l'interface graphique et de la console sont localisées, y compris la progression
  de la vérification et le tableau final ; le journal enregistré dans un fichier reste en
  anglais grâce à la séparation entre texte canonique et traduction affichée.
- Dans l'interface graphique Windows, un séparateur redimensionnable par glisser-déposer a
  été ajouté entre le tableau et le journal : la hauteur des panneaux est modifiée à la
  souris, la position est enregistrée dans `ui-state.json` sous forme de pourcentage (avec
  limitation de la plage) et restaurée au démarrage.
- La logique métier a été déplacée dans `TraceDiag.Core` (`netstandard2.0`) ; l'hôte WinForms
  a été migré vers .NET Framework 4.8 et fonctionne sur Windows 7 SP1 / Server 2008 R2 SP1.
- Ajout d'un hôte console `TraceDiag.Console` pour `net48` (Windows Server 2008 R2 sans
  interface graphique) et `net10.0` (Ubuntu 22.04+ et Windows Server 2012+). Commandes `run`,
  `monitor` et `reset`, formats de sortie `plain`, `csv` et `json`, arrêt par `Ctrl+C`.
  La progression de la vérification est affichée ligne par ligne dans la console en même
  temps qu'elle est écrite ligne par ligne dans le journal fichier via `AsyncLogWriter` ;
  une fois la vérification terminée, un seul tableau récapitulatif `Summary:` est imprimé
  en dernier. Le comportement de WinForms n'a pas changé.
- Les options inconnues proposent le nom le plus proche et la liste complète des options ;
  toutes les chaînes utilisateur et du journal de la console ont été ajoutées à `Strings.cs` ;
  la console réutilise les services, le journal fichier et la rotation du noyau.
- Pour les anciennes cibles framework, des polyfills et des remplacements d'API ont été
  ajoutés (`PeriodicTimer`, `TickCount64`, `Math.Clamp`, `IReadOnlySet`,
  `ArgumentNullException.ThrowIfNull`, `Dns.GetHostAddressesAsync` avec jeton).
- L'installateur Windows vérifie .NET Framework 4.8 dans le registre ; en son absence, il
  exige les droits d'administrateur et récupère l'installateur hors ligne depuis un fichier
  local ou un dépôt alternatif (`-Framework48InstallerPath` / `-Framework48Url`) ; aucun
  téléchargement depuis le site de Microsoft n'est effectué. La console Linux est fournie
  sous forme d'archive tar.gz autonome avec `install-console.sh` ; aucun runtime distinct
  n'est nécessaire.
- Ajout des paquets système : MSI pour Windows x86 et x64 (avec vérification de .NET
  Framework 4.8 et raccourcis dans le menu « Démarrer ») et deb pour Linux amd64 et arm64
  (console autonome et commande `tracediag`). La compilation passe par
  `installer\Build-TraceDiag-Packages.ps1`. Les archives ont été complétées par une variante
  de la console pour `linux-arm64`.
- Le README et les Release Notes ont été traduits en `En`, `Cn`, `Fr`, `De`, `Ru`, `Jp` et
  se trouvent dans des fichiers distincts dans `docs/<langue>/`.
- L'interface graphique pour Linux est prévue dans une évolution ultérieure : l'architecture
  de Core est prête à accueillir un second adaptateur d'interface sans modifier la logique
  métier.

## 1.8.4

- Ajout de la case « Ne pas fragmenter (DF) », décochée par défaut. Lorsqu'elle est cochée,
  la taille du paquet est limitée à `1024` octets, la taille dynamique est désactivée et
  verrouillée, et toutes les sondes de la vérification principale et de la surveillance
  utilisent l'indicateur `DontFragment`.
- La taille maximale du paquet ICMP dans le champ de saisie a été portée à la valeur sûre de
  `65500` octets ; le libellé du champ est généré à partir des mêmes limites que la saisie
  elle-même.
- Avant la vérification principale et une fois au démarrage de la surveillance, un
  précontrôle DF est effectué : une sonde par nœud détecté pour les tailles
  `64 → 128 → 256 → 1024 → 1400`. Les succès et les erreurs sont consignés dans la console
  et le journal fichier, sans modifier les compteurs ni les diagnostics du tableau.
- Les sondes de la vérification ordinaire, de la surveillance et de la découverte de la route
  sont créées sous forme de tâches distinctes avec déclencheur ; les algorithmes de
  vérification et de surveillance ont été extraits dans des contrôleurs indépendants.

## 1.8.3

- Les cases sont à nouveau standards : le rendu personnalisé a été supprimé, ainsi que le
  texte superposé.
- Les couleurs des cases standards changent avec le thème via la palette.
- Les nœuds inactifs apparaissent en gris dans les thèmes sombre et clair.

## 1.8.2

- Dans le thème sombre, les cases et le tableau sont dessinés avec la palette du thème :
  texte actif en blanc, fond et lignes sombres.
- Les nœuds non vérifiables avec le diagnostic `ICMP désactivé` sont mis en évidence en
  gris.
- Les contrôles désactivés utilisent eux aussi du gris plutôt que du noir.

## 1.8.1

- Les cases ont été déplacées dans la partie gauche du panneau de saisie et disposées sur
  une seule ligne.
- La colonne de droite est devenue plus étroite et le panneau supérieur occupe moins de
  hauteur.

## 1.8.0

- Correction de l'alignement du libellé « Nombre de tentatives (1-1000) » par rapport au
  champ de saisie dans la fenêtre normale, lors de l'agrandissement et en mode plein écran.
- Ajout de la case « Mode de surveillance continue ». La taille du paquet est lue une seule
  fois dans le champ, le nombre de tentatives n'est pas utilisé. Après la découverte de la
  route, chaque nœud disponible est interrogé par son propre minuteur une fois par seconde ;
  les nœuds dont le diagnostic est `ICMP désactivé` sont exclus, tandis que ceux soumis à
  une limite de taille de paquet continuent d'être interrogés.
- Le bouton `Stop` arrête la surveillance ; l'état de la case est enregistré dans
  `ui-state.json`.
- En mode surveillance, les interrogations répétées mettent à jour la ligne du nœud sans
  ajouter de nouvelles lignes.
- L'historique des statistiques est limité à 1 000 000 d'instantanés, le tampon de sortie à
  10 000 lignes. Lorsque les limites sont dépassées, les entrées et les lignes les plus
  anciennes sont supprimées.
- Le journal fichier passe à un nouveau fichier lorsqu'il atteint 4 Mo ; le fichier
  précédent reste inchangé. L'écriture du journal et le traitement des statistiques sont
  effectués par des workers distincts, sans bloquer l'interrogation des sockets.

## 1.7.0

- Suppression de la vérification supplémentaire des pertes par requêtes ICMP répétées vers
  les nœuds précédent et courant.
- La colonne « État » affiche le pourcentage de pertes issu de la vérification principale,
  tandis que la cause des pertes reste dans une colonne distincte « Diagnostic ».
- Suppression des statuts erronés « endommagé : perte entre les nœuds » et « perte également
  sur le nœud précédent » lorsque les grands paquets ICMP ne traversent pas le nœud.

## 1.6.0

- À la fermeture de la fenêtre, tous les champs de saisie, les cases, les largeurs des
  colonnes du tableau et l'état de la fenêtre sont enregistrés : taille et position en pixels
  ou maximized.
- Au démarrage, l'état de l'interface est restauré depuis un fichier distinct
  `ui-state.json`. Un fichier endommagé est supprimé et le programme démarre avec les
  valeurs par défaut.
- L'argument de lancement `reset` supprime forcément l'état de l'interface enregistré avant
  le chargement.

## 1.5.0

- Ajout d'une colonne « Diagnostic » dans le tableau de statistiques des nœuds.
- Un nœud reçoit le diagnostic `ICMP désactivé` s'il ne répond à aucune taille de paquet ou
  s'il a été ignoré lors de la découverte de la route.
- Le diagnostic `Taille ICMP tronquée` est attribué lorsque les grandes tailles de paquet
  cessent de répondre ou donnent plus de pertes que les petites tailles sur le même nœud.
- Le diagnostic `Anti-inondation ICMP` est attribué en cas de pertes partielles de paquets
  sans signe de limitation de taille. Les diagnostics sont également consignés dans le
  journal.

## 1.4.1

- Lors du passage à la taille de paquet suivante, les compteurs de paquets envoyés et reçus,
  le pourcentage de pertes et les échantillons RTT du nœud ne sont plus réinitialisés.
- Le ping moyen, minimal et maximal est calculé à partir de toutes les mesures accumulées
  pour le nœud, et non de la seule dernière taille de paquet.

## 1.4.0

- Pendant une vérification, le bouton `Run` affiche `Stop` et arrête l'exécution en cours.
- Les tailles de paquet sont parcourues nœud par nœud : d'abord toutes les valeurs pour un
  nœud (`64 → 128 → 256 → 512 → 1024`), puis le nœud suivant.
- Le champ de délai a été renommé « Délai sur perte, ms ». Le délai ne s'applique qu'après
  un paquet perdu ; les réponses sont traitées sans attente.
- Après la première perte, le délai s'applique à toutes les vérifications ultérieures de ce
  nœud, y compris le contrôle des pertes. La valeur `0` désactive le délai.

## 1.3.0

- Ajout d'un champ « Délai ICMP, ms » dans le panneau de droite ; la valeur par défaut est
  `1000` ms.
- Le délai est appliqué après chaque réponse ICMP lors de la découverte de la route, des
  vérifications principales des nœuds et du contrôle des pertes. Cela réduit le nombre de
  pertes erronées sur les équipements dotés d'une protection contre les inondations ICMP.
- La valeur `0` désactive le délai ; la limite supérieure du champ est de `60000` ms.
- La couleur des cases est désormais définie par la palette du thème, de la même manière
  pour les thèmes sombre et clair.

## 1.2.1

- Correction d'une erreur après laquelle un nouveau lancement avec un nœud valide ne
  fonctionnait pas si la tentative précédente s'était terminée à l'étape de validation de la
  saisie.
- Les adresses non spécifiées (`0.0.0.0`, `::`) ne sont plus acceptées comme nœud de
  destination.
- Le contrôle des pertes n'envoie plus de requêtes ICMP aux nœuds fictifs sans réponse :
  c'est le dernier nœud ayant répondu qui sert de référence pour la comparaison.

## 1.2.0

- Ajout d'un thème sombre avec un commutateur dans le panneau de droite ; le thème sombre
  est activé par défaut.
- Le choix du thème est enregistré dans `settings.json` et restauré au démarrage.
- `cable_24dp.ico` sert d'icône à l'application.

## 1.1.2

- Le nombre de tentatives par défaut est passé de `4` à `100`.

## 1.1.1

- La recherche d'hôtes a été extraite dans un modèle distinct `HostDiscovery`.
- La recherche est effectuée séquentiellement par TTL : jusqu'à `3` tours avec un intervalle
  de `1` s, paquet de `64` octets, indicateur `DontFragment` et délai d'attente de `5` s.
- Les TTL sans réponse sont ajoutés au tableau avec le statut `aucune réponse` et marqués
  comme à ignorer ; DNS et la vérification principale ne touchent pas ces entrées.

## 1.1.0

- La limite de traçage a été portée à `255` sauts.
- Correction d'une surestimation des compteurs dans le tableau : les vérifications de
  contrôle des pertes ne sont plus ajoutées aux paquets envoyés et reçus.
- Ajout d'un installateur du .NET 10 Desktop Runtime et d'une build de distribution pour
  un PC vierge.
