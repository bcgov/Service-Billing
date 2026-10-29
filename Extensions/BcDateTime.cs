namespace Service_Billing.Extensions
{
    public static class BcDateTime
    {
        private static readonly TimeZoneInfo BcTimeZone = ResolveBcTimeZone();

        public static TimeZoneInfo TimeZone => BcTimeZone;

        public static DateTimeOffset UtcNow() => DateTimeOffset.UtcNow;

        public static DateTimeOffset BcNow() => TimeZoneInfo.ConvertTime(UtcNow(), BcTimeZone);

        public static DateTimeOffset ConvertToBc(DateTimeOffset value) => TimeZoneInfo.ConvertTime(value, BcTimeZone);

        public static DateTime ConvertUtcToBc(DateTime utcDateTime) => TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, BcTimeZone);

        private static TimeZoneInfo ResolveBcTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Vancouver");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            }
            catch (InvalidTimeZoneException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            }
        }
    }
}
