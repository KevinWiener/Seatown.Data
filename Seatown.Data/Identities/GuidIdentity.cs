using System;
using System.Collections.Generic;
using System.Text;

namespace Seatown.Data.Identities
{
    public class GuidIdentity : IIdentity
    {
        private Guid m_ID = Guid.Empty;

        public GuidIdentity() { }
        public GuidIdentity(object id)
        {
            this.SetIdentity(id);
        }

        public object DefaultValue => Guid.Empty;

        public bool IsConfigured => !Guid.Empty.Equals(m_ID);

        public void SetIdentity()
        {
            this.SetIdentity(this.DefaultValue);
        }

        public void SetIdentity(object identity)
        {
            try
            {
                // Guid's cannot be directly converted from a string using CType, but the code below converts the type correctly
                if (!string.IsNullOrWhiteSpace(identity.ToString()))
                {
                    m_ID = (Guid)System.ComponentModel.TypeDescriptor.GetConverter(m_ID).ConvertFrom(identity.ToString());
                }
            }
            catch (Exception)
            {
                // throw a more usable exception if we can't parse the identity value passed in...
                throw new ArgumentException(string.Format("The Identity value {0} is invalid.", identity.ToString()));
            }

        }
    }
}
