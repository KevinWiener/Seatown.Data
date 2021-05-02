using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seatown.Data.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Seatown.Data.Tests.Validation
{
    [TestClass]
    public class PersonValidator_Tests
    {
        public TestContext TestContext { get; set; }
        public const string TEST_CATEGORY = "PersonValidator Tests";

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Validate_NoValidValues_FailsValidation()
        {
            Person p = new Person();
            ValidationFactory.Register(new PersonValidator());
            IValidationResult vr = ValidationFactory.Validate(p);
            Assert.IsFalse(vr.IsValid);
            Assert.AreEqual(3, vr.ValidationEntries.Count);
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Validate_FirstNameTooShort_FailsValidation()
        {
            Person p = new Person() { FirstName = "M", LastName = "Smith", Age = 25 };
            ValidationFactory.Register(new PersonValidator());
            IValidationResult vr = ValidationFactory.Validate(p);
            Assert.IsFalse(vr.IsValid);
            Assert.AreEqual(1, vr.ValidationEntries.Count);
            Assert.AreEqual(nameof(p.FirstName), vr.ValidationEntries.First().Field);
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Validate_FirstNameTooLong_FailsValidation()
        {
            Person p = new Person() { FirstName = "Mike12345678901234567890123456789012345678901234567890", LastName = "Smith", Age = 25 };
            ValidationFactory.Register(new PersonValidator());
            IValidationResult vr = ValidationFactory.Validate(p);
            Assert.IsFalse(vr.IsValid);
            Assert.AreEqual(1, vr.ValidationEntries.Count);
            Assert.AreEqual(nameof(p.FirstName), vr.ValidationEntries.First().Field);
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Validate_GenericResult_ReturnsExpectedResult()
        {
            var names = new List<string>() { "alan", "bob", "joe", "mark" };
            var vr = PersonValidator.GetPeople("bob", names.ToArray());
            Assert.IsFalse(vr.IsValid);
            Assert.AreEqual(4, vr.Result?.Length);
            Assert.AreEqual(1, vr.ValidationEntries.Count);
            Assert.IsTrue(vr.ValidationEntries.First().ValidationMessages.First().Contains("bob"));
        }

    }
}
