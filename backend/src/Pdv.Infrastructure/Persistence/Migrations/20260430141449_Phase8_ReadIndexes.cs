using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pdv.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase8_ReadIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductVariationId_CreatedAtUtc",
                table: "StockMovements",
                columns: new[] { "ProductVariationId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Sales_CreatedAtUtc",
                table: "Sales",
                column: "CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockMovements_ProductVariationId_CreatedAtUtc",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_Sales_CreatedAtUtc",
                table: "Sales");
        }
    }
}
