using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Domain.Entities.Identity;
using ResortManagement.Domain.Entities.SaaS;
using ResortManagement.Domain.Entities.Resorts;
using ResortManagement.Domain.Entities.CRM;
using ResortManagement.Domain.Entities.Reservations;
using ResortManagement.Domain.Entities.Billing;
using ResortManagement.Domain.Entities.Operations;

namespace ResortManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Tenant> Tenants { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<TenantSubscription> TenantSubscriptions { get; }

    DbSet<Resort> Resorts { get; }
    DbSet<Building> Buildings { get; }
    DbSet<Floor> Floors { get; }
    DbSet<UnitType> UnitTypes { get; }
    DbSet<Unit> Units { get; }
    DbSet<UnitAmenity> UnitAmenities { get; }
    DbSet<UnitImage> UnitImages { get; }

    DbSet<Customer> Customers { get; }
    DbSet<CustomerDocument> CustomerDocuments { get; }
    DbSet<CustomerNote> CustomerNotes { get; }

    DbSet<Reservation> Reservations { get; }
    DbSet<ReservationHistory> ReservationHistories { get; }
    DbSet<Season> Seasons { get; }
    DbSet<PricingRule> PricingRules { get; }
    DbSet<Discount> Discounts { get; }

    DbSet<Invoice> Invoices { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Employee> Employees { get; }
    DbSet<HousekeepingTask> HousekeepingTasks { get; }
    DbSet<MaintenanceRequest> MaintenanceRequests { get; }
    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
