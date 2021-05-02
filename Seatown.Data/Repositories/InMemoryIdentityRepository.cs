using Seatown.Data.Identities;
using Seatown.Data.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seatown.Data.Repositories
{
    public class InMemoryRepository<T> : IIdentityRepository<T> where T : IIdentityObject
    {
        private List<T> m_ItemList = new List<T>();

        public IValidationResult[] Delete(params IIdentity[] identities)
        {
            var set = new HashSet<IIdentity>(identities);
            this.m_ItemList.RemoveAll(o => set.Contains(o.ID));
            return new[] { new ValidationResult(true, null, null) }; 
        }

        public IValidationResult[] Delete(params T[] objects)
        {
            var set = new HashSet<T>(objects);
            this.m_ItemList.RemoveAll(o => set.Contains(o));
            return new[] { new ValidationResult(true, null, null) };
        }

        public T[] Load()
        {
            return this.m_ItemList.ToArray();
        }

        public T[] Load(params IIdentity[] identities)
        {
            var set = new HashSet<IIdentity>(identities);
            return this.m_ItemList.Where(o => set.Contains(o.ID)).ToArray();
        }

        public IValidationResult[] Save(params T[] objects)
        {
            this.m_ItemList.AddRange(objects);
            return new[] { new ValidationResult(true, null, null) };
        }

        public IValidationResult[] Validate(params T[] objects)
        {
            throw new NotImplementedException();
        }
    }
}
