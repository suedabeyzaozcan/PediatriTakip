using System;
using System.Collections.Generic;

namespace PediatriTakip.Models;

public partial class Asilar
{
    public int Id { get; set; }

    public string Ad { get; set; } = null!;

    public int OnerilenAy { get; set; }

    public string? Aciklama { get; set; }

    public virtual ICollection<AsiKayitlari> AsiKayitlaris { get; set; } = new List<AsiKayitlari>();
}
