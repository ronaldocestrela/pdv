namespace Pdv.Infrastructure.Seed;

internal static class PermissionCatalog
{
    public static readonly IReadOnlyList<string> All = new[]
    {
        "product.create",
        "product.update",
        "product.view",
        "sale.create",
        "sale.view",
        "stock.adjust",
        "stock.view",
        "report.view",
        "cashflow.view",
        "user.manage",
        "role.manage",
    };
}

internal static class RoleNames
{
    public const string SuperAdmin = "Super Admin";
}
