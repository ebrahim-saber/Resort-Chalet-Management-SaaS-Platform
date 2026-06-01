using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Route("guests")]
public class GuestsController : Controller
{
    private readonly IApplicationDbContext _context;

    public GuestsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        List<dynamic> customers = new();
        try
        {
            var dbCustomers = await _context.Customers.ToListAsync();
            foreach (var c in dbCustomers)
            {
                customers.Add(new { 
                    Id = c.Id, 
                    FirstName = c.FirstName, 
                    LastName = c.LastName, 
                    Email = c.Email, 
                    Phone = c.Phone 
                });
            }
        }
        catch (Exception)
        {
            // Database offline, fallback to simulated data
        }

        ViewBag.Customers = customers;
        return View();
    }
}
