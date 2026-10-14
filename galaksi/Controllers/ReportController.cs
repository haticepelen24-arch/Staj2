using galaksi.Services;
using Microsoft.AspNetCore.Mvc;

namespace galaksi.Controllers;

public class ReportController(IPortalData data) : Controller
{
    public IActionResult Index() => View(data.GetReports());

    public IActionResult Detail(string id)
    {
        var report = data.GetReport(id);
        if (report is null) return NotFound();
        ViewData["Report"] = report;
        return View(data.GetMonthlySummary());
    }
}