using System;
using System.Data;
using System.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Seatown.Data.Tests
{
    [TestClass]
    public class SchemaBuilder_Tests
    {
        #region Declarations & Properties

        public TestContext TestContext { get; set; }
        public const string TEST_CATEGORY = "SchemaBuilder Tests";

        #endregion

        #region Helper Methods

        public DbConnection GetConnection()
        {
            return new System.Data.SQLite.SQLiteConnection("Data Source = :memory:");
        }

        #endregion


        [TestCategory(TEST_CATEGORY), TestMethod]
        public void TestMethod1()
        {
            var db = new Seatown.Data.Schemas.Database();

        }

    }
}
