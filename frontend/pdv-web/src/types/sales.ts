/** Matches API JSON enum (camelCase). */
export type PaymentMethodDto = 'cash' | 'card' | 'pix';

export type SaleLinePayload = {
  productVariationId: number;
  quantity: number;
};

export type CreateSalePayload = {
  items: SaleLinePayload[];
  paymentMethod: PaymentMethodDto;
};

export type CreateSaleResultDto = {
  saleId: number;
  totalAmount: number;
};

export type SaleListItemDto = {
  id: number;
  createdAtUtc: string;
  totalAmount: number;
  paymentMethod: PaymentMethodDto;
  itemCount: number;
};
