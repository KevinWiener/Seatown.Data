using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Seatown.Data.Schemas;

namespace Seatown.Data
{
    public class SchemaBuilder : IDisposable
    {

        #region Properties

        public DbConnection Connection { get; set; }
        public DbTransaction Transaction { get; set; }

        #endregion

        #region Constructors

        public SchemaBuilder(DbConnection cn, DbTransaction tx)
        {
            this.Connection = cn;
            this.Transaction = tx;
        }

        #endregion

        #region Routines

        public Database Build()
        {
            var result = new Database();
            try
            {
                result.Name = this.Connection.Database;
                result.Tables.AddRange(this.GetTables(result.Name));
                result.Views.AddRange(this.GetViews(result.Name));
                result.Procedures.AddRange(this.GetProcedures(result.Name));
                result.Functions.AddRange(this.GetFunctions(result.Name));
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        private Column[] GetColumns(string databaseName, string objectSchema, string objectName)
        {
            var result = new List<Column>();
            try
            {

            }
            catch (Exception)
            {
                throw;
            }
            return result.ToArray();
        }

        private Function[] GetFunctions(string databaseName)
        {
            var result = new List<Function>();
            try
            {

            }
            catch (Exception)
            {
                throw;
            }
            return result.ToArray();
        }

        private Parameter[] GetParameters(string databaseName, string objectSchema, string objectName)
        {
            var result = new List<Parameter>();
            try
            {

            }
            catch (Exception)
            {
                throw;
            }
            return result.ToArray();
        }

        private Procedure[] GetProcedures(string databaseName)
        {
            var result = new List<Procedure>();
            try
            {

            }
            catch (Exception)
            {
                throw;
            }
            return result.ToArray();
        }

        private Table[] GetTables(string databaseName)
        {
            var result = new List<Table>();
            try
            {

            }
            catch (Exception)
            {
                throw;
            }
            return result.ToArray();
        }

        private View[] GetViews(string databaseName)
        {
            var result = new List<View>();
            try
            {

            }
            catch (Exception)
            {
                throw;
            }
            return result.ToArray();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            this.Connection = null;
            this.Transaction = null;
        }

        #endregion

    }
}
