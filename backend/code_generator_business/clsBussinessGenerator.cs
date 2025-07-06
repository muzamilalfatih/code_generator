using Azure.Core;
using shared_classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace code_generator_business
{
    internal class clsBussinessGenerator
    {
        public static void GenerateUtilClass()
        {
            var sb = new StringBuilder();

            sb.AppendLine("public static class Utility");
            sb.AppendLine("{");
            sb.AppendLine("    static public string ComputeHash(string input)");
            sb.AppendLine("    {");
            sb.AppendLine("        //SHA is Secutred Hash Algorithm.");
            sb.AppendLine("        // Create an instance of the SHA-256 algorithm");
            sb.AppendLine("        using (SHA256 sha256 = SHA256.Create())");
            sb.AppendLine("        {");
            sb.AppendLine("            // Compute the hash value from the UTF-8 encoded input string");
            sb.AppendLine("            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));");
            sb.AppendLine("");
            sb.AppendLine("            // Convert the byte array to a lowercase hexadecimal string");
            sb.AppendLine("            return BitConverter.ToString(hashBytes).Replace(\"-\", \"\").ToLower();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            File.WriteAllText($"{clsUtil.BussiessProjectName}/clsUtility.cs", sb.ToString());
        }
        public static void GenerateBussiness( IGrouping<string, TableColumnInfoDTO> table, IGrouping<string,viewInfoDTO>? view)
        {
            string className;
            if (table.Key.Equals("People", StringComparison.OrdinalIgnoreCase))
                className = "Person";
            else
                className = table.Key.Substring(0, table.Key.Length - 1);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System.Security.Cryptography;;");
            sb.AppendLine("using System.Text;;");
            sb.AppendLine("using SharedClasses;");
            sb.AppendLine($"using {clsUtil.DataAcessProjectName};");
            sb.AppendLine($"namespace {clsUtil.BussiessProjectName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class cls{className}");
            sb.AppendLine("     {");
            sb.AppendLine("         public enum enMode { AddNew = 0,Update = 1 };");
            sb.AppendLine("         private enMode _mode;");
            foreach (var c in table)
            {
                if (c.columnName.Equals("Gender",StringComparison.OrdinalIgnoreCase))
                    sb.AppendLine($"        public enGender {clsUtil.ToCamel(c.columnName)} {{ get; set; }}");
                else 
                    sb.AppendLine($"        public {clsUtil.MapSqlToCSharpDataType(c.dataType, c.isNullable)} {clsUtil.ToCamel(c.columnName)} {{ get; set; }}");
            }

            // Generate construtor
            sb.AppendLine($"        public cls{className}({className}DTO {clsUtil.ToCamel(className)}DTO, enMode mode = enMode.AddNew)");
            sb.AppendLine("        {");
            foreach (var c in table)
            {
                if (className.Equals("User",StringComparison.OrdinalIgnoreCase) && c.columnName.Equals("Password",StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine("             if (mode == enMode.AddNew)");
                    sb.AppendLine("             {");
                    sb.AppendLine("                 this.Password = Utility.ComputeHash(userDTO.password);");
                    sb.AppendLine("             }");
                    sb.AppendLine("             else");
                    sb.AppendLine("             {");
                    sb.AppendLine("                 this.password = userDTO.password;");
                    sb.AppendLine("             }");
                }
                else
                    sb.AppendLine($"            this.{clsUtil.ToCamel(c.columnName)} = {clsUtil.ToCamel(className)}DTO.{clsUtil.ToCamel(c.columnName)};");
            }
            sb.AppendLine("             this._mode = mode;");
            sb.AppendLine("         }");
            sb.AppendLine($"        public {className}DTO {className.Substring(0, 1)}DTO");
            sb.AppendLine("         {");
            sb.AppendLine("             get");
            sb.AppendLine("                 {");
            sb.AppendLine($"                    return new {className}DTO(" + string.Join(", ", table.Select(c => $"this.{clsUtil.ToCamel(c.columnName)}")) + ");");
            sb.AppendLine("                 }");
            sb.AppendLine("         }");
            string crud = _GenerateCRUD(table, view);
            sb.AppendLine(crud);
            sb.AppendLine("     }");
            sb.AppendLine("}");
            File.WriteAllText($"{clsUtil.BussiessProjectName}/cls{className}.cs", sb.ToString());
        }
        private static string _GenerateCRUD(IGrouping<string, TableColumnInfoDTO> table, IGrouping<string, viewInfoDTO>? view)
        {
            string className;
            if (table.Key.Contains("people", StringComparison.OrdinalIgnoreCase))
                className = "Person";
            else
                className = table.Key.Substring(0, table.Key.Length - 1);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(_GenerateFindMethod(className));
            sb.AppendLine(_GenerateAddNewMethod(className));
            sb.AppendLine(_GenerateUpdateMethod(className));
            sb.AppendLine(_GenerateDeleteMethod(className));
            if (view != null)
                sb.AppendLine(_GenerateGetAllMethod(className));    
            sb.AppendLine(_GenerateSaveMethod(className));

            return sb.ToString();
        }

        private static string _GenerateFindMethod(string className)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"        static public async Task<Result<cls{className}>> FindAsync(int id)");
            sb.AppendLine("         {");
            sb.AppendLine($"            if (id <= 0)");
            sb.AppendLine("             {");
            sb.AppendLine($"                return new Result<cls{className}>(false, \"The request is invalid. Please check the input and try again.\", null, 400);");
            sb.AppendLine("             }");
            sb.AppendLine($"            Result<{className}DTO> result = await cls{className}Data.Get{className}InfoByIDAsync(id); ");
            sb.AppendLine("             if (result.success)");
            sb.AppendLine("                 {");
            sb.AppendLine($"                    return new Result<cls{className}>(true, \"{className} Found.\", new cls{className}(result.data, enMode.Update));");
            sb.AppendLine("                 }");
            sb.AppendLine($"            return new Result<cls{className}>(result.success, result.message, null, result.errorCode);");
            sb.AppendLine("         }");

            return sb.ToString();
        }
        private static string _GenerateAddNewMethod(string className)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"        private async Task<Result<int>> _AddNew{className}Async()");
            sb.AppendLine("         {");
            sb.AppendLine($"            return await cls{className}Data.AddNew{className}Async(this.{className.Substring(0, 1)}DTO);");
            sb.AppendLine("         }");

            return sb.ToString();
        }
        private static string _GenerateUpdateMethod(string className)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"        private async Task<Result<int>> _Update{className}Async()");
            sb.AppendLine("         {");
            sb.AppendLine($"            return await cls{className}Data.Update{className}Async(this.{className.Substring(0, 1)}DTO);");
            sb.AppendLine("         }");

            return sb.ToString();
        }
        private static string _GenerateDeleteMethod(string className)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"        public static async Task<Result<bool>> Delete{className}Async(int id)");
            sb.AppendLine("        {");
            sb.AppendLine($"            if (id <= 0)");
            sb.AppendLine("             {");
            sb.AppendLine($"                return new Result<bool>(false, \"The request is invalid. Please check the input and try again.\", false, 400);");
            sb.AppendLine("             }");
            sb.AppendLine($"            return await cls{className}Data.Delete{className}Async(id);");
            sb.AppendLine("        }");

            return sb.ToString();

        }
        private static string _GenerateGetAllMethod (string className)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"        public static async Task<Result<List<{className}ViewDTO>>> GetAll{className}sAsync()");
            sb.AppendLine("        {");
            sb.AppendLine($"            return await cls{className}Data.GetAll{className}sAsync();");
            sb.AppendLine("        }");

            return sb.ToString();
        }
        private static string _GenerateSaveMethod(string className)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("        public async Task<Result<int>> SaveAsync()");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (_mode)");
            sb.AppendLine("            {");
            sb.AppendLine("                case enMode.AddNew:");
            sb.AppendLine($"                    Result<int> result = await _AddNew{className}Async();");
            sb.AppendLine("                    if (result.success)");
            sb.AppendLine("                    {");
            sb.AppendLine("                        _mode = enMode.Update;");
            sb.AppendLine("                        this.id = result.data;");
            sb.AppendLine("                    }");
            sb.AppendLine("                    return result;");
            sb.AppendLine("                case enMode.Update:");
            sb.AppendLine($"                    return await _Update{className}Async();");
            sb.AppendLine("                default:");
            sb.AppendLine("                    return new Result<int>(false, \"An unexpected error occurred on the server.\", -1, 500);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            return sb.ToString();
        }
    } }
