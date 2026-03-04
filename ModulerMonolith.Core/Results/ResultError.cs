namespace ModulerMonolith.Core.Results;

/// <summary>
/// API yanıtlarındaki hata öğesi. Validasyon ve domain hataları bu formatta döner.
/// </summary>
public sealed record ResultError(string Property, string Message);
