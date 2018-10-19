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
            var cn = new System.Data.SqlClient.SqlConnection();

            // Explicitly set parameters
            var x = Seatown.Data.Factory.Create(cn)
                .WithCommandTimeout(TimeSpan.FromSeconds(30))
                .WithParameter("@FacID", 1);
            Assert.IsTrue(x.Parameters.ContainsKey("@FacID"));
            Assert.AreEqual(1, x.Parameters["@FacID"].Value);

            // Parameters inferred from a DataRow
            var dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("Value", typeof(int));
            dt.Rows.Add("Test", 26);
            var y = Seatown.Data.Factory.Create(cn)
                .WithCommandTimeout(TimeSpan.FromSeconds(30))
                .WithParameters(dt.Rows[0]);
            Assert.IsTrue(y.Parameters.ContainsKey("@Name"));
            Assert.IsTrue(y.Parameters.ContainsKey("@Value"));
            Assert.AreEqual(y.Parameters["@Name"].Value, "Test");
            Assert.AreEqual(y.Parameters["@Value"].Value, 26);

            // Parameters inferred from an object
            var n = new { Name = "Test", Value = 26 };
            var z = Seatown.Data.Factory.Create(cn)
                .WithCommandTimeout(TimeSpan.FromSeconds(30))
                .WithParameters(n);
            Assert.IsTrue(y.Parameters.ContainsKey("@Name"));
            Assert.IsTrue(y.Parameters.ContainsKey("@Value"));
            Assert.AreEqual(y.Parameters["@Name"].Value, "Test");
            Assert.AreEqual(y.Parameters["@Value"].Value, 26);

        }

    }
}
