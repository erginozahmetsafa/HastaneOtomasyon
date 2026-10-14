using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Exceptions;
using HastaneOtomasyon.Core.Repositories;

namespace HastaneOtomasyon.Service.Extensions;

internal static class RepositoryExtensions
{
	public static async Task<T> GetByIdOrThrowAsync<T>(this IGenericRepository<T> repository, int id, CancellationToken ct) where T : BaseEntity =>
		await repository.GetByIdAsync(id, ct) ?? throw BulunamadiException.Kayit<T>(id);

	public static async Task VarOlmaliAsync<T>(this IGenericRepository<T> repository, int id, CancellationToken ct) where T : BaseEntity
	{
		if (!await repository.AnyAsync(e => e.Id == id, ct))
			throw BulunamadiException.Kayit<T>(id);
	}
}
