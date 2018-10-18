using System;
using System.Data;
using System.Data.Common;

namespace Seatown.Data.Common
{
    /// <summary>
    /// Automatically manages the connected state of a DbConnection.
    /// </summary>
    public class ConnectionStateManager : IDisposable
    {
        private DbConnection connection = null;
        public bool CloseConnection { get; set; } = false;

        public ConnectionStateManager(DbConnection cn)
        {
            this.connection = cn;
            this.CloseConnection = ConnectionStateManager.OpenConnection(this.connection);
        }

        public void Dispose()
        {
            if (CloseConnection)
            {
                connection?.Close();
            }
        }

        internal static bool OpenConnection(DbConnection cn)
        {
            bool connectionWasClosed = false;
            if (cn != null && !cn.State.Equals(ConnectionState.Open) && !cn.State.Equals(ConnectionState.Connecting))
            {
                if (cn.State.Equals(ConnectionState.Broken))
                {
                    cn.Close();
                }
                cn.Open();
                connectionWasClosed = true;
            }
            return connectionWasClosed;
        }
    }
}
