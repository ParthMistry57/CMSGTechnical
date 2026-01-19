using System;
using System.Linq;
using CMSGTechnical.Domain.Interfaces;
using CMSGTechnical.Domain.Models;
using CMSGTechnical.Mediator.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMSGTechnical.Mediator.Basket
{
    public record RemoveItemFromBasket(int BasketId, int MenuItemId) : IRequest<BasketDto>;

    public class RemoveItemFromBasketHandler : IRequestHandler<RemoveItemFromBasket, BasketDto>
    {
        private IRepo<Domain.Models.Basket> Baskets { get; }

        public RemoveItemFromBasketHandler(IRepo<Domain.Models.Basket> baskets)
        {
            Baskets = baskets;
        }

        public async Task<BasketDto> Handle(RemoveItemFromBasket request, CancellationToken cancellationToken)
        {
            var basketQuery = Baskets.GetAll()
                .Where(b => b.Id == request.BasketId)
                .Include(b => b.BasketItems.OrderBy(bi => bi.Id))
                    .ThenInclude(bi => bi.MenuItem);
            var basket = await basketQuery.FirstOrDefaultAsync(cancellationToken);
            if (basket == null)
                throw new InvalidOperationException($"Basket with id {request.BasketId} not found");

            var basketItem = basket.BasketItems.FirstOrDefault(bi => bi.MenuItemId == request.MenuItemId);
            if (basketItem != null)
            {
                if (basketItem.Quantity > 1)
                {
                    // Decrement quantity
                    basketItem.Quantity--;
                }
                else
                {
                    // Remove item completely
                    basket.BasketItems.Remove(basketItem);
                }
                await Baskets.Update(basket, cancellationToken);
            }

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
