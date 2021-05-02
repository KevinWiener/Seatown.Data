using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seatown.Data.Validation
{

    public class ValidationResult : IValidationResult
    {
        public ValidationResult(bool isValid, object source, params IValidationEntry[] validationEntries)
        {
            this.IsValid = isValid;
            this.Source = source;
            this.ValidationEntries = (validationEntries != null) ?
                new List<IValidationEntry>(validationEntries) :
                new List<IValidationEntry>();
        }
        public bool IsValid { get; private set; }
        public object Source { get; private set; }
        public IList<IValidationEntry> ValidationEntries { get; private set; }
    }

    public class ValidationResult<T> : IValidationResult<T>
    {
        public ValidationResult(bool isValid, object source, T result, params IValidationEntry[] validationEntries)
        {
            this.IsValid = isValid;
            this.Source = source;
            this.Result = result;
            this.ValidationEntries = (validationEntries != null) ?
                new List<IValidationEntry>(validationEntries) :
                new List<IValidationEntry>();
        }
        public bool IsValid { get; private set; }
        public T Result { get; private set; }
        public object Source { get; private set; }
        public IList<IValidationEntry> ValidationEntries { get; private set; }
    }

}
