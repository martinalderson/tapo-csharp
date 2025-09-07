# TapoCSharp

A C# library and CLI tool for controlling TP-Link Tapo smart devices, specifically P100 smart plugs. This library implements both KLAP and Passthrough protocols for device communication.

## 🚀 Quick Start

### Build and Install CLI

```bash
# Clone and build
git clone https://github.com/martinalderson/tapo-csharp.git
cd tapo-csharp

# Build all platforms
dotnet msbuild TapoCSharp.Cli/TapoCSharp.Cli.csproj -t:PublishAll -p:Configuration=Release

# Install on Linux (builds and installs to ~/.local/bin/tapo)
dotnet msbuild TapoCSharp.Cli/TapoCSharp.Cli.csproj -t:Install -p:Configuration=Release
```

#### CLI Usage

```bash
# Configure authentication and discover devices (first time setup)
tapo auth

# Show status of all devices
tapo status

# Show detailed status of specific device  
tapo status "Living Room Lamp"

# Control devices
tapo on "Living Room Lamp"
tapo off 192.168.1.100

# Manage devices manually
tapo devices add 192.168.1.100 --name "Living Room Lamp"
tapo devices ls
tapo devices rm "Living Room Lamp"
```

### CLI Features

- 🎨 **Beautiful TUI**: Rich terminal interface with colors, tables, and spinners
- 🔧 **Device Management**: Add, remove, and list your devices
- ⚡ **Instant Control**: Turn devices on/off with simple commands
- 📊 **Device Status**: View detailed device information and status
- 🔐 **Secure Storage**: Credentials stored securely in `~/.tapo/`

## 📚 Library Usage

### Installation

```bash
dotnet add package TapoCSharp
```

### Code Example

```csharp
using TapoCSharp;

var client = new ApiClient("username", "password");
var device = await client.P100Async("192.168.1.100");

// Get device information
var deviceInfo = await device.GetDeviceInfoAsync();
Console.WriteLine($"Device: {deviceInfo["nickname"]}");

// Control device
await device.OnAsync();
await device.OffAsync();
```

### Environment Variables Example

```bash
export TAPO_USERNAME="your_tapo_username"
export TAPO_PASSWORD="your_tapo_password" 
export IP_ADDRESS="192.168.1.100"

dotnet run --project TapoCSharp.Example
```

## 🏗️ Architecture

### Core Library
- **ApiClient.cs** - Main entry point for the library
- **P100PlugHandler.cs** - Device-specific control methods  
- **KlapProtocolHandler.cs** - KLAP protocol implementation
- **PassthroughProtocolHandler.cs** - Legacy protocol support
- **KlapCipher.cs** - Cryptographic utilities

### CLI Tool
- **Commands/** - CLI command implementations
- **Services/** - Configuration and device management
- **Models/** - Data models for config and devices

## 🔌 Protocol Support

This library supports both communication protocols used by TP-Link Tapo devices:

1. **KLAP Protocol** - Modern encrypted protocol using AES encryption (P100 v1.2+)
2. **Passthrough Protocol** - Legacy protocol using RSA encryption (older firmware)

The library automatically detects which protocol your device supports and uses the appropriate implementation.

## ✨ Features

- **KLAP Protocol Support**: Modern encrypted communication protocol
- **Passthrough Protocol Support**: Legacy RSA-based protocol for older devices
- **Automatic Protocol Detection**: Detects which protocol the device supports
- **Device Control**: Turn devices on/off, get device information
- **Secure Authentication**: Proper encryption and authentication handling
- **Cross-Platform CLI**: Beautiful terminal interface for all platforms
- **Single-File Binaries**: Self-contained executables with no dependencies

## 🛠️ Building from Source

### Prerequisites
- .NET 8.0 SDK or later

### Build Library
```bash
git clone https://github.com/martinalderson/tapo-csharp.git
cd tapo-csharp
dotnet build
```

### Build CLI Tool
```bash
# Debug build
dotnet run --project TapoCSharp.Cli -- --help

# Build all platforms at once
dotnet msbuild TapoCSharp.Cli/TapoCSharp.Cli.csproj -t:PublishAll -p:Configuration=Release

# Install on Linux (builds and installs to ~/.local/bin/tapo)
dotnet msbuild TapoCSharp.Cli/TapoCSharp.Cli.csproj -t:Install -p:Configuration=Release

# Manual single platform builds
dotnet publish TapoCSharp.Cli -c Release --self-contained -p:PublishSingleFile=true -r linux-x64
dotnet publish TapoCSharp.Cli -c Release --self-contained -p:PublishSingleFile=true -r win-x64
dotnet publish TapoCSharp.Cli -c Release --self-contained -p:PublishSingleFile=true -r osx-x64
```

## 🔧 Dependencies

- .NET 8.0 or later
- System.Text.Json for JSON handling
- System.Security.Cryptography for encryption operations
- Spectre.Console for CLI interface (CLI tool only)

## 🙏 Acknowledgments

This implementation is based on the excellent Rust [tapo](https://github.com/mihai-dinculescu/tapo) library by Mihai Dinculescu. The protocol details and cryptographic implementations are derived from that work.

## ⚠️ Disclaimer

**USE AT YOUR OWN RISK**

This software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the authors or copyright holders be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software.

This is an unofficial implementation and is not affiliated with or endorsed by TP-Link Technologies Co., Ltd. Use of this software may void your device warranty.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.