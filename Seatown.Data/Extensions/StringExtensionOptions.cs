using System;
using System.Collections.Generic;
using System.Globalization;

namespace Seatown.Data.Extensions
{
    public static class StringExtensionOptions
    {
        public static CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;
        public static List<string> DateFormats { get; } = new List<string>() { "yyMMdd", "yyyyMMdd", "MMddyyyy", "MM/dd/yyyy", "MM/dd/yy" };
        public static bool DecimalsToIntegers { get; set; } = true;
        public static bool DecimalsToIntegersRounding { get; set; } = true;
        public static List<string> FalseStrings { get; } = new List<string>() { "false, no, 0" };
        public static NumberStyles NumberStyle { get; set; } = NumberStyles.Any;
        public static StringComparison StringComparison { get; set; } = StringComparison.OrdinalIgnoreCase;
        public static List<string> TrueStrings { get; } = new List<string>() { "true, yes, 1" };
    }
}
