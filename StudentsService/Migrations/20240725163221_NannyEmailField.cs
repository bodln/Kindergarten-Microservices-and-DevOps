using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentsService.Migrations
{
    /// <inheritdoc />
    public partial class NannyEmailField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NannyEmail",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NannyEmail",
                table: "Students");
        }
    }
}
