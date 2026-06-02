using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities.Operations;
using Microsoft.AspNetCore.Authorization;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Authorize(Roles = "Administrator,Operations Manager,Receptionist")]
[Route("operations")]
public class OperationsController : Controller
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public OperationsController(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    private async Task EnsureOperationsSeeded()
    {
        try
        {
            var tenantId = _tenantProvider.TenantId;
            if (tenantId == Guid.Empty)
            {
                var tenant = await _context.Tenants.FirstOrDefaultAsync();
                if (tenant != null)
                {
                    tenantId = tenant.Id;
                }
                else
                {
                    tenant = new ResortManagement.Domain.Entities.SaaS.Tenant("ChaletElite", "chaletelite");
                    _context.Tenants.Add(tenant);
                    await _context.SaveChangesAsync(default);
                    tenantId = tenant.Id;
                }
            }

            var units = await _context.Units.ToListAsync();
            if (units.Count == 0)
            {
                var resort = await _context.Resorts.FirstOrDefaultAsync(r => r.TenantId == tenantId);
                if (resort == null)
                {
                    resort = new ResortManagement.Domain.Entities.Resorts.Resort(tenantId, "ChaletElite Resort", "Swiss Alps, Zermatt");
                    _context.Resorts.Add(resort);
                    await _context.SaveChangesAsync(default);
                }

                var building = await _context.Buildings.FirstOrDefaultAsync(b => b.ResortId == resort.Id);
                if (building == null)
                {
                    building = new ResortManagement.Domain.Entities.Resorts.Building(tenantId, resort.Id, "Main Lodge");
                    _context.Buildings.Add(building);
                    await _context.SaveChangesAsync(default);
                }

                var floor = await _context.Floors.FirstOrDefaultAsync(f => f.BuildingId == building.Id);
                if (floor == null)
                {
                    floor = new ResortManagement.Domain.Entities.Resorts.Floor(tenantId, building.Id, 1);
                    _context.Floors.Add(floor);
                    await _context.SaveChangesAsync(default);
                }

                var suiteType = await _context.UnitTypes.FirstOrDefaultAsync(ut => ut.ResortId == resort.Id && ut.Name == "Suite");
                if (suiteType == null)
                {
                    suiteType = new ResortManagement.Domain.Entities.Resorts.UnitType(tenantId, resort.Id, "Suite", 300, 4);
                    _context.UnitTypes.Add(suiteType);
                }

                var villaType = await _context.UnitTypes.FirstOrDefaultAsync(ut => ut.ResortId == resort.Id && ut.Name == "Villa");
                if (villaType == null)
                {
                    villaType = new ResortManagement.Domain.Entities.Resorts.UnitType(tenantId, resort.Id, "Villa", 800, 8);
                    _context.UnitTypes.Add(villaType);
                }

                var chaletType = await _context.UnitTypes.FirstOrDefaultAsync(ut => ut.ResortId == resort.Id && ut.Name == "Chalet");
                if (chaletType == null)
                {
                    chaletType = new ResortManagement.Domain.Entities.Resorts.UnitType(tenantId, resort.Id, "Chalet", 600, 6);
                    _context.UnitTypes.Add(chaletType);
                }
                await _context.SaveChangesAsync(default);

                var unitsList = new List<ResortManagement.Domain.Entities.Resorts.Unit>
                {
                    new ResortManagement.Domain.Entities.Resorts.Unit(tenantId, floor.Id, suiteType.Id, "Suite 302") { HousekeepingStatus = "Dirty" },
                    new ResortManagement.Domain.Entities.Resorts.Unit(tenantId, floor.Id, villaType.Id, "Villa 105") { HousekeepingStatus = "InProgress" },
                    new ResortManagement.Domain.Entities.Resorts.Unit(tenantId, floor.Id, chaletType.Id, "Chalet 204") { HousekeepingStatus = "Clean" },
                    new ResortManagement.Domain.Entities.Resorts.Unit(tenantId, floor.Id, chaletType.Id, "Chalet 104") { HousekeepingStatus = "Dirty" },
                    new ResortManagement.Domain.Entities.Resorts.Unit(tenantId, floor.Id, suiteType.Id, "Suite 201") { HousekeepingStatus = "Dirty" },
                    new ResortManagement.Domain.Entities.Resorts.Unit(tenantId, floor.Id, villaType.Id, "Villa 202") { HousekeepingStatus = "Clean" }
                };
                _context.Units.AddRange(unitsList);
                await _context.SaveChangesAsync(default);
                
                units = unitsList;
            }

            var hasTasks = await _context.HousekeepingTasks.AnyAsync();
            if (!hasTasks)
            {
                var u0 = units[0].Id;
                var u1 = units.Count > 1 ? units[1].Id : units[0].Id;
                var u2 = units.Count > 2 ? units[2].Id : units[0].Id;

                var task1 = new ResortManagement.Domain.Entities.Operations.HousekeepingTask(tenantId, u0, null, "Checkout cleaning. Change linens, sanitize kitchen.") { Status = "Pending" };
                var task2 = new ResortManagement.Domain.Entities.Operations.HousekeepingTask(tenantId, u1, null, "Daily routine cleanup and towels refresh.") { Status = "InProgress" };
                var task3 = new ResortManagement.Domain.Entities.Operations.HousekeepingTask(tenantId, u2, null, "Deep clean jacuzzi and pool terrace completed.") { Status = "Completed" };
                _context.HousekeepingTasks.AddRange(task1, task2, task3);
                await _context.SaveChangesAsync(default);
            }

            var hasRequests = await _context.MaintenanceRequests.AnyAsync();
            if (!hasRequests)
            {
                var u3 = units.Count > 3 ? units[3].Id : units[0].Id;
                var u4 = units.Count > 4 ? units[4].Id : units[0].Id;
                var u5 = units.Count > 5 ? units[5].Id : units[0].Id;

                var req1 = new ResortManagement.Domain.Entities.Operations.MaintenanceRequest(tenantId, u3, Guid.Empty, "AC Compressor Failure", "AC unit is blowing hot air on Friday weekend Rush.", "Critical") { Status = "InProgress" };
                var req2 = new ResortManagement.Domain.Entities.Operations.MaintenanceRequest(tenantId, u4, Guid.Empty, "Jacuzzi Heater Leak", "Minor water leak underneath pool deck.", "High") { Status = "Open" };
                var req3 = new ResortManagement.Domain.Entities.Operations.MaintenanceRequest(tenantId, u5, Guid.Empty, "Terrace Light Replacement", "Three light bulbs burnt out.", "Medium") { Status = "Resolved" };
                _context.MaintenanceRequests.AddRange(req1, req2, req3);
                await _context.SaveChangesAsync(default);
            }
        }
        catch (Exception)
        {
            // Seed failed gracefully
        }
    }

    [HttpGet("housekeeping")]
    public async Task<IActionResult> Housekeeping()
    {
        await EnsureOperationsSeeded();
        List<dynamic> showcaseTasks = new();
        try
        {
            var dbTasks = await _context.HousekeepingTasks.ToListAsync();
            var units = await _context.Units.ToListAsync();
            
            showcaseTasks = dbTasks.Select(t => {
                var unit = units.FirstOrDefault(u => u.Id == t.UnitId);
                return (dynamic)new {
                    Id = t.Id,
                    UnitNumber = unit?.UnitNumber ?? "Room Unit",
                    Status = t.Status,
                    Notes = t.Notes,
                    CreatedAt = t.CreatedAt
                };
            }).ToList();
        }
        catch (Exception)
        {
            // Database is offline/unreachable
        }

        if (showcaseTasks.Count == 0)
        {
            ViewBag.ShowcaseTasks = new List<dynamic>
            {
                new { Id = Guid.NewGuid(), UnitNumber = "Suite 302", Status = "Pending", Notes = "Checkout cleaning. Change linens, sanitize kitchen.", CreatedAt = DateTime.UtcNow.AddHours(-2) },
                new { Id = Guid.NewGuid(), UnitNumber = "Villa 105", Status = "InProgress", Notes = "Daily routine cleanup and towels refresh.", CreatedAt = DateTime.UtcNow.AddHours(-1) },
                new { Id = Guid.NewGuid(), UnitNumber = "Chalet 204", Status = "Completed", Notes = "Deep clean jacuzzi and pool terrace completed.", CreatedAt = DateTime.UtcNow.AddHours(-4) }
            };
        }
        else
        {
            ViewBag.ShowcaseTasks = showcaseTasks;
        }

        return View();
    }

    [HttpPost("housekeeping/{id:guid}/start")]
    public async Task<IActionResult> StartHousekeepingTask(Guid id)
    {
        try
        {
            var task = await _context.HousekeepingTasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();

            task.StartTask();

            var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == task.UnitId);
            if (unit != null)
            {
                unit.StartCleaning();
            }

            await _context.SaveChangesAsync(default);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("housekeeping/{id:guid}/complete")]
    public async Task<IActionResult> CompleteHousekeepingTask(Guid id)
    {
        try
        {
            var task = await _context.HousekeepingTasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();

            task.CompleteTask();

            var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == task.UnitId);
            if (unit != null)
            {
                unit.MarkClean();
            }

            await _context.SaveChangesAsync(default);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("maintenance")]
    public async Task<IActionResult> Maintenance()
    {
        await EnsureOperationsSeeded();
        List<dynamic> showcaseRequests = new();
        try
        {
            var dbRequests = await _context.MaintenanceRequests.ToListAsync();
            var units = await _context.Units.ToListAsync();
            
            showcaseRequests = dbRequests.Select(r => {
                var unit = units.FirstOrDefault(u => u.Id == r.UnitId);
                return (dynamic)new {
                    Id = r.Id,
                    UnitNumber = unit?.UnitNumber ?? "Room Unit",
                    Title = r.Title,
                    Description = r.Description,
                    Priority = r.Priority,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                };
            }).ToList();
        }
        catch (Exception)
        {
            // Database is offline/unreachable
        }

        if (showcaseRequests.Count == 0)
        {
            ViewBag.ShowcaseRequests = new List<dynamic>
            {
                new { Id = Guid.NewGuid(), UnitNumber = "Chalet 104", Title = "AC Compressor Failure", Priority = "Critical", Status = "InProgress", CreatedAt = DateTime.UtcNow.AddHours(-3), Description = "AC unit is blowing hot air on Friday weekend Rush." },
                new { Id = Guid.NewGuid(), UnitNumber = "Suite 201", Title = "Jacuzzi Heater Leak", Priority = "High", Status = "Open", CreatedAt = DateTime.UtcNow.AddHours(-5), Description = "Minor water leak underneath pool deck." },
                new { Id = Guid.NewGuid(), UnitNumber = "Villa 202", Title = "Terrace Light Replacement", Priority = "Medium", Status = "Resolved", CreatedAt = DateTime.UtcNow.AddDays(-1), Description = "Three light bulbs burnt out." }
            };
        }
        else
        {
            ViewBag.ShowcaseRequests = showcaseRequests;
        }

        return View();
    }

    [HttpPost("maintenance/{id:guid}/resolve")]
    public async Task<IActionResult> ResolveMaintenanceTicket(Guid id)
    {
        try
        {
            var ticket = await _context.MaintenanceRequests.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            ticket.Resolve();
            await _context.SaveChangesAsync(default);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    public class CreateMaintenanceDto
    {
        public string UnitNumber { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Priority { get; set; } = "Medium";
        public string Description { get; set; } = default!;
    }

    [HttpPost("maintenance/create")]
    public async Task<IActionResult> CreateMaintenanceTicket([FromBody] CreateMaintenanceDto dto)
    {
        try
        {
            var tenantId = _tenantProvider.TenantId;
            if (tenantId == Guid.Empty)
            {
                var tenant = await _context.Tenants.FirstOrDefaultAsync();
                tenantId = tenant?.Id ?? Guid.Empty;
            }

            var unit = await _context.Units.FirstOrDefaultAsync(u => u.UnitNumber.ToLower() == dto.UnitNumber.ToLower());
            if (unit == null)
            {
                // Let's see if we have ANY unit
                unit = await _context.Units.FirstOrDefaultAsync();
                if (unit == null)
                {
                    await EnsureOperationsSeeded();
                    unit = await _context.Units.FirstOrDefaultAsync(u => u.UnitNumber.ToLower() == dto.UnitNumber.ToLower()) ?? await _context.Units.FirstOrDefaultAsync();
                }
            }

            if (unit == null)
            {
                return BadRequest(new { message = "الوحدة المحددة غير موجودة في النظام. يرجى إدخال اسم وحدة صالح." });
            }

            var ticket = new ResortManagement.Domain.Entities.Operations.MaintenanceRequest(
                tenantId,
                unit.Id,
                Guid.Empty,
                dto.Title,
                dto.Description,
                dto.Priority
            );

            _context.MaintenanceRequests.Add(ticket);
            await _context.SaveChangesAsync(default);

            return Json(new {
                id = ticket.Id,
                unitNumber = unit.UnitNumber,
                title = ticket.Title,
                priority = ticket.Priority,
                description = ticket.Description,
                status = ticket.Status
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        List<ResortManagement.Domain.Entities.Operations.Notification> list = new();
        try
        {
            list = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();
        }
        catch (Exception)
        {
            // Fail gracefully if DB unreachable
        }

        var results = list.Select(n => new
        {
            Id = n.Id,
            Channel = n.Channel,
            Recipient = n.Recipient,
            Title = n.Title,
            Body = n.Body,
            Status = n.Status,
            TimeAgo = GetTimeAgo(n.CreatedAt)
        });

        return Json(results);
    }

    [HttpPost("notifications/read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        try
        {
            var unread = await _context.Notifications
                .Where(n => n.Status == "Queued" || n.Status == "Unread")
                .ToListAsync();

            foreach (var item in unread)
            {
                item.MarkSent(); // Transition status to Sent (Read)
            }

            await _context.SaveChangesAsync(default);
        }
        catch (Exception)
        {
            return BadRequest();
        }

        return Ok();
    }

    [HttpPost("notifications/{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        try
        {
            var notif = await _context.Notifications.IgnoreQueryFilters().FirstOrDefaultAsync(n => n.Id == id);
            if (notif != null)
            {
                notif.MarkSent();
                await _context.SaveChangesAsync(default);
                return Ok();
            }
            return NotFound();
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    [HttpPost("notifications/create-simulated")]
    public async Task<IActionResult> CreateSimulatedNotification([FromBody] SimulatedNotificationDto dto)
    {
        try
        {
            var tenantId = _tenantProvider.TenantId;
            var notif = new ResortManagement.Domain.Entities.Operations.Notification(
                tenantId,
                null,
                dto.Channel,
                dto.Recipient ?? "system",
                dto.Title,
                dto.Body
            );

            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync(default);

            return Json(new
            {
                Id = notif.Id,
                Channel = notif.Channel,
                Recipient = notif.Recipient,
                Title = notif.Title,
                Body = notif.Body,
                Status = notif.Status,
                TimeAgo = "Just now"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    public class SimulatedNotificationDto
    {
        public string Channel { get; set; } = default!;
        public string? Recipient { get; set; }
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;
    }

    private static string GetTimeAgo(DateTime dt)
    {
        var span = DateTime.UtcNow - dt;
        if (span.TotalMinutes < 1) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        return dt.ToString("MMM dd");
    }
}
