using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HastaneOtomasyon.Data.Migrations
{
    /// <inheritdoc />
    public partial class HesapGuvenligi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AktifMi",
                table: "Kisiler",
                type: "bit",
                nullable: false,
                defaultValue: true); // mevcut kullanıcılar aktif kalır

            migrationBuilder.AddColumn<Guid>(
                name: "GuvenlikDamgasi",
                table: "Kisiler",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()"); // mevcut her kullanıcıya ayrı damga

            migrationBuilder.UpdateData(
                table: "Kisiler",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AktifMi", "GuvenlikDamgasi" },
                values: new object[] { true, new Guid("a6e9f98a-1598-400a-a34a-c019640bcf19") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AktifMi",
                table: "Kisiler");

            migrationBuilder.DropColumn(
                name: "GuvenlikDamgasi",
                table: "Kisiler");
        }
    }
}
