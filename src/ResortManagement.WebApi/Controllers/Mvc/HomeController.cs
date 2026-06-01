using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ResortManagement.Application.Common.Interfaces;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Route("")]
public class HomeController : Controller
{
    private readonly IMediator _mediator;

    public HomeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("login")]
    public IActionResult Login(string email, string password)
    {
        // Simple demonstration login - redirects directly to dashboard
        if (!string.IsNullOrEmpty(email) && email.Contains("@"))
        {
            return RedirectToAction("Index", "Dashboard");
        }

        ViewBag.Error = "Invalid email or password format.";
        return View("Index");
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        return RedirectToAction("Index");
    }
}
