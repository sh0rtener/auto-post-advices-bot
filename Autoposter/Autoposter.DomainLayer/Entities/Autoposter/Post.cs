namespace Autoposter.DomainLayer.Entities.Autoposter
{
    public class Post : EntityBase
    {
        public ulong DiscordId { get; set; }
        public string? TagName { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? BranchId { get; set; }
        public string? ServerId { get; set; }
        public string? ImageUri { get; set; }
        public DateTime LastUpdateAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsAvailableToPost() => BranchId is not null && ServerId is not null && ImageUri is not null;
        public void EndingCreate(string imageUri)
        {
            ImageUri = imageUri;
            LastUpdateAt = DateTime.UtcNow;
            IsActive = true;
        }
    }
}
