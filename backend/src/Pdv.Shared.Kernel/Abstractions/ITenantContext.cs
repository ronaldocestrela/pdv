namespace Pdv.Shared.Kernel.Abstractions;

public interface ITenantContext
{
    int? TenantId { get; }
    bool IsSuperAdmin { get; }
}
