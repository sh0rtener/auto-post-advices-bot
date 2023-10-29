using Autoposter.BotDiscord.Models;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Autoposter.BotDiscord.Services
{
    public class EmbedFactory
    {
        public static Embed GetGeneralEmbed(EmbedModel model)
        {
            Uri branchUri = new Uri($"https://discord.com/channels/{model.GuildId}/" + model.AutoposterId);
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder()
                {
                    IconUrl = model.User.GetAvatarUrl(),
                    Name = model.User.Username,
                },
                ThumbnailUrl = model.User.GetAvatarUrl(),
                Url = "https://discord.com/users/" + model.Post.DiscordId,
                Color = 0x0099FF,
                Description = $"Ник: {model.Post.Name}\nСервер: {model.Server!.Name}\n\n" +
                            $"Объявление торговца:\n {model.Post.Description}\n\nАктивировать услугу: \n{branchUri}" +
                            $"\n\nНаписать торговцу:\n {model.User.Mention}\n" +
                            $"Тег текстом:\n {model.User.Username}"
            };

            embed.WithImageUrl(model.Post.ImageUri);

            return embed.Build();
        }
    }
}
