using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Pdv.Web.Models;
using Pdv.Web.Services;

namespace Pdv.Maui.Services;

public class SyncWorker
{
    private readonly LocalDbService _db;
    private readonly ProductsService _productsService;
    private readonly SalesService _salesService;
    private readonly StockService _stockService;
    private readonly SuppliersService _suppliersService;
    private readonly SemaphoreSlim _syncSemaphore = new(1, 1);

    public SyncWorker(
        LocalDbService db,
        ProductsService productsService,
        SalesService salesService,
        StockService stockService,
        SuppliersService suppliersService)
    {
        _db = db;
        _productsService = productsService;
        _salesService = salesService;
        _stockService = stockService;
        _suppliersService = suppliersService;
    }

    public async Task SyncAllAsync()
    {
        // Garante que apenas uma sincronização ocorra por vez
        if (!await _syncSemaphore.WaitAsync(0))
            return;

        try
        {
            await PushPendingChangesAsync();
            await PullLatestDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro durante a sincronização: {ex.Message}");
        }
        finally
        {
            _syncSemaphore.Release();
        }
    }

    private async Task PushPendingChangesAsync()
    {
        var pendingList = await _db.GetPendingSyncsAsync();
        if (pendingList.Count == 0) return;

        var prodSvc = _productsService as MauiProductsService;
        var saleSvc = _salesService as MauiSalesService;
        var stockSvc = _stockService as MauiStockService;
        var suppSvc = _suppliersService as MauiSuppliersService;

        foreach (var item in pendingList)
        {
            try
            {
                bool success = false;

                switch (item.Type)
                {
                    case "Sale":
                        if (saleSvc != null)
                        {
                            var payload = JsonSerializer.Deserialize<CreateSalePayload>(item.Payload);
                            if (payload != null)
                            {
                                await saleSvc.CreateSaleCloudAsync(payload);
                                success = true;
                            }
                        }
                        break;

                    case "StockAdjust":
                        if (stockSvc != null)
                        {
                            var payload = JsonSerializer.Deserialize<StockAdjustPayload>(item.Payload);
                            if (payload != null)
                            {
                                await stockSvc.AdjustStockCloudAsync(payload.ProductVariationId, payload.Quantity, payload.Reason);
                                success = true;
                            }
                        }
                        break;

                    case "SupplierCreate":
                        if (suppSvc != null)
                        {
                            var payload = JsonSerializer.Deserialize<SupplierPayload>(item.Payload);
                            if (payload != null)
                            {
                                await suppSvc.CreateSupplierCloudAsync(payload.Name, payload.Document, payload.Email, payload.Phone, payload.IsActive);
                                success = true;
                            }
                        }
                        break;

                    case "SupplierUpdate":
                        if (suppSvc != null)
                        {
                            var payload = JsonSerializer.Deserialize<SupplierUpdatePayload>(item.Payload);
                            if (payload != null)
                            {
                                await suppSvc.UpdateSupplierCloudAsync(payload.Id, payload.Name, payload.Document, payload.Email, payload.Phone, payload.IsActive);
                                success = true;
                            }
                        }
                        break;

                    case "SupplierDelete":
                        if (suppSvc != null)
                        {
                            await suppSvc.DeleteSupplierCloudAsync(item.Payload);
                            success = true;
                        }
                        break;

                    case "ProductCreate":
                        if (prodSvc != null)
                        {
                            var payload = JsonSerializer.Deserialize<ProductCreatePayload>(item.Payload);
                            if (payload != null)
                            {
                                await prodSvc.CreateProductCloudAsync(payload.Name, payload.IsActive);
                                success = true;
                            }
                        }
                        break;

                    case "ProductUpdate":
                        if (prodSvc != null)
                        {
                            var payload = JsonSerializer.Deserialize<ProductUpdatePayload>(item.Payload);
                            if (payload != null)
                            {
                                await prodSvc.UpdateProductCloudAsync(payload.Id, payload.Name, payload.IsActive);
                                success = true;
                            }
                        }
                        break;

                    case "ProductDelete":
                        if (prodSvc != null)
                        {
                            await prodSvc.DeleteProductCloudAsync(item.Payload);
                            success = true;
                        }
                        break;

                    case "VariationCreate":
                        if (prodSvc != null)
                        {
                            var payload = JsonSerializer.Deserialize<VariationCreatePayload>(item.Payload);
                            if (payload != null)
                            {
                                await prodSvc.CreateVariationCloudAsync(payload.ProductId, payload.Name, payload.Barcode, payload.StockQuantity, payload.UnitPrice);
                                success = true;
                            }
                        }
                        break;

                    case "VariationUpdate":
                        if (prodSvc != null)
                        {
                            var payload = JsonSerializer.Deserialize<VariationUpdatePayload>(item.Payload);
                            if (payload != null)
                            {
                                await prodSvc.UpdateVariationCloudAsync(payload.Id, payload.Name, payload.Barcode, payload.StockQuantity, payload.UnitPrice);
                                success = true;
                            }
                        }
                        break;

                    case "VariationDelete":
                        if (prodSvc != null)
                        {
                            await prodSvc.DeleteVariationCloudAsync(item.Payload);
                            success = true;
                        }
                        break;
                }

                if (success)
                {
                    // Item sincronizado com sucesso, remove da fila local
                    await _db.DeletePendingSyncAsync(item.Id);
                }
            }
            catch (HttpRequestException)
            {
                // Erro de rede ou indisponibilidade da API. Para o fluxo para não quebrar a ordem cronológica.
                break;
            }
            catch (Exception ex)
            {
                // Erro lógico (ex: dados inválidos). Registra e remove ou pula para não travar a fila.
                System.Diagnostics.Debug.WriteLine($"Erro lógico de sync no item {item.Id}: {ex.Message}");
                await _db.DeletePendingSyncAsync(item.Id);
            }
        }
    }

    private async Task PullLatestDataAsync()
    {
        // 1. Atualizar Fornecedores
        try
        {
            var suppliers = await _suppliersService.ListSuppliersAsync(); // se chamar a partir daqui, ele passará pelo fluxo normal de caching do MauiSuppliersService
        }
        catch { }

        // 2. Atualizar Produtos e Variações
        try
        {
            var products = await _productsService.ListProductsAsync();
            foreach (var p in products)
            {
                await _productsService.GetProductAsync(p.Id); // Isso forçará o carregamento e cache das variações
            }
        }
        catch { }

        // 3. Atualizar Vendas
        try
        {
            await _salesService.ListSalesAsync();
        }
        catch { }
    }

    // Payloads auxiliares para deserialização
    private record StockAdjustPayload(string ProductVariationId, int Quantity, string? Reason);
    private record SupplierPayload(string Name, string? Document, string? Email, string? Phone, bool IsActive);
    private record SupplierUpdatePayload(string Id, string Name, string? Document, string? Email, string? Phone, bool IsActive);
    private record ProductCreatePayload(string Name, bool IsActive);
    private record ProductUpdatePayload(string Id, string Name, bool IsActive);
    private record VariationCreatePayload(string ProductId, string Name, string? Barcode, int StockQuantity, decimal UnitPrice);
    private record VariationUpdatePayload(string Id, string Name, string? Barcode, int StockQuantity, decimal UnitPrice);
}
