using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBAPP_FitMatch.Migrations
{
    /// <inheritdoc />
    public partial class AddLatLonToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Post"" ADD COLUMN IF NOT EXISTS ""Lat"" double precision;
                ALTER TABLE ""Post"" ADD COLUMN IF NOT EXISTS ""Lon"" double precision;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lat",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "Lon",
                table: "Post");
        }
    }
}
