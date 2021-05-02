using Seatown.Data.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Seatown.Data.Validation.Rules
{
    public class RuleValidator<T> : IValidator<T>
    {

        private List<RuleSet<T>> m_RuleSets = new List<RuleSet<T>>();

        public RuleValidator(params RuleSet<T>[] ruleSets)
        {
            if (ruleSets != null && ruleSets.Length > 0)
            {
                this.m_RuleSets.AddRange(ruleSets);
            }
        }

        public IValidationResult Validate(T objectToValidate)
        {
            bool aggregateResult = true;
            foreach (var ruleSet in this.m_RuleSets)
            {
                aggregateResult &= ruleSet.Evaluate(objectToValidate);
            }
            return new ValidationResult(aggregateResult, objectToValidate, null);
        }

    }
}
