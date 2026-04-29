using Pdv.Domain.Enums;

namespace Pdv.API.Contracts;

public sealed class CreateSaleRequestItem
{
    public int ProductVariationId { get; set; }
    public int Quantity { get; set; }
}

public sealed class CreateSaleRequest
{
    public List<CreateSaleRequestItem> Items { get; set; } = [];

    public PaymentMethod PaymentMethod { get; set; }
}
