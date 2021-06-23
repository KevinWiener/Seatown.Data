using System;
using System.Collections.Immutable;
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
                ImmutableList.Create<IValidationEntry>(validationEntries) :
                ImmutableList.Create<IValidationEntry>();
        }
        public bool IsValid { get; private set; }
        public object Source { get; private set; }
        public IImmutableList<IValidationEntry> ValidationEntries { get; private set; }
    }

    public class ValidationResult<T> : IValidationResult<T>
    {
        public ValidationResult(bool isValid, object source, T result, params IValidationEntry[] validationEntries)
        {
            this.IsValid = isValid;
            this.Source = source;
            this.Result = result;
            this.ValidationEntries = (validationEntries != null) ?
                ImmutableList.Create<IValidationEntry>(validationEntries) :
                ImmutableList.Create<IValidationEntry>();
        }
        public bool IsValid { get; private set; }
        public T Result { get; private set; }
        public object Source { get; private set; }
        public IImmutableList<IValidationEntry> ValidationEntries { get; private set; }
    }

}
