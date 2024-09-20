using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MultiDBQ
{
    public class SmoTasks : ISmoTasks
    {
  

        public List<string> GetDatabases(SqlConnectionString connectionString)
        {
            var databases = new List<string>();

            using (var conn = new SqlConnection(connectionString.WithDatabase("master")))
            {
                conn.Open();
                SqlCommand command = new SqlCommand("SELECT name FROM MASTER.sys.sysdatabases", conn);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        databases.Add(reader.GetString(0));
                    }
                }
            }

            return databases;
        }

    }
}
