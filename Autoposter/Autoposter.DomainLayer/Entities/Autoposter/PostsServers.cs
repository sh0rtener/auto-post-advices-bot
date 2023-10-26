namespace Autoposter.DomainLayer.Entities.Autoposter
{
    public class PostsServers : EntityBase
    {
        public Guid PostId { get; set; }
        public Guid ServerId { get; set; }
        public Post? Post { get; set; }
        public Server? Server { get; set; }
    }
}
