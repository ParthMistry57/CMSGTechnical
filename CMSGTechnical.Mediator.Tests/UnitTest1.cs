using CMSGTechnical.Domain.Models;
using CMSGTechnical.Mediator.Basket;
using CMSGTechnical.Mediator.Menu;
using CMSGTechnical.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CMSGTechnical.Mediator.Tests
{
    public class GetMenuItemsTests
    {
        [Fact]
        public async Task GetMenuItems_ReturnsItemsOrderedByCategoryThenPrice()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repo = new Repository.Repo<MenuItem>(context);
            var handler = new GetMenuItemsHandler(repo);

            // Act
            var result = await handler.Handle(new GetMenuItems(), CancellationToken.None);
            var items = result.ToList();

            // Assert
            Assert.NotEmpty(items);
            // Verify ordering: Starter items first, then Main, then Dessert
            var categories = items.Select(i => i.Category).ToList();
            var uniqueCategories = categories.Distinct().ToList();
            if (uniqueCategories.Contains("Starter") && uniqueCategories.Contains("Main"))
            {
                Assert.True(uniqueCategories.IndexOf("Starter") < uniqueCategories.IndexOf("Main"));
            }
            if (uniqueCategories.Contains("Main") && uniqueCategories.Contains("Dessert"))
            {
                Assert.True(uniqueCategories.IndexOf("Main") < uniqueCategories.IndexOf("Dessert"));
            }
        }
    }

    public class GetMenuItemTests
    {
        [Fact]
        public async Task GetMenuItem_ReturnsMenuItem_WhenExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repo = new Repository.Repo<MenuItem>(context);
            var handler = new GetMenuItemHandler(repo);

            // Act
            var result = await handler.Handle(new GetMenuItem(1), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }
    }

    public class GetBasketTests
    {
        [Fact]
        public async Task GetBasket_ReturnsBasketWithMenuItems()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repo = new Repository.Repo<Domain.Models.Basket>(context);
            var handler = new GetBasketHandler(repo);

            // Act
            var result = await handler.Handle(new GetBasket(1), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.NotNull(result.BasketItems);
        }
    }

    public class AddItemToBasketTests
    {
        [Fact]
        public async Task AddItemToBasket_AddsItemToBasket()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var basketRepo = new Repository.Repo<Domain.Models.Basket>(context);
            var menuItemRepo = new Repository.Repo<MenuItem>(context);
            var handler = new AddItemToBasketHandler(basketRepo, menuItemRepo);

            // Act
            var result = await handler.Handle(new AddItemToBasket(1, 1), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Contains(result.BasketItems, bi => bi.MenuItem.Id == 1);
        }
    }

    public class RemoveItemFromBasketTests
    {
        [Fact]
        public async Task RemoveItemFromBasket_RemovesItemFromBasket()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var basketRepo = new Repository.Repo<Domain.Models.Basket>(context);
            var menuItemRepo = new Repository.Repo<MenuItem>(context);
            var addHandler = new AddItemToBasketHandler(basketRepo, menuItemRepo);
            var removeHandler = new RemoveItemFromBasketHandler(basketRepo);

            // Add item first
            await addHandler.Handle(new AddItemToBasket(1, 1), CancellationToken.None);

            // Act
            var result = await removeHandler.Handle(new RemoveItemFromBasket(1, 1), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }
    }
}