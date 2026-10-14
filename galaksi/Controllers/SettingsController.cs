using Microsoft.AspNetCore.Mvc;

namespace galaksi.Controllers;

public class SettingsController : Controller
{
    public IActionResult Index() => View();
}