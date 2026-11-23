using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PediatriTakip.Data;
using PediatriTakip.Models;

namespace PediatriTakip.Controllers;

/// <summary>
/// Ortak yetki mantığı: giriş yapan kullanıcı hangi hastaları görebilir?
/// Admin: tüm hastalar · Doktor: kendi hastaları · Veli: kendi çocukları
/// </summary>
public abstract class TemelController : Controller
{
    protected readonly PediatriContext Db;
    protected TemelController(PediatriContext db) => Db = db;

    protected int KullaniciId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    protected string Rol => User.FindFirstValue(ClaimTypes.Role) ?? "";

    protected IQueryable<Hastalar> ErisilebilirHastalar() => Rol switch
    {
        "Admin" => Db.Hastalars,
        "Doktor" => Db.Hastalars.Where(h => h.DoktorId == KullaniciId),
        "Veli" => Db.Hastalars.Where(h => h.VeliId == KullaniciId),
        _ => Db.Hastalars.Where(h => false)
    };

    protected bool HastayaErisebilir(int hastaId) => ErisilebilirHastalar().Any(h => h.Id == hastaId);

    /// <summary>Doktor ve Admin düzenleme yapabilir; Veli sadece görüntüler.</summary>
    protected bool DuzenleyebilirMi(int hastaId) => Rol != "Veli" && HastayaErisebilir(hastaId);
}
