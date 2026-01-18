using CMSGTechnical.Domain.Models;
using CMSGTechnical.Mediator.Basket;
using CMSGTechnical.Mediator.Dtos;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace CMSGTechnical.Code
{

    public class BasketChangedEventArgs : EventArgs
    {
        public BasketDto Basket { get; set; }
    }


    public class BasketService
    {

        public event EventHandler<BasketChangedEventArgs> OnChange;

        public BasketDto Basket { get; private set; }

        private readonly IMediator _mediator;

        public BasketService(BasketDto basket, IMediator mediator)
        {
            Basket = basket;
            _mediator = mediator;
        }


        public async Task Add(MenuItemDto item)
        {
            var updatedBasket = await _mediator.Send(new AddItemToBasket(Basket.Id, item.Id));
            Basket = updatedBasket;
            OnChange(this, new BasketChangedEventArgs(){Basket = Basket});
        }

        public async Task Remove(MenuItemDto item)
        {
            var updatedBasket = await _mediator.Send(new RemoveItemFromBasket(Basket.Id, item.Id));
            Basket = updatedBasket;
            OnChange(this, new BasketChangedEventArgs() { Basket = Basket });
        }

    }
}
