using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seatown.Data.Identities
{
    public interface IIdentityObject
    {
        IIdentity ID { get; set; }
    }
}
