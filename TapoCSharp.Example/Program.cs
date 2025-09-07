using TapoCSharp;

// Get credentials from environment variables
var username = Environment.GetEnvironmentVariable("TAPO_USERNAME") 
    ?? throw new InvalidOperationException("TAPO_USERNAME environment variable not set");
var password = Environment.GetEnvironmentVariable("TAPO_PASSWORD") 
    ?? throw new InvalidOperationException("TAPO_PASSWORD environment variable not set");
var ipAddress = Environment.GetEnvironmentVariable("IP_ADDRESS") ?? "192.168.0.250";

Console.WriteLine($"Connecting to Tapo device at {ipAddress}...");

try
{
    var client = new ApiClient(username, password);
    var device = await client.P100Async(ipAddress);
    
    Console.WriteLine("Connected! Getting device info...");
    var deviceInfo = await device.GetDeviceInfoAsync();
    Console.WriteLine($"Device Info: {deviceInfo}");
    
    Console.WriteLine("Turning device on...");
    await device.OnAsync();
    
    await Task.Delay(2000);
    
    Console.WriteLine("Turning device off...");
    await device.OffAsync();
    
    Console.WriteLine("Example completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex}");
}
