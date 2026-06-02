using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Domain.Entities.Resorts;
using ResortManagement.Infrastructure.Persistence;
using Xunit;

namespace ResortManagement.Tests;

public class TenantSecurityTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

    public TenantSecurityTests()
    {
        _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GlobalQueryFilter_Should_Isolate_Tenant_Data_For_Reads()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        // 1. Seed data as "System" (with Guid.Empty tenant context)
        var systemTenantProvider = new TestTenantProvider(Guid.Empty);
        using (var seedContext = new ApplicationDbContext(_dbOptions, systemTenantProvider, null!))
        {
            var resortA = new Resort(tenantA, "Tenant A Resort", "Zermatt", "123");
            var resortB = new Resort(tenantB, "Tenant B Resort", "Lake Geneva", "456");

            seedContext.Resorts.AddRange(resortA, resortB);
            await seedContext.SaveChangesAsync();
        }

        // 2. Query as Tenant A
        var tenantAProvider = new TestTenantProvider(tenantA);
        using (var contextA = new ApplicationDbContext(_dbOptions, tenantAProvider, null!))
        {
            var resorts = await contextA.Resorts.ToListAsync();

            // Assert
            Assert.Single(resorts);
            Assert.Equal("Tenant A Resort", resorts[0].Name);
            Assert.Equal(tenantA, resorts[0].TenantId);
        }

        // 3. Query as Tenant B
        var tenantBProvider = new TestTenantProvider(tenantB);
        using (var contextB = new ApplicationDbContext(_dbOptions, tenantBProvider, null!))
        {
            var resorts = await contextB.Resorts.ToListAsync();

            // Assert
            Assert.Single(resorts);
            Assert.Equal("Tenant B Resort", resorts[0].Name);
            Assert.Equal(tenantB, resorts[0].TenantId);
        }
    }

    [Fact]
    public async Task DbContext_Should_Overwrite_TenantId_On_Save_To_Prevent_Data_Tampering()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantAProvider = new TestTenantProvider(tenantA);

        using var context = new ApplicationDbContext(_dbOptions, tenantAProvider, null!);

        // Act
        // Tenant A tries to maliciously save a Resort explicitly setting the TenantId to Tenant B
        var maliciousResort = new Resort(tenantB, "Malicious Tampered Resort", "St. Moritz", "789");
        context.Resorts.Add(maliciousResort);
        await context.SaveChangesAsync();

        // Assert
        // The save should have overridden the TenantId to Tenant A!
        var savedResort = await context.Resorts.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Name == "Malicious Tampered Resort");
        Assert.NotNull(savedResort);
        Assert.Equal(tenantA, savedResort.TenantId); // Interceptor successfully protected against parameter tampering!
        Assert.NotEqual(tenantB, savedResort.TenantId);
    }
}
