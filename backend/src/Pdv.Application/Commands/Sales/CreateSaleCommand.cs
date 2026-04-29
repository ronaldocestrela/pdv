using MediatR;
using Pdv.Domain.Enums;

namespace Pdv.Application.Commands.Sales;

public sealed record CreateSaleLineDto(int ProductVariationId, int Quantity);

public sealed record CreateSaleResultDto(int SaleId, decimal TotalAmount);

public sealed record CreateSaleCommand(
    IReadOnlyList<CreateSaleLineDto> Items,
    PaymentMethod PaymentMethod) : IRequest<CreateSaleResultDto>;
