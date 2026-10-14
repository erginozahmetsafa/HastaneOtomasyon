namespace HastaneOtomasyon.Core.Exceptions;

// İş kuralı ihlali (çakışma, geçersiz durum geçişi vb.). Mesaj kullanıcıya gösterilebilir.
public class IsKuraliException(string mesaj) : Exception(mesaj);
