namespace galaksi.Models;

public record ProductShare(string Name, int Count, string Color);

public record MonthDetail(string Month, int Year, int Policies, decimal Premium,
    int? PrevPolicies, decimal? PrevPremium, int Cancelled, int NewCustomers,
    IReadOnlyList<ProductShare> Products);

public class MonthlyReportVm
{
    public MonthDetail Current { get; set; } = default!;
    public List<MonthDetail> Others { get; set; } = new();
}

public record ComboRow(string[] Products, int Customers);

public class CrossSellReportVm
{
    public int TotalCustomers { get; set; }
    public int MultiProductCustomers { get; set; }
    public double AveragePerCustomer { get; set; }
    public List<CrossSellBucket> Buckets { get; set; } = new();
    public List<ComboRow> Pairs { get; set; } = new();
    public List<ComboRow> Triples { get; set; } = new();
}