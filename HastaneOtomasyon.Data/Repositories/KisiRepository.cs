using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.Data.Repositories;

public class KisiRepository(AppDbContext context) : GenericRepository<Kisi>(context), IKisiRepository
{
	public Task<Kisi?> GetByEmailAsync(string email, CancellationToken ct = default) =>
		Set.AsNoTracking().FirstOrDefaultAsync(k => k.Email == email, ct);
}
