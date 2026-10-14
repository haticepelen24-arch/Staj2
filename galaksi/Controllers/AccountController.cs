using System.Security.Claims;
using galaksi.Models;
using galaksi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace galaksi.Controllers;

[AllowAnonymous]
public class AccountController(IUserService users) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null, string? screen = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new AuthPageViewModel { Screen = Normalize(screen) });
    }

    // Giriş yap sekmesi
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Login([Bind(Prefix = "Login")] LoginViewModel model, string? returnUrl = null)
        => SignInUserAsync(model, "login", returnUrl);

    // Mobil oturum sekmesi
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> MobileLogin([Bind(Prefix = "MobileLogin")] LoginViewModel model, string? returnUrl = null)
        => SignInUserAsync(model, "mobile", returnUrl);

    // Şifremi unuttum
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Forgot([Bind(Prefix = "Forgot")] RecoveryViewModel model)
    {
        var page = new AuthPageViewModel { Screen = "forgot", Forgot = model };
        if (!ModelState.IsValid)
            return View("Login", page);

        // TODO: Gerçek sistemde kullanıcı adı ve telefon eşleşirse SMS ile şifre yenileme bağlantısı gönderilir.
        // Kullanıcının kayıtlı olup olmadığı dışarıya belli edilmez.
        page.Done = "Bilgileriniz alındı. Kayıtlı bilgilerle eşleşiyorsa şifre yenileme bağlantısı telefonunuza gönderilecek.";
        return View("Login", page);
    }

    // Güvenlik kodu yenileme
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult RefreshCode([Bind(Prefix = "Code")] RecoveryViewModel model)
    {
        var page = new AuthPageViewModel { Screen = "code", Code = model };
        if (!ModelState.IsValid)
            return View("Login", page);

        // TODO: Gerçek sistemde eşleşme varsa yeni güvenlik kodu SMS ile gönderilir.
        page.Done = "Bilgileriniz alındı. Kayıtlı bilgilerle eşleşiyorsa yeni güvenlik kodu telefonunuza gönderilecek.";
        return View("Login", page);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    static string Normalize(string? screen) =>
        screen is "mobile" or "forgot" or "code" ? screen : "login";

    async Task<IActionResult> SignInUserAsync(LoginViewModel model, string screen, string? returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;

        AuthPageViewModel Page() => screen == "mobile"
            ? new AuthPageViewModel { Screen = screen, MobileLogin = model }
            : new AuthPageViewModel { Screen = screen, Login = model };

        if (!ModelState.IsValid)
            return View("Login", Page());

        var user = users.Validate(model.UserName, model.Password);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
            model.Password = "";
            return View("Login", Page());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserName),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Role, user.Role),
            new("login_type", screen),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}