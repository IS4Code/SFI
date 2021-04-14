using System;
using System.Globalization;

namespace IS4.MultiArchiver
{
    static class DataTools
    {
        static readonly string[] units = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };

        public static string SizeSuffix(long value, int decimalPlaces)
        {
            if(value < 0) return "-" + SizeSuffix(-value, decimalPlaces);
            if(value == 0) return String.Format(CultureInfo.InvariantCulture, $"{{0:0.{new string('#', decimalPlaces)}}} B", 0);

            int n = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (n * 10));
            if(Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                n += 1;
                adjustedSize /= 1024;
            }
            return String.Format(CultureInfo.InvariantCulture, $"{{0:0.{new string('#', decimalPlaces)}}} {{1}}", adjustedSize, units[n]);
        }
    }
}
