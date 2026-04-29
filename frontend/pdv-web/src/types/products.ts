export interface ProductSummaryDto {
  id: number;
  name: string;
  isActive: boolean;
  variationCount: number;
}

export interface ProductVariationDto {
  id: number;
  productId: number;
  name: string;
  barcode: string | null;
  stockQuantity: number;
  unitPrice: number;
}

export interface ProductDetailDto {
  id: number;
  name: string;
  isActive: boolean;
  variations: ProductVariationDto[];
}
