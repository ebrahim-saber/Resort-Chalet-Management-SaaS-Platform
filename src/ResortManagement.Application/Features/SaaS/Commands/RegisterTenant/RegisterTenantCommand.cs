using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Exceptions;
using ResortManagement.Domain.Entities.SaaS;
using ResortManagement.Domain.Entities.Identity;
using ResortManagement.Domain.Entities.Resorts;
using ResortManagement.Domain.Entities.CRM;
using ResortManagement.Domain.Entities.Reservations;
using ResortManagement.Domain.Entities.Billing;
using ResortManagement.Domain.Entities.Operations;
using Unit = ResortManagement.Domain.Entities.Resorts.Unit;

namespace ResortManagement.Application.Features.SaaS.Commands.RegisterTenant;

public record RegisterTenantCommand(
    string TenantName,
    string Subdomain,
    string AdminEmail,
    string AdminPassword,
    string AdminFullName
) : IRequest<Guid>;

public class RegisterTenantCommandHandler : IRequestHandler<RegisterTenantCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterTenantCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        var subdomainNormalized = request.Subdomain.ToLower().Trim();
        var emailNormalized = request.AdminEmail.ToLower().Trim();

        // 1. Verify subdomain is unique
        var subdomainExists = await _context.Tenants
            .AnyAsync(t => t.Subdomain == subdomainNormalized, cancellationToken);

        if (subdomainExists)
        {
            throw new ValidationException(new System.Collections.Generic.Dictionary<string, string[]>
            {
                { "Subdomain", new[] { "This subdomain is already taken." } }
            });
        }

        // 2. Verify email is globally unique
        var emailExists = await _context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == emailNormalized, cancellationToken);

        if (emailExists)
        {
            throw new ValidationException(new System.Collections.Generic.Dictionary<string, string[]>
            {
                { "AdminEmail", new[] { "A user with this email address already exists." } }
            });
        }

        // 3. Create Tenant
        var tenant = new Tenant(request.TenantName, subdomainNormalized);
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken); // Generates tenant.Id

        // 4. Create Tenant Admin Role
        var adminRole = new Role("TenantAdmin", tenant.Id);
        _context.Roles.Add(adminRole);
        await _context.SaveChangesAsync(cancellationToken); // Generates adminRole.Id

        // 5. Hash Admin password and create Admin User
        var adminPasswordHash = _passwordHasher.HashPassword(request.AdminPassword);
        var adminUser = new User(tenant.Id, emailNormalized, adminPasswordHash, request.AdminFullName);
        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync(cancellationToken); // Generates adminUser.Id

        // 6. Bind Admin User to Admin Role
        var userRole = new UserRole(adminUser.Id, adminRole.Id);
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        // 7. Seed initial tenant data so the dashboard, calendar, booking storefront, and billing are fully populated with real interactive data
        var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == "Premium Plan", cancellationToken)
                   ?? await _context.SubscriptionPlans.FirstOrDefaultAsync(cancellationToken);
        if (plan != null)
        {
            var subscription = new TenantSubscription(tenant.Id, plan.Id, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(355));
            _context.TenantSubscriptions.Add(subscription);
        }

        // Seed Resorts
        var resortAlpine = new Resort(tenant.Id, "Alpine Peak Resort", "Zermatt, Swiss Alps", "+41 27 966 81 00");
        var resortLakeside = new Resort(tenant.Id, "Lakeside Retreat", "Lake Geneva, Switzerland", "+41 21 804 00 00");
        var resortGrand = new Resort(tenant.Id, "The Grand Estate", "St. Moritz, Switzerland", "+41 81 837 10 00");
        _context.Resorts.AddRange(resortAlpine, resortLakeside, resortGrand);
        await _context.SaveChangesAsync(cancellationToken);

        // Seed Buildings
        var mainLodge = new Building(tenant.Id, resortAlpine.Id, "Main Lodge");
        var eastWing = new Building(tenant.Id, resortAlpine.Id, "East Wing");
        var westVillas = new Building(tenant.Id, resortAlpine.Id, "West Chalet Villas");
        _context.Buildings.AddRange(mainLodge, eastWing, westVillas);
        await _context.SaveChangesAsync(cancellationToken);

        // Seed Floors
        var floor1 = new Floor(tenant.Id, mainLodge.Id, 1);
        var floor2 = new Floor(tenant.Id, mainLodge.Id, 2);
        var floorVilla = new Floor(tenant.Id, westVillas.Id, 1);
        _context.Floors.AddRange(floor1, floor2, floorVilla);
        await _context.SaveChangesAsync(cancellationToken);

        // Seed Unit Types
        var royalPoolChalet = new UnitType(tenant.Id, resortAlpine.Id, "Royal Pool Chalet", 1850.00m, 6);
        var premiumSeaVilla = new UnitType(tenant.Id, resortAlpine.Id, "Premium Sea Villa", 1200.00m, 4);
        var boutiqueSuite = new UnitType(tenant.Id, resortAlpine.Id, "Boutique Suite", 650.00m, 2);
        _context.UnitTypes.AddRange(royalPoolChalet, premiumSeaVilla, boutiqueSuite);
        await _context.SaveChangesAsync(cancellationToken);

        // Seed Units
        var unit101 = new Unit(tenant.Id, floor1.Id, royalPoolChalet.Id, "Chalet 101");
        var unit102 = new Unit(tenant.Id, floor1.Id, royalPoolChalet.Id, "Chalet 102");
        var unit103 = new Unit(tenant.Id, floor1.Id, boutiqueSuite.Id, "Suite 103");
        var unit104 = new Unit(tenant.Id, floor2.Id, premiumSeaVilla.Id, "Chalet 104");
        var unit201 = new Unit(tenant.Id, floor2.Id, boutiqueSuite.Id, "Suite 201");
        var unit202 = new Unit(tenant.Id, floorVilla.Id, premiumSeaVilla.Id, "Villa 202");
        var unit302 = new Unit(tenant.Id, floor2.Id, boutiqueSuite.Id, "Suite 302");

        unit101.MarkClean();
        unit102.MarkClean();
        unit103.MarkClean();
        unit104.StartCleaning();
        unit201.MarkDirty();
        unit202.BlockForMaintenance();
        unit302.MarkDirty();

        _context.Units.AddRange(unit101, unit102, unit103, unit104, unit201, unit202, unit302);
        await _context.SaveChangesAsync(cancellationToken);

        // Seed Customers (CRM Guests)
        var custElena = new Customer(tenant.Id, "Elena", "Higgins", "elena.higgins@gmail.com", "+1 (555) 019-2834", "ID-908123", "American");
        var custMarcus = new Customer(tenant.Id, "Marcus", "Rostova", "marcus.rostova@icloud.com", "+44 20 7946 0958", "ID-112233", "British");
        var custSophia = new Customer(tenant.Id, "Sophia", "Varela", "sophia.varela@designstudio.net", "+34 600 123 456", "ID-445566", "Spanish");
        _context.Customers.AddRange(custElena, custMarcus, custSophia);
        await _context.SaveChangesAsync(cancellationToken);

        // Seed Reservations
        var res1 = new Reservation(tenant.Id, custElena.Id, unit101.Id, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(2), 3700.00m);
        var res2 = new Reservation(tenant.Id, custMarcus.Id, unit104.Id, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(5), 4800.00m);
        var res3 = new Reservation(tenant.Id, custSophia.Id, unit201.Id, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), 650.00m);

        res1.Confirm();
        res2.Confirm();
        res3.Confirm();
        res3.CheckIn();
        res3.CheckOut();

        _context.Reservations.AddRange(res1, res2, res3);
        await _context.SaveChangesAsync(cancellationToken);

        // Seed Invoices & Payments
        var inv1 = new Invoice(tenant.Id, res1.Id, "INV-2023-142", 3700.00m, DateTime.UtcNow.AddDays(2));
        var inv2 = new Invoice(tenant.Id, res2.Id, "INV-2023-145", 4800.00m, DateTime.UtcNow.AddDays(1));
        var inv3 = new Invoice(tenant.Id, res3.Id, "INV-2023-148", 650.00m, DateTime.UtcNow.AddDays(3));

        inv1.MarkPaid();
        inv2.Status = "Overdue";
        inv3.Status = "Draft";

        _context.Invoices.AddRange(inv1, inv2, inv3);
        await _context.SaveChangesAsync(cancellationToken);

        var pay1 = new Payment(tenant.Id, inv1.Id, "WireTransfer", 3700.00m, "TXN-908123-EH");
        pay1.Success();
        _context.Payments.Add(pay1);
        await _context.SaveChangesAsync(cancellationToken);

        // Seed Housekeeping Tasks
        var task1 = new HousekeepingTask(tenant.Id, unit302.Id, null, "Checkout cleaning. Change linens, sanitize kitchen.");
        var task2 = new HousekeepingTask(tenant.Id, unit104.Id, null, "Daily routine cleanup and towels refresh.");
        var task3 = new HousekeepingTask(tenant.Id, unit103.Id, null, "Deep clean jacuzzi and pool terrace completed.");

        task2.StartTask();
        task3.CompleteTask();

        _context.HousekeepingTasks.AddRange(task1, task2, task3);
        await _context.SaveChangesAsync(cancellationToken);

        // Seed Maintenance Requests
        var maint1 = new MaintenanceRequest(tenant.Id, unit104.Id, adminUser.Id, "AC Compressor Failure", "AC unit is blowing hot air on Friday weekend Rush.", "Critical");
        var maint2 = new MaintenanceRequest(tenant.Id, unit201.Id, adminUser.Id, "Jacuzzi Heater Leak", "Minor water leak underneath pool deck.", "High");
        var maint3 = new MaintenanceRequest(tenant.Id, unit202.Id, adminUser.Id, "Terrace Light Replacement", "Three light bulbs burnt out.", "Medium");

        maint1.StartRepair();
        maint3.Resolve();

        _context.MaintenanceRequests.AddRange(maint1, maint2, maint3);
        await _context.SaveChangesAsync(cancellationToken);

        // Seed Notifications (WhatsApp, Email, Alerts)
        var notif1 = new Notification(tenant.Id, adminUser.Id, "WhatsApp", "+20100998877", "Smith Family - Alpine Lodge", "Could we arrange for a ski instructor tomorrow morning at 9 AM?");
        var notif2 = new Notification(tenant.Id, adminUser.Id, "Alerts", "system", "Heating Malfunction - Chalet Mont Blanc", "Temperature drop reported in the master suite. Maintenance team dispatched.");
        var notif3 = new Notification(tenant.Id, adminUser.Id, "System", "admin", "New Reservation Confirmed", "Chalet Etoile has been booked for Dec 24 - Jan 2. Deposit received.");
        var notif4 = new Notification(tenant.Id, adminUser.Id, "Email", "chef@resort.com", "Catering Inquiry", "Inquiry regarding private chef availability for next week.");

        notif2.MarkSent();
        notif3.MarkSent();
        notif4.MarkSent();

        _context.Notifications.AddRange(notif1, notif2, notif3, notif4);
        await _context.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }
}
