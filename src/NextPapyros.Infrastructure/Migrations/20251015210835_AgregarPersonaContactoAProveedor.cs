using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextPapyros.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPersonaContactoAProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PersonaContacto",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PersonaContacto",
                table: "Proveedores");
        }
    }
}
