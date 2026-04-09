using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalFinance.Services.Obligations.Migrations
{
    /// <inheritdoc />
    public partial class FixCreditCardCurrencyDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProcessingFeeCurrency",
                table: "Liabilities",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                defaultValue: "INR",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PrincipalCurrency",
                table: "Liabilities",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "INR",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "OutstandingBalanceCurrency",
                table: "Liabilities",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "INR",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "EmiCurrency",
                table: "Liabilities",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "INR",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "TotalLimitCurrency",
                table: "CreditCards",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "INR",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "OutstandingCurrency",
                table: "CreditCards",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "INR",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            // Manual SQL updates for existing records
            migrationBuilder.Sql("UPDATE CreditCards SET TotalLimitCurrency = 'INR' WHERE TotalLimitCurrency IS NULL OR TotalLimitCurrency = ''");
            migrationBuilder.Sql("UPDATE CreditCards SET OutstandingCurrency = 'INR' WHERE OutstandingCurrency IS NULL OR OutstandingCurrency = ''");
            migrationBuilder.Sql("UPDATE Liabilities SET ProcessingFeeCurrency = 'INR' WHERE ProcessingFeeCurrency IS NULL OR ProcessingFeeCurrency = ''");
            migrationBuilder.Sql("UPDATE Liabilities SET PrincipalCurrency = 'INR' WHERE PrincipalCurrency IS NULL OR PrincipalCurrency = ''");
            migrationBuilder.Sql("UPDATE Liabilities SET OutstandingBalanceCurrency = 'INR' WHERE OutstandingBalanceCurrency IS NULL OR OutstandingBalanceCurrency = ''");
            migrationBuilder.Sql("UPDATE Liabilities SET EmiCurrency = 'INR' WHERE EmiCurrency IS NULL OR EmiCurrency = ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProcessingFeeCurrency",
                table: "Liabilities",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true,
                oldDefaultValue: "INR");

            migrationBuilder.AlterColumn<string>(
                name: "PrincipalCurrency",
                table: "Liabilities",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "INR");

            migrationBuilder.AlterColumn<string>(
                name: "OutstandingBalanceCurrency",
                table: "Liabilities",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "INR");

            migrationBuilder.AlterColumn<string>(
                name: "EmiCurrency",
                table: "Liabilities",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "INR");

            migrationBuilder.AlterColumn<string>(
                name: "TotalLimitCurrency",
                table: "CreditCards",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "INR");

            migrationBuilder.AlterColumn<string>(
                name: "OutstandingCurrency",
                table: "CreditCards",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "INR");
        }
    }
}
