using galaksi.Models;
using galaksi.Services;
using Microsoft.AspNetCore.Mvc;

namespace galaksi.Controllers;

public class ClaimController(IPortalData data) : Controller
{
    public IActionResult Index(string? q, string? status)
    {
        ViewData["Q"] = q;
        ViewData["Status"] = status;
        return View(data.SearchClaims(q, status));
    }

    public IActionResult Details(string id)
    {
        var claim = data.GetClaim(id);
        if (claim is null)
        {
            TempData["Error"] = "Hasar dosyası bulunamadı.";
            return RedirectToAction(nameof(Index));
        }
        return View(claim);
    }

    [HttpGet]
    public IActionResult Create(string? policy = null)
    {
        var vm = new ClaimNewVm { Policies = data.GetActivePolicies() };
        vm.Form.PolicyNo = policy;
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create([Bind(Prefix = "Form")] ClaimForm form)
    {
        var policy = string.IsNullOrEmpty(form.PolicyNo) ? null : data.GetPolicy(form.PolicyNo);

        if (!string.IsNullOrEmpty(form.PolicyNo) && policy is null)
            ModelState.AddModelError("Form.PolicyNo", "Seçilen poliçe bulunamadı.");

        if (policy is not null && form.Date is DateTime d)
        {
            if (d.Date > DateTime.Today)
                ModelState.AddModelError("Form.Date", "Hasar tarihi bugünden sonra olamaz.");
            else if (d.Date < policy.Start.Date || d.Date > policy.End.Date)
                ModelState.AddModelError("Form.Date",
                    $"Hasar tarihi poliçe dönemi içinde olmalı ({UiHelpers.Day(policy.Start)} - {UiHelpers.Day(policy.End)}).");
        }

        if (!ModelState.IsValid)
            return View(new ClaimNewVm { Form = form, Policies = data.GetActivePolicies() });

        var claim = data.AddClaim(form, policy!);
        TempData["Ok"] = $"{claim.FileNo} numaralı hasar bildirimi alındı.";
        return RedirectToAction(nameof(Details), new { id = claim.FileNo });
    }
}