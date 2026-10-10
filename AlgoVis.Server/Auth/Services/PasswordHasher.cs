namespace AlgoVis.Server.Auth.Services;

/// <summary>Хеширование паролей через BCrypt (с автоматическим salt).</summary>
public sealed class PasswordHasher
{
    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);

    public bool Verify(string password, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(password, hash); }
        catch { return false; }
    }
}
