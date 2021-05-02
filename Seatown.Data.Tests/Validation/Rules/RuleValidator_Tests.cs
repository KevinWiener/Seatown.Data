using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seatown.Data.Validation;
using Seatown.Data.Validation.Rules;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Seatown.Data.Tests.Validation.Rules
{
    [TestClass]
    public class RuleValidator_Tests
    {
        [TestMethod]
        public void CanCompileRule()
        {
            var person = new Person { FirstName = "John" };

            var firstNameRule = new Rule("firstname", "Equals", "John");
            var lastNameRule = new Rule(nameof(Person.LastName), "IsNullOrWhiteSpace", person.LastName);
            var ruleSet = new RuleSet<Person>(firstNameRule, lastNameRule);
            var result = new RuleValidator<Person>(ruleSet).Validate(person);
            Assert.IsTrue(result.IsValid);
        }

    }
}
