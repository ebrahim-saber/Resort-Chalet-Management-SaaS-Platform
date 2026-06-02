using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Domain.Entities.SaaS;
using ResortManagement.Domain.Entities.Identity;
using ResortManagement.Domain.Entities.Resorts;
using ResortManagement.Domain.Entities.CRM;
using ResortManagement.Domain.Entities.Reservations;
using ResortManagement.Domain.Entities.Billing;
using ResortManagement.Domain.Entities.Operations;
using ResortManagement.Infrastructure.Services;
using Tenant = ResortManagement.Domain.Entities.SaaS.Tenant;
using Unit = ResortManagement.Domain.Entities.Resorts.Unit;

namespace ResortManagement.Infrastructure.Persistence;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        // 1. Recreate database to ensure a clean slate and all tables are present
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var hasher = new PasswordHasher();

        // 3. Seed Subscription Plans
        var basicPlan = new SubscriptionPlan("Basic Plan", 199.00m, 1, 10, "[ \"Resort Management\", \"Standard Support\" ]");
        var premiumPlan = new SubscriptionPlan("Premium Plan", 499.00m, 5, 50, "[ \"Resort Management\", \"Advanced Analytics\", \"24/7 Priority Support\" ]");
        var enterprisePlan = new SubscriptionPlan("Enterprise Plan", 999.00m, 20, 200, "[ \"Unlimited Everything\", \"Custom Integrations\", \"Dedicated Account Manager\" ]");

        context.SubscriptionPlans.AddRange(basicPlan, premiumPlan, enterprisePlan);
        context.SaveChanges();

        // 4. Seed default Tenant (ChaletElite Luxury Resort)
        var tenant = new Tenant("Alpine Peak Resort", "alpinepeak", "https://lh3.googleusercontent.com/aida-public/AB6AXuATh4NCDpF7-t1YlTnFy2TF8frWnxjGPZ4tUgfbvb7ejk0YhJ86NeRYFDVX5vr7vQ-p7p6qL26OqZ2cofZQOAUr8XfkBiO3b9urRZYy-IwqcWTM4hxtw6XGDDcvYWTFabk3a9vpBpRG7c_MKkH1NM6fVRYIeTDsZ6A8Y6m40unqoXTtAMW0XlwhOYyl-Xq8nDNVMh_8i_V9SaB8ncgbp2Vw3Hdv1lHdoHg7vl26C8NREzdXke29miyRmTb1RuLrP9tPeB0MiDNne9K7");
        context.Tenants.Add(tenant);
        context.SaveChanges();

        var tenantId = tenant.Id;

        // 5. Seed Tenant Subscription
        var subscription = new TenantSubscription(tenantId, premiumPlan.Id, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(335));
        context.TenantSubscriptions.Add(subscription);

        // Seed Roles
        var roleAdmin = new Role("Administrator", tenantId);
        var roleManager = new Role("Operations Manager", tenantId);
        var roleCleaner = new Role("Housekeeper", tenantId);
        var roleTech = new Role("Maintenance Technician", tenantId);
        var roleReceptionist = new Role("Receptionist", tenantId);

        context.Roles.AddRange(roleAdmin, roleManager, roleCleaner, roleTech, roleReceptionist);
        context.SaveChanges();

        // 6. Seed Users
        var adminPassword = hasher.HashPassword("admin123");
        var managerPassword = hasher.HashPassword("manager123");
        var cleanerPassword = hasher.HashPassword("cleaner123");
        var techPassword = hasher.HashPassword("tech123");
        var frontdeskPassword = hasher.HashPassword("frontdesk123");
        
        var adminUser = new User(tenantId, "admin@chaletelite.com", adminPassword, "Admin User");
        var managerUser = new User(tenantId, "manager@chaletelite.com", managerPassword, "Ebrahim Saber");
        var cleanerUser = new User(tenantId, "cleaner@chaletelite.com", cleanerPassword, "Maria Schmidt (Housekeeper)");
        var techUser = new User(tenantId, "tech@chaletelite.com", techPassword, "Johann Müller (Maintenance)");
        var frontdeskUser = new User(tenantId, "frontdesk@chaletelite.com", frontdeskPassword, "Sophia Martinez (Receptionist)");
        
        context.Users.AddRange(adminUser, managerUser, cleanerUser, techUser, frontdeskUser);
        context.SaveChanges();

        // Seed UserRoles
        context.UserRoles.AddRange(
            new UserRole(adminUser.Id, roleAdmin.Id),
            new UserRole(managerUser.Id, roleManager.Id),
            new UserRole(cleanerUser.Id, roleCleaner.Id),
            new UserRole(techUser.Id, roleTech.Id),
            new UserRole(frontdeskUser.Id, roleReceptionist.Id)
        );
        context.SaveChanges();


        // 7. Seed Resorts
        var resortAlpine = new Resort(tenantId, "Alpine Peak Resort", "Zermatt, Swiss Alps", "+41 27 966 81 00");
        var resortLakeside = new Resort(tenantId, "Lakeside Retreat", "Lake Geneva, Switzerland", "+41 21 804 00 00");
        var resortGrand = new Resort(tenantId, "The Grand Estate", "St. Moritz, Switzerland", "+41 81 837 10 00");

        context.Resorts.AddRange(resortAlpine, resortLakeside, resortGrand);
        context.SaveChanges();

        // 8. Seed Buildings
        var mainLodge = new Building(tenantId, resortAlpine.Id, "Main Lodge");
        var eastWing = new Building(tenantId, resortAlpine.Id, "East Wing");
        var westVillas = new Building(tenantId, resortAlpine.Id, "West Chalet Villas");

        context.Buildings.AddRange(mainLodge, eastWing, westVillas);
        context.SaveChanges();

        // 9. Seed Floors
        var floor1 = new Floor(tenantId, mainLodge.Id, 1);
        var floor2 = new Floor(tenantId, mainLodge.Id, 2);
        var floorVilla = new Floor(tenantId, westVillas.Id, 1);

        context.Floors.AddRange(floor1, floor2, floorVilla);
        context.SaveChanges();

        // 10. Seed Unit Types
        var royalPoolChalet = new UnitType(tenantId, resortAlpine.Id, "Royal Pool Chalet", 1850.00m, 6);
        var premiumSeaVilla = new UnitType(tenantId, resortAlpine.Id, "Premium Sea Villa", 1200.00m, 4);
        var boutiqueSuite = new UnitType(tenantId, resortAlpine.Id, "Boutique Suite", 650.00m, 2);

        context.UnitTypes.AddRange(royalPoolChalet, premiumSeaVilla, boutiqueSuite);
        context.SaveChanges();

        // 11. Seed Units
        var unit101 = new Unit(tenantId, floor1.Id, royalPoolChalet.Id, "Chalet 101");
        var unit102 = new Unit(tenantId, floor1.Id, royalPoolChalet.Id, "Chalet 102");
        var unit103 = new Unit(tenantId, floor1.Id, boutiqueSuite.Id, "Suite 103");
        var unit104 = new Unit(tenantId, floor2.Id, premiumSeaVilla.Id, "Chalet 104");
        var unit201 = new Unit(tenantId, floor2.Id, boutiqueSuite.Id, "Suite 201");
        var unit202 = new Unit(tenantId, floorVilla.Id, premiumSeaVilla.Id, "Villa 202");
        var unit302 = new Unit(tenantId, floor2.Id, boutiqueSuite.Id, "Suite 302");

        unit101.MarkClean();
        unit102.MarkClean();
        unit103.MarkClean();
        unit104.StartCleaning(); // In Progress
        unit201.MarkDirty(); // Dirty for maintenance/housekeeping demo
        unit202.BlockForMaintenance(); // Out Of Service
        unit302.MarkDirty();

        context.Units.AddRange(unit101, unit102, unit103, unit104, unit201, unit202, unit302);
        context.SaveChanges();

        // 12. Seed Customers (CRM Guests)
        var custElena = new Customer(tenantId, "Elena", "Higgins", "elena.higgins@gmail.com", "+1 (555) 019-2834", "ID-908123", "American");
        var custMarcus = new Customer(tenantId, "Marcus", "Rostova", "marcus.rostova@icloud.com", "+44 20 7946 0958", "ID-112233", "British");
        var custSophia = new Customer(tenantId, "Sophia", "Varela", "sophia.varela@designstudio.net", "+34 600 123 456", "ID-445566", "Spanish");

        context.Customers.AddRange(custElena, custMarcus, custSophia);
        context.SaveChanges();

        // 13. Seed Reservations
        var res1 = new Reservation(tenantId, custElena.Id, unit101.Id, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(2), 3700.00m);
        var res2 = new Reservation(tenantId, custMarcus.Id, unit104.Id, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(5), 4800.00m);
        var res3 = new Reservation(tenantId, custSophia.Id, unit201.Id, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), 650.00m);

        res1.Confirm();
        res2.Confirm();
        
        // Correct transition for Checkout
        res3.Confirm();
        res3.CheckIn();
        res3.CheckOut(); 

        context.Reservations.AddRange(res1, res2, res3);
        context.SaveChanges();

        // 14. Seed Invoices & Payments
        var inv1 = new Invoice(tenantId, res1.Id, "INV-2023-142", 3700.00m, DateTime.UtcNow.AddDays(2));
        var inv2 = new Invoice(tenantId, res2.Id, "INV-2023-145", 4800.00m, DateTime.UtcNow.AddDays(1));
        var inv3 = new Invoice(tenantId, res3.Id, "INV-2023-148", 650.00m, DateTime.UtcNow.AddDays(3));

        inv1.MarkPaid();
        inv2.Status = "Overdue";
        inv3.Status = "Draft";

        context.Invoices.AddRange(inv1, inv2, inv3);
        context.SaveChanges();

        var pay1 = new Payment(tenantId, inv1.Id, "WireTransfer", 3700.00m, "TXN-908123-EH");
        pay1.Success();
        context.Payments.Add(pay1);
        context.SaveChanges();

        // 15. Seed Housekeeping Tasks
        var task1 = new HousekeepingTask(tenantId, unit302.Id, null, "Checkout cleaning. Change linens, sanitize kitchen.");
        var task2 = new HousekeepingTask(tenantId, unit104.Id, null, "Daily routine cleanup and towels refresh.");
        var task3 = new HousekeepingTask(tenantId, unit103.Id, null, "Deep clean jacuzzi and pool terrace completed.");

        task2.StartTask();
        task3.CompleteTask();

        context.HousekeepingTasks.AddRange(task1, task2, task3);
        context.SaveChanges();

        // 16. Seed Maintenance Requests
        var maint1 = new MaintenanceRequest(tenantId, unit104.Id, managerUser.Id, "AC Compressor Failure", "AC unit is blowing hot air on Friday weekend Rush.", "Critical");
        var maint2 = new MaintenanceRequest(tenantId, unit201.Id, managerUser.Id, "Jacuzzi Heater Leak", "Minor water leak underneath pool deck.", "High");
        var maint3 = new MaintenanceRequest(tenantId, unit202.Id, managerUser.Id, "Terrace Light Replacement", "Three light bulbs burnt out.", "Medium");

        maint1.StartRepair();
        maint3.Resolve();

        context.MaintenanceRequests.AddRange(maint1, maint2, maint3);
        context.SaveChanges();

        // 17. Seed Notifications (WhatsApp, Email, Alerts)
        var notif1 = new Notification(tenantId, managerUser.Id, "WhatsApp", "+20100998877", "Smith Family - Alpine Lodge", "Could we arrange for a ski instructor tomorrow morning at 9 AM?");
        var notif2 = new Notification(tenantId, managerUser.Id, "Alerts", "system", "Heating Malfunction - Chalet Mont Blanc", "Temperature drop reported in the master suite. Maintenance team dispatched.");
        var notif3 = new Notification(tenantId, managerUser.Id, "System", "admin", "New Reservation Confirmed", "Chalet Etoile has been booked for Dec 24 - Jan 2. Deposit received.");
        var notif4 = new Notification(tenantId, managerUser.Id, "Email", "chef@resort.com", "Catering Inquiry", "Inquiry regarding private chef availability for next week.");

        notif2.MarkSent();
        notif3.MarkSent();
        notif4.MarkSent();

        context.Notifications.AddRange(notif1, notif2, notif3, notif4);
        context.SaveChanges();
    }
}
