using System.Globalization;

namespace PediatriTakip.Helpers;

public static class YasHelper
{
    public static DateTime? Tarih(string? s) =>
        DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;

    public static string Goster(string? s) => Tarih(s)?.ToString("dd.MM.yyyy") ?? s ?? "";

    /// <summary>Doğum tarihinden bugüne geçen ay sayısı.</summary>
    public static int AySayisi(string dogumTarihi, DateTime? referans = null)
    {
        var d = Tarih(dogumTarihi);
        if (d == null) return 0;
        var r = referans ?? DateTime.Today;
        int ay = (r.Year - d.Value.Year) * 12 + r.Month - d.Value.Month;
        if (r.Day < d.Value.Day) ay--;
        return Math.Max(ay, 0);
    }

    public static string YasMetni(string dogumTarihi)
    {
        int ay = AySayisi(dogumTarihi);
        if (ay < 12) return $"{ay} aylık";
        return ay % 12 == 0 ? $"{ay / 12} yaş" : $"{ay / 12} yaş {ay % 12} ay";
    }
}
