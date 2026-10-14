using Microsoft.AspNetCore.Identity;

namespace galaksi.Services;

public record AppUser(string UserName, string DisplayName, string Role, string PasswordHash);

public interface IUserService
{
    AppUser? Validate(string userName, string password);
    AppUser? Find(string userName);
}

// Örnek kullanıcılar (bellekte). Gerçek sisteme geçildiğinde bu sınıfın yerine yenisi yazılır.
public class SampleUserService : IUserService
{
    static readonly PasswordHasher<string> Hasher = new();
    readonly List<AppUser> _users;

    public SampleUserService()
    {
        _users = new()
        {
            Make("admin", "Yönetici Kullanıcı", "Admin", "Admin123!"),
            Make("acente", "Acente Kullanıcı", "Acente", "Acente123!"),
        };
    }

    static AppUser Make(string userName, string displayName, string role, string password) =>
        new(userName, displayName, role, Hasher.HashPassword(userName, password));

    public AppUser? Find(string userName) =>
        _users.FirstOrDefault(u => string.Equals(u.UserName, userName?.Trim(), StringComparison.OrdinalIgnoreCase));

    public AppUser? Validate(string userName, string password)
    {
        var user = Find(userName);
        if (user is null) return null;

        var result = Hasher.VerifyHashedPassword(user.UserName, user.PasswordHash, password ?? "");
        return result == PasswordVerificationResult.Failed ? null : user;
    }
}