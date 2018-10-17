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
        private bool closeConnectionHere = false;
        private DbConnection connection = null;

        public ConnectionStateManager(DbConnection cn)
        {
            this.connection = cn;
            this.closeConnectionHere = ConnectionStateManager.OpenConnection(this.connection);
        }

        public void Dispose()
        {
            if (closeConnectionHere)
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
