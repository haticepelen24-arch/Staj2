using System.Security.Claims;
using galaksi.Models;
using galaksi.Services;
using Microsoft.AspNetCore.Mvc;

namespace galaksi.Controllers;

public class JetSatisController(IQuoteService quotes) : Controller
{
    string Owner => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    public IActionResult Index() => View(new JetSatisIndexVm { Open = quotes.GetOpen(Owner) });

    // Ürün seçildi: taslak oluştur ve sorulara geç
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Start(string? product)
    {
        if (string.IsNullOrWhiteSpace(product) || !QuoteForm.Products.Contains(product))
        {
            TempData["Error"] = "Devam etmek için bir ürün seçin.";
            return RedirectToAction(nameof(Index));
        }

        var draft = quotes.Start(Owner, product);
        return RedirectToAction(nameof(Wizard), new { id = draft.Id });
    }

    // Kaldığınız adım (soru adımı ya da prim ekranı)
    [HttpGet]
    public IActionResult Wizard(string id)
    {
        var d = quotes.Get(id, Owner);
        if (d is null || d.Status == "Tamamlandı") return Missing();
        return View(BuildVm(d));
    }

    // Soru adımını kaydet: nav = next | back | exit
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Step(string id, string? nav)
    {
        var d = quotes.Get(id, Owner);
        if (d is null || d.Status == "Tamamlandı") return Missing();

        var steps = QuoteCatalog.StepsFor(d.Product);
        if (d.Step >= steps.Length) return RedirectToAction(nameof(Wizard), new { id });

        var step = steps[d.Step];
        var values = new Dictionary<string, string>();
        foreach (var q in step.Questions)
            values[q.Key] = Request.Form["a_" + q.Key].ToString().Trim();

        // Girilenler her durumda kaydedilir, böylece hiçbir şey kaybolmaz
        foreach (var kv in values) d.Answers[kv.Key] = kv.Value;

        if (nav == "back")
        {
            d.Step = Math.Max(0, d.Step - 1);
            quotes.Save(d);
            return RedirectToAction(nameof(Wizard), new { id });
        }

        if (nav == "exit")
        {
            quotes.Save(d);
            TempData["Ok"] = $"{d.No} taslak olarak kaydedildi. Son tekliflerden devam edebilirsiniz.";
            return RedirectToAction(nameof(Index));
        }

        var errors = QuoteCatalog.Validate(step, values);
        if (errors.Count > 0)
        {
            quotes.Save(d);
            var vm = BuildVm(d);
            vm.Errors = errors;
            vm.Values = values;
            return View("Wizard", vm);
        }

        if (d.Step < steps.Length - 1)
        {
            d.Step++;
        }
        else
        {
            // Son adım: Teklif al
            d.Premium = QuotePricing.Calculate(d.Product, d.Answers);
            d.Step = steps.Length;
            d.Status = "Beklemede";
        }

        quotes.Save(d);
        return RedirectToAction(nameof(Wizard), new { id });
    }

    // Prim ekranından bilgileri düzenlemeye dön (prim geçersiz olur, Taslak'a döner)
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult EditAnswers(string id)
    {
        var d = quotes.Get(id, Owner);
        if (d is null || d.Status == "Tamamlandı") return Missing();

        var steps = QuoteCatalog.StepsFor(d.Product);
        d.Step = Math.Max(0, steps.Length - 1);
        d.Premium = null;
        d.Status = "Taslak";
        quotes.Save(d);
        return RedirectToAction(nameof(Wizard), new { id });
    }

    // Teklif oluştur: işlem tamamlanır ve listeden çıkar
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Complete(string id)
    {
        var d = quotes.Get(id, Owner);
        if (d is null) return Missing();

        var steps = QuoteCatalog.StepsFor(d.Product);
        if (d.Status != "Beklemede" || d.Step < steps.Length)
        {
            TempData["Error"] = "Teklif oluşturmak için önce primi hesaplayın.";
            return RedirectToAction(nameof(Index));
        }

        d.Status = "Tamamlandı";
        quotes.Save(d);
        TempData["Ok"] = $"{d.No} numaralı teklif oluşturuldu. Prim: {UiHelpers.Money(d.Premium ?? 0)}.";
        return RedirectToAction(nameof(Index));
    }

    IActionResult Missing()
    {
        TempData["Error"] = "Teklif bulunamadı.";
        return RedirectToAction(nameof(Index));
    }

    static WizardVm BuildVm(QuoteDraft d)
    {
        var steps = QuoteCatalog.StepsFor(d.Product);
        return new WizardVm { Draft = d, Steps = steps, IsPremium = d.Step >= steps.Length };
    }
}