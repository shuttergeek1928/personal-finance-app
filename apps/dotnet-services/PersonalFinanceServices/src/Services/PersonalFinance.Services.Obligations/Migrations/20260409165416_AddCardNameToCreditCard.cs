using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalFinance.Services.Obligations.Migrations
{
    /// <inheritdoc />
    public partial class AddCardNameToCreditCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardName",
                table: "CreditCards",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardName",
                table: "CreditCards");
        }
    }
}
