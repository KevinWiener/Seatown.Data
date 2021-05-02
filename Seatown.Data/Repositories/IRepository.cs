using System;
using Seatown.Data.Validation;

namespace Seatown.Data.Repository
{
    public interface IRepository<T>
    {
        IValidationResult[] Delete(params T[] objects);
        IValidationResult[] Exists(params T[] objects);
        T[] Load();
        T[] Load(object filter);
        IValidationResult[] Save(params T[] objects);
        IValidationResult[] Validate(params T[] objects);
    }
}
