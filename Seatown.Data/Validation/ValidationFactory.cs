using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seatown.Data.Validation
{
    public class ValidationFactory
    {

        private static Dictionary<Type, object> m_Validators = new Dictionary<Type, object>();

        public static void Register<T>(IValidator<T> validator)
        {
            if (validator != null)
            {
                m_Validators[typeof(T)] = validator;
            }
        }

        public static void UnRegister<T>()
        {
            if (m_Validators.ContainsKey(typeof(T)))
            {
                m_Validators.Remove(typeof(T));
            }
        }

        public static IValidationResult Validate<T>(T objectToValidate)
        {
            IValidationResult result = new ValidationResult(true, objectToValidate, null);
            if (m_Validators.ContainsKey(typeof(T)))
            {
                result = ((IValidator<T>)m_Validators[typeof(T)]).Validate(objectToValidate);
            }
            return result;
        }

    }
}
