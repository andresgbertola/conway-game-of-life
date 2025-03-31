using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GOL.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoardStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StateHash = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Iteration = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardStates", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardStates_BoardId_Iteration",
                table: "BoardStates",
                columns: new[] { "BoardId", "Iteration" },
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_BoardStates_BoardId_StateHash_Iteration",
                table: "BoardStates",
                columns: new[] { "BoardId", "StateHash", "Iteration" })
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardStates");
        }
    }
}
