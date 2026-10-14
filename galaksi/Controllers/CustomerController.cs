using galaksi.Services;
using Microsoft.AspNetCore.Mvc;

namespace galaksi.Controllers;

public class CustomerController(IPortalData data) : Controller
{
    public IActionResult Index(string? q)
    {
        ViewData["Q"] = q;
        return View(data.SearchCustomers(q));
    }
}