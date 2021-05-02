using Seatown.Data.Identities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seatown.Data.Tests.Identities
{
    public class Employee : IIdentityObject
    {
        public IIdentity ID { get; set; } = IdentityFactory.GetNewIdentity();
        public IIdentity CompanyID { get; set; } = IdentityFactory.GetNewIdentity();
        public string Name { get; set; } = string.Empty;
        public int? Age { get; set; } = null;
    }
}
