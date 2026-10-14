using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HastaneOtomasyon.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hastaneler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hastaneler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalismaSaatleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HastaneId = table.Column<int>(type: "int", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gun = table.Column<int>(type: "int", nullable: false),
                    Baslangic = table.Column<TimeSpan>(type: "time", nullable: false),
                    Bitis = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalismaSaatleri", x => x.Id);
                    table.CheckConstraint("CK_CalismaSaati_Aralik", "[Baslangic] < [Bitis]");
                    table.ForeignKey(
                        name: "FK_CalismaSaatleri_Hastaneler_HastaneId",
                        column: x => x.HastaneId,
                        principalTable: "Hastaneler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Poliklinikler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HastaneId = table.Column<int>(type: "int", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Poliklinikler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Poliklinikler_Hastaneler_HastaneId",
                        column: x => x.HastaneId,
                        principalTable: "Hastaneler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Hizmetler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SureDakika = table.Column<int>(type: "int", nullable: false),
                    Ucret = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    PoliklinikId = table.Column<int>(type: "int", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hizmetler", x => x.Id);
                    table.CheckConstraint("CK_Hizmet_SureDakika", "[SureDakika] > 0");
                    table.CheckConstraint("CK_Hizmet_Ucret", "[Ucret] >= 0");
                    table.ForeignKey(
                        name: "FK_Hizmetler_Poliklinikler_PoliklinikId",
                        column: x => x.PoliklinikId,
                        principalTable: "Poliklinikler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kisiler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcKimlikNo = table.Column<string>(type: "nchar(11)", fixedLength: true, maxLength: 11, nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SifreHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    KisiTipi = table.Column<int>(type: "int", nullable: false),
                    IseBaslamaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HastaneId = table.Column<int>(type: "int", nullable: true),
                    Unvan = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    UzmanlikAlani = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PoliklinikId = table.Column<int>(type: "int", nullable: true),
                    Gorev = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DogumTarihi = table.Column<DateTime>(type: "date", nullable: true),
                    Cinsiyet = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    KanGrubu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kisiler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kisiler_Hastaneler_HastaneId",
                        column: x => x.HastaneId,
                        principalTable: "Hastaneler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kisiler_Poliklinikler_PoliklinikId",
                        column: x => x.PoliklinikId,
                        principalTable: "Poliklinikler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DoktorHizmetler",
                columns: table => new
                {
                    DoktorId = table.Column<int>(type: "int", nullable: false),
                    HizmetId = table.Column<int>(type: "int", nullable: false),
                    OzelUcret = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoktorHizmetler", x => new { x.DoktorId, x.HizmetId });
                    table.ForeignKey(
                        name: "FK_DoktorHizmetler_Hizmetler_HizmetId",
                        column: x => x.HizmetId,
                        principalTable: "Hizmetler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoktorHizmetler_Kisiler_DoktorId",
                        column: x => x.DoktorId,
                        principalTable: "Kisiler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Musaitlikler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    DoktorId = table.Column<int>(type: "int", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gun = table.Column<int>(type: "int", nullable: false),
                    Baslangic = table.Column<TimeSpan>(type: "time", nullable: false),
                    Bitis = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Musaitlikler", x => x.Id);
                    table.CheckConstraint("CK_Musaitlik_Aralik", "[Baslangic] < [Bitis]");
                    table.ForeignKey(
                        name: "FK_Musaitlikler_Kisiler_DoktorId",
                        column: x => x.DoktorId,
                        principalTable: "Kisiler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Randevular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HastaId = table.Column<int>(type: "int", nullable: false),
                    DoktorId = table.Column<int>(type: "int", nullable: false),
                    HizmetId = table.Column<int>(type: "int", nullable: false),
                    Baslangic = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Bitis = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SureDakika = table.Column<int>(type: "int", nullable: false),
                    Ucret = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    OnayTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OnaylayanId = table.Column<int>(type: "int", nullable: true),
                    IptalRedNedeni = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HastaNotu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Randevular", x => x.Id);
                    table.CheckConstraint("CK_Randevu_Aralik", "[Baslangic] < [Bitis]");
                    table.ForeignKey(
                        name: "FK_Randevular_Hizmetler_HizmetId",
                        column: x => x.HizmetId,
                        principalTable: "Hizmetler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Randevular_Kisiler_DoktorId",
                        column: x => x.DoktorId,
                        principalTable: "Kisiler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Randevular_Kisiler_HastaId",
                        column: x => x.HastaId,
                        principalTable: "Kisiler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Randevular_Kisiler_OnaylayanId",
                        column: x => x.OnaylayanId,
                        principalTable: "Kisiler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalismaSaatleri_HastaneId_Gun",
                table: "CalismaSaatleri",
                columns: new[] { "HastaneId", "Gun" });

            migrationBuilder.CreateIndex(
                name: "IX_DoktorHizmetler_HizmetId",
                table: "DoktorHizmetler",
                column: "HizmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Hizmetler_PoliklinikId",
                table: "Hizmetler",
                column: "PoliklinikId");

            migrationBuilder.CreateIndex(
                name: "IX_Kisiler_Email",
                table: "Kisiler",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kisiler_HastaneId",
                table: "Kisiler",
                column: "HastaneId");

            migrationBuilder.CreateIndex(
                name: "IX_Kisiler_PoliklinikId",
                table: "Kisiler",
                column: "PoliklinikId");

            migrationBuilder.CreateIndex(
                name: "IX_Kisiler_TcKimlikNo",
                table: "Kisiler",
                column: "TcKimlikNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Musaitlikler_DoktorId_Gun",
                table: "Musaitlikler",
                columns: new[] { "DoktorId", "Gun" });

            migrationBuilder.CreateIndex(
                name: "IX_Poliklinikler_HastaneId_Ad",
                table: "Poliklinikler",
                columns: new[] { "HastaneId", "Ad" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_DoktorId_Baslangic",
                table: "Randevular",
                columns: new[] { "DoktorId", "Baslangic" });

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_Durum",
                table: "Randevular",
                column: "Durum");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_HastaId_Baslangic",
                table: "Randevular",
                columns: new[] { "HastaId", "Baslangic" });

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_HizmetId",
                table: "Randevular",
                column: "HizmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_OnaylayanId",
                table: "Randevular",
                column: "OnaylayanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalismaSaatleri");

            migrationBuilder.DropTable(
                name: "DoktorHizmetler");

            migrationBuilder.DropTable(
                name: "Musaitlikler");

            migrationBuilder.DropTable(
                name: "Randevular");

            migrationBuilder.DropTable(
                name: "Hizmetler");

            migrationBuilder.DropTable(
                name: "Kisiler");

            migrationBuilder.DropTable(
                name: "Poliklinikler");

            migrationBuilder.DropTable(
                name: "Hastaneler");
        }
    }
}
