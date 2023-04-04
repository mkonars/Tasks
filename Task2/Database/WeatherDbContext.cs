using Microsoft.EntityFrameworkCore;
using Task2.Domain;

namespace Task2.Database
{
    public class WeatherDbContext: DbContext
    {
        protected readonly IConfiguration Configuration;

        public WeatherDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite(Configuration.GetConnectionString("WeatherDb"));
        }

        public DbSet<WeatherEntry> WeatherEntries { get; set; }
    }
}
