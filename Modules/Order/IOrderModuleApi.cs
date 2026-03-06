namespace Module.Order;

/// <summary>
/// Order modülünün diğer modüllere açtığı public contract.
/// Bu interface'in implementasyonu internal'dır — sadece bu arayüz referans alınır.
/// </summary>
public interface IOrderModuleApi
{
    /// <summary>
    /// Belirtilen ürüne ait iptal edilmemiş sipariş var mı?
    /// </summary>
    Task<bool> HasActiveOrdersForProductAsync(Guid productId, CancellationToken ct = default);
}
