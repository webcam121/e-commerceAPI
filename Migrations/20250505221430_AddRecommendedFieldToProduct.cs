using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecommerceAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRecommendedFieldToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRecommended",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRecommended",
                table: "Products");
        }
    }
}
