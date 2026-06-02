using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Exceptions;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Features.Reservations.Commands.CreateReservation;
using ResortManagement.Domain.Entities.CRM;
using ResortManagement.Domain.Entities.Reservations;
using ResortManagement.Domain.Entities.Resorts;
using ResortManagement.Infrastructure.Persistence;
using ResortManagement.Infrastructure.Services;
using Xunit;

namespace ResortManagement.Tests;

public class DomainTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

    public DomainTests()
    {
        // Use in-memory SQLite or unique database name for standard in-memory EF Core testing
        _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CreateReservation_Should_Calculate_Correct_Total_Price_With_Season_And_Weekend_Multiplier()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);
        var mediatorMock = new MediatR.IMediator[] { null! }; // Not used for DbContext event dispatching in this unit test

        using var context = new ApplicationDbContext(_dbOptions, tenantProvider, null!);
        
        // Seed database
        var resort = new Resort(tenantId, "Swiss Lodge", "Zermatt", "12345");
        context.Resorts.Add(resort);
        await context.SaveChangesAsync();

        var unitType = new UnitType(tenantId, resort.Id, "Chalet Villa", 1000.00m, 4);
        context.UnitTypes.Add(unitType);
        await context.SaveChangesAsync();

        var building = new Building(tenantId, resort.Id, "Lodge A");
        context.Buildings.Add(building);
        await context.SaveChangesAsync();

        var floor = new Floor(tenantId, building.Id, 1);
        context.Floors.Add(floor);
        await context.SaveChangesAsync();

        var unit = new Unit(tenantId, floor.Id, unitType.Id, "Villa 101");
        context.Units.Add(unit);

        var customer = new Customer(tenantId, "John", "Doe", "john@example.com", "123", "ID123", "USA");
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // 1. Seed a high season (e.g. December 20 to December 30) with 1.5x multiplier
        var season = new Season(tenantId, "Peak Winter", new DateTime(2026, 12, 20), new DateTime(2026, 12, 30), 1.5m);
        context.Seasons.Add(season);

        // 2. Seed a weekend pricing rule (1.2x multiplier)
        var weekendRule = new PricingRule(tenantId, "Weekend Rate Modifier", "Weekend", 1.2m, null, null);
        context.PricingRules.Add(weekendRule);
        await context.SaveChangesAsync();

        // Act
        var handler = new CreateReservationCommandHandler(context, tenantProvider);
        
        // Test Stay: Check in Friday Dec 25, 2026, check out Monday Dec 28, 2026.
        // Nights:
        // Dec 25 (Friday): Base (1000) * Season (1.5) * Weekend (1.2) = 1800
        // Dec 26 (Saturday): Base (1000) * Season (1.5) * Weekend (1.2) = 1800
        // Dec 27 (Sunday): Base (1000) * Season (1.5) * Regular = 1500
        // Expected Total = 1800 + 1800 + 1500 = 5100
        var checkIn = new DateTime(2026, 12, 25);
        var checkOut = new DateTime(2026, 12, 28);
        var command = new CreateReservationCommand(customer.Id, unit.Id, checkIn, checkOut);
        
        var reservationId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var reservation = await context.Reservations.FindAsync(reservationId);
        Assert.NotNull(reservation);
        Assert.Equal(5100.00m, reservation.TotalPrice);
    }

    [Fact]
    public async Task CreateReservation_Should_Throw_ValidationException_On_Double_Booking()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);

        using var context = new ApplicationDbContext(_dbOptions, tenantProvider, null!);
        
        var resort = new Resort(tenantId, "Swiss Lodge", "Zermatt", "12345");
        context.Resorts.Add(resort);
        await context.SaveChangesAsync();

        var unitType = new UnitType(tenantId, resort.Id, "Chalet Villa", 1000.00m, 4);
        context.UnitTypes.Add(unitType);
        await context.SaveChangesAsync();

        var building = new Building(tenantId, resort.Id, "Lodge A");
        context.Buildings.Add(building);
        await context.SaveChangesAsync();

        var floor = new Floor(tenantId, building.Id, 1);
        context.Floors.Add(floor);
        await context.SaveChangesAsync();

        var unit = new Unit(tenantId, floor.Id, unitType.Id, "Villa 101");
        context.Units.Add(unit);

        var customer = new Customer(tenantId, "John", "Doe", "john@example.com", "123", "ID123", "USA");
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Save an existing active reservation on Villa 101 (Dec 24 to Dec 28)
        var existingReservation = new Reservation(tenantId, customer.Id, unit.Id, new DateTime(2026, 12, 24), new DateTime(2026, 12, 28), 4000.00m);
        context.Reservations.Add(existingReservation);
        await context.SaveChangesAsync();

        var handler = new CreateReservationCommandHandler(context, tenantProvider);

        // Act & Assert
        // A stay that overlaps (e.g. Dec 27 to Dec 29) should fail
        var overlappingCommand = new CreateReservationCommand(customer.Id, unit.Id, new DateTime(2026, 12, 27), new DateTime(2026, 12, 29));
        
        await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await handler.Handle(overlappingCommand, CancellationToken.None);
        });
    }
}

public class TestTenantProvider : ITenantProvider
{
    public Guid TenantId { get; private set; }

    public TestTenantProvider(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }
}
