/** Matches API JSON enum (camelCase). */
export type PaymentMethodDto = 'cash' | 'card' | 'pix';

export type SaleLinePayload = {
  productVariationId: string;
  quantity: number;
};

export type CreateSalePayload = {
  items: SaleLinePayload[];
  paymentMethod: PaymentMethodDto;
};

export type CreateSaleResultDto = {
  saleId: string;
  totalAmount: number;
};

export type SaleListItemDto = {
  id: string;
  createdAtUtc: string;
  totalAmount: number;
  paymentMethod: PaymentMethodDto;
  itemCount: number;
};
