# PadConnect

<p align="center">
  <strong>Control Windows and OBS Studio using Novation Launchpad</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/. NET-9.0-512BD4?style=flat-square&logo=dotnet" alt=". NET 9.0">
  <img src="https://img.shields.io/badge/MAUI-Blazor-512BD4?style=flat-square" alt="MAUI Blazor">
  <img src="https://img.shields.io/badge/Platform-Windows-0078D6?style=flat-square&logo=windows" alt="Windows">
  <img src="https://img.shields.io/badge/License-MIT-green?style=flat-square" alt="MIT License">
</p>

---

## 📖 Description

**PadConnect** is a desktop application for Windows that turns your Novation Launchpad into a control center.  Control OBS Studio, launch applications, and play sound effects - all with a single button press. The perfect solution for streamers, content creators, and anyone looking to streamline their workflow.

## ✨ Features

- 🎹 **Launchpad Integration** - Support for Novation Launchpad MINI MK3 controllers via MIDI
> *Planned expansion to other Launchpad product line devices*
- 🔌 **OBS WebSocket Connection** - Communication with OBS Studio through obs-websocket
- 🔄 **Auto Reconnect** - Option to automatically restore connection after disconnection
- 🚀 **Application Launcher** - Quickly launch any program with a single button press. Assign your favorite applications to Launchpad keys
- 🔊 **Soundboard** - Play audio files directly from Launchpad buttons. Perfect for sound effects, jingles, and alerts during streams
- 📊 **Grid Visualization** - Interactive interface with visual representation of Launchpad buttons
- 🎨 **Modern UI** - Elegant user interface built with Blazor

## 🔧 Requirements

- **Operating System:** Windows 10 (version 1809 or later)
- **.NET 9.0** or later
- **OBS Studio** with [obs-websocket](https://github.com/obsproject/obs-websocket) plugin installed
- **Novation Launchpad MINI MK3**

## ⚙️ Configuration

### OBS WebSocket Configuration

1. Open OBS Studio
2. Go to **Tools → WebSocket Server Settings**
3. Enable the WebSocket server
4. Set the port (default: `4455`)
5. Set the authentication password

### PadConnect Configuration

1. Launch the PadConnect application
2. Click the **WebSocket** status button in the header
3. Enter the WebSocket URL (e.g., `ws://localhost:4455`)
4. Enter the password set in OBS
5. Optionally enable auto reconnect
6. Click **Apply**

### MIDI Device Configuration

1. Connect the Launchpad to your computer
2. Click the **Device** status button in the header
3. Select the MIDI input device (MIDI In)
4. Select the MIDI output device (MIDI Out)
5. Click **Apply**

## 🛠️ Technologies

| Technology | Description |
|------------|-------------|
| **.NET 9.0** | Application framework |
| **.NET MAUI Blazor** | UI framework (Hybrid) |
| **Windows MIDI API** | Launchpad communication |
| **WebSocket** | OBS Studio communication |
| **obs-websocket protocol** | OBS control protocol |

## 📁 Project Structure

```
PadConnect/
├── Components/
│   ├── Layout/          # Blazor layouts
│   ├── Models/          # Data models and OBS WebSocket
│   ├── Pages/           # Application pages (Home. razor)
│   ├── Services/        # Services (MidiService, WebSocketService)
│   ├── Shared/          # Shared components
│   └── ViewModels/      # ViewModels (MVVM)
├── Platforms/
│   └── Windows/         # Windows-specific code
├── Resources/           # Application resources (icons, fonts)
└── wwwroot/             # Static files (CSS, HTML)
```

## 🤝 Contributing

Contributions are welcome! If you'd like to help: 

1. Fork the repository
2. Create a branch for your feature (`git checkout -b feature/NewFeature`)
3. Commit your changes (`git commit -m 'Add new feature'`)
4. Push the branch (`git push origin feature/NewFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE. txt](LICENSE.txt) file for details.

## 📧 Contact

**B4rawasz** - [@B4rawasz](https://github.com/B4rawasz)

Project link: [https://github.com/B4rawasz/PadConnect](https://github.com/B4rawasz/PadConnect)

---

<p align="center">
  Built with ❤️ for streamers and content creators
</p>
