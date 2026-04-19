<p align="center">
  <img src="assets/simpledpi.png" width="128" alt="SimpleDPI Logo"/>
</p>

<h1 align="center">SimpleDPI</h1>

<p align="center">
  <b>A modern, ultra-lightweight, and feature-rich GUI for GoodbyeDPI</b><br>
  <i>Designed for performance, aesthetics, and ease of use.</i>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011-0078D4?style=flat-square&logo=windows" alt="Platform"/>
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet" alt=".NET"/>
  <img src="https://img.shields.io/badge/Languages-53-2EA043?style=flat-square" alt="Languages"/>
  <img src="https://img.shields.io/badge/Size-~432%20KB-E36209?style=flat-square" alt="Size"/>
  <img src="https://img.shields.io/badge/License-MIT-yellow?style=flat-square" alt="License"/>
</p>

---

SimpleDPI is a premium WPF-based wrapper for [GoodbyeDPI](https://github.com/ValdikSS/GoodbyeDPI). It provides a seamless user experience for bypassing DPI (Deep Packet Inspection) with a focus on visual excellence and system efficiency.

## ✨ Key Features

- 🎨 **Modern Aesthetics** — Sleek dark mode with glassmorphism (blur) effects and smooth micro-animations.
- ⚡ **Ultra-Lightweight** — Single executable (~432 KB) with GZip-compressed payloads.
- 🌍 **Global Support** — Fully localized in **53 languages** with an intuitive onboarding process.
- 🔋 **Win11 Efficiency Mode** — Automatically enters EcoQoS mode when minimized to save CPU and battery.
- 🔄 **Smart Watchdog** — Real-time service monitoring with visual "Pulse" alerts if the service is interrupted.
- 🚀 **Reliable Startup** — Built-in option to start with Windows using a persistent startup shortcut.
- 🖥️ **System Tray Integration** — Runs silently in the background with a simplified context menu.
- 🔧 **Advanced Configuration** — 9 pre-configured profiles, encrypted DNS support, and manual argument editing.

## 📦 Download & Installation

1. Download the latest **[SimpleDPI.exe](builds/SimpleDPI.exe)** from the `builds` folder.
2. Run as Administrator (required for WinDivert drivers).
3. Select your language and follow the onboarding steps.

> **Note:** Requires [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0).

---

## 🏗️ Build from Source

```bash
git clone https://github.com/YourUsername/SimpleDPI.git
cd SimpleDPI/src
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=false
```

---

## 📁 Project Structure

```
SimpleDPI/
├── assets/             # Branding and icons
├── builds/             # Pre-compiled portable binaries
└── src/                # C# / WPF source code
    ├── Payload/        # Compressed GoodbyeDPI binaries (.gz)
    ├── Localization.cs # 53-language dictionary
    └── ...             # Core logic and UI
```

## 🌐 Supported Languages
English, Turkish, Russian, German, French, Spanish, Chinese (Simplified/Traditional), Japanese, Korean, Arabic, and 40+ more.

---

<p align="center">
  <b>Made with ❤️ for the Open Source Community</b>
</p>
