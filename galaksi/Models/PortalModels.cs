using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace galaksi.Models;

public record Kpi(string Label, int Value, string Change, bool Up);
public record PolicyType(string Name, int Count, string Color);
public record NewsItem(int Id, string Title, string Summary, string Body, DateTime Date, string Icon);
public record PolicyItem(string No, string Customer, string Type, DateTime Start, DateTime End, decimal Premium, string Status);
public record QuoteItem(string No, string Customer, string Type, decimal Premium, DateTime Date, string Status);
public record ClaimItem(string FileNo, string PolicyNo, string Customer, DateTime Date, decimal Amount, string Status,
    string Type = "", string Description = "", string Phone = "", DateTime? Reported = null);
public record CustomerItem(int Id, string Name, string Phone, string City, int PolicyCount);
public record DocumentItem(string Title, string Category, string Format, string Size, DateTime Date);
public record ReportItem(string Id, string Title, string Description, string Icon);
public record MonthlyRow(string Month, int Policies, decimal Premium);
public record AppItem(string Name, string Description, string Icon, string Url);
public record NotificationItem(int Id, string Kind, string Title, string Text, DateTime Time, string Url, bool Unread);
public record RankingRow(int Rank, string Agency, int Policies, bool IsMe);
public record RankingSummary(int Rank, int PreviousRank, int TotalAgencies, int Policies, int AveragePolicies, string Period, IReadOnlyList<RankingRow> Top);
public record CrossSellBucket(string Label, int Percent);
public record CrossSellSummary(double AveragePerCustomer, double AgencyAverage, IReadOnlyList<CrossSellBucket> Buckets, IReadOnlyList<string> TopCombos);

public class DashboardViewModel
{
    public string UserName { get; set; } = "Kullanıcı Adı";
    public List<Kpi> Kpis { get; set; } = new();
    public List<PolicyType> PolicyTypes { get; set; } = new();
    public List<NewsItem> News { get; set; } = new();
    public RankingSummary Ranking { get; set; } = default!;
    public List<MonthlyRow> Months { get; set; } = new();
    public CrossSellSummary CrossSell { get; set; } = default!;
    public int TotalPolicies => PolicyTypes.Sum(p => p.Count);
}
public class QuoteForm
{
    public static readonly string[] Products = { "Trafik", "Kasko", "Yangın", "Sağlık", "Konut", "Seyahat" };

    [Required(ErrorMessage = "Müşteri adı girin.")]
    public string? Customer { get; set; }

    [Required(ErrorMessage = "Ürün seçin.")]
    public string? Type { get; set; }

    [Required(ErrorMessage = "Prim girin.")]
    [Range(1, 10000000, ErrorMessage = "Geçerli bir prim girin.")]
    public int? Premium { get; set; }
}

public class JetSatisVm
{
    public QuoteForm Form { get; set; } = new();
    public IReadOnlyList<QuoteItem> Quotes { get; set; } = Array.Empty<QuoteItem>();
}

public static class UiHelpers
{
    static readonly CultureInfo Tr = new("tr-TR");

    public static string Money(decimal v) => v.ToString("C0", Tr);
    public static string Day(DateTime d) => d.ToString("dd.MM.yyyy", Tr);

    public static string Badge(string status) => status switch
    {
        "Aktif" or "Onaylandı" or "Ödendi" or "Kapandı" => "ok",
        "Beklemede" or "İncelemede"  => "warn",
        "İptal" or "Reddedildi" => "bad",
        _ => ""
    };
    public static string Ago(DateTime t)
    {
        var d = DateTime.Now - t;
        if (d.TotalMinutes < 1) return "şimdi";
        if (d.TotalMinutes < 60) return $"{(int)d.TotalMinutes} dk önce";
        if (d.TotalHours < 24) return $"{(int)d.TotalHours} sa önce";
        return $"{(int)d.TotalDays} gün önce";
    }
}