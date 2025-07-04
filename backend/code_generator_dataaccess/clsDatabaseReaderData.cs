using Microsoft.Data.SqlClient;
using shared_classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace code_generator_dataaccess
{
    public class clsDatabaseReaderData
    {
        public static Result<List<string>> GetDatabases()
        {

            var databases = new List<string>();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString()))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT name FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')";
                    SqlCommand command = new SqlCommand(query, connection);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            databases.Add(reader.GetString(0));
                        }
                        if (databases.Count == 0)
                        {
                            return new Result<List<string>>(false, "No database found.", databases, 404);
                        }
                        return new Result<List<string>>(true, "Databases retrieved successfully.", databases);
                    }
                }
                catch (Exception ex)
                {
                    return new Result<List<string>>(false, "An unexpected error occurred on the server.", databases, 500);
                }

            }

        }
        public static Result<List<TableColumnInfoDTO>> GetTablesAndColumns(string database)
        {
            List<TableColumnInfoDTO> tables = new List<TableColumnInfoDTO>();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString(database)))
            {

                string query = @"SELECT 
    c.TABLE_NAME, 
    c.COLUMN_NAME, 
    c.DATA_TYPE, 
    CAST(CASE 
            WHEN c.IS_NULLABLE = 'YES' THEN 1 
            WHEN c.IS_NULLABLE = 'NO' THEN 0 
         END AS BIT) AS IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS c
JOIN INFORMATION_SCHEMA.TABLES t
    ON c.TABLE_NAME = t.TABLE_NAME
    AND c.TABLE_SCHEMA = t.TABLE_SCHEMA
WHERE t.TABLE_TYPE = 'BASE TABLE'
  AND t.TABLE_NAME <> 'sysdiagrams'
";
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tables.Add(new TableColumnInfoDTO(

                                    tableName: reader.GetString(0),
                                    columnName: reader.GetString(1),
                                    dataType: reader.GetString(2),
                                    isNullable: reader.GetBoolean(3))
                                );
                            }
                            if (tables.Count == 0)
                            {
                                return new Result<List<TableColumnInfoDTO>>(false, "No table found.", tables, 404);
                            }
                            return new Result<List<TableColumnInfoDTO>>(true, "Tables retrieved successfully.", tables); ;
                        }
                    }
                }
                catch (Exception ex)
                {
                        return new Result<List<TableColumnInfoDTO>>(false, "An unexpected error occurred on the server.", tables, 500);
                }

            }
        }
        public static Result<List<ProcedureInfoDTO>> GetStoredProcedureInfo(string database)
        {
            List<ProcedureInfoDTO> procedures = new List<ProcedureInfoDTO>();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString(database)))
            {
                string query = @"SELECT 
    r.SPECIFIC_NAME AS ProcedureName,
    p.PARAMETER_NAME,
    p.DATA_TYPE,
    p.PARAMETER_MODE
FROM 
    INFORMATION_SCHEMA.ROUTINES r
LEFT JOIN 
    INFORMATION_SCHEMA.PARAMETERS p 
    ON r.SPECIFIC_NAME = p.SPECIFIC_NAME 
    AND r.SPECIFIC_SCHEMA = p.SPECIFIC_SCHEMA
WHERE 
    r.ROUTINE_TYPE = 'PROCEDURE'
ORDER BY 
    r.SPECIFIC_NAME, p.ORDINAL_POSITION;
";

                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                procedures.Add(new ProcedureInfoDTO(
                                    procedureName: reader.GetString(0),
                                    parameterName: reader.IsDBNull(1) ? null : reader.GetString(1),
                                    dataType: reader.IsDBNull(2) ? null : reader.GetString(2),
                                    parameterMode: reader.IsDBNull(3) ? null : reader.GetString(3)
                                ));
                            }
                            if (procedures.Count == 0)
                            {
                                return new Result<List<ProcedureInfoDTO>>(false, "No procedure found.", procedures, 404);
                            }
                            return new Result<List<ProcedureInfoDTO>>(true, "Procedures retrieved successfully.", procedures);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new Result<List<ProcedureInfoDTO>>(false, "An unexpected error occurred on the server.", procedures, 500);
                }

            }
        }
        public static Result<List<viewInfoDTO>> GetViewInfo(string database)
        {
            List<viewInfoDTO> views = new List<viewInfoDTO>();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString(database)))
            {
                string query = @"select  TABLE_NAME, 
	                                COLUMN_NAME,DATA_TYPE
	                                from INFORMATION_SCHEMA.COLUMNS
	                                where TABLE_NAME in (
	                                select TABLE_NAME
	                                from INFORMATION_SCHEMA.VIEWS) ";

                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                views.Add(new viewInfoDTO(

                                    viewName : reader.GetString(0),
                                    columnName : reader.GetString(1),
                                    columnType : reader.GetString(2)

                                ));
                            }
                            if (views.Count == 0)
                            {
                                return new Result<List<viewInfoDTO>>(true, "No views found.", views, 404);
                            }
                            return new Result<List<viewInfoDTO>>(true, "Views retrieved successfully.", views);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new Result<List<viewInfoDTO>>(false, "An unexpected error occurred on the server.", views, 500);
                }

            }
        }
        public static Result<bool> IsDatabaseExist(string databaseName)
        {
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString()))
            {
                string query = "SELECT Found = 1 FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb') And name = @DatabaseName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DatabaseName", databaseName);
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            bool isFound = reader.HasRows;
                            return new Result<bool>(true, "Database existence check completed", isFound);
                        }
                    }
                    catch (Exception ex)
                    {
                        return new Result<bool>(false, "An unexpected error occurred on the server.", false, 500);
                    }
                }
            }
        }
    }
}
