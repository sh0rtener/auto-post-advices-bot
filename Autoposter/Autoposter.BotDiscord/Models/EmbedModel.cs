using Autoposter.DomainLayer.Entities.Autoposter;
using Discord;
using System.ComponentModel.DataAnnotations;

namespace Autoposter.BotDiscord.Models
{
    public class EmbedModel
    {
        [Required]
        public Post Post { get; set; } = new Post();
        [Required]
        public Server Server { get; set; } = new Server();
        [Required]
        public IUser User { get; set; } = null!;  
        public ulong? GuildId { get; set; }
        public ulong? AutoposterId { get; set; }
    }
}
