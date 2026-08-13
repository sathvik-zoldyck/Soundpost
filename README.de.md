<div align="center">

<img src="assets/logo.svg" alt="Soundpost" width="120" />

# Soundpost

### Ein Zentrum. Jeder Klang.

**Eine Konsole für das Windows-Audio: Ausgabegerät wechseln, jede App mischen und deinen Klang *sehen* — alles an einem Ort. Local-first, kein Konto, keine Telemetrie.**

[![CI](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml/badge.svg)](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml)
[![License: GPLv3](https://img.shields.io/badge/license-GPLv3-blue.svg)](LICENSE)
[![Windows 10 / 11](https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4?logo=windows&logoColor=white)](#installieren)
[![.NET 9](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet&logoColor=white)](#)
[![Stars](https://img.shields.io/github/stars/sathvik-zoldyck/Soundpost?style=social)](https://github.com/sathvik-zoldyck/Soundpost/stargazers)

**[Roadmap](ROADMAP.md) · [Architektur](ARCHITECTURE.md) · [Plugin-SDK](PLUGIN_SDK.md) · [Mitwirken](CONTRIBUTING.md) · [Diskussionen](../../discussions)**

[English](README.md) · [简体中文](README.zh.md) · [Español](README.es.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt.md) · [Français](README.fr.md) · **Deutsch** · [日本語](README.ja.md) · [Русский](README.ru.md) · [한국어](README.ko.md)

<br/>

<img src="assets/media/dashboard.png" alt="Das Soundpost-Dashboard: Master-Lautstärkeregler, Wiedergabegeräte-Umschalter, App-Mixer und Live-Ausgangspegel" width="880" />

</div>

---

Windows verteilt deinen Ton auf das Lautstärke-Flyout, das Gerätemenü, die Sound-Systemsteuerung und eine Handvoll Drittanbieter-Tools, die nicht miteinander reden — und keines merkt sich, was du wolltest. **Soundpost ist das fehlende Zentrum.** Jedes Gerät und jede App fließen in eine einzige Konsole: Du schaltest um, mischst und routest, und alles landet an der richtigen Ausgabe. Und wenn du einfach nur hören willst, macht der Visualizer aus dem Gespielten etwas Sehenswertes.

## Funktionen

- **Sofortiges Geräte-Umschalten.** Standard-Ausgabe oder -Eingabe mit einem Klick ändern.
- **Mixer pro App.** Lautstärke, Stummschaltung und Live-Pegel für jede App, die Ton ausgibt.
- **Live-Ausgangspegel.** Master- und App-Spitzenpegel mit realer Ballistik.
- **Visualizer.** Sieben Live-Stile — Ribbon, Aurora, Spectrum, Radial, Oscilloscope, Cymatics und ein Modus für eigene Bilder — die mit 60 fps auf deinen Ton reagieren, mit einstellbarer Empfindlichkeit, Glättung, Glow und Palette.
- **Vollbild-Overlay.** Blende den Visualizer über ein Musikvideo — mit deckendem, abgedunkeltem oder komplett transparentem Hintergrund.
- **Quick Panel.** Ein kompaktes Tray-Flyout für die Handgriffe mitten im Meeting, ohne die volle Konsole zu öffnen.
- **Vier Themes.** Indigo, Black & Red, Rich Gold und Cherry Blossom — live in den Einstellungen umschaltbar.
- **Lokal und privat.** Kein Konto, keine Cloud, keine Telemetrie. Alles bleibt auf deinem Rechner.

## Ansehen

<div align="center">

<img src="assets/media/themes.png" alt="Soundpost in allen vier Themes: Indigo, Black and Red, Rich Gold und Cherry Blossom" width="880" />

<sub><b>Vier Themes, live gewechselt.</b> Indigo, Black &amp; Red, Rich Gold, Cherry Blossom.</sub>

<br/><br/>

<img src="assets/media/quick-panel.png" alt="Das Quick-Panel-Tray-Flyout mit Master-Lautstärke, Ausgabewechsel und App-Steuerung" width="320" />

<sub><b>Quick Panel.</b> Master-Lautstärke, Ausgabewechsel und App-Stummschaltung — direkt aus dem Tray.</sub>

</div>

## Installieren

Soundpost ist für **Windows 10 und 11**. Lade einen Build aus den [Releases](../../releases), sobald einer veröffentlicht ist, oder baue aus dem Quellcode:

```bash
git clone https://github.com/sathvik-zoldyck/Soundpost.git
cd Soundpost
dotnet run --project src/Soundpost.App
```

Du brauchst das [.NET 9 SDK](https://dotnet.microsoft.com/download). Der Release-Workflow erzeugt eine eigenständige `Soundpost.exe` als Einzeldatei (ohne .NET-Installation).

## Wie es funktioniert

Jede App und jedes Gerät fließen in ein einziges Zentrum; du routest, mischst und automatisierst, und alles landet an der richtigen Ausgabe. Eine einzige Core-Audio-Schicht kapselt die Windows-COM-APIs, sodass der Rest der App sie nie direkt berührt — das hält die Konsole flüssig und die Audioverarbeitung isoliert und testbar.

## Erweitern

Soundpost ist zum Erweitern gebaut.

- **Visualizer.** Ein Stil ist eine Klasse, die `IVisualizerRenderer` implementiert — siehe [visualizers/](visualizers/). Schreiben, registrieren — schon erscheint er in der Stilleiste.
- **Themes.** Paletten sind eigenständige Dictionaries; ein neues Theme ist eine neue Datei plus ein Farbmuster.
- **Plugins.** Eine ereignisgesteuerte Plugin-Schnittstelle steht auf der Roadmap — siehe [PLUGIN_SDK.md](PLUGIN_SDK.md).

## Roadmap

Jetzt verfügbar: Geräte-Umschalten, Mixer und Pegel pro App, der Visualizer, Tray und Quick Panel, Persistenz und Themes. Als Nächstes: Szenen und Profile, eine Automatisierungsschicht, App-Routing und Diagnosen in Klartext. Der vollständige Plan steht in [ROADMAP.md](ROADMAP.md).

## Mitwirken

Beiträge sind willkommen, vom neuen Visualizer bis zum Bugfix. Beginne mit [CONTRIBUTING.md](CONTRIBUTING.md), öffne ein [Issue](../../issues) oder sag Hallo in den [Diskussionen](../../discussions). Wenn Soundpost dir nützt, hilft ein Stern anderen, es zu finden.

## Lizenz

[GPLv3](LICENSE). Freie und quelloffene Software.
