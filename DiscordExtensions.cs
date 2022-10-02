﻿using DSharpPlus.EventArgs;

namespace DiscordListener;
internal static class DiscordExtensions
{
    public static object ToSerializable(this MessageCreateEventArgs e)
    {
        return new
        {
            GuildId = e.Guild.Id,
            GuildName = e.Guild.Name,
            ChannelId = e.Channel.Id,
            ChannelName = e.Channel.Name,
            e.Message.Content,
            e.Message.Embeds,
            e.Message.Attachments,
            Author = new
            {
                e.Author.Username,
                e.Author.Discriminator,
                e.Author.Id,
                e.Author.Mention,
                e.Author.IsBot,
                e.Author.Flags
            }
        };
    }

    public static object ToSerializable(this PresenceUpdateEventArgs e)
    {
        return new
        {
            Type = e.Activity.ActivityType.ToString(),
            Activities = e.PresenceAfter?.Activities
                .Where(x => x is not null)
                .Select(x => x.RichPresence),
            Author = new
            {
                e.User.Username,
                e.User.Discriminator,
                e.User.Id,
                e.User.Mention,
                e.User.IsBot,
                e.User.Flags
            }
        };
    }
}
