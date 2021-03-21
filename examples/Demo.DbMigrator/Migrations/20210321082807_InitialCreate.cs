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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: ""),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false, comment: ""),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "主键"),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "创建时间"),
                    OrderNum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "订单号"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "订单名"),
                    Count = table.Column<int>(type: "int", nullable: false, comment: "商品数量"),
                    OrderType = table.Column<int>(type: "int", nullable: false, comment: "订单类型 0=未知 1=正常")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "主键"),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "订单Id")
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
                name: "IX_Order_OrderNum",
                table: "Order",
                column: "OrderNum",
                unique: true,
                filter: "[OrderNum] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItem",
                column: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "Order");
        }
    }
}
