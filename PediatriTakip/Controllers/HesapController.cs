using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PediatriTakip.Data;
using PediatriTakip.Helpers;
using PediatriTakip.Models;

namespace PediatriTakip.Controllers;

public class HesapController : Controller
{
    private readonly PediatriContext _db;
    public HesapController(PediatriContext db) => _db = db;

    [HttpGet]
    public IActionResult Giris(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new GirisVM { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Giris(GirisVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var k = _db.Kullanicilars.FirstOrDefault(x => x.Email == vm.Email.Trim().ToLower());
        if (k == null || !SifreHelper.Dogrula(vm.Sifre, k.SifreHash))
        {
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(vm);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, k.Id.ToString()),
            new(ClaimTypes.Name, k.AdSoyad),
            new(ClaimTypes.Email, k.Email),
            new(ClaimTypes.Role, k.Rol)
        };
        var kimlik = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(kimlik));

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl)) return Redirect(vm.ReturnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Kayit() => View(new KayitVM());

    /// <summary>Kayıt sayfası veli hesabı oluşturur. Doktor hesaplarını Admin açar.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Kayit(KayitVM vm)
    {
        var email = vm.Email?.Trim().ToLower() ?? "";
        if (_db.Kullanicilars.Any(x => x.Email == email))
            ModelState.AddModelError(nameof(vm.Email), "Bu e-posta zaten kayıtlı.");
        if (!ModelState.IsValid) return View(vm);

        _db.Kullanicilars.Add(new Kullanicilar
        {
            AdSoyad = vm.AdSoyad.Trim(),
            Email = email,
            Telefon = vm.Telefon,
            SifreHash = SifreHelper.Hashle(vm.Sifre),
            Rol = "Veli"
        });
        _db.SaveChanges();
        TempData["Mesaj"] = "Kayıt başarılı! Giriş yapabilirsiniz. Doktorunuz çocuğunuzu hesabınıza bağladığında bilgilerini görebileceksiniz.";
        return RedirectToAction(nameof(Giris));
    }

    /// <summary>AJAX: kayıt formunda e-postanın kullanılıp kullanılmadığını kontrol eder.</summary>
    [HttpGet]
    public IActionResult EmailKontrol(string email)
    {
        bool kullaniliyor = _db.Kullanicilars.Any(x => x.Email == (email ?? "").Trim().ToLower());
        return Json(new { uygun = !kullaniliyor });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cikis()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Giris));
    }

    public IActionResult ErisimEngellendi() => View();
}
