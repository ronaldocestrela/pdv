namespace Pdv.Domain.Entities;

public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<ProductVariation> Variations { get; set; } = new List<ProductVariation>();
}
