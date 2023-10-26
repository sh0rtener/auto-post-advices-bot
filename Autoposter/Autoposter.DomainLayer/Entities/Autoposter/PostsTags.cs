namespace Autoposter.DomainLayer.Entities.Autoposter
{
    public class PostsTags : EntityBase
    {
        public Guid PostId { get; set; }
        public Guid TagId { get; set; }
        public Post? Post { get; set; }
        public Tag? Tag { get; set; }
    }
}
