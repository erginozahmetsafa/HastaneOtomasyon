namespace HastaneOtomasyon.Core.Exceptions;

public class YetkisizIslemException(string mesaj = "Bu işlem için yetkiniz yok.") : Exception(mesaj);
