using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AccessFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bulk_operation",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<string>(type: "text", nullable: false),
                    users_processed = table.Column<int>(type: "integer", nullable: false),
                    successfully_processed = table.Column<int>(type: "integer", nullable: false),
                    error_processed = table.Column<int>(type: "integer", nullable: false),
                    start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bulk_operation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bulk_operation_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_bulk_operation = table.Column<long>(type: "bigint", nullable: false),
                    id_client = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bulk_operation_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_bulk_operation_items_bulk_operation_id_bulk_operation",
                        column: x => x.id_bulk_operation,
                        principalTable: "bulk_operation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bulk_operation_items_clients_id_client",
                        column: x => x.id_client,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "connections",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_client = table.Column<long>(type: "bigint", nullable: false),
                    id_external = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    connection_string = table.Column<string>(type: "text", nullable: false),
                    sub_url = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_connections", x => x.id);
                    table.ForeignKey(
                        name: "FK_connections_clients_id_client",
                        column: x => x.id_client,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_client = table.Column<long>(type: "bigint", nullable: false),
                    id_connection = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    send_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_clients_id_client",
                        column: x => x.id_client,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_connections_id_connection",
                        column: x => x.id_connection,
                        principalTable: "connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bulk_operation_items_id_client",
                table: "bulk_operation_items",
                column: "id_client");

            migrationBuilder.CreateIndex(
                name: "ux_bulk_operation_items_operation_client",
                table: "bulk_operation_items",
                columns: new[] { "id_bulk_operation", "id_client" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_clients_email",
                table: "clients",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_clients_phone_number",
                table: "clients",
                column: "phone_number");

            migrationBuilder.CreateIndex(
                name: "IX_connections_id_client",
                table: "connections",
                column: "id_client");

            migrationBuilder.CreateIndex(
                name: "ux_connections_id_external",
                table: "connections",
                column: "id_external",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_connections_name",
                table: "connections",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_connections_sub_url",
                table: "connections",
                column: "sub_url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_id_client",
                table: "notifications",
                column: "id_client");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_id_connection",
                table: "notifications",
                column: "id_connection");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bulk_operation_items");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "bulk_operation");

            migrationBuilder.DropTable(
                name: "connections");

            migrationBuilder.DropTable(
                name: "clients");
        }
    }
}
