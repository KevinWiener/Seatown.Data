using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seatown.Data.Validation.Rules
{
    public enum ComparisonOperators
    {
        None = 0,
        Contains = 1,
        DoesNotEqual = 2,
        EndsWith = 4,
        Equals = 8,
        GreaterThan = 16,
        GreaterThanOrEqual = 32,
        LessThan = 64,
        LessThanOrEqual = 128,
        StartsWith = 256
    }
}
