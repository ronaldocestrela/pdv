/** API returns enums as JSON numbers (camelCase property names). */

export type SalesReportDto = {
  saleCount: number;
  totalAmount: number;
};

export type TopProductReportDto = {
  productVariationId: number;
  productName: string;
  variationName: string;
  quantitySold: number;
  revenue: number;
};

/** Matches Domain CashFlowType: In = 0, Out = 1 */
export type CashFlowTypeDto = 0 | 1;

export type CashFlowReportRowDto = {
  id: number;
  type: CashFlowTypeDto;
  amount: number;
  description: string;
  createdAtUtc: string;
  saleId: number | null;
};

export type StockReportRowDto = {
  productVariationId: number;
  productName: string;
  variationName: string;
  stockQuantity: number;
};
