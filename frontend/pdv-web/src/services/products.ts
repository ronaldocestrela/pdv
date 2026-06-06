import { api } from './api';
import type { ProductDetailDto, ProductSummaryDto } from '../types/products';

export async function listProducts(): Promise<ProductSummaryDto[]> {
  const { data } = await api.get<ProductSummaryDto[]>('/api/products');
  return data;
}

export async function getProduct(id: string): Promise<ProductDetailDto | null> {
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

export async function createProduct(body: { name: string; isActive: boolean }): Promise<string> {
  const { data } = await api.post<{ id: string }>('/api/products', body);
  return data.id;
}

export async function updateProduct(id: string, body: { name: string; isActive: boolean }): Promise<void> {
  await api.put(`/api/products/${id}`, body);
}

export async function deleteProduct(id: string): Promise<void> {
  await api.delete(`/api/products/${id}`);
}

export async function createVariation(body: {
  productId: string;
  name: string;
  barcode: string | null;
  stockQuantity: number;
  unitPrice: number;
}): Promise<string> {
  const { data } = await api.post<{ id: string }>('/api/variations', body);
  return data.id;
}

export async function updateVariation(
  id: string,
  body: { name: string; barcode: string | null; stockQuantity: number; unitPrice: number },
): Promise<void> {
  await api.put(`/api/variations/${id}`, body);
}

export async function deleteVariation(id: string): Promise<void> {
  await api.delete(`/api/variations/${id}`);
}
