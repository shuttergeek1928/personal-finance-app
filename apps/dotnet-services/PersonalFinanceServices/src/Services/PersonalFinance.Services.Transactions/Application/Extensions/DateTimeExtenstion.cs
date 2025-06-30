namespace PersonalFinance.Services.Transactions.Application.Extensions
{
    public static class DateTimeExtenstion
    {
        public static int CalculateAge(this DateTime dateTime)
        {
            var today = DateTime.Today;
            var age = today.Year - dateTime.Year;

            if (dateTime > today.AddYears(-age)) age--;

            return age;
        }
    }
}
