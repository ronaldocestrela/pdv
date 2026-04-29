import { api } from './api';
import type { CreateSalePayload, CreateSaleResultDto, SaleListItemDto } from '../types/sales';

export async function createSale(body: CreateSalePayload): Promise<CreateSaleResultDto> {
  const { data } = await api.post<CreateSaleResultDto>('/api/sales', body);
  return data;
}

export async function listSales(take = 50): Promise<SaleListItemDto[]> {
  const { data } = await api.get<SaleListItemDto[]>('/api/sales', { params: { take } });
  return data;
}
