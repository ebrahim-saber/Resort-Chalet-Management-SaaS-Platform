using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities.Resorts;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Route("bookings")]
public class BookingsController : Controller
{
    private readonly IApplicationDbContext _context;

    public BookingsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        List<UnitType> unitTypes = new();
        try
        {
            unitTypes = await _context.UnitTypes.ToListAsync();
        }
        catch (Exception)
        {
            // Database is not reachable, proceed to fallback mock data below
        }

        if (unitTypes.Count == 0)
        {
            // Seed a list of luxury room types if none exist yet for demonstration
            ViewBag.ShowcaseUnitTypes = new List<dynamic>
            {
                new { 
                    Id = Guid.NewGuid(), 
                    Name = "Royal Pool Chalet", 
                    BasePrice = 450.00m, 
                    MaxOccupancy = 4, 
                    Amenities = "Private Infinity Pool, Sea View, Butler Service",
                    ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuCO67FORUlLm2FiZkGc7NEB-Vx9G1SjTBACxgQyPHit58-XKsV4127b5tpYEcSL6wjf4QKh0W3foY52Wg4FR7oy8AqUQHtlmy3vUChXUeFx1xoZVmGYLj8ydTvnZEjE4wUlUjPMDznafAQCgNE1ZpPn-t42_M4lfUeBtA8s7_-aFsiodRGhneqLWpxfEC0x_hFRbySssIzgfz5o3iKp29ga72eXq-bMWj6jlK44KkWn5zk2v54l1mcfTSMM9FmZFf4z23c8x5-uXDQJ" 
                },
                new { 
                    Id = Guid.NewGuid(), 
                    Name = "Premium Sea Villa", 
                    BasePrice = 320.00m, 
                    MaxOccupancy = 6, 
                    Amenities = "Beach Access, Kitchenette, Spacious Terrace",
                    ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuBXrKcc-gJv5rtJ9oHbwF035pJRS9fNVqz21qWE4zQGRKZhN67sVfeKDzLdASveZTQw7sFobtHggGJp3IGTh3x4GqRbIHUyCgwznaaNs2h7icD8EAOh-SiUm5l607TfKJip94rwBGbqvcO47uhzVgb_ot3KyMueLMG7re-8zZWjy5SNTYY-dSwYJMg0jU77u_VYXb6oHN0oo67fPTf3rUArmdo79nI2bin5lCucu1-jbpg4DKWJCH6xbHvmlo7IqD0o_7WEFcsE7FI9"
                },
                new { 
                    Id = Guid.NewGuid(), 
                    Name = "Boutique Suite", 
                    BasePrice = 180.00m, 
                    MaxOccupancy = 2, 
                    Amenities = "King Bed, Frosted Jacuzzi, High-speed Wifi",
                    ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuBXrKcc-gJv5rtJ9oHbwF035pJRS9fNVqz21qWE4zQGRKZhN67sVfeKDzLdASveZTQw7sFobtHggGJp3IGTh3x4GqRbIHUyCgwznaaNs2h7icD8EAOh-SiUm5l607TfKJip94rwBGbqvcO47uhzVgb_ot3KyMueLMG7re-8zZWjy5SNTYY-dSwYJMg0jU77u_VYXb6oHN0oo67fPTf3rUArmdo79nI2bin5lCucu1-jbpg4DKWJCH6xbHvmlo7IqD0o_7WEFcsE7FI9"
                }
            };
        }
        else
        {
            ViewBag.ShowcaseUnitTypes = unitTypes;
        }

        return View();
    }
}
