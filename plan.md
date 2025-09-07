# Tapo Console Application Plan

## Overview
Create a modern TUI-based CLI tool using **Spectre.Console** and **Spectre.Console.Cli** for managing Tapo devices with the following commands:
- `tapo auth` - Interactive TUI for saving credentials
- `tapo devices ls` - List saved devices  
- `tapo devices add [ip]` - Add and verify device
- `tapo devices rm [ip]` - Remove device
- `tapo on [ip]` - Turn device on
- `tapo off [ip]` - Turn device off
- `tapo status [ip]` - Show device status

## Technology Choice: Spectre.Console
**Why Spectre.Console over Terminal.Gui:**
- Modern, actively maintained library
- Beautiful output with rich text formatting
- Built-in CLI command framework (Spectre.Console.Cli)
- Excellent prompts and interactive elements
- Smaller learning curve
- Better suited for command-line tools vs full TUI apps

## Project Structure
```
TapoCSharp.Cli/
├── Program.cs                    # Entry point with command registration
├── TapoCSharp.Cli.csproj        # Project file with dependencies
├── Commands/
│   ├── AuthCommand.cs           # Interactive auth setup
│   ├── DevicesCommand.cs        # Device management (ls, add, rm)
│   ├── OnCommand.cs             # Turn device on
│   ├── OffCommand.cs            # Turn device off
│   └── StatusCommand.cs         # Show device status
├── Models/
│   ├── AuthConfig.cs            # Username/password model
│   └── DeviceConfig.cs          # Device info model
├── Services/
│   ├── ConfigService.cs         # Handle ~/.tapo/ files
│   └── DeviceService.cs         # Device operations wrapper
└── Settings/
    ├── DeviceCommandSettings.cs # CLI settings for device commands
    └── GlobalSettings.cs         # Global CLI settings

## Implementation Details

### 1. Configuration Storage
- **Location**: `~/.tapo/` directory
- **Files**:
  - `auth.json` - Encrypted credentials
  - `devices.json` - Device list with names/IPs

### 2. Auth Command (`tapo auth`)
- Interactive prompts using Spectre.Console
- Secure password input (masked)
- Option to test credentials
- Encrypt credentials before saving

### 3. Device Management
- **List** (`tapo devices ls`): Table display with name, IP, status
- **Add** (`tapo devices add [ip]`): 
  - Test connection first
  - Prompt for device name
  - Save to devices.json
- **Remove** (`tapo devices rm [ip]`): Confirmation prompt

### 4. Device Control
- All commands auto-load auth from ~/.tapo/auth.json
- Support both IP and device name
- Show spinner during operations
- Display success/error with colored output

### 5. Status Command
- Rich formatted output with device info
- Show power state with color (green=on, red=off)
- Display additional info (firmware, signal, etc.)

## Dependencies
```xml
<PackageReference Include="Spectre.Console" Version="0.48.0" />
<PackageReference Include="Spectre.Console.Cli" Version="0.48.0" />
<PackageReference Include="System.Text.Json" Version="8.0.0" />
```

## Key Features
1. **Interactive TUI Elements**:
   - Password prompts with masking
   - Selection menus for device choice
   - Confirmation prompts for destructive actions
   - Progress bars/spinners for operations

2. **Rich Output**:
   - Colored status indicators
   - Formatted tables for device lists
   - ASCII art title/branding
   - Error messages with emoji indicators

3. **Security**:
   - Credentials encrypted in auth.json
   - No plaintext passwords in memory longer than needed
   - Secure permission settings on ~/.tapo/ files

4. **User Experience**:
   - Intuitive command structure
   - Helpful error messages
   - --help for all commands
   - Tab completion support (if possible)

## Implementation Order
1. Create project structure and add dependencies
2. Implement ConfigService for file management
3. Create auth command with interactive prompts
4. Implement device add/list/remove commands
5. Add on/off/status commands
6. Add error handling and validation
7. Polish with colors, spinners, and formatting
8. Add comprehensive help text

This approach will create a professional, user-friendly CLI tool that leverages Spectre.Console's strengths for both the command framework and rich terminal output.