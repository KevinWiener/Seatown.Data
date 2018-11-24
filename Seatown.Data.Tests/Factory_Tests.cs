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

        #region Declarations & Properties

        public TestContext TestContext { get; set; }
        public const string TEST_CATEGORY = "Factory Tests";

        #endregion

        #region Helper Methods

        public DbConnection GetConnection()
        {
            return new System.Data.SQLite.SQLiteConnection("Data Source = :memory:");
        }

        #endregion

        #region Parameter Tests

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void WithParameter_AddsParameterAndValue()
        {
            // Explicitly set parameters
            var factory = Seatown.Data.Factory.Create(this.GetConnection())
                .WithParameter("@Name", "Test")
                .WithParameter("@Value", 26);

            Assert.IsTrue(factory.Parameters.ContainsKey("@Name"));
            Assert.IsTrue(factory.Parameters.ContainsKey("@Value"));
            Assert.AreEqual("Test", factory.Parameters["@Name"].Value);
            Assert.AreEqual(26, factory.Parameters["@Value"].Value);
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void WithParameters_FromDataRow_AddsParametersAndValues()
        {
            // Parameters inferred from a DataRow
            var dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("Value", typeof(int));
            dt.Rows.Add("Test", 26);

            var factory = Seatown.Data.Factory.Create(this.GetConnection())
                .WithParameters(dt.Rows[0]);

            Assert.IsTrue(factory.Parameters.ContainsKey("@Name"));
            Assert.IsTrue(factory.Parameters.ContainsKey("@Value"));
            Assert.AreEqual("Test", factory.Parameters["@Name"].Value);
            Assert.AreEqual(26, factory.Parameters["@Value"].Value);

        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void WithParameters_FromObject_AddsParametersAndValues()
        {
            // Parameters inferred from an object
            var n = new { Name = "Test", Value = 26 };

            var factory = Seatown.Data.Factory.Create(this.GetConnection())
                .WithParameters(n);

            Assert.IsTrue(factory.Parameters.ContainsKey("@Name"));
            Assert.IsTrue(factory.Parameters.ContainsKey("@Value"));
            Assert.AreEqual("Test", factory.Parameters["@Name"].Value);
            Assert.AreEqual(26, factory.Parameters["@Value"].Value);

        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void WithParameterMapping_ChangesParameterName()
        {
            var factory = Seatown.Data.Factory.Create(this.GetConnection())
                .WithParameterMapping("Value", "Name")
                .WithParameter("Value", "Tom");

            Assert.IsTrue(factory.Parameters.ContainsKey("@Name"));
            Assert.AreEqual("Tom", factory.Parameters["@Name"].Value);
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void WithParameterPrefix_AddsTheSpecifiedPrefix()
        {
            var factory = Seatown.Data.Factory.Create(this.GetConnection())
                .WithParameterPrefix("$")
                .WithParameter("Name", "Tom");

            Assert.IsTrue(factory.Parameters.ContainsKey("$Name"));
            Assert.AreEqual("Tom", factory.Parameters["$Name"].Value);
        }

        #endregion

    }
}