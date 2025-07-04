using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace code_generator_business
{
    public class clsUtil
    {
        public static string projectName { get; set; } = "MyApp";

        public static string projectPath
        {
            get
            {
                return @$"C:\Users\muzam\source\repos\generated-project\{projectName}";
            }
        }

        public static string APIProjectName
        {
            get
            {
                return $"{projectName}_API";
            }
        }

        public static string BussiessProjectName
        {
            get
            {
                return $"{projectName}_Bussiness";
            }
        }

        public static string DataAcessProjectName
        {
            get
            {
                return $"{projectName}_DataAccess";
            }
        }
            
        public static string SharedClassessProjectName
        {
            get
            {
                return $"SharedClasses";
            }
        } 

        public static string APIProjectPathControllers 
        {
            get
            {
                return $"{projectPath}/{APIProjectName}/Controllers";
            }
        } 
        public static string BusinessProjectPath
        {
            get
            {
                return $"{projectPath}/{APIProjectName}";
            }
        } 
        public static string DataAccessProjectPath
        {
            get
            {
                return $"{projectPath}/{DataAcessProjectName}";
            }
        } 
        public static string SharedClassessProjectPath
        {
            get
            {
                return $"{projectPath}/{SharedClassessProjectName}"; 
            }
        } 

        public static void IntialSettings(string database)
        {
            _CreateDatabaseSettingClass(database);
            _CreateResultClass();
        }
        private static void _CreateResultClass()
        {
            string result = @"
                    public class Result<T>
                    {
                        public bool success { get; set; }
                        public  string sessage { get; set; }
                        public int srrorCode { get; set; }
                        public T? data { get; set; }
                        public Result(bool success, string message, T? data = default, int errorCode = 0)
                        {
                            this.success = success;
                            this.message = message;
                            this.errorCode = errorCode;
                            this.data = data;
                        }


                    }
                ";
            File.WriteAllText($"{clsUtil.SharedClassessProjectName}/Result.cs", result);
        }
        private static void _CreateDatabaseSettingClass(string database)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("public class clsDataAccessSettings");
            sb.AppendLine("{");
            sb.AppendLine("     static public string ConnectionString");
            sb.AppendLine("     {");
            sb.AppendLine("         get");
            sb.AppendLine("         {");
            sb.AppendLine($"             return \"Server=localhost;Database={database};User Id=sa;Password=sa123456;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;\";\r\n");
            sb.AppendLine("         }");
            sb.AppendLine("     }");
            sb.AppendLine("}");
            File.WriteAllText($"{clsUtil.DataAcessProjectName}/clsDataAccessSettings.cs", sb.ToString());
        }
        public static string MapSqlToCSharpDataType(string sqlType, bool isNullable)
        {
            string baseType;

            switch (sqlType.ToLower())
            {
                case "int":
                    baseType = "int";
                    break;

                case "bigint":
                    baseType = "long";
                    break;

                case "smallint":
                    baseType = "short";
                    break;

                case "tinyint":
                    baseType = "byte";
                    break;

                case "bit":
                    baseType = "bool";
                    break;

                case "decimal":
                case "numeric":
                case "money":
                case "smallmoney":
                    baseType = "float";
                    break;

                case "float":
                    baseType = "double";
                    break;

                case "real":
                    baseType = "float";
                    break;

                case "char":
                case "varchar":
                case "nchar":
                case "nvarchar":
                case "text":
                case "ntext":
                    baseType = "string";
                    break;

                case "datetime":
                case "datetime2":
                case "smalldatetime":
                case "date":
                case "time":
                    baseType = "DateTime";
                    break;

                case "uniqueidentifier":
                    baseType = "Guid";
                    break;

                case "binary":
                case "varbinary":
                case "image":
                    baseType = "byte[]";
                    break;

                default:
                    baseType = "object"; // fallback for unrecognized types
                    break;
            }

            // Make value types nullable if the column is nullable
            if (isNullable && baseType != "string" && baseType != "byte[]")
            {
                baseType += "?";
            }

            return baseType;
        }
        public static string ToCamel(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length < 2)
                return name.ToLower();
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }
}
