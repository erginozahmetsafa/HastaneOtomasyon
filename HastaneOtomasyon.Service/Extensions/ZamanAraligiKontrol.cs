using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Exceptions;

namespace HastaneOtomasyon.Service.Extensions;

internal static class ZamanAraligiKontrol
{
	private static readonly TimeSpan GunSonu = TimeSpan.FromDays(1);

	public static void GecerliOlmali(this ZamanAraligiDto aralik)
	{
		if (!Enum.IsDefined(aralik.Gun))
			throw new IsKuraliException("Geçersiz gün.");

		if (aralik.Baslangic < TimeSpan.Zero || aralik.Bitis > GunSonu || aralik.Baslangic >= aralik.Bitis)
			throw new IsKuraliException("Başlangıç saati bitiş saatinden önce olmalı ve aralık gün içinde kalmalıdır.");
	}
}
