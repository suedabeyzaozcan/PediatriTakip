using System;
using System.Collections.Generic;

namespace PediatriTakip.Models;

public partial class Kullanicilar
{
    public int Id { get; set; }

    public string AdSoyad { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string SifreHash { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public string? Telefon { get; set; }

    public string? Uzmanlik { get; set; }

    public string OlusturmaTarihi { get; set; } = null!;

    public virtual ICollection<Hastalar> HastalarDoktors { get; set; } = new List<Hastalar>();

    public virtual ICollection<Hastalar> HastalarVelis { get; set; } = new List<Hastalar>();

    public virtual ICollection<Muayeneler> Muayenelers { get; set; } = new List<Muayeneler>();
}
