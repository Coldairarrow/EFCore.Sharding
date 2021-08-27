using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Demo.DbMigrator.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: ""),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: ""),
                    Content = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false, comment: "主键"),
                    Tags = table.Column<string[]>(type: "text[]", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "主键"),
                    OrderId = table.Column<string>(type: "text", nullable: true, comment: "订单Id")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_CreateTime",
                table: "AuditLog",
                column: "CreateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Tags",
                table: "Order",
                column: "Tags");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "OrderItem");
        }
    }
}
