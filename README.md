# TapoCSharp

A C# library for controlling TP-Link Tapo smart devices, specifically P100 smart plugs. This library implements both KLAP and Passthrough protocols for device communication.

## Features

- **KLAP Protocol Support**: Modern encrypted communication protocol
- **Passthrough Protocol Support**: Legacy RSA-based protocol for older devices
- **Automatic Protocol Detection**: Detects which protocol the device supports
- **Device Control**: Turn devices on/off, get device information
- **Secure Authentication**: Proper encryption and authentication handling

## Usage

### Environment Variables

Set the following environment variables:

```bash
export TAPO_USERNAME="your_tapo_username"
export TAPO_PASSWORD="your_tapo_password" 
export IP_ADDRESS="192.168.0.xxx"
```

### Example

```bash
dotnet run --project TapoCSharp.Example
```

### Code Example

```csharp
using TapoCSharp;

var client = new ApiClient("username", "password");
var device = await client.P100Async("192.168.0.100");

// Get device information
var deviceInfo = await device.GetDeviceInfoAsync();
Console.WriteLine($"Device: {deviceInfo.Nickname}");

// Control device
await device.TurnOnAsync();
await device.TurnOffAsync();
```

## Architecture

- **ApiClient.cs** - Main entry point for the library
- **P100PlugHandler.cs** - Device-specific control methods
- **KlapProtocolHandler.cs** - KLAP protocol implementation
- **PassthroughProtocolHandler.cs** - Legacy protocol support
- **KlapCipher.cs** - Cryptographic utilities

## Protocol Support

This library supports both communication protocols used by TP-Link Tapo devices:

1. **KLAP Protocol** - Modern encrypted protocol using AES encryption
2. **Passthrough Protocol** - Legacy protocol using RSA encryption

The library automatically detects which protocol your device supports and uses the appropriate implementation.

## Dependencies

- .NET 8.0 or later
- System.Text.Json for JSON handling
- System.Security.Cryptography for encryption operations

## Acknowledgments

This implementation is based on the excellent Rust [tapo](https://github.com/mihai-dinculescu/tapo) library by Mihai Dinculescu. The protocol details and cryptographic implementations are derived from that work.

## Disclaimer

**USE AT YOUR OWN RISK**

This software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the authors or copyright holders be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software.

This is an unofficial implementation and is not affiliated with or endorsed by TP-Link Technologies Co., Ltd. Use of this software may void your device warranty.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.