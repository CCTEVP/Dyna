using System;
namespace Dyna.Player.Utilities
{
    public static class DateTimeHelpers
    {
        public static int GetDateTimePart(string part)
        {
            var now = DateTime.Now;
            switch (part.ToLower())
            {
                case "days":
                    return now.Day;
                case "hours":
                    return now.Hour;
                case "minutes":
                    return now.Minute;
                case "seconds":
                    return now.Second;
                default:
                    return 0; // Or throw an exception for invalid parts
            }
        }
    }
}

