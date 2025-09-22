namespace Shared.Kernel.Helpers
{
    public static class BrazilTime
    {
        private static readonly TimeZoneInfo BrazilTimeZone = 
            GetBrazilTimeZone();

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BrazilTimeZone);

        private static TimeZoneInfo GetBrazilTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            }
        }
    }
}