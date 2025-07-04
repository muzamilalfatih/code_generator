using code_generator_dataaccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using shared_classes;

namespace code_generator_business
{
    public class clsDatabaseReader
    {
        public static Result<List<string>> GetDatabases()
        {
            return clsDatabaseReaderData.GetDatabases();
        }
        public static Result<List<TableColumnInfoDTO>> GetTables(string  database)
        {
            return clsDatabaseReaderData.GetTablesAndColumns(database);
        }
        public static Result<bool> IsDatabaseExist(string databaseName)
        {
            return clsDatabaseReaderData.IsDatabaseExist(databaseName);
        }
    }
}
