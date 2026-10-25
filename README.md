# Hastane Otomasyon — Backend

Çok şubeli hastane/klinik randevu sistemi için ASP.NET Core Web API. Arayüz ayrı repodadır:
[HastaneOtomasyon-frontend](https://github.com/erginozahmetsafa/HastaneOtomasyon-frontend) (Next.js).

## Teknolojiler

- .NET 8, ASP.NET Core Web API, JWT kimlik doğrulama, Swagger
- Entity Framework Core + SQL Server (geliştirmede LocalDB)
- xUnit + Moq birim testleri, GitHub Actions ile her push'ta derleme ve test

## Katmanlar

```
HastaneOtomasyon.Core       Varlıklar (entity), DTO'lar, enum'lar, repository ve servis arayüzleri, iş kuralı istisnaları
HastaneOtomasyon.Data       AppDbContext, EF Core yapılandırmaları, migration'lar, repository'ler, UnitOfWork
HastaneOtomasyon.Service    İş kuralları ve yetkilendirme (servisler), şifre hash'i, demo veri tohumlayıcı
HastaneOtomasyon.API        Controller'lar, JWT, merkezi hata yönetimi (ProblemDetails), CORS, hız sınırı
HastaneOtomasyon.UnitTests  Servis, varlık ve güvenlik testleri
```

Nesneye dayalı tasarım:

- **Kalıtım:** `Kisi` → `Hasta`, `Calisan` → `Doktor`, `SaglikPersoneli`, `Yonetici`; `Kisi` → `SistemYoneticisi`;
  `ZamanDilimi` → `CalismaSaati`, `Musaitlik`; tüm varlıklar `BaseEntity`'den türer.
- **Çok biçimlilik:** her kişi türü `Rol` özelliğini kendisi belirtir; servisler türü bilmeden rol üzerinden yetki verir.
- **Kapsülleme:** randevu durum geçişleri (`Onayla`, `Reddet`, `IptalEt`, `Tamamla`) ve hesap işlemleri
  (`Pasiflestir`, `SifreyiDegistir`) varlığın kendi metotlarındadır; kurala aykırı geçiş `IsKuraliException` fırlatır.
- **Soyutlama:** servisler veri erişimine `IGenericRepository<T>` ve `IUnitOfWork` arayüzleri üzerinden ulaşır.

## Kurulum

Gereksinimler: .NET 8 SDK ve SQL Server LocalDB (Visual Studio ile gelir).

```bash
dotnet run --project HastaneOtomasyon.API --launch-profile http
```

API `http://localhost:5004` adresinde açılır, Swagger arayüzü `http://localhost:5004/swagger`.
Geliştirme ortamında uygulama açılırken bekleyen migration'lar uygulanır ve demo veriler eklenir.

```bash
dotnet test     # birim testleri
```

## Demo hesaplar

Geliştirme ortamında `DemoVeriTohumlayici` örnek bir hastane (Sakarya Merkez Hastanesi), çalışma saatleri, Kardiyoloji ve
Dahiliye poliklinikleri, süre ve ücretiyle hizmetler, müsaitliği tanımlı bir doktor ve iki örnek randevu oluşturur.
Kayıtlar zaten varsa yeniden kullanılır; uygulama her açılışta güvenle çalıştırabilir.

| Rol | E-posta | Şifre |
| --- | --- | --- |
| Hasta | `hasta@mail.com` | `Admin123` |
| Doktor | `doktor@mail.com` | `Admin123` |
| Hastane yöneticisi | `yonetici@mail.com` | `Admin123` |
| Sistem yöneticisi | `admin@hastane.local` | `Admin123!` |

Sistem yöneticisi hesabı migration ile her ortamda oluşturulur; canlı ortamda şifresi değiştirilmelidir.

## Roller ve yetkiler

| Rol | Yapabilecekleri |
| --- | --- |
| Hasta | Kayıt olur; hastane → poliklinik → hizmet → doktor → saat seçerek randevu alır, randevusunu iptal eder |
| Doktor | Takvimini görür; randevuları onaylar, reddeder, iptal eder, tamamlar; müsaitlik dilimlerini yönetir |
| Sağlık personeli | Kurum bilgilerini görür |
| Hastane yöneticisi | Hastane bilgisi ve çalışma saatleri, poliklinik ve hizmetler, doktor ve personel kaydı, doktora hizmet atama, randevu onayı |
| Sistem yöneticisi | Hastane/şube tanımlar ve siler, hastanelere yönetici atar; tüm hastaneler üzerinde yönetici yetkisine sahiptir |

## Randevu kuralları

- Hastalar yalnızca doktorun müsaitlik dilimleri içindeki boş saatlere randevu alabilir; dilimler hizmet süresine göre bölünür.
- Doktorun ve hastanın çakışan randevusu olamaz; eşzamanlı istekler kilitle sırayla işlenir.
- Randevu "Bekliyor" durumunda başlar; doktor veya hastane yöneticisi onaylar ya da gerekçeyle reddeder.
- Randevu, oluşturulduğu andaki süre ve ücreti saklar (doktora özel ücret varsa o geçerlidir).
- Müsaitlik, hastanenin o günkü çalışma saatleri içinde olmalıdır.
