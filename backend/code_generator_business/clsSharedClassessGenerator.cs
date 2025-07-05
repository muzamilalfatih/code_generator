using Microsoft.Identity.Client;
using shared_classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace code_generator_business
{
    public class clsSharedClassessGenerator
    {
        public static void GenerateSharedClasses( IGrouping<string, TableColumnInfoDTO> table,  IEnumerable<IGrouping<string, viewInfoDTO>>? views)
        {
            _GenerateTablesDTOs( table,  views);
            if (table.Key.Equals("People", StringComparison.OrdinalIgnoreCase))
                _GenerateGenderEnum();
        }
        private static void _GenerateViewsDTOs( IEnumerable<IGrouping<string, viewInfoDTO>> views,  string className)
        {
            foreach (var view in views)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"namespace {clsUtil.SharedClassessProjectName}");
                sb.AppendLine("{");
                sb.AppendLine($"    public class {className}ViewDTO");
                sb.AppendLine("     {");
                foreach (var c in view)
                {
                    sb.AppendLine($"            public {clsUtil.MapSqlToCSharpDataType(c.columnType, false)} {clsUtil.ToCamel(c.columnName)} {{ get; set; }}");
                }
                
                // create the conscrotror
                sb.AppendLine($"            public {className}ViewDTO(" + string.Join(", ", view.Select(c => $@"{clsUtil.MapSqlToCSharpDataType(c.columnType, false)} {clsUtil.ToCamel(c.columnName)}")) + ")");
                sb.AppendLine("             {");
                foreach (var c in view)
                {
                    sb.AppendLine($"                this.{clsUtil.ToCamel(c.columnName)} = {clsUtil.ToCamel(c.columnName)};");
                }
                sb.AppendLine("             }");
                sb.AppendLine("     }");
                sb.AppendLine("}");
                File.WriteAllText($"{clsUtil.SharedClassessProjectName}/{className}ViewDTO.cs", sb.ToString());
            }
        }
        private static void _GenerateTablesDTOs( IGrouping<string, TableColumnInfoDTO> table,  IEnumerable<IGrouping<string, viewInfoDTO>>? views)
        {
            string className;
            if (table.Key.Equals("People", StringComparison.OrdinalIgnoreCase))
                 className = "Person";
            else 
                 className = table.Key.Substring(0, table.Key.Length - 1);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"namespace {clsUtil.SharedClassessProjectName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className}DTO");
            sb.AppendLine("     {");
            foreach (var c in table)
            {
                if (c.columnName.Equals("Gender",StringComparison.OrdinalIgnoreCase))
                    sb.AppendLine($"        public enGender {clsUtil.ToCamel(c.columnName)} {{ get; set; }}");
                else
                    sb.AppendLine($"        public {clsUtil.MapSqlToCSharpDataType(c.dataType, c.isNullable)} {clsUtil.ToCamel(c.columnName)} {{ get; set; }}");
            }
            // add the construtor
            sb.AppendLine($"        public {className}DTO(" + string.Join(", ", table.Select(c => $@"{(c.columnName.Equals("Gender",StringComparison.OrdinalIgnoreCase) ? "enGender" : clsUtil.MapSqlToCSharpDataType(c.dataType, c.isNullable))} {clsUtil.ToCamel(c.columnName)}")) + ")");

            sb.AppendLine("         {");
            foreach (var c in table)
            {
                sb.AppendLine($"             this.{clsUtil.ToCamel(c.columnName)} = {clsUtil.ToCamel(c.columnName)};");
            }
            sb.AppendLine("         }");
            sb.AppendLine("     }");
            sb.AppendLine("}");
            File.WriteAllText($"{clsUtil.SharedClassessProjectName}/{className}DTO.cs", sb.ToString());
            if (views != null)
            _GenerateViewsDTOs(views,  className );
            
        }
        public static void GenerateResultClass()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"namespace {clsUtil.SharedClassessProjectName}");
            sb.AppendLine("{");
            sb.AppendLine("     public class Result<T>");
            sb.AppendLine("     {");
            sb.AppendLine("         public bool success { get; set; }");
            sb.AppendLine("         public  string message { get; set; }");
            sb.AppendLine("         public int errorCode { get; set; }");
            sb.AppendLine("         public T? data { get; set; }");
            sb.AppendLine("         public Result(bool success, string message, T? data = default, int errorCode = 0)");
            sb.AppendLine("         {");
            sb.AppendLine("             this.success = success;");
            sb.AppendLine("             this.message = message;");
            sb.AppendLine("             this.errorCode = errorCode;");
            sb.AppendLine("             this.data = data;");
            sb.AppendLine("         }");
            sb.AppendLine("     }");
            sb.AppendLine("}");

            File.WriteAllText($"{clsUtil.SharedClassessProjectName}/Result.cs", sb.ToString());
        }
        private static void _GenerateGenderEnum()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"namespace {clsUtil.SharedClassessProjectName}");
            sb.AppendLine("{");
            sb.AppendLine("         public enum enGender : byte { Unknown = 0, Male = 1, Female = 2 };");
            sb.AppendLine("}");

            File.WriteAllText($"{clsUtil.SharedClassessProjectName}/enGender.cs", sb.ToString());
        }
    }
}
