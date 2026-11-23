using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PediatriTakip.Data;
using PediatriTakip.Models;

namespace PediatriTakip.Controllers;

[Authorize]
public class HomeController : TemelController
{
    public HomeController(PediatriContext db) : base(db) { }

    public IActionResult Index()
    {
        if (Rol == "Veli") return RedirectToAction("Cocuklarim", "Hasta");

        var hastaIdleri = ErisilebilirHastalar().Select(h => h.Id);
        string ayBasi = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToString("yyyy-MM-dd");

        var vm = new PanelVM
        {
            HastaSayisi = hastaIdleri.Count(),
            DoktorSayisi = Db.Kullanicilars.Count(k => k.Rol == "Doktor"),
            VeliSayisi = Db.Kullanicilars.Count(k => k.Rol == "Veli"),
            BuAyMuayene = Db.Muayenelers.Count(m => hastaIdleri.Contains(m.HastaId) && string.Compare(m.Tarih, ayBasi) >= 0),
            SonMuayeneler = Db.Muayenelers.Include(m => m.Hasta)
                .Where(m => hastaIdleri.Contains(m.HastaId))
                .OrderByDescending(m => m.Tarih).ThenByDescending(m => m.Id).Take(5).ToList()
        };
        return View(vm);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
