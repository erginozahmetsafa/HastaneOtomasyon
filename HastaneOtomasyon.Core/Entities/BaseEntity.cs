namespace HastaneOtomasyon.Core.Entities;

// Kimliği olan tüm varlıkların ortak atası. Tarih alanları DbContext tarafından otomatik doldurulur.
public abstract class BaseEntity
{
	public int Id { get; set; }
	public DateTime OlusturmaTarihi { get; set; }
	public DateTime? GuncellemeTarihi { get; set; }
}
