namespace DiscordListener.Nonclass;
public interface IAuthConfig
{
    string RedisHost { get; }
    string RedisPassword { get; }
    string DiscordToken { get; }
}
