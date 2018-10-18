﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Concurrent;
using Seatown.Data.Common;

namespace Seatown.Data
{
    /// <summary>
    /// Manages connection state for common command methods similar to how a data adapter works.
    /// </summary>
    public class Factory
    {

        // TODO: Make this a non-static class?
        // TODO: Implement fluent methods for easier use?
        // TODO: Maybe change the class name to CommandFactory, and implement IDisposable?

        // TODO: Investigate reflection or expressions for setting parameters from objects?
        // TODO: Add fluent methods for setting parameters and other misc?
        // TODO: Add support for out parameters?
        // TODO: Finish adding ExecuteScaler, Fill/Get DataTable/DataSet methods
        // TODO: Add async ExecuteScaler, Fill/Get DataTable/DataSet methods??
        // TODO: Add misc utility methods ValueIsNumeric, ItemIsInList, MapRowChanges, DataTableContainsColumns?

        #region Properties

        public DbCommand Command { get; private set; }
        public DbConnection Connection { get; set; } = null;
        public DbTransaction Transaction { get; set; } = null;
        public Dictionary<string, DbParameter> Parameters { get; private set; } = new Dictionary<string, DbParameter>();

        public static int DefaultCommandTimeout { get; set; } = 30;
        public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(DefaultCommandTimeout);

        private DbType m_DefaultDbType = (DbType)Enumerable.Range(28, 228).Where(n => !Enum.IsDefined(typeof(DbType), n)).FirstOrDefault();
        private ParameterDirection m_DefaultParameterDirection = ParameterDirection.Input;
        private CommandType m_CommandType = CommandType.Text;

        private static List<string> m_DatabaseProviders = new List<string>()
        {
            "System.Data.Odbc",
            "System.Data.OleDb",
            "System.Data.OracleClient",
            "System.Data.SqlClient",
            "System.Data.SqlServerCe.3.5",
            "System.Data.SqlServerCe.4.0"
        };
        private static string m_DatabaseProvider = string.Empty;
        public static string DatabaseProvider
        {
            get
            {
                return m_DatabaseProvider;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || m_DatabaseProviders.Contains(value, StringComparer.CurrentCultureIgnoreCase))
                {
                    m_DatabaseProvider = value;
                }
                else
                {
                    throw new ArgumentException($"Invalid database provider: {value}");
                }
            }
        }


        public bool AddNewParameters { get; set; } = false;
        public bool ConvertEmptyStringsToNull { get; set; } = true;
        public bool InitializeParametersToNull { get; set; } = true;
        public string ParameterPrefix { get; set; } = "@";


        #endregion

        #region Parameters

        public IDbDataParameter CreateCommandParameter(IDbCommand cmd, string parameterName, DbType parameterType, object parameterValue)
        {
            // Apply parameter prefix
            var prefixedParameterName = parameterName;
            if (!string.IsNullOrWhiteSpace(ParameterPrefix) && !prefixedParameterName.StartsWith(ParameterPrefix))
            {
                prefixedParameterName = $"{ParameterPrefix}{prefixedParameterName}";
            }

            // Convert empty strings to NULL
            if (ConvertEmptyStringsToNull && parameterValue is string)
            {
                if (string.IsNullOrWhiteSpace(parameterValue.ToString()))
                {
                    parameterValue = DBNull.Value;
                }
            }

            // Create and return the parameter
            IDbDataParameter result = null;
            result = cmd.CreateParameter();
            result.ParameterName = prefixedParameterName;
            result.SourceColumn = parameterName;
            result.DbType = parameterType;
            result.Value = parameterValue;

            return result;
        }

        public void SetCommandParameters(IDbCommand cmd, DataRow parameterSource)
        {
            SetCommandParameters(cmd, parameterSource, InitializeParametersToNull, AddNewParameters, null);
        }

        public void SetCommandParameters(IDbCommand cmd, DataRow parameterSource, bool initializeToNull)
        {
            SetCommandParameters(cmd, parameterSource, initializeToNull, AddNewParameters, null);
        }

        public void SetCommandParameters(IDbCommand cmd, DataRow parameterSource, bool initializeToNull, bool addNewParameters)
        {
            SetCommandParameters(cmd, parameterSource, initializeToNull, addNewParameters, null);
        }

        public void SetCommandParameters(IDbCommand cmd, DataRow parameterSource, bool initializeToNull, bool addNewParameters, Dictionary<string, string> fieldMap)
        {
            if (initializeToNull)
            {
                // Initialize parameters to NULL
                foreach (IDbDataParameter parameterToInitialize in cmd.Parameters)
                {
                    parameterToInitialize.Value = DBNull.Value;
                }

                foreach (DataColumn column in parameterSource.Table.Columns)
                {
                    // Apply field mappings
                    string parameterName = column.ColumnName;
                    if (fieldMap != null && fieldMap.ContainsKey(column.ColumnName))
                    {
                        if (!string.IsNullOrWhiteSpace(fieldMap[column.ColumnName]))
                        {
                            parameterName = fieldMap[column.ColumnName];
                        }
                    }

                    parameterName = GetParameterName(parameterName);

                    // Get the current property value
                    object parameterValue = parameterSource[column.ColumnName];

                    // Convert empty strings to NULL
                    if (ConvertEmptyStringsToNull && parameterValue is string)
                    {
                        if (string.IsNullOrWhiteSpace(parameterValue.ToString()))
                        {
                            parameterValue = null;
                        }
                    }

                    // Check to see if the parameter already exists
                    IDbDataParameter parameter = null;
                    if (cmd.Parameters.Contains(parameterName))
                    {
                        parameter = cmd.Parameters[parameterName] as IDbDataParameter;
                    }
                    else if (addNewParameters)
                    {
                        parameter = cmd.CreateParameter();
                        parameter.ParameterName = parameterName;
                        cmd.Parameters.Add(parameter);
                    }

                    // If we found or created a reference to the parameter, set its value
                    if (parameter != null)
                    {
                        parameter.Value = parameterValue ?? DBNull.Value;
                    }
                }
            }
        }

        #endregion

        #region Routines

        #endregion

        #region Helper Methods

        protected void ConfigureCommand(string sql)
        {
            this.Command.CommandText = sql;
            this.Command.CommandTimeout = (int)this.CommandTimeout.TotalSeconds;
            this.Command.CommandType = this.m_CommandType.Equals(CommandType.Text) ? this.PredictCommandType(sql) : this.m_CommandType;
            this.Command.Connection = this.Connection;
            this.Command.Transaction = this.Transaction;
            if (this.Parameters != null && this.Parameters.Count > 0)
            {
                this.Command.Parameters.Clear();
                foreach (var kvp in this.Parameters)
                {
                    this.Command.Parameters.Add(kvp.Value);
                }
            }
        }

        protected CommandType PredictCommandType(string sql)
        {
            var result = CommandType.Text;
            if (!sql.Trim().Contains(" "))
            {
                result = CommandType.StoredProcedure;
            }
            return result;
        }

        protected string PredictDatabaseProvider(DbConnection cn)
        {
            var result = DatabaseProvider;
            if (string.IsNullOrWhiteSpace(result))
            {
                // Connection types include the owning namespace and the object type, so a SqlClient connection would have the type
                // System.Data.SqlClient.SqlConnection, so we have to strip off the object type to get the underlying namespace.
                string connectionType = cn.GetType().ToString();
                connectionType = connectionType.Substring(0, connectionType.LastIndexOf("."));
                result = m_DatabaseProviders.Where((s) => s.StartsWith(connectionType)).FirstOrDefault();
            }
            return result;
        }

        protected string GetParameterName(string parameterName)
        {
            string result = parameterName;
            if (!string.IsNullOrWhiteSpace(parameterName) && !string.IsNullOrWhiteSpace(this.ParameterPrefix))
            {
                if (!parameterName.StartsWith(this.ParameterPrefix.Trim()))
                {
                    result = this.ParameterPrefix.Trim() + parameterName.Trim();
                }
            }
            return result;
        }

        protected object GetParameterValue(object parameterValue)
        {
            // Convert empty strings to NULL
            object result = parameterValue;
            if (ConvertEmptyStringsToNull && parameterValue is string)
            {
                if (string.IsNullOrWhiteSpace(parameterValue.ToString()))
                {
                    parameterValue = null;
                }
            }
            // Convert null objects to DBNull.Value
            return result ?? DBNull.Value;
        }

        #endregion

        #region Fluent Methods

        public static Factory Create(DbConnection cn, DbTransaction tx)
        {
            return new Factory { Command = cn.CreateCommand(), Connection = cn, Transaction = tx };
        }

        public Factory AsStoredProcedure()
        {
            this.m_CommandType = CommandType.StoredProcedure;
            return this;
        }

        public Factory WithCommandTimeout(TimeSpan timeout)
        {
            if ((int)timeout.TotalSeconds <= 0)
            {
                throw new ArgumentException($"Invalid command timeout {((int)timeout.TotalSeconds).ToString()}, value must be greater than zero.");
            }
            this.CommandTimeout = timeout;
            return this;
        }

        public Factory WithParameter(string name, object value)
        {
            return WithParameter(name, value, this.m_DefaultDbType, this.m_DefaultParameterDirection);
        }

        public Factory WithParameter(string name, object value, ParameterDirection direction)
        {
            return WithParameter(name, value, this.m_DefaultDbType, direction);
        }

        public Factory WithParameter(string name, object value, DbType type)
        {
            return this.WithParameter(name, value, type, this.m_DefaultParameterDirection);
        }

        public Factory WithParameter(string name, object value, DbType type, ParameterDirection direction)
        {
            //----------------------------------------------------------------------------------------------
            // 1. If a DbType is manually set to a specific value, the parameter will be cast to that type.
            // 2. If the DbType is not set, the DbType will be inferred from the parameter value. 
            // 3. DbType parameters cannot be passed as null.
            // 4. At the time of writing, the inferred DbType for DbNull.Value/null was string.
            // 
            // With those rules in mind, there is a class level variable that was boxed into an undefined
            // DbType so it could correctly be identified and ignored, which allowed the the DbType to be
            // inferred when not specified by the user in one of the overloaded signatures for this method.
            //----------------------------------------------------------------------------------------------
            var p = this.Command.CreateParameter();
            if (Enum.IsDefined(typeof(DbType), type))
            {
                p.DbType = type;
            }
            p.Direction = direction;
            p.ParameterName = this.GetParameterName(name);
            p.Value = this.GetParameterValue(value);
            if (this.Parameters.ContainsKey(p.ParameterName))
            {
                this.Parameters[p.ParameterName].DbType = p.DbType;
                this.Parameters[p.ParameterName].Direction = p.Direction;
                this.Parameters[p.ParameterName].Value = p.Value;
            }
            else
            {
                this.Parameters.Add(p.ParameterName, p);
            }
            return this;
        }

        public Factory WithParameterPrefix(string prefix)
        {
            this.ParameterPrefix = prefix;
            return this;
        }



        public Factory SetCommandParameters(DataRow parameterSource)
        {
            return SetCommandParameters(parameterSource, this.InitializeParametersToNull, this.AddNewParameters);
        }

        public Factory SetCommandParameters(DataRow parameterSource, bool initializeToNull)
        {
            return SetCommandParameters(parameterSource, initializeToNull, this.AddNewParameters);
        }

        public Factory SetCommandParameters(DataRow parameterSource, bool initializeToNull, bool addNewParameters)
        {
            this.InitializeParametersToNull = initializeToNull;
            this.AddNewParameters = AddNewParameters;

            // TODO: set command parameters from data row.

            return this;
        }

        #endregion

        #region Action Methods

        public bool ExecuteCommand(string sql)
        {
            this.ConfigureCommand(sql);
            using (var csm = new ConnectionStateManager(this.Command.Connection))
            {
                this.Command.ExecuteNonQuery();
            }
            return true;
        }

        public DbDataReader ExecuteReader(string sql)
        {
            // Data readers require the connection to remain open during the lifetime of the reader, 
            // so we cannot manage the connection state other than ensuring it is open for the caller.
            this.ConfigureCommand(sql);
            ConnectionStateManager.OpenConnection(this.Command.Connection);
            return this.Command.ExecuteReader();
        }

        public object ExecuteScaler(string sql)
        {
            this.ConfigureCommand(sql);
            using (var csm = new ConnectionStateManager(this.Command.Connection))
            {
                return this.Command.ExecuteNonQuery();
            }
        }



        public bool FillDataSet(DataSet ds)
        {
            throw new NotImplementedException();
        }

        public bool FillDataTable(string sql, DataTable dt)
        {
            this.ConfigureCommand(sql);
            using (var csm = new ConnectionStateManager(this.Command.Connection))
            {
                dt.BeginLoadData();
                dt.Load(this.Command.ExecuteReader());
                dt.EndLoadData();
            }
            return true;
        }

        public DataSet GetDataSet()
        {
            throw new NotImplementedException();
        }

        public DataTable GetDataTable(string sql)
        {
            var result = new DataTable();
            this.FillDataTable(sql, result);
            return result;
        }



        #endregion
    }
}
