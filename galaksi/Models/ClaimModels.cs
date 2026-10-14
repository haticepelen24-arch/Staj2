using System.ComponentModel.DataAnnotations;

namespace galaksi.Models;

public class ClaimForm
{
    public static readonly string[] Types =
        { "Kaza", "Çalınma", "Yangın", "Su baskını", "Doğal afet", "Sağlık gideri", "Diğer" };

    [Required(ErrorMessage = "Poliçe seçin.")]
    public string? PolicyNo { get; set; }

    [Required(ErrorMessage = "Hasar tarihini girin.")]
    public DateTime? Date { get; set; }

    [Required(ErrorMessage = "Hasar türünü seçin.")]
    public string? Type { get; set; }

    [Range(0, 100000000, ErrorMessage = "Geçerli bir tutar girin.")]
    public int? EstimatedAmount { get; set; }

    [Required(ErrorMessage = "Cep telefonu girin.")]
    [RegularExpression(@"^(\+?90\s?|0\s?)?5\d{2}\s?\d{3}\s?\d{2}\s?\d{2}$",
        ErrorMessage = "Geçerli bir cep telefonu girin (05XX XXX XX XX).")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Açıklama girin.")]
    [MinLength(10, ErrorMessage = "Açıklama en az 10 karakter olmalı.")]
    [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
    public string? Description { get; set; }
}

public class ClaimNewVm
{
    public ClaimForm Form { get; set; } = new();
    public IReadOnlyList<PolicyItem> Policies { get; set; } = Array.Empty<PolicyItem>();
}