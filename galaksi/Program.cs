using galaksi.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // Tüm sayfalar giriş ister; Account/Login [AllowAnonymous] ile açık bırakıldı.
    options.Filters.Add(new AuthorizeFilter());
});

builder.Services.AddSingleton<IPortalData, SamplePortalData>();
builder.Services.AddSingleton<IUserService, SampleUserService>();
builder.Services.AddSingleton<IQuoteService, SampleQuoteService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "galaksi.auth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();