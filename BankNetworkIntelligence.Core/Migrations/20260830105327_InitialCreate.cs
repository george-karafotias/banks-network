using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BankNetworkIntelligence.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "banks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hebic_imports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceFile = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RecordCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hebic_imports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "municipalities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Prefecture = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_municipalities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bank_locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BankId = table.Column<int>(type: "integer", nullable: false),
                    HebicCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bank_locations_banks_BankId",
                        column: x => x.BankId,
                        principalTable: "banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "location_snapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    ImportId = table.Column<int>(type: "integer", nullable: false),
                    MunicipalityId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Phone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Fax = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HasBranch = table.Column<bool>(type: "boolean", nullable: false),
                    HasAtm = table.Column<bool>(type: "boolean", nullable: false),
                    HasAps = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_location_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_location_snapshots_bank_locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "bank_locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_location_snapshots_hebic_imports_ImportId",
                        column: x => x.ImportId,
                        principalTable: "hebic_imports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_location_snapshots_municipalities_MunicipalityId",
                        column: x => x.MunicipalityId,
                        principalTable: "municipalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bank_locations_BankId_HebicCode",
                table: "bank_locations",
                columns: new[] { "BankId", "HebicCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_banks_Name",
                table: "banks",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hebic_imports_Period",
                table: "hebic_imports",
                column: "Period",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_location_snapshots_ImportId",
                table: "location_snapshots",
                column: "ImportId");

            migrationBuilder.CreateIndex(
                name: "IX_location_snapshots_LocationId_ImportId",
                table: "location_snapshots",
                columns: new[] { "LocationId", "ImportId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_location_snapshots_MunicipalityId",
                table: "location_snapshots",
                column: "MunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_municipalities_Name_Prefecture",
                table: "municipalities",
                columns: new[] { "Name", "Prefecture" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "location_snapshots");

            migrationBuilder.DropTable(
                name: "bank_locations");

            migrationBuilder.DropTable(
                name: "hebic_imports");

            migrationBuilder.DropTable(
                name: "municipalities");

            migrationBuilder.DropTable(
                name: "banks");
        }
    }
}
