using System.Collections.Generic;

namespace MultiDBQ
{
    public interface ISmoTasks
    {
        List<string> GetDatabases(SqlConnectionString connectionString);
    }
}
