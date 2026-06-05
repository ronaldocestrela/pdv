namespace Pdv.Shared.Kernel.Abstractions;

public interface ITenantScoped
{
    int TenantId { get; set; }
}
