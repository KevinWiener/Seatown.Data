using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seatown.Data.Identities
{
    public class NullableDecimalIdentity : IIdentity
    {
        public NullableDecimalIdentity()
        {
            // Empty constructor...
        }

        public NullableDecimalIdentity(object id)
        {
            this.SetIdentity(id);
        }

        private decimal? m_ID = null;
        public object DefaultValue
        {
            get
            {
                return null;
            }
        }

        public bool IsConfigured
        {
            get
            {
                return m_ID.HasValue;
            }
        }

        public void SetIdentity()
        {
            this.m_ID = null;
        }

        public void SetIdentity(object identity)
        {
            decimal newID = 0;
            if (decimal.TryParse(identity.ToString(), out newID))
            {
                this.m_ID = newID;
            }
        }

        public override string ToString()
        {
            return this.m_ID.HasValue ? this.m_ID.Value.ToString() : string.Empty;
        }
    }
}
