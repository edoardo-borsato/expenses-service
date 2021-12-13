using System;

namespace ExpensesService.Utility
{
    public class Watch : IWatch
    {
        public DateTimeOffset Now()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}