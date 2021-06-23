using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Seatown.Data.Validation
{
    public class ValidationEntry : IValidationEntry
    {
        public ValidationEntry(string field, params string[] validationMessages)
        {
            this.Field = field;
            this.ValidationMessages = (validationMessages != null) ?
                ImmutableList.Create(validationMessages) :
                ImmutableList.Create<string>();
        }
        public string Field { get; private set; } = string.Empty;
        public IImmutableList<string> ValidationMessages { get; private set; }
    }
}
