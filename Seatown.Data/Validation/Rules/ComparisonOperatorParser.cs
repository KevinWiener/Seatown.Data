using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seatown.Data.Validation.Rules
{
    public class ComparisonOperatorParser
    {

        public static ComparisonOperators Parse(string value)
        {
            var result = ComparisonOperators.None;

            if (ParseContains(value))
                result = ComparisonOperators.Contains;
            else if (ParseDoesNotEqual(value))
                result = ComparisonOperators.DoesNotEqual;
            else if (ParseEndsWith(value))
                result = ComparisonOperators.EndsWith;
            else if (ParseEquals(value))
                result = ComparisonOperators.Equals;
            else if (ParseGreaterThan(value))
                result = ComparisonOperators.GreaterThan;
            else if (ParseGreaterThanOrEqual(value))
                result = ComparisonOperators.GreaterThanOrEqual;
            else if (ParseLessThan(value))
                result = ComparisonOperators.LessThan;
            else if (ParseLessThanOrEqual(value))
                result = ComparisonOperators.LessThanOrEqual;
            else if (ParseStartsWith(value))
                result = ComparisonOperators.StartsWith;

            return result;
        }

        private static bool ParseContains(string value)
        {
            var result = false;
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = (
                    value.Equals("1") ||
                    value.Equals("Contains", StringComparison.CurrentCultureIgnoreCase)
                    );
            }
            return result;
        }

        private static bool ParseDoesNotEqual(string value)
        {
            var result = false;
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = (
                    value.Equals("<>") ||
                    value.Equals("!=") ||
                    value.Equals("2") ||
                    value.Equals("DoesNotEqual", StringComparison.CurrentCultureIgnoreCase)
                    );
            }
            return result;
        }

        private static bool ParseEndsWith(string value)
        {
            var result = false;
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = (
                    value.Equals("4") ||
                    value.Equals("EndsWith", StringComparison.CurrentCultureIgnoreCase)
                    );
            }
            return result;
        }

        private static bool ParseEquals(string value)
        {
            var result = false;
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = (
                    value.Equals("=") ||
                    value.Equals("==") ||
                    value.Equals("8") ||
                    value.Equals("Equals", StringComparison.CurrentCultureIgnoreCase)
                    );
            }
            return result;
        }

        private static bool ParseGreaterThan(string value)
        {
            var result = false;
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = (
                    value.Equals(">") ||
                    value.Equals("16") ||
                    value.Equals("GreaterThan", StringComparison.CurrentCultureIgnoreCase)
                    );
            }
            return result;
        }

        private static bool ParseGreaterThanOrEqual(string value)
        {
            var result = false;
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = (
                    value.Equals(">=") ||
                    value.Equals("32") ||
                    value.Equals("GreaterThanOrEqual", StringComparison.CurrentCultureIgnoreCase)
                    );
            }
            return result;
        }

        private static bool ParseLessThan(string value)
        {
            var result = false;
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = (
                    value.Equals("<") ||
                    value.Equals("64") ||
                    value.Equals("LessThan", StringComparison.CurrentCultureIgnoreCase)
                    );
            }
            return result;
        }

        private static bool ParseLessThanOrEqual(string value)
        {
            var result = false;
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = (
                    value.Equals("<=") ||
                    value.Equals("128") ||
                    value.Equals("LessThanOrEqual", StringComparison.CurrentCultureIgnoreCase)
                    );
            }
            return result;
        }

        private static bool ParseStartsWith(string value)
        {
            var result = false;
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = (
                    value.Equals("256") ||
                    value.Equals("StartsWith", StringComparison.CurrentCultureIgnoreCase)
                    );
            }
            return result;
        }

    }
}
