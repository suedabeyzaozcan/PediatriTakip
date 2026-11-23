using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PediatriTakip.Data;
using PediatriTakip.Helpers;
using PediatriTakip.Models;

namespace PediatriTakip.Controllers;

[Authorize]
public class HastaController : TemelController
{
    public HastaController(PediatriContext db) : base(db) { }

    // ---------- LİSTE (Admin, Doktor) ----------
    [Authorize(Roles = "Admin,Doktor")]
    public IActionResult Index()
    {
        var liste = ErisilebilirHastalar().Include(h => h.Doktor).Include(h => h.Veli)
            .OrderBy(h => h.AdSoyad).ToList();
        return View(liste);
    }

    /// <summary>AJAX: hasta arama (ad / TC no). Sonuç JSON döner, tablo jQuery ile yeniden çizilir.</summary>
    [Authorize(Roles = "Admin,Doktor")]
    public IActionResult Ara(string? q)
    {
        q = (q ?? "").Trim().ToLower();
        var sonuc = ErisilebilirHastalar().Include(h => h.Doktor).Include(h => h.Veli)
            .Where(h => q == "" || h.AdSoyad.ToLower().Contains(q) || (h.TcKimlikNo != null && h.TcKimlikNo.Contains(q)))
            .OrderBy(h => h.AdSoyad).ToList()
            .Select(h => new
            {
                h.Id, h.AdSoyad, h.Cinsiyet, h.TcKimlikNo,
                dogum = YasHelper.Goster(h.DogumTarihi),
                yas = YasHelper.YasMetni(h.DogumTarihi),
                doktor = h.Doktor.AdSoyad,
                veli = h.Veli?.AdSoyad
            });
        return Json(sonuc);
    }

    // ---------- VELİ: ÇOCUKLARIM ----------
    [Authorize(Roles = "Veli")]
    public IActionResult Cocuklarim()
    {
        var liste = ErisilebilirHastalar().Include(h => h.Doktor).OrderBy(h => h.AdSoyad).ToList();
        return View(liste);
    }

    // ---------- DETAY (her rol, yetki kontrolüyle) ----------
    public IActionResult Detay(int id)
    {
        var h = ErisilebilirHastalar().Include(x => x.Doktor).Include(x => x.Veli).FirstOrDefault(x => x.Id == id);
        if (h == null) return Forbid();   // başkasının hastası veya yok
        ViewBag.Duzenleyebilir = Rol != "Veli";
        ViewBag.Asilar = Db.Asilars.OrderBy(a => a.OnerilenAy).ThenBy(a => a.Id).ToList();
        return View(h);
    }

    // ---------- EKLE ----------
    [Authorize(Roles = "Admin,Doktor")]
    public IActionResult Ekle()
    {
        DropdownDoldur();
        return View("Form", new HastaVM());
    }

    [Authorize(Roles = "Admin,Doktor"), HttpPost, ValidateAntiForgeryToken]
    public IActionResult Ekle(HastaVM vm)
    {
        Dogrula(vm);
        if (!ModelState.IsValid) { DropdownDoldur(); return View("Form", vm); }

        var h = new Hastalar { KayitTarihi = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
        Aktar(vm, h);
        Db.Hastalars.Add(h);
        Db.SaveChanges();
        TempData["Mesaj"] = $"{h.AdSoyad} kaydedildi.";
        return RedirectToAction(nameof(Detay), new { id = h.Id });
    }

    // ---------- DÜZENLE ----------
    [Authorize(Roles = "Admin,Doktor")]
    public IActionResult Duzenle(int id)
    {
        var h = ErisilebilirHastalar().FirstOrDefault(x => x.Id == id);
        if (h == null) return Forbid();
        DropdownDoldur();
        return View("Form", new HastaVM
        {
            Id = h.Id, AdSoyad = h.AdSoyad, TcKimlikNo = h.TcKimlikNo, DogumTarihi = YasHelper.Tarih(h.DogumTarihi),
            Cinsiyet = h.Cinsiyet, KanGrubu = h.KanGrubu, Alerjiler = h.Alerjiler, DoktorId = h.DoktorId, VeliId = h.VeliId
        });
    }

    [Authorize(Roles = "Admin,Doktor"), HttpPost, ValidateAntiForgeryToken]
    public IActionResult Duzenle(int id, HastaVM vm)
    {
        var h = ErisilebilirHastalar().FirstOrDefault(x => x.Id == id);
        if (h == null) return Forbid();
        vm.Id = id;
        Dogrula(vm);
        if (!ModelState.IsValid) { DropdownDoldur(); return View("Form", vm); }

        Aktar(vm, h);
        Db.SaveChanges();
        TempData["Mesaj"] = "Hasta bilgileri güncellendi.";
        return RedirectToAction(nameof(Detay), new { id });
    }

    // ---------- SİL (AJAX) ----------
    [Authorize(Roles = "Admin,Doktor"), HttpPost, ValidateAntiForgeryToken]
    public IActionResult Sil(int id)
    {
        var h = ErisilebilirHastalar().FirstOrDefault(x => x.Id == id);
        if (h == null) return Json(new { basarili = false, mesaj = "Yetkiniz yok." });
        Db.Hastalars.Remove(h);   // muayene ve aşı kayıtları DB'de CASCADE ile silinir
        Db.SaveChanges();
        return Json(new { basarili = true });
    }

    // ---------- yardımcılar ----------
    private void Dogrula(HastaVM vm)
    {
        if (vm.DogumTarihi > DateTime.Today)
            ModelState.AddModelError(nameof(vm.DogumTarihi), "Doğum tarihi gelecekte olamaz.");
        if (!string.IsNullOrEmpty(vm.TcKimlikNo) && Db.Hastalars.Any(x => x.TcKimlikNo == vm.TcKimlikNo && x.Id != vm.Id))
            ModelState.AddModelError(nameof(vm.TcKimlikNo), "Bu TC Kimlik No ile kayıtlı hasta var.");
        if (Rol == "Admin" && !Db.Kullanicilars.Any(k => k.Id == vm.DoktorId && k.Rol == "Doktor"))
            ModelState.AddModelError(nameof(vm.DoktorId), "Doktor seçin.");
        if (vm.VeliId != null && !Db.Kullanicilars.Any(k => k.Id == vm.VeliId && k.Rol == "Veli"))
            ModelState.AddModelError(nameof(vm.VeliId), "Geçersiz veli.");
    }

    private void Aktar(HastaVM vm, Hastalar h)
    {
        h.AdSoyad = vm.AdSoyad.Trim();
        h.TcKimlikNo = string.IsNullOrWhiteSpace(vm.TcKimlikNo) ? null : vm.TcKimlikNo;
        h.DogumTarihi = vm.DogumTarihi!.Value.ToString("yyyy-MM-dd");
        h.Cinsiyet = vm.Cinsiyet;
        h.KanGrubu = vm.KanGrubu;
        h.Alerjiler = vm.Alerjiler;
        h.VeliId = vm.VeliId;
        // Doktor kendi adına kayıt açar; Admin doktor seçebilir
        h.DoktorId = Rol == "Admin" ? vm.DoktorId!.Value : KullaniciId;
    }

    private void DropdownDoldur()
    {
        ViewBag.Doktorlar = new SelectList(Db.Kullanicilars.Where(k => k.Rol == "Doktor").OrderBy(k => k.AdSoyad), "Id", "AdSoyad");
        ViewBag.Veliler = new SelectList(Db.Kullanicilars.Where(k => k.Rol == "Veli").OrderBy(k => k.AdSoyad)
            .Select(k => new { k.Id, Ad = k.AdSoyad + " (" + k.Email + ")" }), "Id", "Ad");
    }
}
