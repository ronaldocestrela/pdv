import { api } from './api';
import type { StockMovementDto } from '../types/stock';

export async function adjustStock(body: {
  productVariationId: number;
  quantity: number;
  reason: string | null;
}): Promise<void> {
  await api.post('/api/stock/adjust', {
    productVariationId: body.productVariationId,
    quantity: body.quantity,
    reason: body.reason,
  });
}

export async function listStockMovements(params?: {
  variationId?: number;
  take?: number;
}): Promise<StockMovementDto[]> {
  const { data } = await api.get<StockMovementDto[]>('/api/stock/movements', {
    params: {
      variationId: params?.variationId,
      take: params?.take ?? 100,
    },
  });
  return data;
}
