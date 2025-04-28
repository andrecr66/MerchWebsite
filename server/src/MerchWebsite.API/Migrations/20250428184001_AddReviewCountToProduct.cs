using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerchWebsite.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewCountToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfReviews",
                table: "Products",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfReviews",
                table: "Products");
        }
    }
}
