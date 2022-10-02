using StackExchange.Redis;

namespace DiscordListener;
internal sealed class RedisConnection
{
    public IDatabase Db { get; private set; }
    public IDatabaseAsync DbAsync { get; private set; }
    public ISubscriber Sub { get; private set; }

    public RedisConnection(string host, string pass)
    {
        var con = ConnectionMultiplexer.Connect($"{host},password={pass}");
        Db = con.GetDatabase();
        DbAsync = con.GetDatabase();
        Sub = con.GetSubscriber();
        Log.Information("Redis connected");
    }
}
