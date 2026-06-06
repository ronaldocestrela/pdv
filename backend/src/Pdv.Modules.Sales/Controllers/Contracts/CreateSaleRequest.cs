using Pdv.Shared.Kernel.Enums;

namespace Pdv.Modules.Sales.Controllers.Contracts;

public sealed class CreateSaleRequestItem
{
    public Guid ProductVariationId { get; set; }
    public int Quantity { get; set; }
}

public sealed class CreateSaleRequest
{
    public List<CreateSaleRequestItem> Items { get; set; } = [];

    public PaymentMethod PaymentMethod { get; set; }
}
