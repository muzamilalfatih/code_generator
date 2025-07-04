using shared_classes;
using System.Text;

namespace code_generator_business
{
    internal partial class clsDataAccessGenerator
    {
        private static string _GenerateRawParameters(IGrouping<string, TableColumnInfoDTO> table, string className, enQueryType queryType = enQueryType.add)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in table)
            {
                 if (c.columnName.Equals("id", StringComparison.OrdinalIgnoreCase) && table.Count() > 1 && queryType == enQueryType.add)
                    continue;
                else
                    sb.AppendLine($"                    command.Parameters.AddWithValue(\"@{c.columnName}\", {clsUtil.ToCamel(className)}DTO.{clsUtil.ToCamel(c.columnName)});");
            }
            return sb.ToString();
        }
        private static string _GenerateBasicRawCode(string query, string className, enQueryType queryType = enQueryType.add, IGrouping<string, TableColumnInfoDTO> table = null)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("         using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            sb.AppendLine("         {");
            sb.AppendLine($"            string query = @\"{query}\";");
            sb.AppendLine("             using (SqlCommand command = new SqlCommand(query, connection))");
            sb.AppendLine("             {");
            if (table != null)
            {
                string parameters = _GenerateRawParameters(table, className, queryType);
                sb.AppendLine(parameters);
            }
            else
            {
                sb.AppendLine($"                    command.Parameters.AddWithValue(\"@id\", id);");
            }

            return sb.ToString();

        }

        private static string _GenerateRawGetAllMethod(IGrouping<string, viewInfoDTO> view, string className)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"public static async Task<Result<List<{className}ViewDTO>>> GetAll{className}sAsync()");
            sb.AppendLine("{");
            string query = $"SELECT * FROM {className}s_View;";
            sb.AppendLine(_GenerateBasicRawCode(query, className));
            sb.AppendLine($"        List<{className}ViewDTO> {clsUtil.ToCamel(className)}sList = new List<{className}ViewDTO>();");
            sb.AppendLine("         try");
            sb.AppendLine("         {");
            sb.AppendLine("             await connection.OpenAsync();");
            sb.AppendLine("             using (var reader = await command.ExecuteReaderAsync())");
            sb.AppendLine("             {");
            sb.AppendLine("                 while (await reader.ReadAsync())");
            sb.AppendLine("                 {");
            sb.AppendLine($"                                 {clsUtil.ToCamel(className)}sList.Add(new {className}ViewDTO(");
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var c in view)
            {
                string method = _SqlToReaderMethodMap.TryGetValue(c.columnType, out var m) ? m : "GetValue";
                if (method.Contains("GetDecimal", StringComparison.OrdinalIgnoreCase))
                    stringBuilder.AppendLine($"                                     Convert.ToSingle(reader.{method}(reader.GetOrdinal(\"{c.columnName}\"))),");
                else
                    stringBuilder.AppendLine($"                                     reader.{method}(reader.GetOrdinal(\"{c.columnName}\")),");
            }
            int lastNewLine = stringBuilder.ToString().LastIndexOf(Environment.NewLine);
            if (lastNewLine >= 0)
                stringBuilder.Length = lastNewLine;
            if (stringBuilder.Length > 0)
                stringBuilder.Length--;
            sb.AppendLine(stringBuilder.ToString());
            sb.AppendLine("                                  ));");
            sb.AppendLine("                              }");
            sb.AppendLine($"                             if ({clsUtil.ToCamel(className)}sList.Count == 0)");
            sb.AppendLine("                              {");
            sb.AppendLine($"                                 return new Result<List<{className}ViewDTO>>(false, \"Data not found.\", null, 404);");
            sb.AppendLine("                              }");
            sb.AppendLine($"                             return new Result<List<{className}ViewDTO>>(true, \"Data found successfully\", {clsUtil.ToCamel(className)}sList);");
            sb.AppendLine("                          }");
            sb.AppendLine("                     }");
            string exeptionBlock = _GenerateExeptionBlock($"List<{className}ViewDTO>");
            sb.AppendLine(exeptionBlock);
            sb.AppendLine("                 }"); ;
            sb.AppendLine("            }");
            sb.AppendLine("         }");

            return sb.ToString();

        }
        private static string _GenerateRawGetByIdMethod(string className, IGrouping<string, TableColumnInfoDTO> table)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"        public static async Task<Result<{className}DTO>> Get{className}InfoByIDAsync(int id)");
            sb.AppendLine("        {");
            string query = $"SELECT * FROM {(className.Contains("person", StringComparison.OrdinalIgnoreCase) ? "People" : className + "s")} WHERE Id = @id";
            string basicCode = _GenerateBasicRawCode(query, className);
            sb.AppendLine(basicCode);
            sb.AppendLine("                     try");
            sb.AppendLine("                     {");
            sb.AppendLine("                         await connection.OpenAsync();");
            sb.AppendLine("                         using (SqlDataReader reader = await command.ExecuteReaderAsync())");
            sb.AppendLine("                         {");
            sb.AppendLine("                             if (await reader.ReadAsync())");
            sb.AppendLine("                             {");
            sb.AppendLine($"                                {className}DTO {clsUtil.ToCamel(className)}DTO =  new {className}DTO");
            sb.AppendLine("                                 (");
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var column in table)
            {
                string method = _SqlToReaderMethodMap.TryGetValue(column.dataType, out var m) ? m : "GetValue";
                if (method.Contains("GetDecimal", StringComparison.OrdinalIgnoreCase))
                    stringBuilder.AppendLine($"                                     Convert.ToSingle(reader.{method}(reader.GetOrdinal(\"{column.columnName}\"))),");
                else
                    stringBuilder.AppendLine($"                                     reader.{method}(reader.GetOrdinal(\"{column.columnName}\")),");
            }
            int lastNewLine = stringBuilder.ToString().LastIndexOf(Environment.NewLine);
            if (lastNewLine >= 0)
                stringBuilder.Length = lastNewLine;
            if (stringBuilder.Length > 0)
                stringBuilder.Length--;
            sb.AppendLine(stringBuilder.ToString());
            sb.AppendLine("                                 );");
            sb.AppendLine($"                                return new Result<{className}DTO>(true, \"{className} found successfully\", {clsUtil.ToCamel(className)}DTO);");
            sb.AppendLine("                             }");
            sb.AppendLine("                             else");
            sb.AppendLine("                             {");
            sb.AppendLine($"                                return new Result<{className}DTO>(false, \"{className} not found.\", null, 404);");
            sb.AppendLine("                             }");
            sb.AppendLine("                         }");
            sb.AppendLine("                     }");
            string exeptionBlock = _GenerateExeptionBlock($"{className}DTO");
            sb.AppendLine(exeptionBlock);
            sb.AppendLine("                 }"); ;
            sb.AppendLine("            }");
            sb.AppendLine("         }");

            return sb.ToString();
        }
        private static string _GenerateRawAddNewMethod(string className, IGrouping<string, TableColumnInfoDTO> table)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"        public static async Task<Result<int>> AddNew{className}Async({className}DTO {clsUtil.ToCamel(className)}DTO)");
            sb.AppendLine("         {");
            string query = GenerateInsertQuery(table);
            string basicCode = _GenerateBasicRawCode(query, className, enQueryType.add, table);
            sb.AppendLine(basicCode);
            sb.AppendLine("                     try");
            sb.AppendLine("                     {");
            sb.AppendLine("                         await connection.OpenAsync();");
            sb.AppendLine("                        object result = await command.ExecuteScalarAsync();");
            sb.AppendLine("                        int id = result != DBNull.Value ? Convert.ToInt32(result) : 0;");
            sb.AppendLine($"                         if (id > 0)");
            sb.AppendLine("                         {");
            sb.AppendLine($"                             return new Result<int>(true, \"{className} added successfully.\", id);");
            sb.AppendLine("                          }");
            sb.AppendLine("                          else");
            sb.AppendLine("                          {");
            sb.AppendLine($"                             return new Result<int>(false, \"Failed to add {clsUtil.ToCamel(className)}.\", -1);");
            sb.AppendLine("                          }");
            sb.AppendLine("                     }");

            string exeptionBlock = _GenerateExeptionBlock("int");
            sb.AppendLine(exeptionBlock);
            sb.AppendLine("                 }"); ;
            sb.AppendLine("             }");
            sb.AppendLine("         }");
            return sb.ToString();
        }
        private static string _GenerateRawUpdateMethod(string className, IGrouping<string, TableColumnInfoDTO> table)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"        public static async Task<Result<int>> Update{className}Async({className}DTO {clsUtil.ToCamel(className)}DTO)");
            sb.AppendLine("        {");
            string query = GenerateUpdateQuery(table, "Id");
            string basicCode = _GenerateBasicRawCode(query, className, enQueryType.update, table);
            sb.AppendLine(basicCode);
            sb.AppendLine("                     try");
            sb.AppendLine("                     {");
            sb.AppendLine("                         await connection.OpenAsync();");
            sb.AppendLine("                        object result = await command.ExecuteScalarAsync();");
            sb.AppendLine("                        int rowAffected = result != DBNull.Value ? Convert.ToInt32(result) : 0;");
            sb.AppendLine("                         if (rowAffected > 0)");
            sb.AppendLine("                         {");
            sb.AppendLine($"                             return new Result<int>(true, \"{className} updated successfully.\", rowAffected);");
            sb.AppendLine("                          }");
            sb.AppendLine("                          else");
            sb.AppendLine("                          {");
            sb.AppendLine($"                             return new Result<int>(false, \"Failed to update {clsUtil.ToCamel(className)}.\", -1);");
            sb.AppendLine("                          }");
            sb.AppendLine("                     }");
            string exeptionBlock = _GenerateExeptionBlock("int");
            sb.AppendLine(exeptionBlock);
            sb.AppendLine("                 }"); ;
            sb.AppendLine("            }");
            sb.AppendLine("         }");

            return sb.ToString();
        }
        private static string _GenerateRawDeleteMethod(string className, IGrouping<string, TableColumnInfoDTO> table)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"        public static async Task<Result<bool>> Delete{className}Async(int id)");
            sb.AppendLine("        {");
            string query = $"DELETE FROM {table.Key} WHERE Id = @id";
            string basicCode = _GenerateBasicRawCode(query, className);
            sb.AppendLine(basicCode);
            sb.AppendLine("                     try");
            sb.AppendLine("                     {");
            sb.AppendLine("                         await connection.OpenAsync();");
            sb.AppendLine("                        object result = await command.ExecuteScalarAsync();");
            sb.AppendLine("                        int rowAffected = result != DBNull.Value ? Convert.ToInt32(result) : 0;");
            sb.AppendLine("                         if (rowAffected > 0)");
            sb.AppendLine("                         {");
            sb.AppendLine($"                             return new Result<bool>(true, \"{className} deleted successfully.\", true);");
            sb.AppendLine("                          }");
            sb.AppendLine("                          else");
            sb.AppendLine("                          {");
            sb.AppendLine($"                             return new Result<bool>(false, \"Failed to delete {clsUtil.ToCamel(className)}.\", false);");
            sb.AppendLine("                          }");
            sb.AppendLine("                     }");
            string exeptionBlock = _GenerateExeptionBlock("bool");
            sb.AppendLine(exeptionBlock);
            sb.AppendLine("                 }"); ;
            sb.AppendLine("            }");
            sb.AppendLine("         }");

            return sb.ToString();
        }
       
        public static string GenerateInsertQuery(IGrouping<string, TableColumnInfoDTO> table)
        {
            if (table == null || !table.Any())
                throw new ArgumentException("Table group cannot be null or empty.");

            string tableName = table.Key;

            var columnNames = new List<string>();
            foreach (var col in table)
            {
                if (col.columnName.Equals("id", StringComparison.OrdinalIgnoreCase))
                    continue;
                columnNames.Add(col.columnName);
            }

            var columnList = string.Join(Environment.NewLine + "      ,", columnNames);
            var paramList = string.Join(Environment.NewLine + "      ,@", columnNames);

            string query = $@"
INSERT INTO {tableName}
      (
      {columnList})
VALUES
      (
      @{paramList});
SELECT SCOPE_IDENTITY();";

            return query;
        }
        public static string GenerateUpdateQuery(IGrouping<string, TableColumnInfoDTO> table, string keyColumn)
        {
            if (table == null || !table.Any())
                throw new ArgumentException("Table group cannot be null or empty.");

            string tableName = table.Key;
            var setClauses = new List<string>();

            foreach (var col in table)
            {
                if (col.columnName.Equals(keyColumn, StringComparison.OrdinalIgnoreCase))
                    continue; // skip key column from SET clause
                setClauses.Add($"{col.columnName} = @{col.columnName}");
            }

            string setClauseJoined = string.Join("," + Environment.NewLine + "    ", setClauses);

            string query = $@"
UPDATE {tableName}
SET 
    {setClauseJoined}
WHERE Id = @id;";

            return query;
        }
    }
}
