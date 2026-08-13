<div align="center">

<img src="assets/logo.svg" alt="Soundpost" width="120" />

# Soundpost

### 一个中心，掌控每一种声音。

**Windows 音频的统一控制台：切换输出设备、混合每个应用、并且“看见”你的声音——全部集中在一处。本地优先，无需账户，无遥测。**

[![CI](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml/badge.svg)](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml)
[![License: GPLv3](https://img.shields.io/badge/license-GPLv3-blue.svg)](LICENSE)
[![Windows 10 / 11](https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4?logo=windows&logoColor=white)](#获取)
[![.NET 9](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet&logoColor=white)](#)
[![Stars](https://img.shields.io/github/stars/sathvik-zoldyck/Soundpost?style=social)](https://github.com/sathvik-zoldyck/Soundpost/stargazers)

**[路线图](ROADMAP.md) · [架构](ARCHITECTURE.md) · [插件 SDK](PLUGIN_SDK.md) · [参与贡献](CONTRIBUTING.md) · [讨论区](../../discussions)**

[English](README.md) · **简体中文** · [Español](README.es.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [日本語](README.ja.md) · [Русский](README.ru.md) · [한국어](README.ko.md)

<br/>

<img src="assets/media/dashboard.png" alt="Soundpost 仪表盘：主音量旋钮、播放设备切换、单应用混音器和实时输出电平表" width="880" />

</div>

---

Windows 把你的音频分散在音量弹窗、设备菜单、声音控制面板，以及一堆互不相通的第三方工具里——而且没有一个会记住你的意图。**Soundpost 就是那个缺失的中心。** 每个设备和每个应用都汇入同一个控制台：你切换、混音、路由，声音就会落到正确的输出上。当你只想聆听时,可视化会把正在播放的声音变成值得一看的画面。

## 功能

- **即时设备切换。** 一键更改默认输出或输入设备。
- **单应用混音器。** 为每个发声的应用提供音量、静音和实时电平。
- **实时输出电平。** 主输出与单应用峰值电平表，带真实的表头动态。
- **可视化。** 七种实时风格——Ribbon、Aurora、Spectrum、Radial、Oscilloscope、Cymatics 以及自定义图片模式——以 60 fps 随音频律动，灵敏度、平滑度、辉光与配色均可调。
- **全屏叠加。** 把可视化弹出并叠加在音乐视频之上，可选实底、暗化或全透明背景。
- **快捷面板。** 一个精简的托盘弹出面板，无需打开完整控制台即可完成会议中的常用操作。
- **四款主题。** Indigo、Black & Red、Rich Gold 与 Cherry Blossom——可在设置中实时切换。
- **本地且私密。** 无账户、无云端、无遥测。一切都留在你的机器上。

## 一览

<div align="center">

<img src="assets/media/themes.png" alt="Soundpost 的四款主题：Indigo、Black and Red、Rich Gold 与 Cherry Blossom" width="880" />

<sub><b>四款主题，实时切换。</b> Indigo、Black &amp; Red、Rich Gold、Cherry Blossom。</sub>

<br/><br/>

<img src="assets/media/quick-panel.png" alt="带有主音量、输出切换和单应用控制的快捷面板托盘弹窗" width="320" />

<sub><b>快捷面板。</b> 主音量、输出切换、单应用静音——直接从托盘操作。</sub>

</div>

## 获取

Soundpost 面向 **Windows 10 和 11**。在发布后可从 [Releases](../../releases) 下载构建版本，或从源码构建：

```bash
git clone https://github.com/sathvik-zoldyck/Soundpost.git
cd Soundpost
dotnet run --project src/Soundpost.App
```

你需要 [.NET 9 SDK](https://dotnet.microsoft.com/download)。发布工作流会生成一个单文件、自包含的 `Soundpost.exe`（无需安装 .NET）。

## 工作原理

每个应用和设备都汇入同一个中心；你路由、混音、并让其自动化，声音便落到正确的输出上。单一的 Core Audio 层封装了 Windows COM API，使应用其余部分无需直接接触它们，从而让控制台保持流畅，音频处理保持隔离与可测试。

## 扩展

Soundpost 生来就为被扩展而设计。

- **可视化。** 一种风格就是一个实现 `IVisualizerRenderer` 的类——参见 [visualizers/](visualizers/)。写好、注册,它就会出现在风格栏中。
- **主题。** 配色是自包含的字典；新增一款主题只需一个新文件加一个色板。
- **插件。** 事件驱动的插件接口已在路线图中——参见 [PLUGIN_SDK.md](PLUGIN_SDK.md)。

## 路线图

现已提供：设备切换、单应用混音与电平、可视化、托盘与快捷面板、持久化以及主题。接下来：场景与配置、自动化层、单应用路由，以及通俗易懂的诊断。完整计划见 [ROADMAP.md](ROADMAP.md)。

## 参与贡献

欢迎各种贡献,从一个新的可视化到一处 bug 修复。请从 [CONTRIBUTING.md](CONTRIBUTING.md) 开始,提交 [issue](../../issues),或在 [讨论区](../../discussions) 打个招呼。如果 Soundpost 对你有用,点个 star 能帮助更多人发现它。

## 许可证

[GPLv3](LICENSE)。自由开源软件。
