using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecommerceAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategoryAttributeRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryAttributes_Categories_CategoryId",
                table: "CategoryAttributes");

            migrationBuilder.DropIndex(
                name: "IX_CategoryAttributes_CategoryId",
                table: "CategoryAttributes");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "CategoryAttributes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CategoryAttributes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CategoryAttributes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CategoryAttributeCategories",
                columns: table => new
                {
                    AttributesId = table.Column<int>(type: "integer", nullable: false),
                    CategoriesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryAttributeCategories", x => new { x.AttributesId, x.CategoriesId });
                    table.ForeignKey(
                        name: "FK_CategoryAttributeCategories_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryAttributeCategories_CategoryAttributes_AttributesId",
                        column: x => x.AttributesId,
                        principalTable: "CategoryAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryAttributeCategories_CategoriesId",
                table: "CategoryAttributeCategories",
                column: "CategoriesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryAttributeCategories");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CategoryAttributes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CategoryAttributes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "CategoryAttributes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryAttributes_CategoryId",
                table: "CategoryAttributes",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryAttributes_Categories_CategoryId",
                table: "CategoryAttributes",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
