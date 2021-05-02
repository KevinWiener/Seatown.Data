using System;
using System.Collections.Generic;
using System.Text;

namespace Seatown.Data.Identities
{
    public enum IdentityFactoryTypes
    {
        None, 
        Decimal,
        Guid,
        String
    }

    public static class IdentityFactory
    {
        public static IdentityFactoryTypes IdentityType { get; set; } = IdentityFactoryTypes.Guid;

        public static IIdentity GetIdentity()
        {
            IIdentity result = null;
            switch (IdentityType)
            {
                case IdentityFactoryTypes.Decimal:
                    result = new NullableDecimalIdentity();
                    break;
                case IdentityFactoryTypes.Guid:
                    result = new GuidIdentity();
                    break;
                case IdentityFactoryTypes.String:
                    result = new StringIdentity();
                    break;
            }
            return result;
        }

        public static IIdentity GetIdentity(object source)
        {
            IIdentity result = null;
            switch (IdentityType)
            {
                case IdentityFactoryTypes.Decimal:
                    result = new NullableDecimalIdentity(source);
                    break;
                case IdentityFactoryTypes.Guid:
                    result = new GuidIdentity(source);
                    break;
                case IdentityFactoryTypes.String:
                    result = new StringIdentity(source);
                    break;
            }
            return result;
        }

        public static IIdentity GetNewIdentity()
        {
            IIdentity result = null;
            switch (IdentityType)
            {
                case IdentityFactoryTypes.Decimal:
                    result = new NullableDecimalIdentity(1);
                    break;
                case IdentityFactoryTypes.Guid:
                    result = new GuidIdentity(Guid.NewGuid());
                    break;
                case IdentityFactoryTypes.String:
                    result = new StringIdentity("Hello");
                    break;
            }
            return result;
        }
    }
}
