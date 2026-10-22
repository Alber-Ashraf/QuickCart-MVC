using QuickCart.Models;
using Xunit;

namespace QuickCart.Tests
{
    public class PricingTests
    {
        private static double CalculateTieredPrice(ShoppingCart cart)
        {
            if (cart.Count <= 50)
            {
                return cart.Product.Price;
            }
            if (cart.Count <= 100)
            {
                return cart.Product.Price50;
            }
            return cart.Product.Price100;
        }

        [Theory]
        [InlineData(1, 100.0)]     // 1 unit => base Price ($100)
        [InlineData(50, 100.0)]    // 50 units => base Price ($100)
        [InlineData(51, 85.0)]     // 51 units => Price50 ($85)
        [InlineData(100, 85.0)]    // 100 units => Price50 ($85)
        [InlineData(101, 70.0)]    // 101 units => Price100 ($70)
        [InlineData(500, 70.0)]    // 500 units => Price100 ($70)
        public void CalculateTieredPrice_ReturnsCorrectPricePerTier(int count, double expectedUnitPrice)
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Title = "C# in Depth",
                ListPrice = 120.0,
                Price = 100.0,
                Price50 = 85.0,
                Price100 = 70.0
            };

            var cart = new ShoppingCart
            {
                ProductId = 1,
                Product = product,
                Count = count
            };

            // Act
            var actualPrice = CalculateTieredPrice(cart);

            // Assert
            Assert.Equal(expectedUnitPrice, actualPrice);
        }
    }
}
