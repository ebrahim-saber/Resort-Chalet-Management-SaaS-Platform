using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;

using Microsoft.AspNetCore.Authorization;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Authorize(Roles = "Administrator,Operations Manager")]
[Route("billing")]
public class BillingController : Controller
{
    private readonly IApplicationDbContext _context;

    public BillingController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        List<dynamic> invoices = new();
        decimal totalRevenue = 0;
        decimal revPar = 0;
        decimal adr = 0;
        var recentActivities = new List<dynamic>();
        var monthlyRevenue = new decimal[6];
        var monthsLabels = new string[6];

        try
        {
            // 1. Fetch Invoices from SQL Server
            var dbInvoices = await _context.Invoices.ToListAsync();
            foreach (var inv in dbInvoices)
            {
                invoices.Add(new { 
                    Id = inv.Id, 
                    ShortId = inv.Id.ToString().Substring(0, 5).ToUpper(),
                    InvoiceNumber = inv.InvoiceNumber, 
                    TotalAmount = inv.TotalAmount, 
                    Status = inv.Status.ToString(),
                    CreatedAt = inv.CreatedAt
                });
            }

            // 2. Compute financial stats
            totalRevenue = await _context.Payments
                .Where(p => p.Status == "Succeeded")
                .SumAsync(p => p.Amount);
                
            var roomsSold = await _context.Reservations
                .CountAsync(r => r.Status != "Cancelled");
                
            var totalUnits = await _context.Units.CountAsync();
            
            if (roomsSold > 0)
            {
                adr = totalRevenue / roomsSold;
            }
            if (totalUnits > 0)
            {
                revPar = totalRevenue / totalUnits;
            }

            // 3. Compute dynamic Chart.js monthly data
            var today = DateTime.Today;
            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = today.AddMonths(-i);
                monthsLabels[5-i] = targetMonth.ToString("MMM");
                monthlyRevenue[5-i] = await _context.Payments
                    .Where(p => p.Status == "Succeeded" && p.CreatedAt.Month == targetMonth.Month && p.CreatedAt.Year == targetMonth.Year)
                    .SumAsync(p => p.Amount);
            }

            // 4. Fetch dynamic recent activity log
            var recentPayments = await _context.Payments
                .OrderByDescending(p => p.CreatedAt)
                .Take(2)
                .ToListAsync();
                
            foreach (var p in recentPayments)
            {
                recentActivities.Add(new {
                    Time = p.CreatedAt,
                    Title = "Payment Received",
                    Description = $"{p.Amount:C} via {p.PaymentMethod} transaction."
                });
            }
            
            var recentInvs = await _context.Invoices
                .OrderByDescending(i => i.CreatedAt)
                .Take(2)
                .ToListAsync();
                
            foreach (var inv in recentInvs)
            {
                recentActivities.Add(new {
                    Time = inv.CreatedAt,
                    Title = "Invoice Generated",
                    Description = $"Invoice INV-{inv.InvoiceNumber} created for {inv.TotalAmount:C}."
                });
            }
        }
        catch (Exception)
        {
            // Database offline, fallbacks handled by zero values or empty lists safely
        }

        ViewBag.Invoices = invoices;
        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.RevPAR = revPar;
        ViewBag.ADR = adr;
        ViewBag.RecentActivities = recentActivities.OrderByDescending(a => a.Time).Take(4).ToList();
        ViewBag.MonthlyRevenue = monthlyRevenue;
        ViewBag.MonthsLabels = monthsLabels;

        return View();
    }
}
