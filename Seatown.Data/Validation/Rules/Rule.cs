using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Seatown.Data.Validation.Rules
{
    public class Rule 
   {
        public Rule(string field, string op, string value)
        {
            this.Field = field;
            this.Operator = op;
            this.Value = value;
        }
        public string Field { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
    }
}
