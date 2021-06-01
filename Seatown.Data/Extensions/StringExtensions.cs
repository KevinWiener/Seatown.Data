using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Seatown.Data.Extensions
{
    /// <summary>
    /// Class to add extension methods to string
    /// </summary>
    public static class StringExtensions
    {

        /// <summary>
        /// Removes non-printable characters from the source string.
        /// </summary>
        /// <param name="source">The string to evaulate.</param>
        /// <returns>The source string without any non-printable characters.</returns>
        public static string RemoveNonPrintableCharacters(this string source)
        {
            // https://stackoverflow.com/questions/40564692/c-sharp-regex-to-remove-non-printable-characters-and-control-characters-in-a
            string result = string.Empty;
            if (!string.IsNullOrWhiteSpace(source))
            {
                result = System.Text.RegularExpressions.Regex.Replace(source, @"\p{C}+", string.Empty);
            }
            return result;
        }

        /// <summary>
        /// Converts a string to the specified data type.
        /// </summary>
        /// <typeparam name="T">The target data type.</typeparam>
        /// <param name="source">The string to convert.</param>
        /// <param name="defaultValue">An optional default value of the target type that will be returned if the string cannot be converted.</param>
        /// <param name="cultureInfo">The culture information that will be used when converting to dates, times, and numbers.</param>
        /// <param name="numberStyle">The number style that will be used during numeric conversions.</param>
        /// <returns>The string converted to the target type, the caller provided default value, or the the target data types default value.</returns>
        public static T ToType<T>(this string source)
        {
            return ToType<T>(source, default, StringExtensionOptions.CultureInfo, StringExtensionOptions.NumberStyle);
        }

        /// <summary>
        /// Converts a string to the specified data type.
        /// </summary>
        /// <typeparam name="T">The target data type.</typeparam>
        /// <param name="source">The string to convert.</param>
        /// <param name="defaultValue">An optional default value of the target type that will be returned if the string cannot be converted.</param>
        /// <returns>The string converted to the target type, the caller provided default value, or the the target data types default value.</returns>
        public static T ToType<T>(this string source, T defaultValue)
        {
            return ToType<T>(source, defaultValue, StringExtensionOptions.CultureInfo, StringExtensionOptions.NumberStyle);
        }

        /// <summary>
        /// Converts a string to the specified data type.
        /// </summary>
        /// <typeparam name="T">The target data type.</typeparam>
        /// <param name="source">The string to convert.</param>
        /// <param name="defaultValue">An optional default value of the target type that will be returned if the string cannot be converted.</param>
        /// <param name="cultureInfo">The culture information that will be used when converting to dates, times, and numbers.</param>
        /// <returns>The string converted to the target type, the caller provided default value, or the the target data types default value.</returns>
        public static T ToType<T>(this string source, T defaultValue, CultureInfo cultureInfo)
        {
            return ToType<T>(source, defaultValue, cultureInfo, StringExtensionOptions.NumberStyle);
        }

        /// <summary>
        /// Converts a string to the specified data type.
        /// </summary>
        /// <typeparam name="T">The target data type.</typeparam>
        /// <param name="source">The string to convert.</param>
        /// <param name="defaultValue">An optional default value of the target type that will be returned if the string cannot be converted.</param>
        /// <param name="cultureInfo">The culture information that will be used when converting to dates, times, and numbers.</param>
        /// <param name="numberStyle">The number style that will be used during numeric conversions.</param>
        /// <returns>The string converted to the target type, the caller provided default value, or the the target data types default value.</returns>
        public static T ToType<T>(this string source, T defaultValue, CultureInfo cultureInfo, NumberStyles numberStyle)
        {
            T result = defaultValue;
            var comparer = StringExtensionOptions.StringComparison;
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (cultureInfo == null) cultureInfo = StringExtensionOptions.CultureInfo;

            var stringToEvaluate = source.RemoveNonPrintableCharacters();
            if (!string.IsNullOrWhiteSpace(stringToEvaluate))
            {
                if (typeof(bool).Equals(targetType))
                {
                    // We allow converting non-boolean values like yes/no.
                    if (StringExtensionOptions.FalseStrings.Any(o => o.Equals(stringToEvaluate, comparer)))
                    {
                        result = (T)(object)false;
                    }
                    else if (StringExtensionOptions.TrueStrings.Any(o => o.Equals(stringToEvaluate, comparer)))
                    {
                        result = (T)(object)true;
                    }
                    else if (bool.TryParse(stringToEvaluate, out bool val))
                    {
                        result = (T)(object)val;
                    }
                }
                else if (typeof(byte).Equals(targetType))
                {
                    if (byte.TryParse(stringToEvaluate, numberStyle, cultureInfo, out byte val))
                    {
                        result = (T)(object)val;
                    }
                }
                else if (typeof(DateTime).Equals(targetType))
                {
                    DateTime val;
                    if (DateTime.TryParseExact(stringToEvaluate, StringExtensionOptions.DateFormats.ToArray(), cultureInfo.DateTimeFormat, DateTimeStyles.None, out val))
                    {
                        result = (T)(object)val;
                    }
                    else if (DateTime.TryParse(stringToEvaluate, cultureInfo.DateTimeFormat, DateTimeStyles.None, out val))
                    {
                        result = (T)(object)val;
                    }
                }
                else if (typeof(decimal).Equals(targetType))
                {
                    if (decimal.TryParse(stringToEvaluate, numberStyle, cultureInfo, out decimal val))
                    {
                        result = (T)(object)val;
                    }
                }
                else if (typeof(double).Equals(targetType))
                {
                    if (double.TryParse(stringToEvaluate, numberStyle, cultureInfo, out double val))
                    {
                        result = (T)(object)val;
                    }
                }
                else if (typeof(int).Equals(targetType))
                {
                    // We optionally allow parsing integers from decimal values.
                    if (decimal.TryParse(stringToEvaluate, numberStyle, cultureInfo, out decimal val))
                    {
                        if (val >= int.MinValue && val <= int.MaxValue)
                        {
                            if (val % 1 != 0)
                            {
                                if (StringExtensionOptions.DecimalsToIntegers)
                                {
                                    result = (T)(object)(int)(
                                        StringExtensionOptions.DecimalsToIntegersRounding ?
                                        Math.Round(val) :
                                        val);
                                }
                            }
                            else
                            {
                                result = (T)(object)(int)val;
                            }
                        }
                    }
                }
                else if (typeof(float).Equals(targetType))
                {
                    if (float.TryParse(stringToEvaluate, numberStyle, cultureInfo, out float val))
                    {
                        result = (T)(object)val;
                    }
                }
                else if (typeof(long).Equals(targetType))
                {
                    // We optionally allow parsing longs from decimal values.
                    if (decimal.TryParse(stringToEvaluate, numberStyle, cultureInfo, out decimal val))
                    {
                        if (val >= long.MinValue && val <= long.MaxValue)
                        {
                            if (val % 1 != 0)
                            {
                                if (StringExtensionOptions.DecimalsToIntegers)
                                {
                                    result = (T)(object)(long)(
                                        StringExtensionOptions.DecimalsToIntegersRounding ?
                                        Math.Round(val) :
                                        val);
                                }
                            }
                            else
                            {
                                result = (T)(object)(long)val;
                            }
                        }
                    }
                }
                else if (typeof(short).Equals(targetType))
                {
                    // We optionally allow parsing shorts from decimal values.
                    if (decimal.TryParse(stringToEvaluate, numberStyle, cultureInfo, out decimal val))
                    {
                        if (val >= short.MinValue && val <= short.MaxValue)
                        {
                            if (val % 1 != 0)
                            {
                                if (StringExtensionOptions.DecimalsToIntegers)
                                {
                                    result = (T)(object)(short)(
                                        StringExtensionOptions.DecimalsToIntegersRounding ?
                                        Math.Round(val) :
                                        val);
                                }
                            }
                            else
                            {
                                result = (T)(object)(short)val;
                            }
                        }
                    }
                }
                else if (typeof(Guid).Equals(targetType))
                {
                    if (Guid.TryParse(stringToEvaluate, out Guid val))
                    {
                        result = (T)(object)val;
                    }
                }
                else if (targetType.IsEnum)
                {
                    // We can do enum parsing by case sensitive or insensitive name, and numeric value.
                    // We can't use the default Enum.TryParse<T> method because it has the where T : struct restriction.
                    var nameMatches = Enum.GetNames(targetType).Where(o => o.Equals(stringToEvaluate, comparer));
                    if (nameMatches != null && nameMatches.Count().Equals(1))
                    {
                        result = (T)Enum.Parse(targetType, nameMatches.First());
                    }
                    else
                    {
                        foreach (var val in Enum.GetValues(targetType))
                        {
                            if (val.ToString().Equals(stringToEvaluate))
                            {
                                result = (T)val;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    var converter = System.ComponentModel.TypeDescriptor.GetConverter(targetType);
                    if (converter != null)
                    {
                        if (converter.IsValid(stringToEvaluate))
                        {
                            result = (T)converter.ConvertFromString(stringToEvaluate);
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"Cannot convert the type {targetType}.");
                    }
                }
            }
            return result;
        }

    }
}
