namespace HastaneOtomasyon.Core.Exceptions;

// Aynı kayıt üzerinde eşzamanlı işlem nedeniyle kilit alınamadığında fırlatılır. İstemci tekrar deneyebilir.
public class EszamanliIslemException(string mesaj = "Sistem şu anda yoğun, lütfen birkaç saniye sonra tekrar deneyin.") : Exception(mesaj);
