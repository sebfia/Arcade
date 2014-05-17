using System;

namespace Arcade.Dsl
{
    public static class ExtendsTimeSpan
    {
        public static TimeSpan Seconds(this int value)
        {
            return TimeSpan.FromSeconds(value);
        }
        
        public static TimeSpan Minutes(this int value)
        {
            return TimeSpan.FromMinutes(value);
        }
        
        public static TimeSpan Hours(this int value)
        {
            return TimeSpan.FromHours(value);
        }
        
        public static TimeSpan Days(this int value)
        {
            return TimeSpan.FromDays(value);
        }
    }
}