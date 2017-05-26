using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seatown.Data
{
    /// <summary>
    /// Manages connection state for common command methods similar to how a data adapter works.
    /// </summary>
    public class Factory
    {

        #region Properties

        public static bool AddNewParameters { get; set; } = false;
        public static bool ConvertEmptyStringsToNull { get; set; } = true;
        public static bool InitializeParametersToNull { get; set; } = true;
        public static string ParameterPrefix { get; set; } = "@";

        private static List<string> m_DatabaseProviderList = new List<string>()
        {
            "System.Data.Odbc",
            "System.Data.OleDb",
            "System.Data.OracleClient",
            "System.Data.SqlClient",
            "System.Data.SqlServerCe.3.5",
            "System.Data.SqlServerCe.4.0"
        };

        public int CommandTimeout { get; set; } = 30;

        private static string m_DatabaseProvider = string.Empty;
        public static string DatabaseProvider
        {
            get
            {
                return m_DatabaseProvider;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || m_DatabaseProviderList.Contains(value, StringComparer.CurrentCultureIgnoreCase))
                {
                    m_DatabaseProvider = value;
                }
                else
                {
                    throw new ArgumentException($"Invalid database provider: {value}");
                }
            }
        }

        #endregion

        #region Parameters

        public static IDbDataParameter CreateCommandParameter(IDbCommand cmd, string parameterName, DbType parameterType, object parameterValue)
        {
            IDbDataParameter result = null;
            try
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

                result = cmd.CreateParameter();
                result.ParameterName = prefixedParameterName;
                result.SourceColumn = parameterName;
                result.DbType = parameterType;
                result.Value = parameterValue;
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        #endregion

        #region Routines

        protected static CommandType PredictCommandType(string sql)
        {
            var result = CommandType.Text;
            try
            {
                if (!sql.Trim().Contains(" "))
                {
                    result = CommandType.StoredProcedure;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        protected static string PredictDatabaseProvider(DbConnection cn)
        {
            var result = DatabaseProvider;
            try
            {
                if (string.IsNullOrWhiteSpace(result))
                {
                    // Connection types include the owning namespace and the object type, so a SqlClient connection would have the type
                    // System.Data.SqlClient.SqlConnection, so we have to strip off the object type to get the underlying namespace.
                    string connectionType = cn.GetType().ToString();
                    connectionType = connectionType.Substring(0, connectionType.LastIndexOf("."));
                    result = m_DatabaseProviderList.Where((s) => s.StartsWith(connectionType)).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        #endregion


    }
}
