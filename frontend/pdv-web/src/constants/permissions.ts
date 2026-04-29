/** Mirrors backend `KnownPermissions` claim values. */
export const PERMISSIONS = {
  productCreate: 'product.create',
  productUpdate: 'product.update',
  productDelete: 'product.delete',
  productView: 'product.view',
  variationCreate: 'variation.create',
  variationUpdate: 'variation.update',
  variationDelete: 'variation.delete',
  variationView: 'variation.view',
  stockAdjust: 'stock.adjust',
  stockView: 'stock.view',
  saleCreate: 'sale.create',
  saleView: 'sale.view',
  reportView: 'report.view',
  cashflowView: 'cashflow.view',
} as const;
