# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TapoCSharp is a C# library and CLI tool for controlling TP-Link Tapo smart devices (primarily P100 smart plugs). It supports both KLAP and Passthrough protocols for device communication.

## Build & Development Commands

```bash
# Build the entire solution
dotnet build

# Run the CLI tool in development
dotnet run --project TapoCSharp.Cli -- --help

# Run the example project (requires environment variables)
TAPO_USERNAME="user@email.com" TAPO_PASSWORD="password" IP_ADDRESS="192.168.0.250" dotnet run --project TapoCSharp.Example

# Build release CLI for Linux x64
dotnet publish TapoCSharp.Cli -c Release --self-contained -p:PublishSingleFile=true -r linux-x64

# Build for other platforms
dotnet publish TapoCSharp.Cli -c Release --self-contained -p:PublishSingleFile=true -r win-x64
dotnet publish TapoCSharp.Cli -c Release --self-contained -p:PublishSingleFile=true -r osx-x64
dotnet publish TapoCSharp.Cli -c Release --self-contained -p:PublishSingleFile=true -r osx-arm64
dotnet publish TapoCSharp.Cli -c Release --self-contained -p:PublishSingleFile=true -r linux-arm64
```

## Solution Structure

The solution contains three main projects:

- **TapoCSharp** - Core library containing protocol implementations
- **TapoCSharp.Cli** - CLI application using Spectre.Console
- **TapoCSharp.Example** - Simple console example

## Core Library Architecture

### Protocol Layer
- **TapoProtocol.cs** - Main protocol orchestrator that auto-detects device capabilities
- **KlapProtocolHandler.cs** - Modern KLAP protocol (AES encryption) for newer devices
- **PassthroughProtocolHandler.cs** - Legacy RSA-based protocol for older firmware
- **KlapCipher.cs** - Cryptographic utilities for KLAP protocol

### Device Layer  
- **ApiClient.cs** - Main entry point, creates authenticated device handlers
- **P100PlugHandler.cs** - Device-specific methods (OnAsync, OffAsync, GetDeviceInfoAsync)

## CLI Architecture

The CLI uses Spectre.Console.Cli with the following structure:

### Commands
- `tapo auth` - Configure authentication credentials
- `tapo devices ls` - List configured devices  
- `tapo devices add <ip> --name <name>` - Add device
- `tapo devices rm <name>` - Remove device
- `tapo on <device>` - Turn device on
- `tapo off <device>` - Turn device off  
- `tapo status <device>` - Get device status

### CLI Components
- **Commands/** - Command implementations using Spectre.Console.Cli
- **Services/** - ConfigService (manages ~/.tapo/ config), DeviceService (device operations)
- **Models/** - AuthConfig, DeviceConfig for configuration data
- **Settings/** - Command-line argument settings classes

## Key Design Patterns

### Protocol Auto-Detection
The library automatically tries KLAP protocol first, then falls back to Passthrough protocol. This ensures compatibility across different firmware versions.

### Configuration Management
CLI stores credentials in `~/.tapo/auth.json` and device list in `~/.tapo/devices.json` with proper file permissions.

### Error Handling
Both library and CLI use structured error handling with meaningful exceptions for authentication failures, network issues, and protocol errors.

## Environment Variables

For testing and examples:
- `TAPO_USERNAME` - Tapo account email
- `TAPO_PASSWORD` - Tapo account password  
- `IP_ADDRESS` - Device IP address

## Dependencies

Core library uses minimal dependencies:
- System.Text.Json for JSON handling
- Built-in System.Security.Cryptography for encryption

CLI additionally uses:
- Spectre.Console for rich terminal UI
- Spectre.Console.Cli for command-line parsing