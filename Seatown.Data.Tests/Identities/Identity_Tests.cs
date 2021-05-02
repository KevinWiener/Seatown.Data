﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seatown.Data.Identities;
using Seatown.Data.Repositories;

namespace Seatown.Data.Tests.Identities
{
    [TestClass]
    public class Identity_Tests
    {
        [TestMethod]
        public void TestMethod1()
        {
            // Configured once for the app domain, but could be inferred from the 
            // identity type associated to a specific object.
            IdentityFactory.IdentityType = IdentityFactoryTypes.Guid;

            // Simulates loading an existing identity value from a database,
            // but would never actually reference the Guid type.
            var companyID = IdentityFactory.GetIdentity(Guid.NewGuid());

            var employee = new Employee() {
                ID = IdentityFactory.GetNewIdentity(),
                CompanyID = companyID,
                Name = "Tom Petty",
                Age = 57
            };

            var repository = new InMemoryRepository<Employee>();
            repository.Save(employee);

            var savedEmployee = repository.Load(employee.ID).FirstOrDefault();
            Assert.AreEqual(employee.ID, savedEmployee.ID);
            Assert.AreEqual(employee.CompanyID, savedEmployee.CompanyID);
            Assert.IsTrue(employee.ID is GuidIdentity);
            Assert.IsTrue(employee.CompanyID is GuidIdentity);


            // Changes the datatype for the app domain
            IdentityFactory.IdentityType = IdentityFactoryTypes.Decimal;

            // Simulates loading an existing identity value from a database,
            // but would never actually reference the decimal type.
            companyID = IdentityFactory.GetIdentity(26D);

            // Code to assign and compare identity properties does not change,
            // only the datasource and IdentityFactory configurations change. 
            employee = new Employee()
            {
                ID = IdentityFactory.GetNewIdentity(),
                CompanyID = companyID,
                Name = "Paul Wall",
                Age = 34
            };
            repository.Save(employee);

            savedEmployee = repository.Load(employee.ID).FirstOrDefault();
            Assert.AreEqual(employee.ID, savedEmployee.ID);
            Assert.AreEqual(employee.CompanyID, savedEmployee.CompanyID);
            Assert.IsTrue(employee.ID is NullableDecimalIdentity);
            Assert.IsTrue(employee.CompanyID is NullableDecimalIdentity);

        }
    }
}
