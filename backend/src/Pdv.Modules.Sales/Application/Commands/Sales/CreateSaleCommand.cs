using MediatR;
using Pdv.Shared.Kernel.Enums;

namespace Pdv.Modules.Sales.Application.Commands.Sales;

public sealed record CreateSaleLineDto(Guid ProductVariationId, int Quantity);

public sealed record CreateSaleResultDto(Guid SaleId, decimal TotalAmount);

public sealed record CreateSaleCommand(
    IReadOnlyList<CreateSaleLineDto> Items,
    PaymentMethod PaymentMethod) : IRequest<CreateSaleResultDto>;
