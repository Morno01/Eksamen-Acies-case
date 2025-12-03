using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Elementer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Maerke = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Serie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Hoejde = table.Column<int>(type: "int", nullable: false),
                    Bredde = table.Column<int>(type: "int", nullable: false),
                    Dybde = table.Column<int>(type: "int", nullable: false),
                    Vaegt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ErSpecialelement = table.Column<bool>(type: "bit", nullable: false),
                    ErGeometrielement = table.Column<bool>(type: "bit", nullable: false),
                    RotationsRegel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    KraeverPalletype = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaksElementerPrPalle = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elementer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Paller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PalleBeskrivelse = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Laengde = table.Column<int>(type: "int", nullable: false),
                    Bredde = table.Column<int>(type: "int", nullable: false),
                    Hoejde = table.Column<int>(type: "int", nullable: false),
                    Pallegruppe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Palletype = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Vaegt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaksHoejde = table.Column<int>(type: "int", nullable: false),
                    MaksVaegt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Overmaal = table.Column<int>(type: "int", nullable: false),
                    LuftMellemElementer = table.Column<int>(type: "int", nullable: false),
                    Aktiv = table.Column<bool>(type: "bit", nullable: false),
                    Sortering = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Navn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaksLag = table.Column<int>(type: "int", nullable: false),
                    TilladVendeOpTilMaksKg = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HoejdeBreddefaktor = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    HoejdeBreddefaktorKunForEnkeltElementer = table.Column<bool>(type: "bit", nullable: false),
                    TilladStablingOpTilMaksHoejdeInklPalle = table.Column<int>(type: "int", nullable: true),
                    TilladStablingOpTilMaksElementVaegt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TillaegMonteringAfEndeplade = table.Column<int>(type: "int", nullable: false),
                    Aktiv = table.Column<bool>(type: "bit", nullable: false),
                    SorteringsPrioritering = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PlacerLaengsteElementerYderst = table.Column<bool>(type: "bit", nullable: false),
                    MaksBalanceVaerdi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pakkeplaner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdreReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Oprettet = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SettingsId = table.Column<int>(type: "int", nullable: true),
                    AntalPaller = table.Column<int>(type: "int", nullable: false),
                    AntalElementer = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pakkeplaner", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pakkeplaner_Settings_SettingsId",
                        column: x => x.SettingsId,
                        principalTable: "Settings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PakkeplanPaller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PakkeplanId = table.Column<int>(type: "int", nullable: false),
                    PalleNummer = table.Column<int>(type: "int", nullable: false),
                    PalleId = table.Column<int>(type: "int", nullable: false),
                    SamletHoejde = table.Column<int>(type: "int", nullable: false),
                    SamletVaegt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AntalLag = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PakkeplanPaller", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PakkeplanPaller_Pakkeplaner_PakkeplanId",
                        column: x => x.PakkeplanId,
                        principalTable: "Pakkeplaner",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PakkeplanPaller_Paller_PalleId",
                        column: x => x.PalleId,
                        principalTable: "Paller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PakkeplanElementer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PakkeplanPalleId = table.Column<int>(type: "int", nullable: false),
                    ElementId = table.Column<int>(type: "int", nullable: false),
                    Lag = table.Column<int>(type: "int", nullable: false),
                    Plads = table.Column<int>(type: "int", nullable: false),
                    ErRoteret = table.Column<bool>(type: "bit", nullable: false),
                    Sortering = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PakkeplanElementer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PakkeplanElementer_Elementer_ElementId",
                        column: x => x.ElementId,
                        principalTable: "Elementer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PakkeplanElementer_PakkeplanPaller_PakkeplanPalleId",
                        column: x => x.PakkeplanPalleId,
                        principalTable: "PakkeplanPaller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Paller",
                columns: new[] { "Id", "Aktiv", "Bredde", "Hoejde", "Laengde", "LuftMellemElementer", "MaksHoejde", "MaksVaegt", "Overmaal", "PalleBeskrivelse", "Pallegruppe", "Palletype", "Sortering", "Vaegt" },
                values: new object[,]
                {
                    { 1, true, 750, 150, 2400, 10, 2800, 1000m, 50, "75'er Træpalle", "75", "Trae", 1, 25m },
                    { 2, true, 800, 150, 2400, 10, 2800, 1200m, 50, "80'er Træpalle", "80", "Trae", 2, 27m },
                    { 3, true, 1000, 150, 2400, 10, 2800, 1500m, 50, "100'er Træpalle", "100", "Trae", 3, 30m }
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "Aktiv", "HoejdeBreddefaktor", "HoejdeBreddefaktorKunForEnkeltElementer", "MaksBalanceVaerdi", "MaksLag", "Navn", "PlacerLaengsteElementerYderst", "SorteringsPrioritering", "TilladStablingOpTilMaksElementVaegt", "TilladStablingOpTilMaksHoejdeInklPalle", "TilladVendeOpTilMaksKg", "TillaegMonteringAfEndeplade" },
                values: new object[] { 1, true, 0.3m, true, 100m, 2, "Standard", true, "Maerke,Specialelement,Pallestorrelse,Elementstorrelse,Vaegt,Serie", 70m, 1500, 50m, 20 });

            migrationBuilder.CreateIndex(
                name: "IX_PakkeplanElementer_ElementId",
                table: "PakkeplanElementer",
                column: "ElementId");

            migrationBuilder.CreateIndex(
                name: "IX_PakkeplanElementer_PakkeplanPalleId",
                table: "PakkeplanElementer",
                column: "PakkeplanPalleId");

            migrationBuilder.CreateIndex(
                name: "IX_Pakkeplaner_SettingsId",
                table: "Pakkeplaner",
                column: "SettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_PakkeplanPaller_PakkeplanId",
                table: "PakkeplanPaller",
                column: "PakkeplanId");

            migrationBuilder.CreateIndex(
                name: "IX_PakkeplanPaller_PalleId",
                table: "PakkeplanPaller",
                column: "PalleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PakkeplanElementer");

            migrationBuilder.DropTable(
                name: "Elementer");

            migrationBuilder.DropTable(
                name: "PakkeplanPaller");

            migrationBuilder.DropTable(
                name: "Pakkeplaner");

            migrationBuilder.DropTable(
                name: "Paller");

            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
