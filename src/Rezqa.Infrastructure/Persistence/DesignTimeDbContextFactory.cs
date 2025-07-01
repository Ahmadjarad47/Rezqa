using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Rezqa.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connectionString = "Server=db22626.public.databaseasp.net; Database=db22626; User Id=db22626; Password=M=q82iA?d-9T; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";

        builder.UseSqlServer(connectionString);

        // Create a mock IHttpContextAccessor for design-time
        var serviceProvider = new ServiceCollection()
            .AddHttpContextAccessor()
            .BuildServiceProvider();

        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

        return new ApplicationDbContext(builder.Options, httpContextAccessor);
    }
}