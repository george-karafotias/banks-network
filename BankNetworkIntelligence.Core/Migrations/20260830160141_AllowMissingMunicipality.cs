using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankNetworkIntelligence.Core.Migrations
{
    /// <inheritdoc />
    public partial class AllowMissingMunicipality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MunicipalityId",
                table: "location_snapshots",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MunicipalityId",
                table: "location_snapshots",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
