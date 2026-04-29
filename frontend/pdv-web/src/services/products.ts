import { api } from './api';
import type { ProductDetailDto, ProductSummaryDto } from '../types/products';

export async function listProducts(): Promise<ProductSummaryDto[]> {
  const { data } = await api.get<ProductSummaryDto[]>('/api/products');
  return data;
}

export async function getProduct(id: number): Promise<ProductDetailDto | null> {
  try {
    const { data } = await api.get<ProductDetailDto>(`/api/products/${id}`);
    return data;
  } catch (e: unknown) {
    const ax = e as { response?: { status?: number } };
    if (ax.response?.status === 404) {
      return null;
    }
    throw e;
  }
}

export async function createProduct(body: { name: string; isActive: boolean }): Promise<number> {
  const { data } = await api.post<{ id: number }>('/api/products', body);
  return data.id;
}

export async function updateProduct(id: number, body: { name: string; isActive: boolean }): Promise<void> {
  await api.put(`/api/products/${id}`, body);
}

export async function deleteProduct(id: number): Promise<void> {
  await api.delete(`/api/products/${id}`);
}

export async function createVariation(body: {
  productId: number;
  name: string;
  barcode: string | null;
  stockQuantity: number;
}): Promise<number> {
  const { data } = await api.post<{ id: number }>('/api/variations', body);
  return data.id;
}

export async function updateVariation(
  id: number,
  body: { name: string; barcode: string | null; stockQuantity: number },
): Promise<void> {
  await api.put(`/api/variations/${id}`, body);
}

export async function deleteVariation(id: number): Promise<void> {
  await api.delete(`/api/variations/${id}`);
}
