namespace galaksi.Models;

// Type: "text" | "number" | "select" | "date" | "yesno"
public record Question(string Key, string Label, string Type, bool Required = true,
    string[]? Options = null, string? Hint = null, int? Min = null, int? Max = null);

public record QuoteStep(string Title, Question[] Questions);

public class QuoteDraft
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string No { get; set; } = "";
    public string Owner { get; set; } = "";
    public string Product { get; set; } = "";
    public string Status { get; set; } = "Taslak";   // Taslak | Beklemede | Tamamlandı
    public int Step { get; set; }                    // soru adımı; adım sayısına eşitse prim ekranı
    public Dictionary<string, string> Answers { get; set; } = new();
    public decimal? Premium { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Updated { get; set; } = DateTime.Now;

    public QuoteDraft Clone() => new()
    {
        Id = Id,
        No = No,
        Owner = Owner,
        Product = Product,
        Status = Status,
        Step = Step,
        Answers = new Dictionary<string, string>(Answers),
        Premium = Premium,
        Created = Created,
        Updated = Updated
    };
}

public class JetSatisIndexVm
{
    public IReadOnlyList<QuoteDraft> Open { get; set; } = Array.Empty<QuoteDraft>();
}

public class WizardVm
{
    public QuoteDraft Draft { get; set; } = default!;
    public QuoteStep[] Steps { get; set; } = Array.Empty<QuoteStep>();
    public bool IsPremium { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();
    public Dictionary<string, string> Values { get; set; } = new();
}