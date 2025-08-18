using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PatchServer.Data;

namespace PatchServer.Data
{
    public class PatchDbContextFactory : IDesignTimeDbContextFactory<PatchDbContext>
    {
        public PatchDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PatchDbContext>();

            // Adjust path if needed
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new PatchDbContext(optionsBuilder.Options);
        }
    }
}
