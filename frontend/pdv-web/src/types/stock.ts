/** DTO from GET /api/stock/movements (camelCase JSON). */
export interface StockMovementDto {
  id: string;
  productVariationId: string;
  productName: string;
  variationName: string;
  type: string;
  quantity: number;
  createdAtUtc: string;
  reason: string | null;
}
