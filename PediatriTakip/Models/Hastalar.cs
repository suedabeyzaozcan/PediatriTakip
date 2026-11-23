using System;
using System.Collections.Generic;

namespace PediatriTakip.Models;

public partial class Hastalar
{
    public int Id { get; set; }

    public string AdSoyad { get; set; } = null!;

    public string? TcKimlikNo { get; set; }

    public string DogumTarihi { get; set; } = null!;

    public string Cinsiyet { get; set; } = null!;

    public string? KanGrubu { get; set; }

    public string? Alerjiler { get; set; }

    public int DoktorId { get; set; }

    public int? VeliId { get; set; }

    public string KayitTarihi { get; set; } = null!;

    public virtual ICollection<AsiKayitlari> AsiKayitlaris { get; set; } = new List<AsiKayitlari>();

    public virtual Kullanicilar Doktor { get; set; } = null!;

    public virtual ICollection<Muayeneler> Muayenelers { get; set; } = new List<Muayeneler>();

    public virtual Kullanicilar? Veli { get; set; }
}
