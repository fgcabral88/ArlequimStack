namespace SportsEquipment.Api.Presentation.Configuration.Jwt
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = "SportsEquipment.Api";
        public string Audience { get; set; } = "SportsEquipment.Client";
        public int ExpiryMinutes { get; set; } = 60;
    }
}
