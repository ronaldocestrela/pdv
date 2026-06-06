namespace Pdv.Shared.Kernel.Abstractions;

public interface ITenantContext
{
    Guid? TenantId { get; }
    bool IsSuperAdmin { get; }
}
