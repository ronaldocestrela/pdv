namespace Pdv.Infrastructure.Seed;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string SuperAdminEmail { get; set; } = "admin@local";
    public string SuperAdminPassword { get; set; } = "ChangeMe123!";
}
