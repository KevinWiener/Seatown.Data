using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seatown.Data.Validation
{
    public interface IValidator<T>
    {
        IValidationResult Validate(T objectToValidate);
    }
}
