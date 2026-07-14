using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;
using Pdv.Web.Models;
using Pdv.Web.Services;

namespace Pdv.Maui.Services;

public class MauiConnectionStatusService : ConnectionStatusService
{
    private readonly LocalDbService _db;
    private bool _isOnline;
    private int _pendingCount;

    public MauiConnectionStatusService(LocalDbService db)
    {
        _db = db;
        _isOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        
        // Iniciar atualização periódica da contagem
        _ = UpdatePendingCountAsync();
    }

    public override bool IsOnline => _isOnline;
    public override int PendingItemsCount => _pendingCount;

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        var online = e.NetworkAccess == NetworkAccess.Internet;
        if (_isOnline != online)
        {
            _isOnline = online;
            NotifyStatusChanged();
            if (_isOnline)
            {
                // Se voltou a ficar online, dispara a sincronização
                _ = SyncAsync();
            }
        }
    }

    public async Task UpdatePendingCountAsync()
    {
        var count = await _db.GetPendingCountAsync();
        if (_pendingCount != count)
        {
            _pendingCount = count;
            NotifyStatusChanged();
        }
    }

    public override async Task SyncAsync()
    {
        // Será resolvido e executado pelo SyncWorker
        var syncWorker = App.ServiceProvider?.GetService<SyncWorker>();
        if (syncWorker != null)
        {
            await syncWorker.SyncAllAsync();
            await UpdatePendingCountAsync();
        }
    }
}

public class MauiProductsService : ProductsService
{
    private readonly LocalDbService _db;
    private readonly ConnectionStatusService _conn;

    public MauiProductsService(HttpClient http, LocalDbService db, ConnectionStatusService conn) : base(http)
    {
        _db = db;
        _conn = conn;
    }

    public override async Task<List<ProductSummaryDto>> ListProductsAsync()
    {
        if (_conn.IsOnline)
        {
            try
            {
                var cloudProducts = await base.ListProductsAsync();
                
                // Atualiza cache local
                var localProducts = cloudProducts.Select(p => new LocalProduct
                {
                    Id = p.Id,
                    Name = p.Name,
                    IsActive = p.IsActive
                }).ToList();
                await _db.SaveProductsAsync(localProducts);

                return cloudProducts;
            }
            catch
            {
                // Fallback para offline se der erro de conexão
            }
        }

        // Modo offline: carrega do SQLite
        var products = await _db.GetProductsAsync();
        var dtos = new List<ProductSummaryDto>();
        foreach (var p in products)
        {
            var variations = await _db.GetVariationsAsync(p.Id);
            dtos.Add(new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                IsActive = p.IsActive,
                VariationCount = variations.Count
            });
        }
        return dtos;
    }

    public override async Task<ProductDetailDto?> GetProductAsync(string id)
    {
        if (_conn.IsOnline)
        {
            try
            {
                var detail = await base.GetProductAsync(id);
                if (detail != null)
                {
                    // Cache local do produto e variações
                    await _db.SaveProductAsync(new LocalProduct
                    {
                        Id = detail.Id,
                        Name = detail.Name,
                        IsActive = detail.IsActive
                    });
                    
                    var variations = detail.Variations.Select(v => new LocalProductVariation
                    {
                        Id = v.Id,
                        ProductId = v.ProductId,
                        Name = v.Name,
                        Barcode = v.Barcode,
                        StockQuantity = v.StockQuantity,
                        UnitPrice = v.UnitPrice
                    }).ToList();
                    await _db.SaveVariationsAsync(variations);
                }
                return detail;
            }
            catch
            {
                // Fallback
            }
        }

        // Modo offline
        var pLocal = await _db.GetProductAsync(id);
        if (pLocal == null) return null;

        var vLocals = await _db.GetVariationsAsync(id);
        return new ProductDetailDto
        {
            Id = pLocal.Id,
            Name = pLocal.Name,
            IsActive = pLocal.IsActive,
            Variations = vLocals.Select(v => new ProductVariationDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Name = v.Name,
                Barcode = v.Barcode,
                StockQuantity = v.StockQuantity,
                UnitPrice = v.UnitPrice
            }).ToList()
        };
    }

    public override async Task<string> CreateProductAsync(string name, bool isActive)
    {
        if (_conn.IsOnline)
        {
            try
            {
                var id = await base.CreateProductAsync(name, isActive);
                await _db.SaveProductAsync(new LocalProduct { Id = id, Name = name, IsActive = isActive });
                return id;
            }
            catch
            {
                // Fallback
            }
        }

        // Criar offline
        var newId = Guid.NewGuid().ToString();
        await _db.SaveProductAsync(new LocalProduct { Id = newId, Name = name, IsActive = isActive });
        
        var payload = new { Name = name, IsActive = isActive };
        await _db.AddPendingSyncAsync("ProductCreate", JsonSerializer.Serialize(payload));
        
        NotifyPendingCountChanged();
        return newId;
    }

    public override async Task UpdateProductAsync(string id, string name, bool isActive)
    {
        if (_conn.IsOnline)
        {
            try
            {
                await base.UpdateProductAsync(id, name, isActive);
                await _db.SaveProductAsync(new LocalProduct { Id = id, Name = name, IsActive = isActive });
                return;
            }
            catch
            {
                // Fallback
            }
        }

        await _db.SaveProductAsync(new LocalProduct { Id = id, Name = name, IsActive = isActive });
        var payload = new { Id = id, Name = name, IsActive = isActive };
        await _db.AddPendingSyncAsync("ProductUpdate", JsonSerializer.Serialize(payload));
        NotifyPendingCountChanged();
    }

    public override async Task DeleteProductAsync(string id)
    {
        if (_conn.IsOnline)
        {
            try
            {
                await base.DeleteProductAsync(id);
                await _db.DeleteProductAsync(id);
                return;
            }
            catch
            {
                // Fallback
            }
        }

        await _db.DeleteProductAsync(id);
        await _db.AddPendingSyncAsync("ProductDelete", id);
        NotifyPendingCountChanged();
    }

    public override async Task<string> CreateVariationAsync(string productId, string name, string? barcode, int stockQuantity, decimal unitPrice)
    {
        if (_conn.IsOnline)
        {
            try
            {
                var id = await base.CreateVariationAsync(productId, name, barcode, stockQuantity, unitPrice);
                await _db.SaveVariationAsync(new LocalProductVariation
                {
                    Id = id,
                    ProductId = productId,
                    Name = name,
                    Barcode = barcode,
                    StockQuantity = stockQuantity,
                    UnitPrice = unitPrice
                });
                return id;
            }
            catch
            {
                // Fallback
            }
        }

        var newId = Guid.NewGuid().ToString();
        var variation = new LocalProductVariation
        {
            Id = newId,
            ProductId = productId,
            Name = name,
            Barcode = barcode,
            StockQuantity = stockQuantity,
            UnitPrice = unitPrice
        };
        await _db.SaveVariationAsync(variation);

        var payload = new { ProductId = productId, Name = name, Barcode = barcode, StockQuantity = stockQuantity, UnitPrice = unitPrice };
        await _db.AddPendingSyncAsync("VariationCreate", JsonSerializer.Serialize(payload));
        NotifyPendingCountChanged();
        return newId;
    }

    public override async Task UpdateVariationAsync(string id, string name, string? barcode, int stockQuantity, decimal unitPrice)
    {
        if (_conn.IsOnline)
        {
            try
            {
                await base.UpdateVariationAsync(id, name, barcode, stockQuantity, unitPrice);
                var existing = await _db.GetVariationAsync(id);
                if (existing != null)
                {
                    existing.Name = name;
                    existing.Barcode = barcode;
                    existing.StockQuantity = stockQuantity;
                    existing.UnitPrice = unitPrice;
                    await _db.SaveVariationAsync(existing);
                }
                return;
            }
            catch
            {
                // Fallback
            }
        }

        var v = await _db.GetVariationAsync(id);
        if (v != null)
        {
            v.Name = name;
            v.Barcode = barcode;
            v.StockQuantity = stockQuantity;
            v.UnitPrice = unitPrice;
            await _db.SaveVariationAsync(v);
        }

        var payload = new { Id = id, Name = name, Barcode = barcode, StockQuantity = stockQuantity, UnitPrice = unitPrice };
        await _db.AddPendingSyncAsync("VariationUpdate", JsonSerializer.Serialize(payload));
        NotifyPendingCountChanged();
    }

    public override async Task DeleteVariationAsync(string id)
    {
        if (_conn.IsOnline)
        {
            try
            {
                await base.DeleteVariationAsync(id);
                await _db.DeleteVariationAsync(id);
                return;
            }
            catch
            {
                // Fallback
            }
        }

        await _db.DeleteVariationAsync(id);
        await _db.AddPendingSyncAsync("VariationDelete", id);
        NotifyPendingCountChanged();
    }

    // Métodos para uso direto do SyncWorker (bypass offline local logic)
    public Task<string> CreateProductCloudAsync(string name, bool isActive) => base.CreateProductAsync(name, isActive);
    public Task UpdateProductCloudAsync(string id, string name, bool isActive) => base.UpdateProductAsync(id, name, isActive);
    public Task DeleteProductCloudAsync(string id) => base.DeleteProductAsync(id);
    public Task<string> CreateVariationCloudAsync(string productId, string name, string? barcode, int stockQuantity, decimal unitPrice) => base.CreateVariationAsync(productId, name, barcode, stockQuantity, unitPrice);
    public Task UpdateVariationCloudAsync(string id, string name, string? barcode, int stockQuantity, decimal unitPrice) => base.UpdateVariationAsync(id, name, barcode, stockQuantity, unitPrice);
    public Task DeleteVariationCloudAsync(string id) => base.DeleteVariationAsync(id);

    private void NotifyPendingCountChanged()
    {
        if (_conn is MauiConnectionStatusService mConn)
        {
            _ = mConn.UpdatePendingCountAsync();
        }
    }
}

public class MauiSalesService : SalesService
{
    private readonly LocalDbService _db;
    private readonly ConnectionStatusService _conn;

    public MauiSalesService(HttpClient http, LocalDbService db, ConnectionStatusService conn) : base(http)
    {
        _db = db;
        _conn = conn;
    }

    public override async Task<CreateSaleResultDto> CreateSaleAsync(CreateSalePayload payload)
    {
        if (_conn.IsOnline)
        {
            try
            {
                var result = await base.CreateSaleAsync(payload);
                
                // Grava no histórico local de vendas
                await _db.SaveSaleAsync(new LocalSale
                {
                    Id = result.SaleId,
                    CreatedAtUtc = DateTime.UtcNow.ToString("o"),
                    TotalAmount = result.TotalAmount,
                    PaymentMethod = payload.PaymentMethod,
                    ItemCount = payload.Items.Sum(i => i.Quantity)
                });

                return result;
            }
            catch
            {
                // Fallback para offline se falhar HTTP
            }
        }

        // Criar venda Offline
        var tempSaleId = "OFFLINE-" + Guid.NewGuid().ToString().Substring(0, 8);
        decimal totalAmount = 0;
        int itemCount = 0;

        foreach (var item in payload.Items)
        {
            var variation = await _db.GetVariationAsync(item.ProductVariationId);
            if (variation != null)
            {
                totalAmount += variation.UnitPrice * item.Quantity;
                itemCount += item.Quantity;

                // Decrementa estoque local para resposta imediata ao usuário
                variation.StockQuantity -= item.Quantity;
                await _db.SaveVariationAsync(variation);
            }
        }

        // Salva venda no histórico local
        await _db.SaveSaleAsync(new LocalSale
        {
            Id = tempSaleId,
            CreatedAtUtc = DateTime.UtcNow.ToString("o"),
            TotalAmount = totalAmount,
            PaymentMethod = payload.PaymentMethod,
            ItemCount = itemCount
        });

        // Adiciona à fila de sincronização
        await _db.AddPendingSyncAsync("Sale", JsonSerializer.Serialize(payload));
        
        if (_conn is MauiConnectionStatusService mConn)
        {
            _ = mConn.UpdatePendingCountAsync();
        }

        return new CreateSaleResultDto
        {
            SaleId = tempSaleId,
            TotalAmount = totalAmount
        };
    }

    public override async Task<List<SaleListItemDto>> ListSalesAsync(int take = 50)
    {
        if (_conn.IsOnline)
        {
            try
            {
                var cloudSales = await base.ListSalesAsync(take);
                
                // Atualiza cache de vendas locais
                foreach (var s in cloudSales)
                {
                    await _db.SaveSaleAsync(new LocalSale
                    {
                        Id = s.Id,
                        CreatedAtUtc = s.CreatedAtUtc,
                        TotalAmount = s.TotalAmount,
                        PaymentMethod = s.PaymentMethod,
                        ItemCount = s.ItemCount
                    });
                }
                return cloudSales;
            }
            catch
            {
                // Fallback
            }
        }

        // Offline
        var localSales = await _db.GetSalesAsync();
        return localSales.Take(take).Select(s => new SaleListItemDto
        {
            Id = s.Id,
            CreatedAtUtc = s.CreatedAtUtc,
            TotalAmount = s.TotalAmount,
            PaymentMethod = s.PaymentMethod,
            ItemCount = s.ItemCount
        }).ToList();
    }

    public Task<CreateSaleResultDto> CreateSaleCloudAsync(CreateSalePayload payload) => base.CreateSaleAsync(payload);
}

public class MauiStockService : StockService
{
    private readonly LocalDbService _db;
    private readonly ConnectionStatusService _conn;

    public MauiStockService(HttpClient http, LocalDbService db, ConnectionStatusService conn) : base(http)
    {
        _db = db;
        _conn = conn;
    }

    public override async Task AdjustStockAsync(string productVariationId, int quantity, string? reason)
    {
        if (_conn.IsOnline)
        {
            try
            {
                await base.AdjustStockAsync(productVariationId, quantity, reason);
                
                // Atualiza estoque localmente
                var variation = await _db.GetVariationAsync(productVariationId);
                if (variation != null)
                {
                    variation.StockQuantity += quantity; // quantidade pode ser negativa para decremento
                    await _db.SaveVariationAsync(variation);
                }
                return;
            }
            catch
            {
                // Fallback
            }
        }

        // Modo Offline
        var localVar = await _db.GetVariationAsync(productVariationId);
        if (localVar != null)
        {
            localVar.StockQuantity += quantity;
            await _db.SaveVariationAsync(localVar);
        }

        var payload = new { ProductVariationId = productVariationId, Quantity = quantity, Reason = reason };
        await _db.AddPendingSyncAsync("StockAdjust", JsonSerializer.Serialize(payload));
        
        if (_conn is MauiConnectionStatusService mConn)
        {
            _ = mConn.UpdatePendingCountAsync();
        }
    }

    public Task AdjustStockCloudAsync(string productVariationId, int quantity, string? reason) => base.AdjustStockAsync(productVariationId, quantity, reason);
}

public class MauiSuppliersService : SuppliersService
{
    private readonly LocalDbService _db;
    private readonly ConnectionStatusService _conn;

    public MauiSuppliersService(HttpClient http, LocalDbService db, ConnectionStatusService conn) : base(http)
    {
        _db = db;
        _conn = conn;
    }

    public override async Task<List<SupplierSummaryDto>> ListSuppliersAsync()
    {
        if (_conn.IsOnline)
        {
            try
            {
                var cloudSuppliers = await base.ListSuppliersAsync();
                
                // Atualiza cache local
                var locals = cloudSuppliers.Select(s => new LocalSupplier
                {
                    Id = s.Id,
                    Name = s.Name,
                    Document = s.Document,
                    Email = s.Email,
                    Phone = s.Phone,
                    IsActive = s.IsActive
                }).ToList();
                await _db.SaveSuppliersAsync(locals);

                return cloudSuppliers;
            }
            catch
            {
                // Fallback
            }
        }

        // Offline
        var suppliers = await _db.GetSuppliersAsync();
        return suppliers.Select(s => new SupplierSummaryDto
        {
            Id = s.Id,
            Name = s.Name,
            Document = s.Document,
            Email = s.Email,
            Phone = s.Phone,
            IsActive = s.IsActive
        }).ToList();
    }

    public override async Task<SupplierSummaryDto?> GetSupplierAsync(string id)
    {
        if (_conn.IsOnline)
        {
            try
            {
                var supplier = await base.GetSupplierAsync(id);
                if (supplier != null)
                {
                    await _db.SaveSupplierAsync(new LocalSupplier
                    {
                        Id = supplier.Id,
                        Name = supplier.Name,
                        Document = supplier.Document,
                        Email = supplier.Email,
                        Phone = supplier.Phone,
                        IsActive = supplier.IsActive
                    });
                }
                return supplier;
            }
            catch
            {
                // Fallback
            }
        }

        var s = await _db.GetSupplierAsync(id);
        if (s == null) return null;
        return new SupplierSummaryDto
        {
            Id = s.Id,
            Name = s.Name,
            Document = s.Document,
            Email = s.Email,
            Phone = s.Phone,
            IsActive = s.IsActive
        };
    }

    public override async Task<string> CreateSupplierAsync(string name, string? document, string? email, string? phone, bool isActive)
    {
        if (_conn.IsOnline)
        {
            try
            {
                var id = await base.CreateSupplierAsync(name, document, email, phone, isActive);
                await _db.SaveSupplierAsync(new LocalSupplier
                {
                    Id = id,
                    Name = name,
                    Document = document,
                    Email = email,
                    Phone = phone,
                    IsActive = isActive
                });
                return id;
            }
            catch
            {
                // Fallback
            }
        }

        var newId = Guid.NewGuid().ToString();
        await _db.SaveSupplierAsync(new LocalSupplier
        {
            Id = newId,
            Name = name,
            Document = document,
            Email = email,
            Phone = phone,
            IsActive = isActive
        });

        var payload = new { Name = name, Document = document, Email = email, Phone = phone, IsActive = isActive };
        await _db.AddPendingSyncAsync("SupplierCreate", JsonSerializer.Serialize(payload));
        
        NotifyPendingCountChanged();
        return newId;
    }

    public override async Task UpdateSupplierAsync(string id, string name, string? document, string? email, string? phone, bool isActive)
    {
        if (_conn.IsOnline)
        {
            try
            {
                await base.UpdateSupplierAsync(id, name, document, email, phone, isActive);
                await _db.SaveSupplierAsync(new LocalSupplier
                {
                    Id = id,
                    Name = name,
                    Document = document,
                    Email = email,
                    Phone = phone,
                    IsActive = isActive
                });
                return;
            }
            catch
            {
                // Fallback
            }
        }

        await _db.SaveSupplierAsync(new LocalSupplier
        {
            Id = id,
            Name = name,
            Document = document,
            Email = email,
            Phone = phone,
            IsActive = isActive
        });

        var payload = new { Id = id, Name = name, Document = document, Email = email, Phone = phone, IsActive = isActive };
        await _db.AddPendingSyncAsync("SupplierUpdate", JsonSerializer.Serialize(payload));
        NotifyPendingCountChanged();
    }

    public override async Task DeleteSupplierAsync(string id)
    {
        if (_conn.IsOnline)
        {
            try
            {
                await base.DeleteSupplierAsync(id);
                await _db.DeleteSupplierAsync(id);
                return;
            }
            catch
            {
                // Fallback
            }
        }

        await _db.DeleteSupplierAsync(id);
        await _db.AddPendingSyncAsync("SupplierDelete", id);
        NotifyPendingCountChanged();
    }

    public Task<string> CreateSupplierCloudAsync(string name, string? document, string? email, string? phone, bool isActive) => base.CreateSupplierAsync(name, document, email, phone, isActive);
    public Task UpdateSupplierCloudAsync(string id, string name, string? document, string? email, string? phone, bool isActive) => base.UpdateSupplierAsync(id, name, document, email, phone, isActive);
    public Task DeleteSupplierCloudAsync(string id) => base.DeleteSupplierAsync(id);

    private void NotifyPendingCountChanged()
    {
        if (_conn is MauiConnectionStatusService mConn)
        {
            _ = mConn.UpdatePendingCountAsync();
        }
    }
}
