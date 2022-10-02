using System.Text.Json;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using IntervalTimer = System.Timers.Timer;

namespace DiscordListener;
internal sealed class Discord
{
    public DiscordClient Client { get; private set; }

    private readonly Dictionary<ulong, DiscordChannel> _channels = new();
    private readonly IntervalTimer _timer = new();

    public Discord(string token)
    {
        var config = new DiscordConfiguration()
        {
            Token = token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMessages | DiscordIntents.GuildPresences,
            LoggerFactory = new LoggerFactory().AddSerilog(Log.Logger)
        };
        Client = new DiscordClient(config);
        Client.ConnectAsync().GetAwaiter().GetResult();

        Client.MessageCreated += OnMessage;
        Client.PresenceUpdated += PresenceUpdated;

        LoadChannels();

        _timer.Interval = TimeSpan.FromHours(1).TotalMilliseconds;
        _timer.AutoReset = true;
        _timer.Enabled = true;
        _timer.Elapsed += (_, _) => LoadChannels();

        Program.Redis.Sub.Subscribe("discord:messages:send").OnMessage(async x => await HandleSendMessage(x));
    }

    private void LoadChannels()
    {
        try
        {
            _channels.Clear();
            foreach (DiscordChannel channel in Client.Guilds.Values.SelectMany(x => x.Channels.Values))
            {
                _channels.Add(channel.Id, channel);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load Discord Channels");
        }
    }

    private async Task PresenceUpdated(DiscordClient sender, PresenceUpdateEventArgs e)
    {
        string json = JsonSerializer.Serialize(e.ToSerializable());
        _ = await Program.Redis.Sub.PublishAsync("discord:presences", json);
    }

    private async Task OnMessage(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (!_channels.ContainsKey(e.Channel.Id)) _channels.Add(e.Channel.Id, e.Channel);
        string json = JsonSerializer.Serialize(e.ToSerializable());
        _ = await Program.Redis.Sub.PublishAsync("discord:messages", json);
    }

    private async Task HandleSendMessage(ChannelMessage channelMessage)
    {
        if (!channelMessage.Message.HasValue || channelMessage.Message.IsNull) return;
        try
        {
            SendMessageData data = JsonSerializer.Deserialize<SendMessageData>(channelMessage.Message!);
            _ = await Client.SendMessageAsync(_channels[data.ToChannelId], data.Content);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error handling ChannelMessage");
        }
    }

    private record struct SendMessageData(ulong ToChannelId, string Content);
}
