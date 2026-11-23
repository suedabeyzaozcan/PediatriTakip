// Tüm jQuery AJAX isteklerine anti-forgery token ekle (CSRF koruması)
$.ajaxSetup({
    headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').first().val() }
});

// Sağ altta kısa bildirim gösterir (DOM manipülasyonu)
function bildir(mesaj, basarili = true) {
    const $t = $("#bildirim");
    $t.removeClass("bg-success bg-danger").addClass(basarili ? "bg-success" : "bg-danger");
    $t.find(".toast-body").text(mesaj);
    bootstrap.Toast.getOrCreateInstance($t[0], { delay: 2500 }).show();
}

// HTML kaçış (XSS'e karşı)
function esc(s) {
    return $("<div>").text(s == null ? "" : s).html();
}
