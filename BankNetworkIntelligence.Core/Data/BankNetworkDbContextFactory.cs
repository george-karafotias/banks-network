using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace BankNetworkIntelligence.Core.Data;

public class BankNetworkDbContextFactory
    : IDesignTimeDbContextFactory<BankNetworkDbContext>
{
    public BankNetworkDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            "Host=localhost;" +
            "Port=5432;" +
            "Database=bank_network_intelligence;" +
            "Username=postgres;" +
            "Password=gk433";

        var optionsBuilder =
            new DbContextOptionsBuilder<BankNetworkDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new BankNetworkDbContext(optionsBuilder.Options);
    }
}
