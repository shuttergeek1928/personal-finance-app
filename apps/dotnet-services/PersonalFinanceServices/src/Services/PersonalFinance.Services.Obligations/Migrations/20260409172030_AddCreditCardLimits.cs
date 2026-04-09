using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalFinance.Services.Obligations.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditCardLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OutstandingAmount",
                table: "CreditCards",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "OutstandingCurrency",
                table: "CreditCards",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalLimitAmount",
                table: "CreditCards",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TotalLimitCurrency",
                table: "CreditCards",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutstandingAmount",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "OutstandingCurrency",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "TotalLimitAmount",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "TotalLimitCurrency",
                table: "CreditCards");
        }
    }
}
