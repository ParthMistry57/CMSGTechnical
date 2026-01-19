using System;
using System.Linq;
using CMSGTechnical.Domain.Interfaces;
using CMSGTechnical.Domain.Models;
using CMSGTechnical.Mediator.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMSGTechnical.Mediator.Basket
{
    public record AddItemToBasket(int BasketId, int MenuItemId) : IRequest<BasketDto>;

    public class AddItemToBasketHandler : IRequestHandler<AddItemToBasket, BasketDto>
    {
        private IRepo<Domain.Models.Basket> Baskets { get; }
        private IRepo<Domain.Models.MenuItem> MenuItems { get; }

        public AddItemToBasketHandler(IRepo<Domain.Models.Basket> baskets, IRepo<Domain.Models.MenuItem> menuItems)
        {
            Baskets = baskets;
            MenuItems = menuItems;
        }

        public async Task<BasketDto> Handle(AddItemToBasket request, CancellationToken cancellationToken)
        {
            var basketQuery = Baskets.GetAll()
                .Where(b => b.Id == request.BasketId)
                .Include(b => b.BasketItems.OrderBy(bi => bi.Id))
                    .ThenInclude(bi => bi.MenuItem);
            var basket = await basketQuery.FirstOrDefaultAsync(cancellationToken);
            if (basket == null)
                throw new InvalidOperationException($"Basket with id {request.BasketId} not found");

            var menuItem = await MenuItems.Get(request.MenuItemId, cancellationToken);
            if (menuItem == null)
                throw new InvalidOperationException($"MenuItem with id {request.MenuItemId} not found");

            // Check if item already exists in basket
            var existingBasketItem = basket.BasketItems.FirstOrDefault(bi => bi.MenuItemId == request.MenuItemId);
            if (existingBasketItem != null)
            {
                // Increment quantity
                existingBasketItem.Quantity++;
            }
            else
            {
                // Create new basket item
                var newBasketItem = new Domain.Models.BasketItem
                {
                    BasketId = basket.Id,
                    MenuItemId = menuItem.Id,
                    Quantity = 1
                };
                basket.BasketItems.Add(newBasketItem);
            }

            await Baskets.Update(basket, cancellationToken);

            // Reload with fresh query to get updated state
            var reloadQuery = Baskets.GetAll()
                .Where(b => b.Id == request.BasketId)
                .Include(b => b.BasketItems.OrderBy(bi => bi.Id))
                    .ThenInclude(bi => bi.MenuItem);
            var updatedBasket = await reloadQuery.FirstOrDefaultAsync(cancellationToken);
            return updatedBasket!.ToDto();
        }
    }
}
