<div align="center">

<img src="assets/logo.svg" alt="Soundpost" width="120" />

# Soundpost

### Un centro. Todo tu sonido.

**Una sola consola para el audio de Windows: cambia de dispositivo de salida, mezcla cada aplicación y *ve* tu sonido, todo en un mismo lugar. Local primero, sin cuenta, sin telemetría.**

[![CI](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml/badge.svg)](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml)
[![License: GPLv3](https://img.shields.io/badge/license-GPLv3-blue.svg)](LICENSE)
[![Windows 10 / 11](https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4?logo=windows&logoColor=white)](#obtenerlo)
[![.NET 9](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet&logoColor=white)](#)
[![Stars](https://img.shields.io/github/stars/sathvik-zoldyck/Soundpost?style=social)](https://github.com/sathvik-zoldyck/Soundpost/stargazers)

**[Hoja de ruta](ROADMAP.md) · [Arquitectura](ARCHITECTURE.md) · [SDK de complementos](PLUGIN_SDK.md) · [Contribuir](CONTRIBUTING.md) · [Debates](../../discussions)**

[English](README.md) · [简体中文](README.zh.md) · **Español** · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [日本語](README.ja.md) · [Русский](README.ru.md) · [한국어](README.ko.md)

<br/>

<img src="assets/media/dashboard.png" alt="El panel de Soundpost: dial de volumen maestro, selector de dispositivos de reproducción, mezclador por aplicación y medidores de salida en vivo" width="880" />

</div>

---

Windows reparte tu audio entre el control de volumen, el menú de dispositivos, el panel de sonido y un puñado de utilidades de terceros que no se hablan entre sí, y ninguna recuerda lo que querías. **Soundpost es el centro que faltaba.** Cada dispositivo y cada aplicación fluyen hacia una única consola: cambias, mezclas y enrutas, y todo llega a la salida correcta. Y cuando solo quieres escuchar, el visualizador convierte lo que suena en algo que vale la pena mirar.

## Funciones

- **Cambio instantáneo de dispositivo.** Cambia la salida o entrada predeterminada con un clic.
- **Mezclador por aplicación.** Volumen, silencio y medidores en vivo para cada aplicación que emite sonido.
- **Medición de salida en vivo.** Medidores de pico maestros y por aplicación con dinámica realista.
- **Visualizador.** Siete estilos en vivo —Ribbon, Aurora, Spectrum, Radial, Oscilloscope, Cymatics y un modo de imagen personalizada— que reaccionan a tu audio a 60 fps, con sensibilidad, suavizado, brillo y paleta ajustables.
- **Superposición a pantalla completa.** Saca el visualizador sobre un vídeo musical con fondo sólido, atenuado o totalmente transparente.
- **Panel rápido.** Un desplegable compacto en la bandeja para lo que haces en plena reunión, sin abrir la consola completa.
- **Cuatro temas.** Indigo, Black & Red, Rich Gold y Cherry Blossom, conmutables en vivo desde Ajustes.
- **Local y privado.** Sin cuenta, sin nube, sin telemetría. Todo se queda en tu equipo.

## Míralo

<div align="center">

<img src="assets/media/themes.png" alt="Soundpost en sus cuatro temas: Indigo, Black and Red, Rich Gold y Cherry Blossom" width="880" />

<sub><b>Cuatro temas, cambiados en vivo.</b> Indigo, Black &amp; Red, Rich Gold, Cherry Blossom.</sub>

<br/><br/>

<img src="assets/media/quick-panel.png" alt="El desplegable Panel rápido con volumen maestro, cambio de salida y controles por aplicación" width="320" />

<sub><b>Panel rápido.</b> Volumen maestro, cambio de salida y silencio por aplicación, directo desde la bandeja.</sub>

</div>

## Obtenerlo

Soundpost es para **Windows 10 y 11**. Descarga una compilación desde [Releases](../../releases) cuando haya una publicada, o compila desde el código fuente:

```bash
git clone https://github.com/sathvik-zoldyck/Soundpost.git
cd Soundpost
dotnet run --project src/Soundpost.App
```

Necesitas el [SDK de .NET 9](https://dotnet.microsoft.com/download). El flujo de publicación produce un `Soundpost.exe` de un solo archivo y autónomo (no requiere instalar .NET).

## Cómo funciona

Cada aplicación y dispositivo fluyen hacia un único centro; lo enrutas, lo mezclas y lo automatizas, y llega a la salida correcta. Una única capa de Core Audio envuelve las API COM de Windows para que el resto de la aplicación nunca las toque directamente, lo que mantiene la consola fluida y el manejo del audio aislado y comprobable.

## Extiéndelo

Soundpost está hecho para ampliarse.

- **Visualizadores.** Un estilo es una clase que implementa `IVisualizerRenderer` — consulta [visualizers/](visualizers/). Escríbelo, regístralo y aparece en la barra de estilos.
- **Temas.** Las paletas son diccionarios autónomos; un tema nuevo es un archivo nuevo más una muestra.
- **Complementos.** Una superficie de complementos basada en eventos está en la hoja de ruta — consulta [PLUGIN_SDK.md](PLUGIN_SDK.md).

## Hoja de ruta

Ya disponible: cambio de dispositivo, mezclador y medidores por aplicación, el visualizador, la bandeja y el Panel rápido, persistencia y temas. A continuación: escenas y perfiles, una capa de automatización, enrutamiento por aplicación y diagnósticos en lenguaje claro. El plan completo está en [ROADMAP.md](ROADMAP.md).

## Contribuir

Las contribuciones son bienvenidas, desde un nuevo visualizador hasta una corrección de errores. Empieza por [CONTRIBUTING.md](CONTRIBUTING.md), abre un [issue](../../issues) o saluda en [Debates](../../discussions). Si Soundpost te resulta útil, una estrella ayuda a que otras personas lo encuentren.

## Licencia

[GPLv3](LICENSE). Software libre y de código abierto.
