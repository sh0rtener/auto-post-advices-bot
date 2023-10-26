namespace Autoposter.DomainLayer.Entities.Autoposter
{
    public class Post : EntityBase
    {
        public User? User { get; set; }
        public string? Description { get; set; }
        public Guid PostsServersId { get; set; }
        public Guid PostsTagsId { get; set; }
        public DateTime LastUpdateAt { get; set; }
        public bool IsActive = true;
        public PostsServers? Servers { get; set; }
        public PostsTags? Tags { get; set; }
    }
}
