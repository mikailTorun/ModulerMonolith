using ModulerMonolith.Core.Results;
using ModulerMonolith.Infrastructure.Persistence;

namespace ModulerMonolith.Infrastructure.Mediator;

/// <summary>
/// Command'ları bir EF Core transaction içinde çalıştırır.
///
/// Kurallar:
/// - Transaction zaten açıksa yenisini açmaz, mevcut transaction'a katılır.
/// - Handler başarısız Result döndürürse (IsSuccess = false) SaveChanges çağrılmaz,
///   transaction rollback yapılır — yarım kaydedilmiş veri bırakmaz.
/// - Exception fırlatılırsa yine rollback.
/// </summary>
internal sealed class TransactionBehavior
{
    private readonly AppDbContext _db;

    public TransactionBehavior(AppDbContext db) => _db = db;

    public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> work, CancellationToken ct)
        where TResult : IResultBase
    {
        if (_db.Database.CurrentTransaction is not null)
            return await work();

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var result = await work();

            if (result.IsSuccess)
            {
                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            else
            {
                await tx.RollbackAsync(ct);
            }

            return result;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
