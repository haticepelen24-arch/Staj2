using System.ComponentModel.DataAnnotations;

namespace galaksi.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı girin.")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Şifre girin.")]
    public string Password { get; set; } = "";
}