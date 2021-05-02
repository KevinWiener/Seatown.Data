using Seatown.Data.Identities;
using Seatown.Data.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seatown.Data.Repositories
{
    public interface IIdentityRepository<T> where T : IIdentityObject
    {
        IValidationResult[] Delete(params T[] objects);
        IValidationResult[] Delete(params IIdentity[] identities);
        T[] Load();
        T[] Load(params IIdentity[] identities);
        IValidationResult[] Save(params T[] objects);
        IValidationResult[] Validate(params T[] objects);
    }
}
