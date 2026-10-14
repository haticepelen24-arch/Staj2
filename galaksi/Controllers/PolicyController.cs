using galaksi.Services;
using Microsoft.AspNetCore.Mvc;

namespace galaksi.Controllers;

public class PolicyController(IPortalData data) : Controller
{
    public IActionResult Index(string? q, string? type)
    {
        ViewData["Q"] = q;
        ViewData["Type"] = type;
        return View(data.SearchPolicies(q, type));
    }
}