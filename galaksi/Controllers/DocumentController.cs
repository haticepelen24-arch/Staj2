using galaksi.Services;
using Microsoft.AspNetCore.Mvc;

namespace galaksi.Controllers;

public class DocumentController(IPortalData data) : Controller
{
    public IActionResult Index(string? category)
    {
        ViewData["Category"] = category;
        return View(data.GetDocuments(category));
    }
}