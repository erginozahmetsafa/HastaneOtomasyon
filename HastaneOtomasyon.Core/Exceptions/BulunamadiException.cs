namespace HastaneOtomasyon.Core.Exceptions;

public class BulunamadiException(string mesaj) : Exception(mesaj)
{
	public static BulunamadiException Kayit<T>(int id) => new($"{typeof(T).Name} bulunamadı (Id: {id}).");
}
