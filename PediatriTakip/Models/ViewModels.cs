using System.ComponentModel.DataAnnotations;

namespace PediatriTakip.Models;

public class GirisVM
{
    [Required(ErrorMessage = "E-posta zorunludur"), EmailAddress(ErrorMessage = "Geçerli bir e-posta girin")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre zorunludur"), DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public class KayitVM
{
    [Required(ErrorMessage = "Ad soyad zorunludur"), StringLength(100)]
    [Display(Name = "Ad Soyad")]
    public string AdSoyad { get; set; } = "";

    [Required(ErrorMessage = "E-posta zorunludur"), EmailAddress(ErrorMessage = "Geçerli bir e-posta girin")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = "";

    [Display(Name = "Telefon")]
    public string? Telefon { get; set; }

    [Required(ErrorMessage = "Şifre zorunludur"), MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalı"), DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = "";

    [Required(ErrorMessage = "Şifre tekrarı zorunludur"), Compare(nameof(Sifre), ErrorMessage = "Şifreler eşleşmiyor"), DataType(DataType.Password)]
    [Display(Name = "Şifre (Tekrar)")]
    public string SifreTekrar { get; set; } = "";
}

/// <summary>Admin'in kullanıcı (doktor/veli/admin) ekleme-düzenleme formu</summary>
public class KullaniciVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad soyad zorunludur"), Display(Name = "Ad Soyad")]
    public string AdSoyad { get; set; } = "";

    [Required(ErrorMessage = "E-posta zorunludur"), EmailAddress, Display(Name = "E-posta")]
    public string Email { get; set; } = "";

    [Required, Display(Name = "Rol")]
    public string Rol { get; set; } = "Doktor";

    [Display(Name = "Telefon")]
    public string? Telefon { get; set; }

    [Display(Name = "Uzmanlık")]
    public string? Uzmanlik { get; set; }

    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalı"), DataType(DataType.Password)]
    [Display(Name = "Şifre (düzenlemede boş bırakılırsa değişmez)")]
    public string? Sifre { get; set; }
}

public class HastaVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad soyad zorunludur"), Display(Name = "Ad Soyad")]
    public string AdSoyad { get; set; } = "";

    [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalı"), RegularExpression(@"^\d{11}$", ErrorMessage = "Sadece rakam girin")]
    [Display(Name = "TC Kimlik No")]
    public string? TcKimlikNo { get; set; }

    [Required(ErrorMessage = "Doğum tarihi zorunludur"), DataType(DataType.Date), Display(Name = "Doğum Tarihi")]
    public DateTime? DogumTarihi { get; set; }

    [Required(ErrorMessage = "Cinsiyet seçin"), Display(Name = "Cinsiyet")]
    public string Cinsiyet { get; set; } = "";

    [Display(Name = "Kan Grubu")]
    public string? KanGrubu { get; set; }

    [Display(Name = "Alerjiler")]
    public string? Alerjiler { get; set; }

    [Display(Name = "Sorumlu Doktor")]
    public int? DoktorId { get; set; }

    [Display(Name = "Veli Hesabı")]
    public int? VeliId { get; set; }
}

public class MuayeneVM
{
    public int HastaId { get; set; }

    [Required(ErrorMessage = "Tarih zorunludur")]
    public DateTime? Tarih { get; set; }

    [Range(20, 220, ErrorMessage = "Boy 20-220 cm arasında olmalı")]
    public double? BoyCm { get; set; }

    [Range(0.5, 200, ErrorMessage = "Kilo 0,5-200 kg arasında olmalı")]
    public double? KiloKg { get; set; }

    [Range(20, 70, ErrorMessage = "Baş çevresi 20-70 cm arasında olmalı")]
    public double? BasCevresiCm { get; set; }

    public string? Tani { get; set; }
    public string? DoktorNotu { get; set; }
}

public class AsiKaydiVM
{
    public int HastaId { get; set; }
    [Required] public int AsiId { get; set; }
    [Required(ErrorMessage = "Tarih zorunludur")] public DateTime? UygulamaTarihi { get; set; }
    public string? Notlar { get; set; }
}

public class PanelVM
{
    public int HastaSayisi { get; set; }
    public int DoktorSayisi { get; set; }
    public int VeliSayisi { get; set; }
    public int BuAyMuayene { get; set; }
    public List<Muayeneler> SonMuayeneler { get; set; } = new();
}
