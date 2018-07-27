using System;
using MySql.Data.MySqlClient;

namespace RawSql.Models
{
    public class Database : IDisposable
    {
        public MySqlConnection Connection;

        public Database(string connectionString)
        {
            Connection = new MySqlConnection(connectionString);
        }

        public void Dispose()
        {
            Connection.Close();
        }

    }
}
