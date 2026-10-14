using galaksi.Services;
using Microsoft.AspNetCore.Mvc;

namespace galaksi.Controllers;

public class NewsController(IPortalData data) : Controller
{
    public IActionResult Index() => View(data.GetNews());

    public IActionResult Details(int id)
    {
        var item = data.GetNewsById(id);
        return item is null ? NotFound() : View(item);
    }
}
