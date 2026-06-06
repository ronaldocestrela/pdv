/**
 * @file suppliers.ts
 * @description Provides HTTP client service calls for Supplier CRUD endpoints.
 */

import { api } from './api';
import type { SupplierSummaryDto } from '../types/suppliers';

/**
 * Fetches a list of all suppliers for the active tenant session.
 * @returns {Promise<SupplierSummaryDto[]>} A list of supplier summaries.
 */
export async function listSuppliers(): Promise<SupplierSummaryDto[]> {
  const { data } = await api.get<SupplierSummaryDto[]>('/api/suppliers');
  return data;
}

/**
 * Fetches detail information of a specific supplier.
 * @param {string} id The unique identifier of the supplier.
 * @returns {Promise<SupplierSummaryDto | null>} The supplier summary if found, or null.
 */
export async function getSupplier(id: string): Promise<SupplierSummaryDto | null> {
  try {
    const { data } = await api.get<SupplierSummaryDto>(`/api/suppliers/${id}`);
    return data;
  } catch (e: unknown) {
    const ax = e as { response?: { status?: number } };
    if (ax.response?.status === 404) {
      return null;
    }
    throw e;
  }
}

/**
 * Creates a new supplier.
 * @param {Object} body The payload containing supplier data.
 * @param {string} body.name The name of the supplier.
 * @param {string | null} body.document The document of the supplier.
 * @param {string | null} body.email The email of the supplier.
 * @param {string | null} body.phone The phone of the supplier.
 * @param {boolean} body.isActive Whether the supplier is active.
 * @returns {Promise<string>} The created supplier's unique identifier.
 */
export async function createSupplier(body: {
  name: string;
  document: string | null;
  email: string | null;
  phone: string | null;
  isActive: boolean;
}): Promise<string> {
  const { data } = await api.post<{ id: string }>('/api/suppliers', body);
  return data.id;
}

/**
 * Updates an existing supplier.
 * @param {string} id The unique identifier of the supplier to update.
 * @param {Object} body The payload containing updated supplier data.
 * @param {string} body.name The name of the supplier.
 * @param {string | null} body.document The document of the supplier.
 * @param {string | null} body.email The email of the supplier.
 * @param {string | null} body.phone The phone of the supplier.
 * @param {boolean} body.isActive Whether the supplier is active.
 * @returns {Promise<void>}
 */
export async function updateSupplier(
  id: string,
  body: {
    name: string;
    document: string | null;
    email: string | null;
    phone: string | null;
    isActive: boolean;
  },
): Promise<void> {
  await api.put(`/api/suppliers/${id}`, body);
}

/**
 * Deletes a supplier.
 * @param {string} id The unique identifier of the supplier to delete.
 * @returns {Promise<void>}
 */
export async function deleteSupplier(id: string): Promise<void> {
  await api.delete(`/api/suppliers/${id}`);
}
