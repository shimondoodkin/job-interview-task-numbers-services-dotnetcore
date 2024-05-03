using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceA.Migrations
{
    /// <inheritdoc />
    public partial class addprocessedfieldtomessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Processed",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Processed",
                table: "Messages");
        }
    }
}
