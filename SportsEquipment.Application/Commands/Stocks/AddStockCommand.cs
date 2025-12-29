namespace SportsEquipment.Application.Commands.Stocks
{
    public class AddStockCommand
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string FiscalNoteNumber { get; set; } = string.Empty;
    }
}
