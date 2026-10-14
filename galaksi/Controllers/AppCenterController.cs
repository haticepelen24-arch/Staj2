using galaksi.Services;
using Microsoft.AspNetCore.Mvc;

namespace galaksi.Controllers;

public class AppCenterController(IPortalData data) : Controller
{
    public IActionResult Index() => View(data.GetApps());
}