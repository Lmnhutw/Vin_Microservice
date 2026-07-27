using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vin.Services.InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InboxMessages",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ReceivedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: false),
                    UpdatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.ProductId);
                    table.CheckConstraint("CK_InventoryItems_AvailableQuantity", "AvailableQuantity >= 0");
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProcessedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeadLetteredOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    InventoryReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.InventoryReservationId);
                });

            migrationBuilder.CreateTable(
                name: "ReservationLines",
                columns: table => new
                {
                    InventoryReservationLineId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationLines", x => x.InventoryReservationLineId);
                    table.ForeignKey(
                        name: "FK_ReservationLines_Reservations_InventoryReservationId",
                        column: x => x.InventoryReservationId,
                        principalTable: "Reservations",
                        principalColumn: "InventoryReservationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_DeadLetteredOnUtc_OccurredOnUtc",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "DeadLetteredOnUtc", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationLines_InventoryReservationId_ProductId",
                table: "ReservationLines",
                columns: new[] { "InventoryReservationId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_OrderId",
                table: "Reservations",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxMessages");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "ReservationLines");

            migrationBuilder.DropTable(
                name: "Reservations");
        }
    }
}
