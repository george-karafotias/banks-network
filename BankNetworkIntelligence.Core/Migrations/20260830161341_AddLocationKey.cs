using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankNetworkIntelligence.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_bank_locations_BankId_HebicCode",
                table: "bank_locations");

            migrationBuilder.AlterColumn<string>(
                name: "HebicCode",
                table: "bank_locations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "LocationKey",
                table: "bank_locations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_bank_locations_BankId",
                table: "bank_locations",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_bank_locations_LocationKey",
                table: "bank_locations",
                column: "LocationKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_bank_locations_BankId",
                table: "bank_locations");

            migrationBuilder.DropIndex(
                name: "IX_bank_locations_LocationKey",
                table: "bank_locations");

            migrationBuilder.DropColumn(
                name: "LocationKey",
                table: "bank_locations");

            migrationBuilder.AlterColumn<string>(
                name: "HebicCode",
                table: "bank_locations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_bank_locations_BankId_HebicCode",
                table: "bank_locations",
                columns: new[] { "BankId", "HebicCode" },
                unique: true);
        }
    }
}
