namespace ModulerMonolith.Core;

public interface ICurrentUser
{
    Guid Id { get; }
    string Email { get; }
    string Name { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsInRole(string role);
}
