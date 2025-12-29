namespace SportsEquipment.Application.DTOs.Rabbit
{
    public class QueuePurgeResultDto
    {
        public string QueueName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public uint PurgedMessages { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
