using CMSGTechnical.Domain.Interfaces;

namespace CMSGTechnical.Domain.Models
{
    public class BasketItem : IEntity
    {
        public int Id { get; set; }
        public int BasketId { get; set; }
        public Basket Basket { get; set; } = null!;
        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; } = null!;
        public int Quantity { get; set; }
    }
}
