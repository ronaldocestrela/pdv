/**
 * @file suppliers.ts
 * @description Defines data transfer objects (DTO) and TypeScript interfaces for managing suppliers (fornecedores).
 */

/**
 * Summary data of a supplier.
 */
export interface SupplierSummaryDto {
  /** The unique identifier of the supplier. */
  id: string;
  
  /** The corporate name or trade name of the supplier. */
  name: string;
  
  /** Optional document identifier (e.g. CPF, CNPJ). */
  document: string | null;
  
  /** Optional email contact of the supplier. */
  email: string | null;
  
  /** Optional phone contact of the supplier. */
  phone: string | null;
  
  /** Indicates whether the supplier is active. */
  isActive: boolean;
}
