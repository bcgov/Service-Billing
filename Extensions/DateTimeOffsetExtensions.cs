namespace Service_Billing.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static string FormatDateTimeOffsetInPst(DateTimeOffset? dateTimeOffset, string format = "MM/dd/yyyy hh:mm:ss tt", string nullValue = "")
        {
            if (!dateTimeOffset.HasValue)
            {
                return nullValue;
            }

            var bcTime = BcDateTime.ConvertToBc(dateTimeOffset.Value);
            return bcTime.ToString(format);
        }
    }
}
