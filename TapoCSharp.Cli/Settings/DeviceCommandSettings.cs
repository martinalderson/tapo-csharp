using System.ComponentModel;
using Spectre.Console.Cli;

namespace TapoCSharp.Cli.Settings;

public class DeviceCommandSettings : CommandSettings
{
    [CommandArgument(0, "[device]")]
    [Description("Device IP address or name (omit for all devices)")]
    public string? Device { get; init; }
}

public class AddDeviceSettings : CommandSettings
{
    [CommandArgument(0, "<ip>")]
    [Description("Device IP address")]
    public required string IpAddress { get; init; }
    
    [CommandOption("-n|--name")]
    [Description("Device name (optional)")]
    public string? Name { get; init; }
}

public class RemoveDeviceSettings : CommandSettings
{
    [CommandArgument(0, "<device>")]
    [Description("Device IP address or name to remove")]
    public required string Device { get; init; }
}