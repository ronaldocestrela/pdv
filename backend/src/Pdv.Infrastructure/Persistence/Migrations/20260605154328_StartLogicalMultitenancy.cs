using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pdv.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StartLogicalMultitenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_ProductVariationId_CreatedAtUtc",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_Roles_Name",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariations_Barcode",
                table: "ProductVariations");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "StockMovements",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "SaleItems",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ProductVariations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "CashFlows",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_TenantId",
                table: "StockMovements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_TenantId_ProductVariationId",
                table: "StockMovements",
                columns: new[] { "TenantId", "ProductVariationId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_TenantId_ProductVariationId_CreatedAtUtc",
                table: "StockMovements",
                columns: new[] { "TenantId", "ProductVariationId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Sales_TenantId",
                table: "Sales",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_TenantId_CreatedAtUtc",
                table: "Sales",
                columns: new[] { "TenantId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_TenantId",
                table: "SaleItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_TenantId_ProductVariationId",
                table: "SaleItems",
                columns: new[] { "TenantId", "ProductVariationId" });

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_TenantId_SaleId",
                table: "SaleItems",
                columns: new[] { "TenantId", "SaleId" });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId",
                table: "Roles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Name",
                table: "Roles",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariations_TenantId",
                table: "ProductVariations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariations_TenantId_Barcode",
                table: "ProductVariations",
                columns: new[] { "TenantId", "Barcode" },
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariations_TenantId_ProductId",
                table: "ProductVariations",
                columns: new[] { "TenantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId",
                table: "Products",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId_Name",
                table: "Products",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_CashFlows_TenantId",
                table: "CashFlows",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlows_TenantId_SaleId",
                table: "CashFlows",
                columns: new[] { "TenantId", "SaleId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_TenantId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_TenantId_ProductVariationId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_TenantId_ProductVariationId_CreatedAtUtc",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_Sales_TenantId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_TenantId_CreatedAtUtc",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_TenantId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_TenantId_ProductVariationId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_TenantId_SaleId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_Roles_TenantId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_TenantId_Name",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariations_TenantId",
                table: "ProductVariations");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariations_TenantId_Barcode",
                table: "ProductVariations");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariations_TenantId_ProductId",
                table: "ProductVariations");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId_Name",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_CashFlows_TenantId",
                table: "CashFlows");

            migrationBuilder.DropIndex(
                name: "IX_CashFlows_TenantId_SaleId",
                table: "CashFlows");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductVariations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CashFlows");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductVariationId_CreatedAtUtc",
                table: "StockMovements",
                columns: new[] { "ProductVariationId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariations_Barcode",
                table: "ProductVariations",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");
        }
    }
}
