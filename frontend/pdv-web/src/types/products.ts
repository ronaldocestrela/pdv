export interface ProductSummaryDto {
  id: string;
  name: string;
  isActive: boolean;
  variationCount: number;
}

export interface ProductVariationDto {
  id: string;
  productId: string;
  name: string;
  barcode: string | null;
  stockQuantity: number;
  unitPrice: number;
}

export interface ProductDetailDto {
  id: string;
  name: string;
  isActive: boolean;
  variations: ProductVariationDto[];
}
