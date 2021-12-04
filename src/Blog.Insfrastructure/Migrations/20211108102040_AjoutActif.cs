using Microsoft.EntityFrameworkCore.Migrations;

namespace Blog.Insfrastructure.Migrations
{
    public partial class AjoutActif : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "Auteurs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actif",
                table: "Auteurs");
        }
    }
}
