using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Seatown.Data.Validation
{
    public interface IValidationEntry
    {
        string Field { get; }
        IImmutableList<string> ValidationMessages { get; }        
    }
}
