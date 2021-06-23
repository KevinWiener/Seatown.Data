using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Seatown.Data.Validation
{

    public interface IValidationResult
    {
        object Source { get; }
        bool IsValid { get; }
        IImmutableList<IValidationEntry> ValidationEntries { get; }
    }

    public interface IValidationResult<T> : IValidationResult
    {
        T Result { get; }
    }

}
