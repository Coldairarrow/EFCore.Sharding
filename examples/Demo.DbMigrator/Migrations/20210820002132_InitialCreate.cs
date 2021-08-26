using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

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
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "创建时间"),
                    OrderNum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "订单号"),
                    Name = table.Column<string>(type: "text", nullable: true, comment: "订单名"),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true, comment: "")
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "Name", "OrderNum" }),
                    Count = table.Column<int>(type: "integer", nullable: false, comment: "商品数量"),
                    OrderType = table.Column<int>(type: "integer", nullable: false, comment: "订单类型 0=未知 1=正常"),
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
                name: "IX_Order_SearchVector",
                table: "Order",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Tags",
                table: "Order",
                column: "Tags")
                .Annotation("Npgsql:IndexMethod", "GIN");

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
