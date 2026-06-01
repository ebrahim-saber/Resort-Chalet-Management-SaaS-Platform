using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;

namespace ResortManagement.WebApi.Controllers.Mvc;

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
        try
        {
            var dbInvoices = await _context.Invoices.ToListAsync();
            foreach (var inv in dbInvoices)
            {
                invoices.Add(new { 
                    Id = inv.Id, 
                    InvoiceNumber = inv.InvoiceNumber, 
                    TotalAmount = inv.TotalAmount, 
                    Status = inv.Status.ToString(),
                    CreatedAt = inv.CreatedAt
                });
            }
        }
        catch (Exception)
        {
            // Database offline, fallback to simulated data
        }

        ViewBag.Invoices = invoices;
        return View();
    }
}
