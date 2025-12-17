using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nawel.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "family",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_family", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    login = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    pwd = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    first_name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    last_name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    avatar = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    pseudo = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    notify_list_edit = table.Column<bool>(type: "INTEGER", nullable: false),
                    notify_gift_taken = table.Column<bool>(type: "INTEGER", nullable: false),
                    display_popup = table.Column<bool>(type: "INTEGER", nullable: false),
                    reset_token = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    token_expiry = table.Column<DateTime>(type: "TEXT", nullable: true),
                    isChildren = table.Column<bool>(type: "INTEGER", nullable: false),
                    is_admin = table.Column<bool>(type: "INTEGER", nullable: false),
                    family_id = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_family_family_id",
                        column: x => x.family_id,
                        principalTable: "family",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lists",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lists", x => x.id);
                    table.ForeignKey(
                        name: "FK_lists_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "opengraph_request",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    url = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    success = table.Column<bool>(type: "INTEGER", nullable: false),
                    error_message = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opengraph_request", x => x.id);
                    table.ForeignKey(
                        name: "FK_opengraph_request_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gifts",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    list_id = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    image = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    link = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    cost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    available = table.Column<bool>(type: "INTEGER", nullable: false),
                    taken_by = table.Column<int>(type: "INTEGER", nullable: true),
                    is_group_gift = table.Column<bool>(type: "INTEGER", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gifts", x => x.id);
                    table.ForeignKey(
                        name: "FK_gifts_lists_list_id",
                        column: x => x.list_id,
                        principalTable: "lists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gifts_user_taken_by",
                        column: x => x.taken_by,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "gift_participation",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    gift_id = table.Column<int>(type: "INTEGER", nullable: false),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gift_participation", x => x.id);
                    table.ForeignKey(
                        name: "FK_gift_participation_gifts_gift_id",
                        column: x => x.gift_id,
                        principalTable: "gifts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gift_participation_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_family_name",
                table: "family",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_gift_participation_gift_id_user_id",
                table: "gift_participation",
                columns: new[] { "gift_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "IX_gift_participation_user_id",
                table: "gift_participation",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_gifts_list_id",
                table: "gifts",
                column: "list_id");

            migrationBuilder.CreateIndex(
                name: "IX_gifts_taken_by",
                table: "gifts",
                column: "taken_by");

            migrationBuilder.CreateIndex(
                name: "IX_gifts_year",
                table: "gifts",
                column: "year");

            migrationBuilder.CreateIndex(
                name: "IX_lists_user_id",
                table: "lists",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_opengraph_request_created_at",
                table: "opengraph_request",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_opengraph_request_user_id",
                table: "opengraph_request",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_email",
                table: "user",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_user_family_id",
                table: "user",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_login",
                table: "user",
                column: "login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gift_participation");

            migrationBuilder.DropTable(
                name: "opengraph_request");

            migrationBuilder.DropTable(
                name: "gifts");

            migrationBuilder.DropTable(
                name: "lists");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "family");
        }
    }
}
