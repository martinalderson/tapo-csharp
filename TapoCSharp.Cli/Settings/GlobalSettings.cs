using System.ComponentModel;
using Spectre.Console.Cli;

namespace TapoCSharp.Cli.Settings;

public class GlobalSettings : CommandSettings
{
    [CommandOption("--verbose")]
    [Description("Enable verbose output")]
    public bool Verbose { get; init; }
}