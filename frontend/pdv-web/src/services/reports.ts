import { api } from './api';
import type {
  CashFlowReportRowDto,
  SalesReportDto,
  StockReportRowDto,
  TopProductReportDto,
} from '../types/reports';

export async function getSalesReport(fromUtc: string, toUtc: string): Promise<SalesReportDto> {
  const { data } = await api.get<SalesReportDto>('/api/reports/sales', {
    params: { fromUtc, toUtc },
  });
  return data;
}

export async function getTopProductsReport(
  fromUtc: string,
  toUtc: string,
  take = 20,
): Promise<TopProductReportDto[]> {
  const { data } = await api.get<TopProductReportDto[]>('/api/reports/top-products', {
    params: { fromUtc, toUtc, take },
  });
  return data;
}

export async function getCashFlowReport(
  fromUtc: string,
  toUtc: string,
  take = 100,
): Promise<CashFlowReportRowDto[]> {
  const { data } = await api.get<CashFlowReportRowDto[]>('/api/reports/cashflow', {
    params: { fromUtc, toUtc, take },
  });
  return data;
}

export async function getStockReport(take = 500): Promise<StockReportRowDto[]> {
  const { data } = await api.get<StockReportRowDto[]>('/api/reports/stock', {
    params: { take },
  });
  return data;
}
