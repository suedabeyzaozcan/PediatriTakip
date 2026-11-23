using System;
using System.Collections.Generic;

namespace PediatriTakip.Models;

public partial class Muayeneler
{
    public int Id { get; set; }

    public int HastaId { get; set; }

    public int DoktorId { get; set; }

    public string Tarih { get; set; } = null!;

    public double? BoyCm { get; set; }

    public double? KiloKg { get; set; }

    public double? BasCevresiCm { get; set; }

    public string? Tani { get; set; }

    public string? DoktorNotu { get; set; }

    public virtual Kullanicilar Doktor { get; set; } = null!;

    public virtual Hastalar Hasta { get; set; } = null!;
}
