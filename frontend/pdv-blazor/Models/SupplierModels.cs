namespace Pdv.Web.Models;

/// <summary>Espelho de SupplierSummaryDto.</summary>
public class SupplierSummaryDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Document { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
}
