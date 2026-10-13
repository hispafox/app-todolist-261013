using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTodoList.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tareas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    Prioridad = table.Column<int>(type: "INTEGER", nullable: false),
                    CreadaEn = table.Column<long>(type: "INTEGER", nullable: false),
                    ActualizadaEn = table.Column<long>(type: "INTEGER", nullable: false),
                    VenceEn = table.Column<long>(type: "INTEGER", nullable: true),
                    TextoBusqueda = table.Column<string>(type: "TEXT", maxLength: 2201, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tareas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_Estado_CreadaEn",
                table: "Tareas",
                columns: new[] { "Estado", "CreadaEn" });

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_Prioridad_CreadaEn",
                table: "Tareas",
                columns: new[] { "Prioridad", "CreadaEn" });

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_VenceEn",
                table: "Tareas",
                column: "VenceEn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tareas");
        }
    }
}
