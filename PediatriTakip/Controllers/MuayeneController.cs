using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PediatriTakip.Data;
using PediatriTakip.Helpers;
using PediatriTakip.Models;

namespace PediatriTakip.Controllers;

/// <summary>Muayene ve aşı işlemleri — hasta detay sayfasından AJAX ile çağrılır, JSON döner.</summary>
[Authorize]
public class MuayeneController : TemelController
{
    public MuayeneController(PediatriContext db) : base(db) { }

    // ---------- MUAYENE ----------
    [HttpGet]
    public IActionResult Liste(int hastaId)
    {
        if (!HastayaErisebilir(hastaId)) return Forbid();
        var liste = Db.Muayenelers.Where(m => m.HastaId == hastaId).OrderBy(m => m.Tarih).ThenBy(m => m.Id).ToList()
            .Select(m => new
            {
                m.Id, m.Tarih, tarihGoster = YasHelper.Goster(m.Tarih),
                m.BoyCm, m.KiloKg, m.BasCevresiCm, m.Tani, m.DoktorNotu,
                vki = (m.BoyCm > 0 && m.KiloKg > 0) ? Math.Round(m.KiloKg.Value / Math.Pow(m.BoyCm.Value / 100, 2), 1) : (double?)null
            });
        return Json(liste);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Ekle(MuayeneVM vm)
    {
        if (!DuzenleyebilirMi(vm.HastaId)) return Json(new { basarili = false, mesaj = "Bu işlem için yetkiniz yok." });
        if (vm.Tarih > DateTime.Today) ModelState.AddModelError("Tarih", "Muayene tarihi gelecekte olamaz.");
        if (vm.BoyCm == null && vm.KiloKg == null && vm.BasCevresiCm == null && string.IsNullOrWhiteSpace(vm.Tani))
            ModelState.AddModelError("", "En az bir ölçüm veya tanı girin.");
        if (!ModelState.IsValid)
            return Json(new { basarili = false, mesaj = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        Db.Muayenelers.Add(new Muayeneler
        {
            HastaId = vm.HastaId,
            DoktorId = Db.Hastalars.Where(h => h.Id == vm.HastaId).Select(h => h.DoktorId).First(),
            Tarih = vm.Tarih!.Value.ToString("yyyy-MM-dd"),
            BoyCm = vm.BoyCm, KiloKg = vm.KiloKg, BasCevresiCm = vm.BasCevresiCm,
            Tani = vm.Tani, DoktorNotu = vm.DoktorNotu
        });
        Db.SaveChanges();
        return Json(new { basarili = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Sil(int id)
    {
        var m = Db.Muayenelers.Find(id);
        if (m == null || !DuzenleyebilirMi(m.HastaId)) return Json(new { basarili = false, mesaj = "Yetkiniz yok." });
        Db.Muayenelers.Remove(m);
        Db.SaveChanges();
        return Json(new { basarili = true });
    }

    // ---------- AŞI ----------
    /// <summary>Aşı takvimi: her aşı için yapıldı / zamanı geldi / bekliyor durumu.</summary>
    [HttpGet]
    public IActionResult AsiTakvimi(int hastaId)
    {
        var hasta = ErisilebilirHastalar().FirstOrDefault(h => h.Id == hastaId);
        if (hasta == null) return Forbid();
        int ay = YasHelper.AySayisi(hasta.DogumTarihi);
        var kayitlar = Db.AsiKayitlaris.Where(k => k.HastaId == hastaId).ToList();

        var takvim = Db.Asilars.OrderBy(a => a.OnerilenAy).ThenBy(a => a.Id).ToList().Select(a =>
        {
            var k = kayitlar.FirstOrDefault(x => x.AsiId == a.Id);
            string durum = k != null ? "yapildi" : (a.OnerilenAy <= ay ? "gecikti" : "bekliyor");
            return new
            {
                asiId = a.Id, a.Ad, a.OnerilenAy, durum,
                kayitId = k?.Id, tarih = k != null ? YasHelper.Goster(k.UygulamaTarihi) : null, notlar = k?.Notlar
            };
        });
        return Json(takvim);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult AsiEkle(AsiKaydiVM vm)
    {
        if (!DuzenleyebilirMi(vm.HastaId)) return Json(new { basarili = false, mesaj = "Bu işlem için yetkiniz yok." });
        if (!ModelState.IsValid || vm.UygulamaTarihi > DateTime.Today)
            return Json(new { basarili = false, mesaj = "Geçerli bir uygulama tarihi girin." });
        if (Db.AsiKayitlaris.Any(k => k.HastaId == vm.HastaId && k.AsiId == vm.AsiId))
            return Json(new { basarili = false, mesaj = "Bu aşı zaten kayıtlı." });

        Db.AsiKayitlaris.Add(new AsiKayitlari
        {
            HastaId = vm.HastaId, AsiId = vm.AsiId,
            UygulamaTarihi = vm.UygulamaTarihi!.Value.ToString("yyyy-MM-dd"), Notlar = vm.Notlar
        });
        Db.SaveChanges();
        return Json(new { basarili = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult AsiSil(int id)
    {
        var k = Db.AsiKayitlaris.Find(id);
        if (k == null || !DuzenleyebilirMi(k.HastaId)) return Json(new { basarili = false, mesaj = "Yetkiniz yok." });
        Db.AsiKayitlaris.Remove(k);
        Db.SaveChanges();
        return Json(new { basarili = true });
    }
}
