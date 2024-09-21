using System.Collections.Generic;
using System.Data.SqlClient;

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
                SqlCommand command = new SqlCommand("SELECT name FROM MASTER.sys.sysdatabases WHERE name <> 'master' and name <> 'tempdb' and name <> 'model' and name <> 'msdb' and (status = 65544 or status = 65536)", conn);
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
