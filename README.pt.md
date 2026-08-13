<div align="center">

<img src="assets/logo.svg" alt="Soundpost" width="120" />

# Soundpost

### Um centro. Todo o som.

**Um único console para o áudio do Windows: troque o dispositivo de saída, mixe cada aplicativo e *veja* o seu som — tudo em um só lugar. Local em primeiro lugar, sem conta, sem telemetria.**

[![CI](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml/badge.svg)](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml)
[![License: GPLv3](https://img.shields.io/badge/license-GPLv3-blue.svg)](LICENSE)
[![Windows 10 / 11](https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4?logo=windows&logoColor=white)](#obter)
[![.NET 9](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet&logoColor=white)](#)
[![Stars](https://img.shields.io/github/stars/sathvik-zoldyck/Soundpost?style=social)](https://github.com/sathvik-zoldyck/Soundpost/stargazers)

**[Roadmap](ROADMAP.md) · [Arquitetura](ARCHITECTURE.md) · [SDK de plugins](PLUGIN_SDK.md) · [Contribuir](CONTRIBUTING.md) · [Discussões](../../discussions)**

[English](README.md) · [简体中文](README.zh.md) · [Español](README.es.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · **Português** · [Français](README.fr.md) · [Deutsch](README.de.md) · [日本語](README.ja.md) · [Русский](README.ru.md) · [한국어](README.ko.md)

<br/>

<img src="assets/media/dashboard.png" alt="O painel do Soundpost: dial de volume mestre, seletor de dispositivos de reprodução, mixer por aplicativo e medidores de saída ao vivo" width="880" />

</div>

---

O Windows espalha o seu áudio entre o controle de volume, o menu de dispositivos, o painel de som e um punhado de utilitários de terceiros que não conversam entre si — e nenhum deles lembra o que você queria. **O Soundpost é o centro que faltava.** Cada dispositivo e cada aplicativo fluem para um único console: você troca, mixa e roteia, e tudo chega à saída certa. E quando você só quer ouvir, o visualizador transforma o que está tocando em algo que vale a pena assistir.

## Recursos

- **Troca instantânea de dispositivo.** Mude a saída ou entrada padrão com um clique.
- **Mixer por aplicativo.** Volume, mudo e medidores ao vivo para cada aplicativo que emite som.
- **Medição de saída ao vivo.** Medidores de pico mestre e por aplicativo com dinâmica realista.
- **Visualizador.** Sete estilos ao vivo — Ribbon, Aurora, Spectrum, Radial, Oscilloscope, Cymatics e um modo de imagem personalizada — que reagem ao seu áudio a 60 fps, com sensibilidade, suavização, brilho e paleta ajustáveis.
- **Sobreposição em tela cheia.** Destaque o visualizador sobre um vídeo musical com fundo sólido, escurecido ou totalmente transparente.
- **Painel Rápido.** Um menu compacto na bandeja para o que você faz no meio de uma reunião, sem abrir o console completo.
- **Quatro temas.** Indigo, Black & Red, Rich Gold e Cherry Blossom — alternáveis ao vivo nas Configurações.
- **Local e privado.** Sem conta, sem nuvem, sem telemetria. Tudo permanece na sua máquina.

## Veja

<div align="center">

<img src="assets/media/themes.png" alt="O Soundpost nos quatro temas: Indigo, Black and Red, Rich Gold e Cherry Blossom" width="880" />

<sub><b>Quatro temas, trocados ao vivo.</b> Indigo, Black &amp; Red, Rich Gold, Cherry Blossom.</sub>

<br/><br/>

<img src="assets/media/quick-panel.png" alt="O menu Painel Rápido na bandeja com volume mestre, troca de saída e controles por aplicativo" width="320" />

<sub><b>Painel Rápido.</b> Volume mestre, troca de saída e mudo por aplicativo — direto da bandeja.</sub>

</div>

## Obter

O Soundpost é para **Windows 10 e 11**. Baixe uma versão em [Releases](../../releases) quando houver uma publicada, ou compile a partir do código-fonte:

```bash
git clone https://github.com/sathvik-zoldyck/Soundpost.git
cd Soundpost
dotnet run --project src/Soundpost.App
```

Você precisa do [SDK do .NET 9](https://dotnet.microsoft.com/download). O fluxo de release produz um `Soundpost.exe` de arquivo único e autossuficiente (sem necessidade de instalar o .NET).

## Como funciona

Cada aplicativo e dispositivo flui para um único centro; você roteia, mixa e automatiza, e tudo chega à saída certa. Uma única camada de Core Audio encapsula as APIs COM do Windows para que o resto do aplicativo nunca as toque diretamente, o que mantém o console fluido e o tratamento de áudio isolado e testável.

## Estenda

O Soundpost foi feito para receber acréscimos.

- **Visualizadores.** Um estilo é uma classe que implementa `IVisualizerRenderer` — veja [visualizers/](visualizers/). Escreva, registre e ele aparece na barra de estilos.
- **Temas.** As paletas são dicionários autossuficientes; um novo tema é um novo arquivo mais uma amostra.
- **Plugins.** Uma superfície de plugins orientada a eventos está no roadmap — veja [PLUGIN_SDK.md](PLUGIN_SDK.md).

## Roadmap

Disponível agora: troca de dispositivo, mixer e medidores por aplicativo, o visualizador, a bandeja e o Painel Rápido, persistência e temas. A seguir: cenas e perfis, uma camada de automação, roteamento por aplicativo e diagnósticos em linguagem clara. O plano completo está em [ROADMAP.md](ROADMAP.md).

## Contribuir

Contribuições são bem-vindas, de um novo visualizador a uma correção de bug. Comece por [CONTRIBUTING.md](CONTRIBUTING.md), abra uma [issue](../../issues) ou dê um alô nas [Discussões](../../discussions). Se o Soundpost for útil para você, uma estrela ajuda outras pessoas a encontrá-lo.

## Licença

[GPLv3](LICENSE). Software livre e de código aberto.
