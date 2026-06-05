namespace Pdv.Application.Abstractions;

public interface ITenantContext
{
    int? TenantId { get; }
    bool IsSuperAdmin { get; }
}
