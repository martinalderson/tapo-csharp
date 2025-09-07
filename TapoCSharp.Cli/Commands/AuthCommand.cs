using System.ComponentModel;
using System.Text.Json.Nodes;
using Spectre.Console;
using Spectre.Console.Cli;
using TapoCSharp.Cli.Models;
using TapoCSharp.Cli.Services;
using TapoCSharp.Cli.Settings;

namespace TapoCSharp.Cli.Commands;

[Description("Configure Tapo authentication credentials")]
public class AuthCommand : Command<GlobalSettings>
{
    public override int Execute(CommandContext context, GlobalSettings settings)
    {
        return ExecuteAsync(context, settings).GetAwaiter().GetResult();
    }

    private async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings)
    {
        try
        {
            var configService = new ConfigService();
            var deviceService = new DeviceService(configService);

            AnsiConsole.Write(new FigletText("Tapo Auth").Centered().Color(Color.Blue));
            AnsiConsole.WriteLine();

            // Check if auth already exists
            var existingAuth = await configService.LoadAuthConfigAsync();
            if (existingAuth != null)
            {
                var overwrite = AnsiConsole.Confirm($"Authentication is already configured for [green]{existingAuth.Username}[/]. Overwrite?");
                if (!overwrite)
                {
                    AnsiConsole.MarkupLine("[yellow]Authentication configuration cancelled.[/]");
                    return 0;
                }
            }

            // Prompt for credentials
            var username = AnsiConsole.Ask<string>("Enter your Tapo [blue]username/email[/]:");
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter your Tapo [blue]password[/]:")
                    .Secret());

            // Test credentials
            AnsiConsole.WriteLine();
            bool testCredentials = AnsiConsole.Confirm("Test credentials with a device?", defaultValue: false);
            
            if (testCredentials)
            {
                var testIp = AnsiConsole.Ask<string>("Enter device IP to test:");
                
                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .StartAsync("Testing credentials...", async ctx =>
                    {
                        try
                        {
                            // Test credentials directly without loading from disk
                            var client = new TapoCSharp.ApiClient(username, password);
                            var device = await client.P100Async(testIp);
                            var deviceInfo = await device.GetDeviceInfoAsync();
                            
                            var model = deviceInfo?["model"]?.ToString();
                            AnsiConsole.MarkupLine($"[green]✓[/] Successfully connected to {model ?? "device"}");
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[red]✗[/] Credential test failed: {ex.Message}");
                            throw new InvalidOperationException("Credential test failed");
                        }
                    });
            }

            // Save credentials
            var authConfig = new AuthConfig
            {
                Username = username,
                Password = password
            };

            await configService.SaveAuthConfigAsync(authConfig);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green]✓[/] Authentication configuration saved successfully!");
            AnsiConsole.MarkupLine("Configuration stored in [dim]~/.tapo/auth.json[/]");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
            return 1;
        }
    }
}