using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSGTechnical.Domain.Interfaces;
using CMSGTechnical.Mediator.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMSGTechnical.Mediator.Basket
{
    public record GetBasket(int Id) : IRequest<BasketDto>;

    public class GetBasketHandler : IRequestHandler<GetBasket, BasketDto>
    {
        private IRepo<Domain.Models.Basket> Baskets { get; }

        public GetBasketHandler(IRepo<Domain.Models.Basket> baskets)
        {
            Baskets = baskets;
        }

        public async Task<BasketDto> Handle(GetBasket request, CancellationToken cancellationToken)
        {
            var query = Baskets.GetAll().Where(b => b.Id == request.Id).Include(b => b.MenuItems.OrderBy(m => m.Id));
            var r = await query.FirstOrDefaultAsync(cancellationToken);
            if (r == null)
                throw new InvalidOperationException($"Basket with id {request.Id} not found");
            return r.ToDto();
        }
    }
}
