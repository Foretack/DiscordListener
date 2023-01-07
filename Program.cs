global using Serilog;
using Config.Net;
using DiscordListener.Nonclass;
using Serilog.Events;
using IntervalTimer = System.Timers.Timer;

namespace DiscordListener;
internal static class Program
{
    public static RedisConnection Redis { get; private set; } = default!;
    private static Discord Client { get; set; } = default!;
    private static readonly IntervalTimer _timer = new();

    private static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File("logs.txt", LogEventLevel.Information, "{Timestamp:HH:mm:ss zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", flushToDiskInterval: TimeSpan.FromMinutes(30), rollingInterval: RollingInterval.Month)
            .WriteTo.File("debug.txt", LogEventLevel.Debug, "{Timestamp:HH:mm:ss zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", flushToDiskInterval: TimeSpan.FromMinutes(30), rollingInterval: RollingInterval.Day)
            .WriteTo.Console(LogEventLevel.Information)
            .CreateLogger();
        Log.Verbose("Logger initialized");

        IAuthConfig config = new ConfigurationBuilder<IAuthConfig>()
            .UseJsonFile("./config.json")
            .Build();
        Log.Verbose("Config initialized");

        Redis = new(config.RedisHost, config.RedisPassword);
        Log.Verbose("Redis initialized");

        Client = new Discord(config.DiscordToken);
        Log.Verbose("Discord initialized");

        _ = Console.ReadLine();
    }
}