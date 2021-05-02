using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seatown.Data.Validation
{
    public interface IValidationEntry
    {
        string Field { get; }
        IList<string> ValidationMessages { get; }
    }
}
