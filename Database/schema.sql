-- =====================================================
-- Pediatri Hasta Gelişim Takip Sistemi - Veritabanı Şeması
-- Veritabanı: SQLite  (Database-First yaklaşımı)
-- =====================================================
PRAGMA foreign_keys = ON;

-- Kullanıcılar: Admin, Doktor, Veli
CREATE TABLE Kullanicilar (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    AdSoyad         TEXT    NOT NULL,
    Email           TEXT    NOT NULL UNIQUE,
    SifreHash       TEXT    NOT NULL,
    Rol             TEXT    NOT NULL CHECK (Rol IN ('Admin','Doktor','Veli')),
    Telefon         TEXT,
    Uzmanlik        TEXT,                       -- sadece doktorlar için
    OlusturmaTarihi TEXT    NOT NULL DEFAULT (datetime('now','localtime'))
);

-- Hastalar (çocuklar)
CREATE TABLE Hastalar (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    AdSoyad     TEXT    NOT NULL,
    TcKimlikNo  TEXT    UNIQUE,
    DogumTarihi TEXT    NOT NULL,
    Cinsiyet    TEXT    NOT NULL CHECK (Cinsiyet IN ('Kız','Erkek')),
    KanGrubu    TEXT,
    Alerjiler   TEXT,
    DoktorId    INTEGER NOT NULL REFERENCES Kullanicilar(Id),
    VeliId      INTEGER REFERENCES Kullanicilar(Id) ON DELETE SET NULL,
    KayitTarihi TEXT    NOT NULL DEFAULT (datetime('now','localtime'))
);

-- Muayene / gelişim ölçümleri
CREATE TABLE Muayeneler (
    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
    HastaId      INTEGER NOT NULL REFERENCES Hastalar(Id) ON DELETE CASCADE,
    DoktorId     INTEGER NOT NULL REFERENCES Kullanicilar(Id),
    Tarih        TEXT    NOT NULL,
    BoyCm        REAL,
    KiloKg       REAL,
    BasCevresiCm REAL,
    Tani         TEXT,
    DoktorNotu   TEXT
);

-- Aşı tanımları (aşı takvimi)
CREATE TABLE Asilar (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    Ad         TEXT    NOT NULL,
    OnerilenAy INTEGER NOT NULL,   -- doğumdan itibaren kaçıncı ay
    Aciklama   TEXT
);

-- Hastaya uygulanan aşılar
CREATE TABLE AsiKayitlari (
    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
    HastaId        INTEGER NOT NULL REFERENCES Hastalar(Id) ON DELETE CASCADE,
    AsiId          INTEGER NOT NULL REFERENCES Asilar(Id),
    UygulamaTarihi TEXT    NOT NULL,
    Notlar         TEXT
);

CREATE INDEX IX_Hastalar_DoktorId     ON Hastalar(DoktorId);
CREATE INDEX IX_Hastalar_VeliId       ON Hastalar(VeliId);
CREATE INDEX IX_Muayeneler_HastaId    ON Muayeneler(HastaId);
CREATE INDEX IX_AsiKayitlari_HastaId  ON AsiKayitlari(HastaId);

-- ----------------- Örnek veriler -----------------
-- Aşı takvimi (T.C. Sağlık Bakanlığı takviminden sadeleştirilmiş)
INSERT INTO Asilar (Ad, OnerilenAy, Aciklama) VALUES
 ('Hepatit B - 1. doz', 0,  'Doğumda'),
 ('Hepatit B - 2. doz', 1,  '1. ay sonu'),
 ('BCG (Verem)',        2,  '2. ay sonu'),
 ('DaBT-İPA-Hib-HepB - 1. doz', 2, 'Beşli/altılı karma'),
 ('KPA (Pnömokok) - 1. doz',    2, ''),
 ('DaBT-İPA-Hib-HepB - 2. doz', 4, ''),
 ('KPA (Pnömokok) - 2. doz',    4, ''),
 ('DaBT-İPA-Hib-HepB - 3. doz', 6, ''),
 ('Hepatit B - 3. doz', 6,  ''),
 ('KKK (Kızamık-Kızamıkçık-Kabakulak)', 12, ''),
 ('Su Çiçeği', 12, ''),
 ('Hepatit A - 1. doz', 18, ''),
 ('Hepatit A - 2. doz', 24, ''),
 ('DaBT-İPA (Rapel)', 48, 'İlkokul öncesi');
