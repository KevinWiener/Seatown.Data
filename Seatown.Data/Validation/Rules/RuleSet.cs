using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Seatown.Data.Validation.Rules
{
    public class RuleSet<T>
    {

        private List<Rule> m_AndRules = new List<Rule>();
        private List<Rule> m_OrRules = new List<Rule>();

        public RuleSet(params Rule[] rules)
        {
            if (rules != null && rules.Length > 0)
            {
                this.m_AndRules.AddRange(rules);
            }
        }

        public RuleSet<T> And(params Rule[] rules)
        {
            if (rules != null && rules.Length > 0)
            {
                this.m_AndRules.AddRange(rules);
            }
            return this;
        }

        public RuleSet<T> Or(params Rule[] rules)
        {
            if (rules != null && rules.Length > 0)
            {
                this.m_OrRules.AddRange(rules);
            }
            return this;
        }

        public bool Evaluate(T target)
        {
            bool result = true;
            if (this.m_AndRules.Count > 0 || this.m_OrRules.Count > 0)
            {
                Func<T, bool> evaluationFunction = this.Compile();
                result = evaluationFunction(target);
            }
            return result;
        }

        private Func<T, bool> Compile()
        {
            Expression<Func<T, bool>> result = null;
            foreach (Rule rule in this.m_AndRules)
            {
                if (result == null)
                {
                    result = this.BuildExpression(rule);
                }
                else
                {
                    var expressionToAdd = this.BuildExpression(rule);
                    var parameterModifiedExpression = new ExpressionParameterReplacer(expressionToAdd.Parameters, result.Parameters).Visit(expressionToAdd.Body);
                    result = Expression.Lambda<Func<T, bool>>(Expression.AndAlso(result.Body, parameterModifiedExpression), result.Parameters);
                }
            }

            foreach (Rule rule in this.m_OrRules)
            {
                if (result == null)
                {
                    result = this.BuildExpression(rule);
                }
                else
                {
                    var expressionToAdd = this.BuildExpression(rule);
                    var parameterModifiedExpression = new ExpressionParameterReplacer(expressionToAdd.Parameters, result.Parameters).Visit(expressionToAdd.Body);
                    result = Expression.Lambda<Func<T, bool>>(Expression.OrElse(result.Body, parameterModifiedExpression), result.Parameters);
                }
            }
            return result?.Compile();
        }

        private Expression<Func<T, bool>> BuildExpression(Rule rule)
        {
            Expression<Func<T, bool>> result = null;
            var objectTypeExpression = Expression.Parameter(typeof(T));
            var fieldExpression = Expression.PropertyOrField(objectTypeExpression, rule.Field);
            var operatorName = rule.Operator;

            ExpressionType binaryExpression;
            if (Enum.TryParse(operatorName, out binaryExpression))
            {
                // Person.FirstName == "John";
                var valueExpression = Expression.Constant(Convert.ChangeType(rule.Value, fieldExpression.Type));
                var evaluationExpression = Expression.MakeBinary(binaryExpression, fieldExpression, valueExpression);
                result = Expression.Lambda<Func<T, bool>>(evaluationExpression, objectTypeExpression);
            }
            else
            {
                // Person.FirstName.Contains("John");
                var methodExpression = fieldExpression.Type.GetMethod(operatorName, new[] { fieldExpression.Type });                
                var valueExpression = Expression.Constant(rule.Value, methodExpression.GetParameters()[0].ParameterType);
                var evaluationExpression = methodExpression.IsStatic ?
                    Expression.Call(methodExpression, valueExpression) :
                    Expression.Call(fieldExpression, methodExpression, valueExpression);
                result = Expression.Lambda<Func<T, bool>>(evaluationExpression, objectTypeExpression);
            }
            return result;
        }

        // Stolen from: https://stackoverflow.com/questions/2231302/append-to-an-expression
        private class ExpressionParameterReplacer : ExpressionVisitor
        {
            private IDictionary<ParameterExpression, ParameterExpression> m_ParameterReplacements = new Dictionary<ParameterExpression, ParameterExpression>();

            public ExpressionParameterReplacer(IList<ParameterExpression> fromParameters, IList<ParameterExpression> toParameters)
            {
                for (int i = 0; i != fromParameters.Count && i != toParameters.Count; i++)
                {
                    m_ParameterReplacements.Add(fromParameters[i], toParameters[i]);
                }
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                ParameterExpression replacement;
                if (m_ParameterReplacements.TryGetValue(node, out replacement))
                {
                    node = replacement;
                }
                return base.VisitParameter(node);
            }
        }

    }
}
