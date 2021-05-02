using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seatown.Data.Validation
{

    public interface IValidationResult
    {
        object Source { get; }
        bool IsValid { get; }
        IList<IValidationEntry> ValidationEntries { get; }
    }

    public interface IValidationResult<T> : IValidationResult
    {
        T Result { get; }
    }

}
