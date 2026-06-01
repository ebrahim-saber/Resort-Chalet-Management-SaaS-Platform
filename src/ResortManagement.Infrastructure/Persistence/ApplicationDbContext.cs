using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using ResortManagement.Domain.Common;
using ResortManagement.Domain.Entities.Identity;
using ResortManagement.Domain.Entities.SaaS;
using ResortManagement.Domain.Entities.Resorts;
using ResortManagement.Domain.Entities.CRM;
using ResortManagement.Domain.Entities.Reservations;
using ResortManagement.Domain.Entities.Billing;
using ResortManagement.Domain.Entities.Operations;
using ResortManagement.Application.Common.Interfaces;
using Unit = ResortManagement.Domain.Entities.Resorts.Unit;

namespace ResortManagement.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantProvider _tenantProvider;

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();

    public DbSet<Resort> Resorts => Set<Resort>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<UnitType> UnitTypes => Set<UnitType>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<UnitAmenity> UnitAmenities => Set<UnitAmenity>();
    public DbSet<UnitImage> UnitImages => Set<UnitImage>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();
    public DbSet<CustomerNote> CustomerNotes => Set<CustomerNote>();

    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationHistory> ReservationHistories => Set<ReservationHistory>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<PricingRule> PricingRules => Set<PricingRule>();
    public DbSet<Discount> Discounts => Set<Discount>();

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<HousekeepingTask> HousekeepingTasks => Set<HousekeepingTask>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();

    private readonly IMediator _mediator;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantProvider tenantProvider,
        IMediator mediator) : base(options)
    {
        _tenantProvider = tenantProvider;
        _mediator = mediator;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Multi-Tenant global query filters and soft-delete filters
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                
                // Construct: e.TenantId == _tenantProvider.TenantId && !e.IsDeleted
                var tenantFilter = Expression.Equal(
                    Expression.Property(parameter, nameof(IMustHaveTenant.TenantId)),
                    Expression.Property(Expression.Constant(this), nameof(CurrentTenantId))
                );

                var deleteFilter = Expression.Equal(
                    Expression.Property(parameter, nameof(EntityBase.IsDeleted)),
                    Expression.Constant(false)
                );

                var filterExpression = Expression.Lambda(
                    Expression.AndAlso(tenantFilter, deleteFilter),
                    parameter
                );

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filterExpression);
            }
            else if (typeof(EntityBase).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");

                // Construct: !e.IsDeleted
                var deleteFilter = Expression.Equal(
                    Expression.Property(parameter, nameof(EntityBase.IsDeleted)),
                    Expression.Constant(false)
                );

                var filterExpression = Expression.Lambda(deleteFilter, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filterExpression);
            }
        }
    }

    // Helper property exposed to expression builder above
    public Guid CurrentTenantId => _tenantProvider.TenantId;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUsername = "System"; // Fallback username, can be resolved from ICurrentUserService

        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = currentUsername;
                    entry.Entity.CreatedAt = DateTime.UtcNow;

                    if (entry.Entity is IMustHaveTenant tenantEntity)
                    {
                        // Safely inject tenant context automatically
                        if (tenantEntity.TenantId == Guid.Empty)
                        {
                            tenantEntity.TenantId = _tenantProvider.TenantId;
                        }
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedBy = currentUsername;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    // Perform soft-delete instead of hard-delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedBy = currentUsername;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);
        await DispatchEventsAsync(cancellationToken);
        return result;
    }

    private async Task DispatchEventsAsync(CancellationToken cancellationToken)
    {
        var entities = ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        foreach (var entity in entities)
        {
            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
        }
    }
}
