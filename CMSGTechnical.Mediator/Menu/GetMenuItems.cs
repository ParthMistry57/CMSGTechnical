using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSGTechnical.Domain.Interfaces;
using CMSGTechnical.Domain.Models;
using CMSGTechnical.Mediator.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMSGTechnical.Mediator.Menu
{
    public record GetMenuItems : IRequest<IEnumerable<MenuItemDto>>;

    public class GetMenuItemsHandler : IRequestHandler<GetMenuItems, IEnumerable<MenuItemDto>>
    {

        private IRepo<MenuItem> MenuItems { get; }

        public GetMenuItemsHandler(IRepo<MenuItem> menuItems)
        {
            MenuItems = menuItems;
        }

        public async Task<IEnumerable<MenuItemDto>> Handle(GetMenuItems request, CancellationToken cancellationToken)
        {
            // Define category order: Starter, Main, Dessert
            var categoryOrder = new Dictionary<string, int>
            {
                { "Starter", 1 },
                { "Main", 2 },
                { "Dessert", 3 }
            };

            var q = MenuItems.GetAll()
                .OrderBy(m => categoryOrder.GetValueOrDefault(m.Category, 99))
                .ThenBy(m => m.Price);
            var r = await q.ToListAsync(cancellationToken);
            return r.ToDto();
        }
    }



}
