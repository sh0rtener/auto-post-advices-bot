namespace Autoposter.DomainLayer.Entities.Autoposter
{
    public class User : EntityBase
    {
        public string? DiscordId { get; set; }
        public string? TagName { get; set; }
        public string? Name { get; set; }
        public string? ServerName { get; set; }
    }
}
