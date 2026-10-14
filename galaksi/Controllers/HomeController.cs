using galaksi.Services;
using Microsoft.AspNetCore.Mvc;

namespace galaksi.Controllers;

public class HomeController(IPortalData data) : Controller
{
    public IActionResult Index()
    {
        var vm = data.GetDashboard();
        vm.UserName = User.Identity?.Name ?? vm.UserName;
        return View(vm);
    }
}