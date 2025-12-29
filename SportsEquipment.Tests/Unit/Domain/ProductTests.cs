using FluentAssertions;
using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Domain.ValueObjects;

namespace SportsEquipment.Tests.Unit.Domain
{
    public class ProductTests
    {
        [Fact]
        public void Constructor_NullName_Throws()
        {
            Action act = () => new Product(null!, "desc", new Money(10m, "BRL"));

            act.Should().Throw<DomainException>().WithMessage("*Nome do produto é obrigatório*");
        }

        [Fact]
        public void SetPrice_InvalidPrice_Throws()
        {
            var product = new Product("X", "D", new Money(1m, "BRL"));

            Action act = () => product.SetPrice(new Money(0m, "BRL"));

            act.Should().Throw<DomainException>().WithMessage("*Preço deve ser maior que zero*");
        }

        [Fact]
        public void ActivateDeactivate_TogglesIsActive()
        {
            var product = new Product("X", "D", new Money(10m, "BRL"));

            product.Deactivate();
            product.IsActive.Should().BeFalse();
            product.Activate();
            product.IsActive.Should().BeTrue();
        }
    }
}
