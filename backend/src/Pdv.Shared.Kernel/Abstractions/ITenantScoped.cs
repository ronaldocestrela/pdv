namespace Pdv.Shared.Kernel.Abstractions;

public interface ITenantScoped
{
    Guid TenantId { get; set; }
}
