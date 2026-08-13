<div align="center">

<img src="assets/logo.svg" alt="Soundpost" width="120" />

# Soundpost

### Un centre. Tout votre son.

**Une seule console pour l'audio de Windows : changez de périphérique de sortie, mixez chaque application et *voyez* votre son — le tout au même endroit. Local d'abord, sans compte, sans télémétrie.**

[![CI](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml/badge.svg)](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml)
[![License: GPLv3](https://img.shields.io/badge/license-GPLv3-blue.svg)](LICENSE)
[![Windows 10 / 11](https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4?logo=windows&logoColor=white)](#obtenir)
[![.NET 9](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet&logoColor=white)](#)
[![Stars](https://img.shields.io/github/stars/sathvik-zoldyck/Soundpost?style=social)](https://github.com/sathvik-zoldyck/Soundpost/stargazers)

**[Feuille de route](ROADMAP.md) · [Architecture](ARCHITECTURE.md) · [SDK de plugins](PLUGIN_SDK.md) · [Contribuer](CONTRIBUTING.md) · [Discussions](../../discussions)**

[English](README.md) · [简体中文](README.zh.md) · [Español](README.es.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt.md) · **Français** · [Deutsch](README.de.md) · [日本語](README.ja.md) · [Русский](README.ru.md) · [한국어](README.ko.md)

<br/>

<img src="assets/media/dashboard.png" alt="Le tableau de bord Soundpost : molette de volume principal, sélecteur de périphériques de lecture, mixeur par application et vumètres de sortie en direct" width="880" />

</div>

---

Windows éparpille votre audio entre la fenêtre de volume, le menu des périphériques, le panneau de configuration du son et une poignée d'utilitaires tiers qui ne se parlent pas — et aucun ne se souvient de ce que vous vouliez. **Soundpost est le centre qui manquait.** Chaque périphérique et chaque application convergent vers une seule console : vous commutez, mixez et routez, et tout arrive sur la bonne sortie. Et quand vous voulez simplement écouter, le visualiseur transforme ce qui joue en quelque chose qui mérite d'être regardé.

## Fonctionnalités

- **Changement de périphérique instantané.** Modifiez la sortie ou l'entrée par défaut en un clic.
- **Mixeur par application.** Volume, sourdine et vumètres en direct pour chaque application qui émet du son.
- **Mesure de sortie en direct.** Vumètres de crête principaux et par application, avec une dynamique réaliste.
- **Visualiseur.** Sept styles en direct — Ribbon, Aurora, Spectrum, Radial, Oscilloscope, Cymatics et un mode Image personnalisée — qui réagissent à votre audio à 60 ips, avec sensibilité, lissage, halo et palette réglables.
- **Superposition plein écran.** Détachez le visualiseur par-dessus une vidéo musicale, avec un fond opaque, atténué ou totalement transparent.
- **Panneau rapide.** Un menu compact dans la zone de notification pour les gestes du quotidien en pleine réunion, sans ouvrir la console complète.
- **Quatre thèmes.** Indigo, Black & Red, Rich Gold et Cherry Blossom — commutables en direct depuis les Paramètres.
- **Local et privé.** Sans compte, sans cloud, sans télémétrie. Tout reste sur votre machine.

## Aperçu

<div align="center">

<img src="assets/media/themes.png" alt="Soundpost dans ses quatre thèmes : Indigo, Black and Red, Rich Gold et Cherry Blossom" width="880" />

<sub><b>Quatre thèmes, changés en direct.</b> Indigo, Black &amp; Red, Rich Gold, Cherry Blossom.</sub>

<br/><br/>

<img src="assets/media/quick-panel.png" alt="Le menu Panneau rapide avec volume principal, changement de sortie et commandes par application" width="320" />

<sub><b>Panneau rapide.</b> Volume principal, changement de sortie et sourdine par application — directement depuis la zone de notification.</sub>

</div>

## Obtenir

Soundpost cible **Windows 10 et 11**. Téléchargez une version depuis les [Releases](../../releases) dès qu'une est publiée, ou compilez depuis les sources :

```bash
git clone https://github.com/sathvik-zoldyck/Soundpost.git
cd Soundpost
dotnet run --project src/Soundpost.App
```

Il vous faut le [SDK .NET 9](https://dotnet.microsoft.com/download). Le workflow de publication produit un `Soundpost.exe` autonome en un seul fichier (aucune installation de .NET requise).

## Comment ça marche

Chaque application et périphérique converge vers un centre unique ; vous le routez, le mixez et l'automatisez, et il arrive sur la bonne sortie. Une seule couche Core Audio enveloppe les API COM de Windows afin que le reste de l'application ne les touche jamais directement, ce qui garde la console fluide et le traitement audio isolé et testable.

## Étendre

Soundpost est conçu pour être enrichi.

- **Visualiseurs.** Un style est une classe qui implémente `IVisualizerRenderer` — voir [visualizers/](visualizers/). Écrivez-le, enregistrez-le et il apparaît dans la barre de styles.
- **Thèmes.** Les palettes sont des dictionnaires autonomes ; un nouveau thème, c'est un nouveau fichier plus un échantillon.
- **Plugins.** Une surface de plugins pilotée par les événements est sur la feuille de route — voir [PLUGIN_SDK.md](PLUGIN_SDK.md).

## Feuille de route

Disponible maintenant : changement de périphérique, mixeur et vumètres par application, le visualiseur, la zone de notification et le Panneau rapide, la persistance et les thèmes. Ensuite : scènes et profils, une couche d'automatisation, le routage par application et des diagnostics en langage clair. Le plan complet se trouve dans [ROADMAP.md](ROADMAP.md).

## Contribuer

Les contributions sont les bienvenues, d'un nouveau visualiseur à une correction de bug. Commencez par [CONTRIBUTING.md](CONTRIBUTING.md), ouvrez une [issue](../../issues) ou dites bonjour dans les [Discussions](../../discussions). Si Soundpost vous est utile, une étoile aide les autres à le découvrir.

## Licence

[GPLv3](LICENSE). Logiciel libre et open source.
