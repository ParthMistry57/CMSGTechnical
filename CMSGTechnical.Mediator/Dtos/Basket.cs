using CMSGTechnical.Domain.Interfaces;
using CMSGTechnical.Domain.Models;

namespace CMSGTechnical.Mediator.Dtos
{
    public class BasketItemDto
    {
        public MenuItemDto MenuItem { get; set; } = null!;
        public int Quantity { get; set; }
    }

    public class BasketDto
    {
        public int Id { get; set; }
        public ICollection<BasketItemDto> BasketItems { get; set; } = new List<BasketItemDto>();

        public int UserId { get; set; }

    }


    public static class BasketExtensions
    {


        public static IEnumerable<BasketDto> ToDto(this IEnumerable<Domain.Models.Basket> models) =>
            models.Select(i => i.ToDto()).ToArray();

        public static BasketDto ToDto(this Domain.Models.Basket model)
        {
            return new BasketDto()
            {
                Id = model.Id,
                BasketItems = model.BasketItems.Select(bi => new BasketItemDto
                {
                    MenuItem = bi.MenuItem.ToDto(),
                    Quantity = bi.Quantity
                }).ToList(),
                UserId = model.UserId
            };
        }
    }

}
