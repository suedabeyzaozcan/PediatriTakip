using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PediatriTakip.Data;
using PediatriTakip.Helpers;
using PediatriTakip.Models;

namespace PediatriTakip.Controllers;

/// <summary>Kullanıcı yönetimi — sadece Admin erişebilir.</summary>
[Authorize(Roles = "Admin")]
public class KullaniciController : TemelController
{
    private static readonly string[] Roller = { "Admin", "Doktor", "Veli" };
    public KullaniciController(PediatriContext db) : base(db) { }

    public IActionResult Index(string? rol)
    {
        var q = Db.Kullanicilars.AsQueryable();
        if (!string.IsNullOrEmpty(rol)) q = q.Where(k => k.Rol == rol);
        ViewBag.Rol = rol;
        return View(q.OrderBy(k => k.Rol).ThenBy(k => k.AdSoyad).ToList());
    }

    public IActionResult Ekle() => View("Form", new KullaniciVM());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Ekle(KullaniciVM vm)
    {
        if (string.IsNullOrEmpty(vm.Sifre)) ModelState.AddModelError(nameof(vm.Sifre), "Yeni kullanıcı için şifre zorunludur.");
        Kontrol(vm);
        if (!ModelState.IsValid) return View("Form", vm);

        var k = new Kullanicilar { OlusturmaTarihi = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), SifreHash = SifreHelper.Hashle(vm.Sifre!) };
        Aktar(vm, k);
        Db.Kullanicilars.Add(k);
        Db.SaveChanges();
        TempData["Mesaj"] = $"{k.AdSoyad} eklendi.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Duzenle(int id)
    {
        var k = Db.Kullanicilars.Find(id);
        if (k == null) return NotFound();
        return View("Form", new KullaniciVM { Id = k.Id, AdSoyad = k.AdSoyad, Email = k.Email, Rol = k.Rol, Telefon = k.Telefon, Uzmanlik = k.Uzmanlik });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Duzenle(int id, KullaniciVM vm)
    {
        var k = Db.Kullanicilars.Find(id);
        if (k == null) return NotFound();
        vm.Id = id;
        Kontrol(vm);
        if (k.Rol == "Doktor" && vm.Rol != "Doktor" && Db.Hastalars.Any(h => h.DoktorId == id))
            ModelState.AddModelError(nameof(vm.Rol), "Bu doktorun hastaları var; önce hastaları başka doktora aktarın.");
        if (!ModelState.IsValid) return View("Form", vm);

        Aktar(vm, k);
        if (!string.IsNullOrEmpty(vm.Sifre)) k.SifreHash = SifreHelper.Hashle(vm.Sifre);
        Db.SaveChanges();
        TempData["Mesaj"] = "Kullanıcı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>AJAX ile silme.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Sil(int id)
    {
        if (id == KullaniciId) return Json(new { basarili = false, mesaj = "Kendi hesabınızı silemezsiniz." });
        var k = Db.Kullanicilars.Find(id);
        if (k == null) return Json(new { basarili = false, mesaj = "Kullanıcı bulunamadı." });
        if (Db.Hastalars.Any(h => h.DoktorId == id) || Db.Muayenelers.Any(m => m.DoktorId == id))
            return Json(new { basarili = false, mesaj = "Bu doktora bağlı hasta/muayene kayıtları var, silinemez." });
        Db.Kullanicilars.Remove(k);   // veli silinirse çocuğun VeliId'si NULL olur
        Db.SaveChanges();
        return Json(new { basarili = true });
    }

    private void Kontrol(KullaniciVM vm)
    {
        if (!Roller.Contains(vm.Rol)) ModelState.AddModelError(nameof(vm.Rol), "Geçersiz rol.");
        var email = (vm.Email ?? "").Trim().ToLower();
        if (Db.Kullanicilars.Any(k => k.Email == email && k.Id != vm.Id))
            ModelState.AddModelError(nameof(vm.Email), "Bu e-posta başka bir kullanıcıda kayıtlı.");
    }

    private static void Aktar(KullaniciVM vm, Kullanicilar k)
    {
        k.AdSoyad = vm.AdSoyad.Trim();
        k.Email = vm.Email.Trim().ToLower();
        k.Rol = vm.Rol;
        k.Telefon = vm.Telefon;
        k.Uzmanlik = vm.Rol == "Doktor" ? vm.Uzmanlik : null;
    }
}
