using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PediatriTakip.Data;
using PediatriTakip.Helpers;
using PediatriTakip.Models;

// Ondalık sayılar (54.5 gibi) her bilgisayarda aynı okunsun
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Veritabanı bağlantısı (Database-First: modeller pediatri.db'den scaffold edildi)
builder.Services.AddDbContext<PediatriContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("PediatriDb")));

// Authentication: Cookie tabanlı giriş
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Hesap/Giris";
        o.AccessDeniedPath = "/Hesap/ErisimEngellendi";
        o.ExpireTimeSpan = TimeSpan.FromHours(2);
    });
builder.Services.AddAuthorization();

// AJAX isteklerinde anti-forgery token başlıkta gönderilir
builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

var app = builder.Build();

// İlk çalıştırmada örnek kullanıcıları ekle (şifreler hash'lenerek saklanır)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PediatriContext>();
    if (!db.Kullanicilars.Any())
    {
        var admin = new Kullanicilar { AdSoyad = "Sistem Yöneticisi", Email = "admin@pediatri.com", SifreHash = SifreHelper.Hashle("Admin123!"), Rol = "Admin" };
        var doktor = new Kullanicilar { AdSoyad = "Dr. Ayşe Yılmaz", Email = "doktor@pediatri.com", SifreHash = SifreHelper.Hashle("Doktor123!"), Rol = "Doktor", Uzmanlik = "Çocuk Sağlığı ve Hastalıkları" };
        var veli = new Kullanicilar { AdSoyad = "Mehmet Demir", Email = "veli@pediatri.com", SifreHash = SifreHelper.Hashle("Veli123!"), Rol = "Veli", Telefon = "0555 111 22 33" };
        db.AddRange(admin, doktor, veli);
        db.SaveChanges();

        var hasta = new Hastalar { AdSoyad = "Elif Demir", DogumTarihi = "2024-03-15", Cinsiyet = "Kız", KanGrubu = "A Rh+", DoktorId = doktor.Id, VeliId = veli.Id };
        db.Hastalars.Add(hasta);
        db.SaveChanges();

        db.Muayenelers.AddRange(
            new Muayeneler { HastaId = hasta.Id, DoktorId = doktor.Id, Tarih = "2024-04-15", BoyCm = 54, KiloKg = 4.3, BasCevresiCm = 37, Tani = "Sağlıklı", DoktorNotu = "1. ay kontrolü" },
            new Muayeneler { HastaId = hasta.Id, DoktorId = doktor.Id, Tarih = "2024-09-15", BoyCm = 65, KiloKg = 7.2, BasCevresiCm = 42, Tani = "Sağlıklı", DoktorNotu = "6. ay kontrolü, ek gıdaya başlandı" },
            new Muayeneler { HastaId = hasta.Id, DoktorId = doktor.Id, Tarih = "2025-03-15", BoyCm = 74, KiloKg = 9.1, BasCevresiCm = 45, Tani = "Sağlıklı" },
            new Muayeneler { HastaId = hasta.Id, DoktorId = doktor.Id, Tarih = "2026-03-15", BoyCm = 86, KiloKg = 12.0, BasCevresiCm = 47.5, Tani = "Hafif üst solunum yolu enfeksiyonu" });
        db.AsiKayitlaris.AddRange(
            new AsiKayitlari { HastaId = hasta.Id, AsiId = 1, UygulamaTarihi = "2024-03-15" },
            new AsiKayitlari { HastaId = hasta.Id, AsiId = 2, UygulamaTarihi = "2024-04-15" });
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
