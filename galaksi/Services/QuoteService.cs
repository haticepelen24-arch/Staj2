using galaksi.Models;

namespace galaksi.Services;

public interface IQuoteService
{
    IReadOnlyList<QuoteDraft> GetOpen(string owner);        // Taslak + Beklemede, en yeni üstte
    QuoteDraft? Get(string id, string owner);
    QuoteDraft Start(string owner, string product);
    void Save(QuoteDraft draft);
}

// Örnek, bellekte tutulan teklif deposu. Gerçek veritabanı geldiğinde yenisi yazılır.
public class SampleQuoteService : IQuoteService
{
    readonly object _lock = new();
    readonly List<QuoteDraft> _items = new();
    int _next = 100300;
    int _seedNo = 100245;

    public SampleQuoteService()
    {
        Seed("admin", "Trafik", 1, new() { ["plaka"] = "34 ABC 123", ["marka"] = "Renault", ["model_yili"] = "2019", ["kullanim"] = "Hususi" }, TimeSpan.FromHours(3));
        Seed("admin", "Kasko", 3, new()
        {
            ["plaka"] = "34 DEF 456",
            ["marka"] = "Toyota",
            ["model_yili"] = "2021",
            ["deger"] = "850000",
            ["surucu_yas"] = "38",
            ["ehliyet_yili"] = "2008",
            ["park"] = "Garaj",
            ["paket"] = "Genişletilmiş",
            ["muafiyet"] = "%2",
            ["km"] = "15000"
        }, TimeSpan.FromDays(1));
        Seed("admin", "Konut", 0, new(), TimeSpan.FromDays(2));
        Seed("acente", "Sağlık", 1, new() { ["ad_soyad"] = "Deniz Acar", ["yas"] = "34", ["cinsiyet"] = "Kadın" }, TimeSpan.FromHours(6));
    }

    void Seed(string owner, string product, int step, Dictionary<string, string> answers, TimeSpan ago)
    {
        var total = QuoteCatalog.StepsFor(product).Length;
        var premium = step >= total;
        _items.Add(new QuoteDraft
        {
            No = "TKL-" + _seedNo++,
            Owner = owner,
            Product = product,
            Step = step,
            Answers = answers,
            Status = premium ? "Beklemede" : "Taslak",
            Premium = premium ? QuotePricing.Calculate(product, answers) : null,
            Created = DateTime.Now - ago,
            Updated = DateTime.Now - ago,
        });
    }

    public IReadOnlyList<QuoteDraft> GetOpen(string owner)
    {
        lock (_lock)
            return _items.Where(x => x.Owner == owner && x.Status != "Tamamlandı")
                         .OrderByDescending(x => x.Updated)
                         .Select(x => x.Clone())
                         .ToList();
    }

    public QuoteDraft? Get(string id, string owner)
    {
        lock (_lock)
            return _items.FirstOrDefault(x => x.Id == id && x.Owner == owner)?.Clone();
    }

    public QuoteDraft Start(string owner, string product)
    {
        lock (_lock)
        {
            var d = new QuoteDraft { No = "TKL-" + _next++, Owner = owner, Product = product };
            _items.Insert(0, d);
            return d.Clone();
        }
    }

    public void Save(QuoteDraft draft)
    {
        lock (_lock)
        {
            var i = _items.FindIndex(x => x.Id == draft.Id && x.Owner == draft.Owner);
            if (i < 0) return;
            var copy = draft.Clone();
            copy.Updated = DateTime.Now;
            _items[i] = copy;
        }
    }
}