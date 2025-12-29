using FluentAssertions;
using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Domain.ValueObjects;

namespace SportsEquipment.Tests.Unit.Domain
{
    public class OrderItemTests
    {
        [Fact]
        public void Constructor_InvalidProductId_Throws()
        {
            Action act = () => new OrderItem(Guid.Empty, 1, new Money(10m, "BRL"));

            act.Should().Throw<DomainException>().WithMessage("*ProductId inválido*");
        }

        [Fact]
        public void Constructor_InvalidQuantity_Throws()
        {
            Action act = () => new OrderItem(Guid.NewGuid(), 0, new Money(10m, "BRL"));

            act.Should().Throw<DomainException>().WithMessage("*Quantidade*");
        }

        [Fact]
        public void LineTotal_ReturnsUnitPriceTimesQuantity()
        {
            var item = new OrderItem(Guid.NewGuid(), 3, new Money(7m, "BRL"));

            item.LineTotal().Amount.Should().Be(21m);
        }
    }
}
