using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerchWebsite.API.Migrations
{
    /// <inheritdoc />
    public partial class SyncAfterRatingAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "Products",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Products");
        }
    }
}
