using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HastaneOtomasyon.Data.Migrations
{
    /// <inheritdoc />
    public partial class SistemYoneticisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Kisiler",
                columns: new[] { "Id", "Ad", "Email", "GuncellemeTarihi", "KisiTipi", "OlusturmaTarihi", "SifreHash", "Soyad", "TcKimlikNo", "Telefon" },
                values: new object[] { 1, "Sistem", "admin@hastane.local", null, 5, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), "100000.umQQHF/mQNVupUfcYdQhBg==.lhe8lsA0rlUwlub9rCfJCEBaBihmStrWPYvp/52tF9k=", "Yöneticisi", "10000000146", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Kisiler",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
