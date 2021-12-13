using System;
using System.Linq;

namespace ExpensesServiceTests.Utility
{
    internal static class RandomData
    {
        public static class Double
        {
            public static double Positive()
            {
                return new Random().NextDouble() * int.MaxValue;
            }

            public static double Negative()
            {
                var positive = Positive();
                return positive == 0 ? -1 : -positive;
            }
        }

        public static class Integer
        {
            public static int Between(int min, int max)
            {
                return new Random().Next(min, max);
            }

            public static int LessThan(int max)
            {
                return new Random().Next(max);
            }

            public static int Positive()
            {
                return new Random().Next(int.MaxValue);
            }

            public static int Negative()
            {
                var positive = Positive();
                return positive == 0 ? -1 : -positive;
            }
        }

        public static class TimeSpan
        {
            public static System.TimeSpan Days()
            {
                return System.TimeSpan.FromDays(new Random().NextDouble());
            }
        }

        public static class String
        {
            public static string Alphanumeric()
            {
                return new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", 10).Select(s => s[new Random().Next(s.Length)]).ToArray());
            }
        }

        public static class DateTimeOffset
        {
            public static System.DateTimeOffset Past()
            {
                return System.DateTimeOffset.UtcNow.Subtract(System.TimeSpan.FromDays(new Random().NextDouble()));
            }

            public static System.DateTimeOffset Future()
            {
                return System.DateTimeOffset.UtcNow.Add(System.TimeSpan.FromDays(new Random().NextDouble()));
            }
        }

        public static class Enum
        {
            public static T Any<T>()
            {
                if (!typeof(T).IsEnum)
                {
                    throw new ArgumentException("T must be an enumerated type");
                }

                var values = System.Enum.GetValues(typeof(T));
                return (T)values.GetValue(Integer.Between(0, values.Length - 1));
            }

            public static T AnyExcept<T>(params T[] excludedValues) where T : struct
            {
                if (!typeof(T).IsEnum)
                {
                    throw new ArgumentException("T must be an enumerated type");
                }

                var array = System.Enum.GetValues(typeof(T)).Cast<T>().Except(excludedValues).ToArray();

                return (T)array.GetValue(Integer.Between(0, array.Length - 1));
            }
        }
    }
}
