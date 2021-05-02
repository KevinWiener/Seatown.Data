using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seatown.Data.Identities
{
    public interface IIdentity
    {
        object DefaultValue { get; }
        bool IsConfigured { get; }
        void SetIdentity();
        void SetIdentity(object identity);
    }
}
