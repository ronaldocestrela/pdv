using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Pdv.Maui.Services;

public class LocalProduct
{
    [PrimaryKey]
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
}

public class LocalProductVariation
{
    [PrimaryKey]
    public string Id { get; set; } = "";
    public string ProductId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Barcode { get; set; }
    public int StockQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class LocalSupplier
{
    [PrimaryKey]
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Document { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
}

public class LocalSale
{
    [PrimaryKey]
    public string Id { get; set; } = "";
    public string CreatedAtUtc { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = "";
    public int ItemCount { get; set; }
}

public class LocalPendingSync
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Type { get; set; } = ""; // "Sale", "StockAdjust", "Supplier", "Product"
    public string Payload { get; set; } = ""; // JSON
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class LocalDbService
{
    private SQLiteAsyncConnection? _database;

    private async Task InitAsync()
    {
        if (_database is not null)
            return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "pdv_local.db");
        _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<LocalProduct>();
        await _database.CreateTableAsync<LocalProductVariation>();
        await _database.CreateTableAsync<LocalSupplier>();
        await _database.CreateTableAsync<LocalSale>();
        await _database.CreateTableAsync<LocalPendingSync>();
    }

    // ── Products ──
    public async Task<List<LocalProduct>> GetProductsAsync()
    {
        await InitAsync();
        return await _database!.Table<LocalProduct>().ToListAsync();
    }

    public async Task<LocalProduct?> GetProductAsync(string id)
    {
        await InitAsync();
        return await _database!.Table<LocalProduct>().Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task SaveProductsAsync(List<LocalProduct> products)
    {
        await InitAsync();
        foreach (var p in products)
        {
            await _database!.InsertOrReplaceAsync(p);
        }
    }

    public async Task SaveProductAsync(LocalProduct product)
    {
        await InitAsync();
        await _database!.InsertOrReplaceAsync(product);
    }

    public async Task DeleteProductAsync(string id)
    {
        await InitAsync();
        await _database!.Table<LocalProduct>().DeleteAsync(p => p.Id == id);
        await _database!.Table<LocalProductVariation>().DeleteAsync(v => v.ProductId == id);
    }

    // ── Variations ──
    public async Task<List<LocalProductVariation>> GetVariationsAsync(string productId)
    {
        await InitAsync();
        return await _database!.Table<LocalProductVariation>().Where(v => v.ProductId == productId).ToListAsync();
    }

    public async Task<LocalProductVariation?> GetVariationAsync(string id)
    {
        await InitAsync();
        return await _database!.Table<LocalProductVariation>().Where(v => v.Id == id).FirstOrDefaultAsync();
    }

    public async Task SaveVariationsAsync(List<LocalProductVariation> variations)
    {
        await InitAsync();
        foreach (var v in variations)
        {
            await _database!.InsertOrReplaceAsync(v);
        }
    }

    public async Task SaveVariationAsync(LocalProductVariation variation)
    {
        await InitAsync();
        await _database!.InsertOrReplaceAsync(variation);
    }

    public async Task DeleteVariationAsync(string id)
    {
        await InitAsync();
        await _database!.Table<LocalProductVariation>().DeleteAsync(v => v.Id == id);
    }

    // ── Suppliers ──
    public async Task<List<LocalSupplier>> GetSuppliersAsync()
    {
        await InitAsync();
        return await _database!.Table<LocalSupplier>().ToListAsync();
    }

    public async Task<LocalSupplier?> GetSupplierAsync(string id)
    {
        await InitAsync();
        return await _database!.Table<LocalSupplier>().Where(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task SaveSuppliersAsync(List<LocalSupplier> suppliers)
    {
        await InitAsync();
        foreach (var s in suppliers)
        {
            await _database!.InsertOrReplaceAsync(s);
        }
    }

    public async Task SaveSupplierAsync(LocalSupplier supplier)
    {
        await InitAsync();
        await _database!.InsertOrReplaceAsync(supplier);
    }

    public async Task DeleteSupplierAsync(string id)
    {
        await InitAsync();
        await _database!.Table<LocalSupplier>().DeleteAsync(s => s.Id == id);
    }

    // ── Sales ──
    public async Task<List<LocalSale>> GetSalesAsync()
    {
        await InitAsync();
        return await _database!.Table<LocalSale>().OrderByDescending(s => s.CreatedAtUtc).ToListAsync();
    }

    public async Task SaveSaleAsync(LocalSale sale)
    {
        await InitAsync();
        await _database!.InsertOrReplaceAsync(sale);
    }

    // ── Pending Sync Fila ──
    public async Task<List<LocalPendingSync>> GetPendingSyncsAsync()
    {
        await InitAsync();
        return await _database!.Table<LocalPendingSync>().OrderBy(s => s.CreatedAtUtc).ToListAsync();
    }

    public async Task AddPendingSyncAsync(string type, string payloadJson)
    {
        await InitAsync();
        var pending = new LocalPendingSync
        {
            Type = type,
            Payload = payloadJson,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _database!.InsertAsync(pending);
    }

    public async Task DeletePendingSyncAsync(int id)
    {
        await InitAsync();
        await _database!.Table<LocalPendingSync>().DeleteAsync(p => p.Id == id);
    }

    public async Task<int> GetPendingCountAsync()
    {
        await InitAsync();
        return await _database!.Table<LocalPendingSync>().CountAsync();
    }
}
