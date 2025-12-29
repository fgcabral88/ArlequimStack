using Microsoft.EntityFrameworkCore;

namespace SportsEquipment.Infrastructure.Data
{
    /// <summary>
    /// Helper para criar opções do DbContext usando MySQL (Pomelo).
    /// </summary>
    public static class DbContextOptionsFactory
    {
        public static DbContextOptions<ApplicationDbContext> CreateOptions(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure(maxRetryCount: 5);
            });

            return builder.Options;
        }
    }
}
