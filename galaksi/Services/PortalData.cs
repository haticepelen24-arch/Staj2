using System.Globalization;
using galaksi.Models;

namespace galaksi.Services;

public interface IPortalData
{
    DashboardViewModel GetDashboard();
    IReadOnlyList<QuoteItem> GetQuotes();
    QuoteItem AddQuote(QuoteForm form);
    IReadOnlyList<PolicyItem> SearchPolicies(string? q, string? type);
    IReadOnlyList<ClaimItem> SearchClaims(string? q, string? status);
    ClaimItem? GetClaim(string fileNo);
    ClaimItem AddClaim(ClaimForm form, PolicyItem policy);
    PolicyItem? GetPolicy(string no);
    IReadOnlyList<PolicyItem> GetActivePolicies();
    IReadOnlyList<CustomerItem> SearchCustomers(string? q);
    IReadOnlyList<DocumentItem> GetDocuments(string? category);
    IReadOnlyList<ReportItem> GetReports();
    ReportItem? GetReport(string id);
    IReadOnlyList<MonthlyRow> GetMonthlySummary();
    IReadOnlyList<NewsItem> GetNews();
    NewsItem? GetNewsById(int id);
    IReadOnlyList<AppItem> GetApps();
    IReadOnlyList<NotificationItem> GetNotifications();
}

// Örnek veri. Gerçek servis/veritabanı geldiğinde yeni bir IPortalData uygulaması yazılır,
// Program.cs'te yalnızca kayıt satırı değiştirilir.
public class SamplePortalData : IPortalData
{
    static readonly CultureInfo Tr = new("tr-TR");

    static bool Has(string? text, string? q) =>
        string.IsNullOrWhiteSpace(q) ||
        (text != null && Tr.CompareInfo.IndexOf(text, q.Trim(), CompareOptions.IgnoreCase) >= 0);

    static DateTime D(int month, int day) => new(2026, month, day);

    readonly object _lock = new();
    readonly List<QuoteItem> _quotes = new()
    {
        new("TKL-100245", "Ahmet Yılmaz", "Kasko", 18450, D(10, 7), "Onaylandı"),
        new("TKL-100244", "Elif Kaya", "Trafik", 6200, D(10, 7), "Beklemede"),
        new("TKL-100243", "Mehmet Demir", "Konut", 3150, D(10, 6), "Onaylandı"),
        new("TKL-100242", "Zeynep Arslan", "Sağlık", 12900, D(10, 5), "Taslak"),
    };

    static readonly PolicyItem[] Policies = BuildPolicies();

    // Örnek veri: türlere göre dağılmış 60 poliçe (sabit tohumla, her açılışta aynı)
    static PolicyItem[] BuildPolicies()
    {
        string[] names =
        {
            "Ahmet Yılmaz", "Elif Kaya", "Mehmet Demir", "Zeynep Arslan", "Can Öztürk", "Ayşe Çelik",
            "Burak Şahin", "Selin Aydın", "Murat Koç", "Derya Yıldız", "Emre Aksoy", "Hülya Polat"
        };
        string[] types =
        {
            "Trafik", "Trafik", "Trafik", "Trafik", "Trafik", "Kasko", "Kasko", "Kasko",
            "Yangın", "Yangın", "Sağlık", "Sağlık", "Konut", "Konut", "Seyahat", "Diğer"
        };

        var rnd = new Random(42);
        var list = new List<PolicyItem>();
        for (int i = 1; i <= 60; i++)
        {
            var type = types[rnd.Next(types.Length)];
            var start = D(1, 1).AddDays(rnd.Next(0, 280));
            var end = type == "Seyahat" ? start.AddDays(rnd.Next(7, 31)) : start.AddYears(1);
            var (lo, hi) = type switch
            {
                "Trafik" => (4000, 8000),
                "Kasko" => (12000, 26000),
                "Yangın" => (2500, 6000),
                "Sağlık" => (8000, 18000),
                "Konut" => (2000, 5000),
                "Seyahat" => (400, 1500),
                _ => (1000, 5000)
            };
            var premium = Math.Round(rnd.Next(lo, hi) / 50.0) * 50;
            var roll = rnd.Next(100);
            var status = roll < 8 ? "İptal" : roll < 18 ? "Beklemede" : "Aktif";
            list.Add(new PolicyItem($"POL-2026-{i:0000}", names[rnd.Next(names.Length)], type, start, end, (decimal)premium, status));
        }
        return list.OrderBy(p => p.No).ToArray();
    }

    readonly List<ClaimItem> _claims = new()
    {
        new("HSR-50121", "POL-2026-0001", "Ahmet Yılmaz", D(9, 12), 42500, "İncelemede", "Kaza",
            "Park halindeyken aracın yan tarafına çarpıldı.", "0532 000 00 01", D(9, 13)),
        new("HSR-50098", "POL-2026-0008", "Selin Aydın", D(9, 2), 18700, "Ödendi", "Çalınma",
            "Araç içindeki kişisel eşyalar çalındı.", "0533 000 00 02", D(9, 3)),
        new("HSR-50077", "POL-2026-0003", "Mehmet Demir", D(8, 21), 9300, "Kapandı", "Su baskını",
            "Üst kattan sızan su nedeniyle tavan hasar gördü.", "0534 000 00 03", D(8, 22)),
        new("HSR-50064", "POL-2026-0007", "Burak Şahin", D(8, 10), 15200, "Reddedildi", "Kaza",
            "Kavşakta yan yana çarpışma yaşandı.", "0535 000 00 04", D(8, 11)),
    };
    int _nextClaim = 50122;

    static readonly CustomerItem[] Customers =
    {
        new(1, "Ahmet Yılmaz", "0532 000 00 01", "İstanbul", 2),
        new(2, "Elif Kaya", "0533 000 00 02", "Ankara", 1),
        new(3, "Mehmet Demir", "0534 000 00 03", "İzmir", 1),
        new(4, "Zeynep Arslan", "0535 000 00 04", "Bursa", 1),
        new(5, "Can Öztürk", "0536 000 00 05", "İstanbul", 1),
        new(6, "Ayşe Çelik", "0537 000 00 06", "Antalya", 1),
    };

    static readonly DocumentItem[] Documents =
    {
        new("Teklif Formu", "Formlar", "PDF", "240 KB", D(9, 30)),
        new("Hasar Bildirim Formu", "Formlar", "PDF", "180 KB", D(9, 12)),
        new("Kasko Ürün Kılavuzu", "Ürün Kılavuzları", "PDF", "1,2 MB", D(8, 15)),
        new("Sağlık Paketleri Genel Şartları", "Ürün Kılavuzları", "PDF", "2,4 MB", D(8, 10)),
        new("Sigorta Mevzuatı Özeti", "Mevzuat", "PDF", "860 KB", D(7, 1)),
        new("Yeni Teklif Modülü Eğitimi", "Eğitim", "PPTX", "5,1 MB", D(8, 8)),
    };

    static readonly ReportItem[] Reports =
    {
        new("uretim", "Aylık üretim raporu", "Aylık poliçe sayısı ve prim üretimi.", "📈"),
        new("capraz-satis", "Çapraz satış raporu", "Müşteri başına ürün çeşitliliği.", "📊"),
        new("musteri", "Müşteri raporu", "Müşteri portföyü ve yenileme durumu.", "👥"),
        new("hasar", "Hasar raporu", "Hasar dosyaları ve ödeme özeti.", "🛡"),
    };

    static readonly MonthlyRow[] Months =
    {
        new("Mayıs", 982, 1180000), new("Haziran", 1040, 1240000), new("Temmuz", 1105, 1310000),
        new("Ağustos", 1090, 1275000), new("Eylül", 1190, 1420000), new("Ekim", 1248, 1495000),
    };

    static readonly NewsItem[] News =
    {
        new(1, "Yeni ürün: Sağlık sigortası paketleri", "Müşterilerinize özel yeni sağlık paketlerini inceleyin.",
            "Yeni sağlık sigortası paketleri bugünden itibaren satışa açıldı. Paket detayları ve prim tablosu Doküman Merkezi'nde yer alıyor.", D(8, 10), "📣"),
        new(2, "Sistem güncellemesi", "11.08.2026 tarihinde 22:00 - 24:00 arasında bakım yapılacaktır.",
            "Belirtilen saatlerde sistem bakım nedeniyle kullanılamayacaktır. Bekleyen işlemlerinizi önceden tamamlamanızı öneririz.", D(8, 9), "🛠"),
        new(3, "Eğitim duyurusu", "12 Ağustos'ta \"Yeni Teklif Modülü\" eğitimi yapılacaktır.",
            "Eğitim çevrim içi yapılacaktır. Katılım bağlantısı kayıtlı e-posta adreslerinize gönderilecektir.", D(8, 8), "🎓"),
        new(4, "Kampanya", "Trafik sigortasında özel indirim fırsatını kaçırmayın.",
            "Kampanya koşulları ve geçerlilik tarihleri için Doküman Merkezi'ndeki kampanya broşürüne bakabilirsiniz.", D(8, 6), "🏷"),
    };

    static readonly AppItem[] Apps =
    {
        new("Acente Portalı", "Ana panel ve günlük işlemler.", "🏠", "/Home"),
        new("Salesbox", "Satış performansı ve fırsat takibi.", "📦", "#"),
        new("Hasar sorgulama", "Hasar dosya durumlarını sorgulayın.", "🔍", "/Claim"),
        new("Poliçe sorgulama", "Poliçe numarası veya müşteriyle arayın.", "📄", "/Policy"),
        new("Müşteri işlemleri", "Müşteri listesi ve iletişim bilgileri.", "👥", "/Customer"),
    };

    public DashboardViewModel GetDashboard() => new()
    {
        Kpis =
        {
            new Kpi("Kesilen poliçe", 128, "%12,5", true),
            new Kpi("Teklif sayısı", 86, "%8,3", true),
            new Kpi("İptal edilen poliçe", 7, "%22,2", false),
            new Kpi("Vadesi gelen poliçe", 34, "%5,6", true),
        },
        PolicyTypes =
        {
            new PolicyType("Trafik", 399, "#1f6fd1"),
            new PolicyType("Kasko", 224, "#14a5a5"),
            new PolicyType("Yangın", 175, "#3aa655"),
            new PolicyType("Sağlık", 150, "#f2b705"),
            new PolicyType("Konut", 125, "#8ec5f0"),
            new PolicyType("Seyahat", 100, "#b7c6dc"),
            new PolicyType("Diğer", 75, "#7b8794"),
        },
        News = News.OrderByDescending(n => n.Date).Take(4).ToList(),
        Ranking = new RankingSummary(12, 15, 148, 1248, 1054, "Ekim 2026", new RankingRow[]
        {
            new(1, "Boğaz Sigorta Aracılık", 2310, false),
            new(2, "Marmara Acente", 2144, false),
            new(3, "Ege Güven Sigorta", 1987, false),
            new(4, "Karadeniz Acente", 1902, false),
            new(5, "Başkent Sigorta", 1860, false),
            new(12, "Sizin acenteniz", 1248, true),
        }),
        Months = Months.ToList(),
        CrossSell = new CrossSellSummary(1.8, 1.6, new CrossSellBucket[]
        {
            new("1 ürün", 58),
            new("2 ürün", 27),
            new("3 ve üzeri", 15),
        }, new[] { "Trafik + Kasko", "Konut + Yangın", "Sağlık + Seyahat" })
    };

    public IReadOnlyList<QuoteItem> GetQuotes()
    {
        lock (_lock) return _quotes.ToList();
    }

    public QuoteItem AddQuote(QuoteForm form)
    {
        lock (_lock)
        {
            var q = new QuoteItem("TKL-" + (100242 + _quotes.Count), form.Customer!.Trim(), form.Type!,
                form.Premium!.Value, DateTime.Now, "Beklemede");
            _quotes.Insert(0, q);
            return q;
        }
    }

    public IReadOnlyList<PolicyItem> SearchPolicies(string? q, string? type) =>
        Policies.Where(p => (Has(p.No, q) || Has(p.Customer, q)) && (string.IsNullOrEmpty(type) || p.Type == type)).ToList();

    public IReadOnlyList<ClaimItem> SearchClaims(string? q, string? status)
    {
        lock (_lock)
            return _claims.Where(c =>
                    (Has(c.FileNo, q) || Has(c.PolicyNo, q) || Has(c.Customer, q)) &&
                    (string.IsNullOrEmpty(status) || c.Status == status))
                .ToList();
    }

    public ClaimItem? GetClaim(string fileNo)
    {
        lock (_lock)
            return _claims.FirstOrDefault(c => string.Equals(c.FileNo, fileNo, StringComparison.OrdinalIgnoreCase));
    }

    public ClaimItem AddClaim(ClaimForm form, PolicyItem policy)
    {
        lock (_lock)
        {
            var claim = new ClaimItem("HSR-" + _nextClaim++, policy.No, policy.Customer, form.Date!.Value,
                form.EstimatedAmount ?? 0, "İncelemede", form.Type!, form.Description!.Trim(),
                form.Phone!.Trim(), DateTime.Now);
            _claims.Insert(0, claim);
            return claim;
        }
    }

    public PolicyItem? GetPolicy(string no) => Policies.FirstOrDefault(p => p.No == no);

    public IReadOnlyList<PolicyItem> GetActivePolicies() =>
        Policies.Where(p => p.Status == "Aktif" && p.End.Date >= DateTime.Today).ToList();
    public IReadOnlyList<CustomerItem> SearchCustomers(string? q) =>
        Customers.Where(c => Has(c.Name, q) || Has(c.City, q) || Has(c.Phone, q)).ToList();

    public IReadOnlyList<DocumentItem> GetDocuments(string? category) =>
        Documents.Where(d => string.IsNullOrEmpty(category) || d.Category == category).ToList();

    public IReadOnlyList<ReportItem> GetReports() => Reports;
    public ReportItem? GetReport(string id) => Reports.FirstOrDefault(r => r.Id == id);
    public IReadOnlyList<MonthlyRow> GetMonthlySummary() => Months;
    public IReadOnlyList<NewsItem> GetNews() => News.OrderByDescending(n => n.Date).ToList();
    public NewsItem? GetNewsById(int id) => News.FirstOrDefault(n => n.Id == id);
    public IReadOnlyList<AppItem> GetApps() => Apps;
    public IReadOnlyList<NotificationItem> GetNotifications()
    {
        var now = DateTime.Now;
        return new NotificationItem[]
        {
            new(1, "news", "Yeni ürün: Sağlık sigortası paketleri", "Müşterilerinize özel yeni paketleri inceleyin.", now.AddHours(-2), "/News/Details/1", true),
            new(2, "promo", "Trafik sigortasında özel indirim", "Kampanya koşullarını Haber Merkezi'nde görün.", now.AddHours(-5), "/News/Details/4", true),
            new(3, "system", "Planlı sistem bakımı", "Bakım saatlerini haber detayında görebilirsiniz.", now.AddDays(-1), "/News/Details/2", true),
            new(4, "claim", "Hasar dosyanız güncellendi", "HSR-50121 dosyası incelemeye alındı.", now.AddDays(-2), "/Claim?q=HSR-50121", false),
        };
    }
}