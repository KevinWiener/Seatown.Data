using Seatown.Data.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seatown.Data.Tests.Validation
{
    public class PersonValidator : IValidator<Person>
    {
        public IValidationResult Validate(Person p)
        {
            var validationEntries = new List<ValidationEntry>();

            // FirstName
            if (string.IsNullOrWhiteSpace(p.FirstName))
            {
                validationEntries.Add(new ValidationEntry(nameof(p.FirstName), $"{nameof(p.FirstName)} is a required field"));
            }
            else
            {
                if (p.FirstName.Length < 2)
                {
                    validationEntries.Add(new ValidationEntry(nameof(p.FirstName), $"{nameof(p.FirstName)} is too short"));
                }
                else if (p.FirstName.Length > 50)
                {
                    validationEntries.Add(new ValidationEntry(nameof(p.FirstName), $"{nameof(p.FirstName)} is too long"));
                }
            }

            // LastName
            if (string.IsNullOrWhiteSpace(p.LastName))
            {
                validationEntries.Add(new ValidationEntry(nameof(p.LastName), $"{nameof(p.LastName)} is a required field"));
            }
            else
            {
                if (p.LastName.Length < 2)
                {
                    validationEntries.Add(new ValidationEntry(nameof(p.LastName), $"{nameof(p.LastName)} is too short"));
                }
                else if (p.LastName.Length > 50)
                {
                    validationEntries.Add(new ValidationEntry(nameof(p.LastName), $"{nameof(p.LastName)} is too long"));
                }
            }

            // Age
            if (!p.Age.HasValue)
            {
                validationEntries.Add(new ValidationEntry(nameof(p.Age), $"{nameof(p.Age)} is a required field"));
            }
            else
            {
                if (p.Age.Value < 10)
                {
                    validationEntries.Add(new ValidationEntry(nameof(p.Age), $"{nameof(p.Age)} is too low"));
                }
                else if (p.Age.Value > 110)
                {
                    validationEntries.Add(new ValidationEntry(nameof(p.Age), $"{nameof(p.Age)} is too high"));
                }
            }

            return new ValidationResult(validationEntries.Count.Equals(0), p, validationEntries.ToArray());
        }

        public static IValidationResult<Person[]> GetPeople(string invalidName, params string[] names)
        {
            var result = new List<Person>();
            foreach (var name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    result.Add(new Person() { FirstName = name });
                }
            }          
            
            var validationEntries = new List<IValidationEntry>();
            if (names.Contains(invalidName))
            {
                validationEntries.Add(new ValidationEntry("Name", new[] { $"{invalidName} is not a valid name." }));
            }

            return new ValidationResult<Person[]>(validationEntries.Count.Equals(0), names, result.ToArray(), validationEntries.ToArray());
        }
    }
}
