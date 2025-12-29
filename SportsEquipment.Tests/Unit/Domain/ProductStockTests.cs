using FluentAssertions;
using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.Entities;

namespace SportsEquipment.Tests.Unit.Domain
{
    public class ProductStockTests
    {
        [Fact]
        public void Constructor_InvalidProductId_Throws()
        {
            Action act = () => new ProductStock(Guid.Empty);

            act.Should().Throw<DomainException>().WithMessage("*ProductId inválido*");
        }

        [Fact]
        public void AddStock_InvalidQuantityOrFiscalNote_Throws()
        {
            var pid = Guid.NewGuid();
            var stock = new ProductStock(pid);

            Action a1 = () => stock.AddStock(0, "NF-1");
            a1.Should().Throw<DomainException>().WithMessage("*maior que zero*");

            Action a2 = () => stock.AddStock(1, null!);
            a2.Should().Throw<DomainException>().WithMessage("*nota fiscal*");
        }

        [Fact]
        public void RemoveStock_Insufficient_Throws()
        {
            var pid = Guid.NewGuid();
            var stock = new ProductStock(pid);
            stock.AddStock(2, "NF-1");

            Action act = () => stock.RemoveStock(3);
            act.Should().Throw<DomainException>().WithMessage("*Estoque insuficiente*");
        }

        [Fact]
        public void AddAndRemoveStock_AdjustsQuantity()
        {
            var pid = Guid.NewGuid();
            var stock = new ProductStock(pid);

            stock.AddStock(5, "NF-1");
            stock.GetAvailableQuantity().Should().Be(5);

            stock.RemoveStock(3);
            stock.GetAvailableQuantity().Should().Be(2);
        }
    }
}
