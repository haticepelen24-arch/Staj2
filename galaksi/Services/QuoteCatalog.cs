using System.Globalization;
using galaksi.Models;

namespace galaksi.Services;

public static class QuoteCatalog
{
    static Question Text(string key, string label, string? hint = null) => new(key, label, "text", true, null, hint);
    static Question Num(string key, string label, int min, int max, string? hint = null, bool required = true) =>
        new(key, label, "number", required, null, hint, min, max);
    static Question Sel(string key, string label, params string[] options) => new(key, label, "select", true, options);
    static Question YesNo(string key, string label) => new(key, label, "yesno");
    static Question Date(string key, string label, string? hint = null) => new(key, label, "date", true, null, hint);

    // ÖRNEK SORULAR: ürün başına adımlar. Gerçek sorular geldiğinde burası değişecek.
    static readonly Dictionary<string, QuoteStep[]> Data = new()
    {
        ["Trafik"] = new[]
        {
            new QuoteStep("Araç bilgileri", new[]
            {
                Text("plaka", "Plaka", "Örn: 34 ABC 123"), Text("marka", "Marka"),
                Num("model_yili", "Model yılı", 1990, 2027),
                Sel("kullanim", "Kullanım tarzı", "Hususi", "Ticari", "Taksi", "Motosiklet"),
            }),
            new QuoteStep("Sigortalı bilgileri", new[]
            {
                Text("ad_soyad", "Ad soyad"), Date("dogum_tarihi", "Doğum tarihi"),
                Num("ehliyet_yili", "Ehliyet alım yılı", 1960, 2026),
            }),
            new QuoteStep("Teminat ve geçmiş", new[]
            {
                Sel("kademe", "Hasarsızlık kademesi", "0", "1", "2", "3", "4", "5", "6", "7", "8"),
                YesNo("hasar_var", "Son 1 yılda hasar kaydı var mı?"),
                Date("baslangic", "Poliçe başlangıç tarihi"),
            }),
        },
        ["Kasko"] = new[]
        {
            new QuoteStep("Araç bilgileri", new[]
            {
                Text("plaka", "Plaka", "Örn: 34 ABC 123"), Text("marka", "Marka"),
                Num("model_yili", "Model yılı", 1990, 2027),
                Num("deger", "Araç değeri (₺)", 50000, 20000000),
            }),
            new QuoteStep("Sürücü bilgileri", new[]
            {
                Num("surucu_yas", "Sürücü yaşı", 18, 90), Num("ehliyet_yili", "Ehliyet alım yılı", 1960, 2026),
                Sel("park", "Park yeri", "Garaj", "Açık otopark", "Sokak"),
            }),
            new QuoteStep("Teminat", new[]
            {
                Sel("paket", "Teminat paketi", "Standart", "Genişletilmiş", "Premium"),
                Sel("muafiyet", "Muafiyet", "Yok", "%2", "%5"),
                Num("km", "Yıllık kilometre", 0, 100000),
            }),
        },
        ["Yangın"] = new[]
        {
            new QuoteStep("Bina bilgileri", new[]
            {
                Sel("il", "İl", "İstanbul", "Ankara", "İzmir", "Bursa", "Antalya", "Diğer"),
                Sel("yapi", "Yapı tarzı", "Betonarme", "Yığma", "Ahşap"),
                Num("bina_yasi", "Bina yaşı", 0, 100), Num("kat", "Kat sayısı", 1, 50),
            }),
            new QuoteStep("Teminat", new[]
            {
                Num("bina_bedeli", "Bina bedeli (₺)", 50000, 50000000),
                Num("esya_bedeli", "Eşya bedeli (₺)", 0, 10000000, null, false),
                YesNo("deprem", "Deprem teminatı eklensin mi?"),
            }),
        },
        ["Sağlık"] = new[]
        {
            new QuoteStep("Sigortalı bilgileri", new[]
            {
                Text("ad_soyad", "Ad soyad"), Num("yas", "Yaş", 0, 80), Sel("cinsiyet", "Cinsiyet", "Kadın", "Erkek"),
            }),
            new QuoteStep("Sağlık durumu", new[]
            {
                YesNo("sigara", "Sigara kullanıyor mu?"), YesNo("kronik", "Kronik rahatsızlığı var mı?"),
                YesNo("onceki_sigorta", "Önceden sağlık sigortası var mıydı?"),
            }),
            new QuoteStep("Paket", new[]
            {
                Sel("paket", "Paket", "Temel", "Gelişmiş", "Tam"),
                Sel("ag", "Hastane ağı", "Anlaşmalı kurumlar", "Tüm kurumlar"),
            }),
        },
        ["Konut"] = new[]
        {
            new QuoteStep("Konut bilgileri", new[]
            {
                Sel("tip", "Konut tipi", "Daire", "Müstakil", "Villa"),
                Num("metrekare", "Brüt metrekare", 20, 1000), Num("bina_yasi", "Bina yaşı", 0, 100),
            }),
            new QuoteStep("Teminat", new[]
            {
                Num("konut_bedeli", "Konut bedeli (₺)", 50000, 50000000),
                Num("esya_bedeli", "Eşya bedeli (₺)", 0, 10000000, null, false),
                YesNo("deprem", "Deprem teminatı eklensin mi?"),
            }),
        },
        ["Seyahat"] = new[]
        {
            new QuoteStep("Seyahat bilgileri", new[]
            {
                Sel("bolge", "Bölge", "Yurt içi", "Schengen", "Dünya"),
                Num("gun", "Gün sayısı", 1, 90), Num("kisi", "Kişi sayısı", 1, 10),
            }),
            new QuoteStep("Teminat", new[]
            {
                Date("baslangic", "Seyahat başlangıç tarihi"),
                Sel("paket", "Paket", "Temel", "Kapsamlı"),
                YesNo("kis_sporu", "Kış sporu yapılacak mı?"),
            }),
        },
    };

    public static QuoteStep[] StepsFor(string product) =>
        Data.TryGetValue(product, out var steps) ? steps : Array.Empty<QuoteStep>();

    public static Dictionary<string, string> Validate(QuoteStep step, IReadOnlyDictionary<string, string> values)
    {
        var errors = new Dictionary<string, string>();
        foreach (var q in step.Questions)
        {
            values.TryGetValue(q.Key, out var v);
            v ??= "";

            if (v.Length == 0)
            {
                if (q.Required) errors[q.Key] = "Bu alan zorunlu.";
                continue;
            }

            switch (q.Type)
            {
                case "number":
                    if (!int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                        errors[q.Key] = "Geçerli bir sayı girin.";
                    else if ((q.Min is int mn && n < mn) || (q.Max is int mx && n > mx))
                        errors[q.Key] = $"{q.Min} ile {q.Max} arasında olmalı.";
                    break;
                case "select":
                    if (q.Options is null || !q.Options.Contains(v)) errors[q.Key] = "Listeden bir seçim yapın.";
                    break;
                case "yesno":
                    if (v != "Evet" && v != "Hayır") errors[q.Key] = "Evet veya Hayır seçin.";
                    break;
                case "date":
                    if (!DateTime.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                        errors[q.Key] = "Geçerli bir tarih girin.";
                    break;
            }
        }
        return errors;
    }
}

// ÖRNEK PRİM FORMÜLÜ: gerçek hesaplama kuralı geldiğinde burası değişecek.
public static class QuotePricing
{
    public static decimal Calculate(string product, IReadOnlyDictionary<string, string> a)
    {
        decimal N(string key, decimal def = 0) =>
            decimal.TryParse(a.GetValueOrDefault(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : def;
        bool Yes(string key) => a.GetValueOrDefault(key) == "Evet";
        string S(string key) => a.GetValueOrDefault(key) ?? "";

        decimal p;
        switch (product)
        {
            case "Trafik":
                p = 4200m;
                p *= S("kullanim") switch { "Ticari" => 1.4m, "Taksi" => 2.2m, "Motosiklet" => 0.8m, _ => 1m };
                p *= 1 + Math.Max(0, 2026 - N("model_yili", 2026)) * 0.01m;
                p *= 1 - Math.Min(8, N("kademe")) * 0.03m;
                if (Yes("hasar_var")) p *= 1.25m;
                break;
            case "Kasko":
                p = N("deger") * 0.022m;
                p *= S("paket") switch { "Genişletilmiş" => 1.15m, "Premium" => 1.35m, _ => 1m };
                p *= S("muafiyet") switch { "%2" => 0.95m, "%5" => 0.88m, _ => 1m };
                p *= S("park") switch { "Garaj" => 0.95m, "Sokak" => 1.1m, _ => 1m };
                if (N("surucu_yas", 30) < 26) p *= 1.3m;
                break;
            case "Yangın":
                p = N("bina_bedeli") * 0.0016m + N("esya_bedeli") * 0.003m;
                p *= S("yapi") switch { "Ahşap" => 1.8m, "Yığma" => 1.2m, _ => 1m };
                if (Yes("deprem")) p *= 1.35m;
                break;
            case "Sağlık":
                p = 6000m + N("yas") * 220m;
                p *= S("paket") switch { "Gelişmiş" => 1.3m, "Tam" => 1.7m, _ => 1m };
                if (Yes("sigara")) p *= 1.15m;
                if (Yes("kronik")) p *= 1.4m;
                break;
            case "Konut":
                p = N("konut_bedeli") * 0.0012m + N("esya_bedeli") * 0.003m + N("metrekare") * 6m;
                if (Yes("deprem")) p *= 1.3m;
                break;
            case "Seyahat":
                p = 90m * N("gun", 1) * N("kisi", 1);
                p *= S("bolge") switch { "Schengen" => 1.4m, "Dünya" => 1.9m, _ => 0.6m };
                if (S("paket") == "Kapsamlı") p *= 1.4m;
                if (Yes("kis_sporu")) p *= 1.25m;
                break;
            default:
                p = 1000m;
                break;
        }
        return Math.Max(100m, Math.Round(p / 10m) * 10m);
    }
}