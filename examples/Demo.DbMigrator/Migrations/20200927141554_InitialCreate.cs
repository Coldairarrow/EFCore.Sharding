using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Demo.DbMigrator.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Order_202009272210",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_202009272210", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Order_202009272211",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_202009272211", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Order_202009272212",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_202009272212", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Order_202009272213",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_202009272213", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Order_202009272214",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_202009272214", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Order_202009272215",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_202009272215", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Order_202009272216",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_202009272216", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Order_202009272217",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_202009272217", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OrderId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItem",
                column: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "Order_202009272210");

            migrationBuilder.DropTable(
                name: "Order_202009272211");

            migrationBuilder.DropTable(
                name: "Order_202009272212");

            migrationBuilder.DropTable(
                name: "Order_202009272213");

            migrationBuilder.DropTable(
                name: "Order_202009272214");

            migrationBuilder.DropTable(
                name: "Order_202009272215");

            migrationBuilder.DropTable(
                name: "Order_202009272216");

            migrationBuilder.DropTable(
                name: "Order_202009272217");
        }
    }
}
