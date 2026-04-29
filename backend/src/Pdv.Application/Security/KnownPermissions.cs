namespace Pdv.Application.Security;

/// <summary>Permission claim values (JWT + authorization policies). Keep in sync with seed.</summary>
public static class KnownPermissions
{
    public const string ProductCreate = "product.create";
    public const string ProductUpdate = "product.update";
    public const string ProductDelete = "product.delete";
    public const string ProductView = "product.view";

    public const string VariationCreate = "variation.create";
    public const string VariationUpdate = "variation.update";
    public const string VariationDelete = "variation.delete";
    public const string VariationView = "variation.view";

    public const string SaleCreate = "sale.create";
    public const string SaleView = "sale.view";
    public const string StockAdjust = "stock.adjust";
    public const string StockView = "stock.view";
    public const string ReportView = "report.view";
    public const string CashflowView = "cashflow.view";
    public const string UserManage = "user.manage";
    public const string RoleManage = "role.manage";

    public static readonly IReadOnlyList<string> All = new[]
    {
        ProductCreate,
        ProductUpdate,
        ProductDelete,
        ProductView,
        VariationCreate,
        VariationUpdate,
        VariationDelete,
        VariationView,
        SaleCreate,
        SaleView,
        StockAdjust,
        StockView,
        ReportView,
        CashflowView,
        UserManage,
        RoleManage,
    };
}
