namespace SportsEquipment.Application.DTOs.Rabbit
{
    public class QueueInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public uint MessageCount { get; set; }
        public uint ConsumerCount { get; set; }
        public bool Exists { get; set; }
        public string? Error { get; set; }
    }
}
