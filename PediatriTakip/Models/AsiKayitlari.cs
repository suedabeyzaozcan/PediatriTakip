using System;
using System.Collections.Generic;

namespace PediatriTakip.Models;

public partial class AsiKayitlari
{
    public int Id { get; set; }

    public int HastaId { get; set; }

    public int AsiId { get; set; }

    public string UygulamaTarihi { get; set; } = null!;

    public string? Notlar { get; set; }

    public virtual Asilar Asi { get; set; } = null!;

    public virtual Hastalar Hasta { get; set; } = null!;
}
