using System;
using System.Collections.Generic;
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
                new List<string>(validationMessages) :
                new List<string>();
        }
        public string Field { get; private set; } = string.Empty;
        public IList<string> ValidationMessages { get; private set; } = new List<string>();
    }
}
