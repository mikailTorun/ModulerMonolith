namespace ModulerMonolith.Core;

/// <summary>
/// Tüm modüller bu sabitlerle policy isimlerine referans verir.
/// Policy tanımları AuthModule içinde yapılır.
/// </summary>
public static class AuthPolicies
{
    public const string Authenticated = "Authenticated";
    public const string ProductRead   = "ProductRead";
    public const string ProductWrite  = "ProductWrite";
    public const string OrderRead     = "OrderRead";
    public const string OrderWrite    = "OrderWrite";
    public const string Admin         = "Admin";
}
