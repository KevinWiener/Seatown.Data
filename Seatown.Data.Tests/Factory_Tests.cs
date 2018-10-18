using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Seatown.Data.Tests
{
    [TestClass]
    public class Factory_Tests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var x = new Seatown.Data.Factory()
                .WithCommandTimeout(TimeSpan.FromSeconds(30))
                .WithParameter("@FacID", 1);

            var factory = Seatown.Data.Factory.Create(null, null)
                .WithCommandTimeout(TimeSpan.FromSeconds(30));
        }

    }
}
