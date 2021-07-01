using System;

namespace Consumer
{
    public class DateTimeFactory
    {
        public static Func<DateTime> Now = () => DateTime.UtcNow;
    }
}
