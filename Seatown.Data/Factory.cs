using System;
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

        // TODO: Maybe change the class name to CommandFactory??  Implement IDisposable?
        // TODO: Add async ExecuteScaler, Fill/Get DataTable/DataSet methods??
        // TODO: Add misc utility methods ValueIsNumeric, ItemIsInList, MapRowChanges, DataTableContainsColumns?

        #region Constructors

        private Factory()
        {
            // Private constructor to force object creation through fluent methods
        }

        #endregion

        #region Properties

        public static bool ConvertEmptyStringsToNull { get; set; } = true;
        public static bool ConvertEnumsToStrings { get; set; } = true;
        public static int DefaultCommandTimeout { get; set; } = 30;
        public static string DefaultParameterPrefix { get; set; } = "@";

        public DbCommand Command { get; private set; }
        public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(DefaultCommandTimeout);
        public DbConnection Connection { get; set; } = null;
        public DbTransaction Transaction { get; set; } = null;
        public Dictionary<string, DbParameter> Parameters { get; private set; } = new Dictionary<string, DbParameter>();
        public Dictionary<string, string> ParameterMappings { get; private set; } = new Dictionary<string, string>();

        private CommandType m_CommandType = CommandType.Text;
        private DbType m_DefaultDbType = (DbType)Enumerable.Range(28, 228).Where(n => !Enum.IsDefined(typeof(DbType), n)).FirstOrDefault();
        private ParameterDirection m_DefaultParameterDirection = ParameterDirection.Input;
        private string m_ParameterPrefix = DefaultParameterPrefix;

        #endregion

        #region Helper Methods

        protected void AddParameter(string name, object value)
        {
            this.AddParameter(name, value, this.m_DefaultDbType, this.m_DefaultParameterDirection);
        }

        protected void AddParameter(string name, object value, DbType type, ParameterDirection direction)
        {
            //----------------------------------------------------------------------------------------------
            // 1. If a DbType is manually set to a specific value, the parameter will be cast to that type.
            // 2. If the DbType is not set, the DbType will be inferred from the parameter value. 
            // 3. DbType parameters cannot be passed as null.
            // 4. At the time of writing, the inferred DbType for DbNull.Value/null was string.
            // 
            // With those rules in mind, there is a class level variable that was boxed into an undefined
            // DbType so it could correctly be identified and ignored, which allowed the the DbType to be
            // inferred when not specified by the user.
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
        }

        protected void ConfigureCommand(string sql)
        {
            this.Command.CommandText = sql;
            this.Command.CommandTimeout = this.CommandTimeout.TotalSeconds > int.MaxValue ? int.MaxValue : (int)this.CommandTimeout.TotalSeconds;
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

        protected string GetParameterName(string parameterName)
        {
            string result = parameterName;
            string prefixedParameterName = this.AddPrefix(parameterName);
            string nonPrefixedParameterName = this.RemovePrefix(parameterName);
            if (this.ParameterMappings.ContainsKey(prefixedParameterName))
            {
                result = this.ParameterMappings[prefixedParameterName];
            }
            else if (this.ParameterMappings.ContainsKey(nonPrefixedParameterName))
            {
                result = this.ParameterMappings[nonPrefixedParameterName];
            }
            return this.AddPrefix(result);
        }

        protected string AddPrefix(string parameterName)
        {
            string result = parameterName;
            if (!string.IsNullOrWhiteSpace(this.m_ParameterPrefix))
            {
                if (!parameterName.StartsWith(this.m_ParameterPrefix))
                {
                    result = this.m_ParameterPrefix + parameterName;
                }
            }
            return result;
        }

        protected string RemovePrefix(string parameterName)
        {
            string result = parameterName;
            if (!string.IsNullOrWhiteSpace(this.m_ParameterPrefix))
            {
                if (parameterName.StartsWith(this.m_ParameterPrefix))
                {
                    result = parameterName.Substring(this.m_ParameterPrefix.Length);
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

        protected void UpdateOutParameters()
        {
            foreach (DbParameter p in this.Command.Parameters)
            {
                if (p.Direction.Equals(ParameterDirection.Output) || p.Direction.Equals(ParameterDirection.InputOutput))
                {
                    if (this.Parameters.ContainsKey(p.ParameterName))
                    {
                        this.Parameters[p.ParameterName].Value = p.Value;
                    }
                }
            }
        }

        #endregion

        #region Fluent Methods

        public static Factory Create(DbConnection cn)
        {
            return Create(cn, null);
        }

        public static Factory Create(DbConnection cn, DbTransaction tx)
        {
            return new Factory { Command = cn.CreateCommand(), Connection = cn, Transaction = tx };
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

        public Factory WithCommandType(CommandType commandType)
        {
            this.m_CommandType = commandType;
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
            this.AddParameter(name, value, type, direction);
            return this;
        }

        public Factory WithParameters(Dictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    this.AddParameter(kvp.Key, kvp.Value);
                }
            }
            return this;
        }

        public Factory WithParameters(DataRow parameters)
        {
            if (parameters != null)
            {
                foreach (DataColumn c in parameters.Table.Columns)
                {
                    this.AddParameter(c.ColumnName, parameters[c.ColumnName]);
                }
            }
            return this;
        }

        public Factory WithParameters(object parameters)
        {
            if (parameters != null)
            {
                foreach (var pi in parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                {
                    if (pi.CanRead)
                    {
                        var indexParameters = pi.GetIndexParameters();
                        if (indexParameters == null || indexParameters.Length.Equals(0))
                        {
                            if (ConvertEnumsToStrings && pi.PropertyType.IsEnum)
                            {
                                this.AddParameter(pi.Name, Convert.ChangeType(pi.GetValue(parameters), Enum.GetUnderlyingType(pi.PropertyType)));
                            }
                            else
                            {
                                this.AddParameter(pi.Name, pi.GetValue(parameters));
                            }
                        }
                        else
                        {
                            // Attempt to get the first value from the indexed property...
                            if (indexParameters[0].ParameterType.Equals(typeof(string)))
                            {
                                this.AddParameter(pi.Name, pi.GetValue(parameters, new object[] { string.Empty }));
                            }
                            else if (indexParameters[0].ParameterType.Equals(typeof(byte)))
                            {
                                this.AddParameter(pi.Name, pi.GetValue(parameters, new object[] { (byte)0 }));
                            }
                            else if (indexParameters[0].ParameterType.Equals(typeof(short)))
                            {
                                this.AddParameter(pi.Name, pi.GetValue(parameters, new object[] { (short)0 }));
                            }
                            else if (indexParameters[0].ParameterType.Equals(typeof(int)))
                            {
                                this.AddParameter(pi.Name, pi.GetValue(parameters, new object[] { (int)0 }));
                            }
                            else if (indexParameters[0].ParameterType.Equals(typeof(long)))
                            {
                                this.AddParameter(pi.Name, pi.GetValue(parameters, new object[] { (long)0 }));
                            }
                        }
                    }
                }
            }
            return this;
        }

        public Factory WithParameterMapping(string source, string target)
        {
            if (this.ParameterMappings.ContainsKey(source))
            {
                this.ParameterMappings[source] = target;
            }
            else
            {
                this.ParameterMappings.Add(source, target);
            }
            return this;
        }

        public Factory WithParameterMapping(Dictionary<string, string> parameterMap)
        {
            if (parameterMap != null)
            {
                foreach (var kvp in parameterMap)
                {
                    this.WithParameter(kvp.Key, kvp.Value);
                }
            }
            return this;
        }

        public Factory WithParameterPrefix(string prefix)
        {
            this.m_ParameterPrefix = prefix;
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
                this.UpdateOutParameters();
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
            object result = null;
            this.ConfigureCommand(sql);
            using (var csm = new ConnectionStateManager(this.Command.Connection))
            {
                result = this.Command.ExecuteScalar();
                this.UpdateOutParameters();
            }
            return result;
        }

        public bool FillDataTable(string sql, ref DataTable dt)
        {
            this.ConfigureCommand(sql);
            using (var csm = new ConnectionStateManager(this.Command.Connection))
            {
                dt.BeginLoadData();
                dt.Load(this.Command.ExecuteReader());
                dt.EndLoadData();
            }
            this.UpdateOutParameters();
            return true;
        }

        public DataTable GetDataTable(string sql)
        {
            var result = new DataTable();
            this.FillDataTable(sql, ref result);
            return result;
        }

        #endregion

    }
}
