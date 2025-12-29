using FluentAssertions;
using SportsEquipment.Domain.Enums;
using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Domain.ValueObjects;

namespace SportsEquipment.Tests.Unit.Domain
{
    public class OrderTests
    {
        [Fact]
        public void AddItem_WithInvalidQuantity_ThrowsDomainException()
        {
            var order = new Order("12345678900", "Seller Test");

            Action act = () => order.AddItem(Guid.NewGuid(), 0, new Money(10m, "BRL"));

            act.Should().Throw<DomainException>().WithMessage("Quantidade do item deve ser maior que zero.");
        }

        [Fact]
        public void AddItem_DuplicateProduct_ThrowsDomainException()
        {
            var pid = Guid.NewGuid();
            var order = new Order("12345678900", "Seller Test");

            order.AddItem(pid, 1, new Money(10m, "BRL"));

            Action act = () => order.AddItem(pid, 1, new Money(10m, "BRL"));

            act.Should().Throw<DomainException>().WithMessage("*Item já adicionado*");
        }

        [Fact]
        public void Confirm_WithoutItems_ThrowsDomainException()
        {
            var order = new Order("12345678900", "Seller Test");

            Action act = () => order.Confirm();

            act.Should().Throw<DomainException>().WithMessage("Pedido não pode ser confirmado sem itens.");
        }

        [Fact]
        public void Confirm_Success_ChangesStatusToConfirmed()
        {
            var order = new Order("12345678900", "Seller Test");

            order.AddItem(Guid.NewGuid(), 1, new Money(5m, "BRL"));

            order.Confirm();

            order.Status.Should().Be(OrderStatus.Confirmed);
        }

        [Fact]
        public void Cancel_AfterConfirm_ThrowsDomainException()
        {
            var order = new Order("12345678900", "Seller Test");
            order.AddItem(Guid.NewGuid(), 1, new Money(5m, "BRL"));

            order.Confirm();

            Action act = () => order.Cancel();

            act.Should().Throw<DomainException>().WithMessage("*confirmado*");
        }

        [Fact]
        public void TotalAmount_ReturnsSumOfLineTotals()
        {
            var order = new Order("12345678900", "Seller Test");

            order.AddItem(Guid.NewGuid(), 2, new Money(10m, "BRL"));
            order.AddItem(Guid.NewGuid(), 1, new Money(5m, "BRL"));

            order.TotalAmount().Should().Be(25m);
        }
    }
}