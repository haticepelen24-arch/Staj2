using System.ComponentModel.DataAnnotations;

namespace galaksi.Models;

// Şifremi unuttum ve Güvenlik kodu yenileme formları: yalnızca kullanıcı adı + cep telefonu
public class RecoveryViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı girin.")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Cep telefonu girin.")]
    [RegularExpression(@"^(\+?90\s?|0\s?)?5\d{2}\s?\d{3}\s?\d{2}\s?\d{2}$",
        ErrorMessage = "Geçerli bir cep telefonu girin (05XX XXX XX XX).")]
    public string Phone { get; set; } = "";
}

public class AuthPageViewModel
{
    public string Screen { get; set; } = "login";   // "login" | "mobile" | "forgot" | "code"
    public string? Done { get; set; }               // form gönderildikten sonraki onay mesajı
    public LoginViewModel Login { get; set; } = new();
    public LoginViewModel MobileLogin { get; set; } = new();
    public RecoveryViewModel Forgot { get; set; } = new();
    public RecoveryViewModel Code { get; set; } = new();
}