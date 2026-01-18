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
            var basketQuery = Baskets.GetAll().Where(b => b.Id == request.BasketId).Include(b => b.MenuItems);
            var basket = await basketQuery.FirstOrDefaultAsync(cancellationToken);
            if (basket == null)
                throw new InvalidOperationException($"Basket with id {request.BasketId} not found");

            var menuItem = await MenuItems.Get(request.MenuItemId, cancellationToken);
            if (menuItem == null)
                throw new InvalidOperationException($"MenuItem with id {request.MenuItemId} not found");

            basket.MenuItems.Add(menuItem);
            await Baskets.Update(basket, cancellationToken);

            // Reload with fresh query to get updated state
            var reloadQuery = Baskets.GetAll().Where(b => b.Id == request.BasketId).Include(b => b.MenuItems);
            var updatedBasket = await reloadQuery.FirstOrDefaultAsync(cancellationToken);
            return updatedBasket!.ToDto();
        }
    }
}
