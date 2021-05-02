using System;
using System.Collections.Generic;
using System.Text;

namespace Seatown.Data.Identities
{
    public class StringIdentity : IIdentity
    {
        private string m_ID = string.Empty;

        public StringIdentity() { }
        public StringIdentity(object Id)
        {
            this.SetIdentity(Id);
        }

        public object DefaultValue => string.Empty;

        public bool IsConfigured => !string.IsNullOrWhiteSpace(m_ID);

        public void SetIdentity()
        {
            this.SetIdentity(this.DefaultValue);
        }

        public void SetIdentity(object identity)
        {
            this.m_ID = identity.ToString();
        }
    }
}
