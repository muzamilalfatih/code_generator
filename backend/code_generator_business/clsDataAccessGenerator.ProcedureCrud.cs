

using shared_classes;
using System.Text;

namespace code_generator_business
{
    internal partial class clsDataAccessGenerator
    {
        private static string _GenerateParameters(IGrouping<string, ProcedureInfoDTO> procedure, string className)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var p in procedure)
            {
                if (p.parameterName == null)
                    continue;
                if (p.parameterName.Contains("id", StringComparison.OrdinalIgnoreCase) && procedure.Count() == 1)
                    sb.AppendLine($"                    command.Parameters.AddWithValue(\"{p.parameterName}\", id);");
                else
                {
                    if (p.procedureName.Equals("Gender",StringComparison.OrdinalIgnoreCase))
                        sb.AppendLine($"                    command.Parameters.AddWithValue(\"{p.parameterName}\", (byte){clsUtil.ToCamel(className)}DTO.{clsUtil.ToCamel(p.parameterName.Substring(1))});");
                    else
                        sb.AppendLine($"                    command.Parameters.AddWithValue(\"{p.parameterName}\", {clsUtil.ToCamel(className)}DTO.{clsUtil.ToCamel(p.parameterName.Substring(1))});");
                }
            }
            return sb.ToString();
        }
        private static string _GenerateBasicCrudCode(IGrouping<string, ProcedureInfoDTO> procedure, string className)
        {
            StringBuilder sb = new StringBuilder();


            sb.AppendLine("            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            sb.AppendLine("            {");
            sb.AppendLine($"                using (SqlCommand command = new SqlCommand(\"{procedure.Key}\", connection))");
            sb.AppendLine("                {");
            sb.AppendLine("                    command.CommandType = CommandType.StoredProcedure;");
            string parameters = _GenerateParameters(procedure, className);
            sb.AppendLine(parameters);

            return sb.ToString();
        }
        private static string _GenerateAddNewMethod(IGrouping<string, ProcedureInfoDTO> procedure, string className)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"        public static async Task<Result<int>> AddNew{className}Async({className}DTO {clsUtil.ToCamel(className)}DTO)");
            sb.AppendLine("         {");
            string basicCode = _GenerateBasicCrudCode(procedure, className);
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
        private static string _GenerateUpdateMethod(IGrouping<string, ProcedureInfoDTO> procedure, string className)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"        public static async Task<Result<int>> Update{className}Async({className}DTO {clsUtil.ToCamel(className)}DTO)");
            sb.AppendLine("        {");
            string basicCode = _GenerateBasicCrudCode(procedure, className);
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
        private static string _GenerateDeleteMethod(IGrouping<string, ProcedureInfoDTO> procedure, string className)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"        public static async Task<Result<bool>> Delete{className}Async(int id)");
            sb.AppendLine("        {");
            string basicCode = _GenerateBasicCrudCode(procedure, className);
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
        private static string _GenerateGetInfoByIDMethod(IGrouping<string, ProcedureInfoDTO> procedure, string className, IGrouping<string, TableColumnInfoDTO> table)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"        public static async Task<Result<{className}DTO>> Get{className}InfoByIDAsync(int id)");
            sb.AppendLine("        {");
            string basicCode = _GenerateBasicCrudCode(procedure, className);
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
                {
                    if (table.Key.Equals("people",StringComparison.InvariantCulture) && column.columnName.Equals("Gender",StringComparison.OrdinalIgnoreCase))
                        stringBuilder.AppendLine($"                                    (enGender)reader.{method}(reader.GetOrdinal(\"{column.columnName}\")),");
                    else
                        stringBuilder.AppendLine($"                                     reader.{method}(reader.GetOrdinal(\"{column.columnName}\")),");
                }
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
        private static string _GenerateGetAllMethod(IGrouping<string, ProcedureInfoDTO> procedure, string className, IGrouping<string, viewInfoDTO> view)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"        public static async Task<Result<List<{className}ViewDTO>>> GetAll{className}sAsync()");
            sb.AppendLine("        {");
            string basicCode = _GenerateBasicCrudCode(procedure, className);
            sb.AppendLine(basicCode);
            sb.AppendLine("                     try");
            sb.AppendLine("                     {");
            sb.AppendLine("                         await connection.OpenAsync();");
            sb.AppendLine("                          using (SqlDataReader reader = await command.ExecuteReaderAsync())");
            sb.AppendLine("                          {");
            sb.AppendLine($"                             List<{className}ViewDTO> {clsUtil.ToCamel(className)}sList = new List<{className}ViewDTO>();");
            sb.AppendLine("                              while (reader.Read())");
            sb.AppendLine("                              {");
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

    }
}
