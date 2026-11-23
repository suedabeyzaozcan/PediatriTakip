using System.Security.Cryptography;

namespace PediatriTakip.Helpers;

/// <summary>Şifreleri PBKDF2 ile tuzlu (salt) olarak hash'ler.</summary>
public static class SifreHelper
{
    private const int Iterasyon = 100_000;

    public static string Hashle(string sifre)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(sifre, salt, Iterasyon, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Dogrula(string sifre, string kayitliHash)
    {
        var parcalar = kayitliHash.Split('.');
        if (parcalar.Length != 2) return false;
        byte[] salt = Convert.FromBase64String(parcalar[0]);
        byte[] beklenen = Convert.FromBase64String(parcalar[1]);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(sifre, salt, Iterasyon, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(hash, beklenen);
    }
}
