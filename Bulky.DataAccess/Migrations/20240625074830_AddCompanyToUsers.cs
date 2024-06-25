using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyWeb.Bulky.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Companies_CategoryId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "AspNetUsers",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_CategoryId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Companies_CompanyId",
                table: "AspNetUsers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Companies_CompanyId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "AspNetUsers",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_CompanyId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Companies_CategoryId",
                table: "AspNetUsers",
                column: "CategoryId",
                principalTable: "Companies",
                principalColumn: "Id");
        }
    }
}
