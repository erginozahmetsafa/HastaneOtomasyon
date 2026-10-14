namespace HastaneOtomasyon.Core.Services;

public interface ISifreHasher
{
	string Hashle(string sifre);
	bool Dogrula(string sifre, string sifreHash);
}
