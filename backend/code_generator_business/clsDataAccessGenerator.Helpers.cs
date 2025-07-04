

using System.Text;

namespace code_generator_business
{
    internal partial class clsDataAccessGenerator
    {
        private static string _GenerateExeptionBlock(string dataType)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("                     catch (Exception ex)");
            sb.AppendLine("                     {");
            sb.AppendLine($"                         return new Result<{dataType}>(false, \"An unexpected error occurred on the server.\", {(dataType == "bool" ? "false" : (dataType == "int" ? "-1" : "null"))}, 500);");
            sb.AppendLine("                     }");

            return sb.ToString();
        }
        private static readonly Dictionary<string, string> _SqlToReaderMethodMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["int"] = "GetInt32",
            ["bigint"] = "GetInt64",
            ["smallint"] = "GetInt16",
            ["tinyint"] = "GetByte",
            ["bit"] = "GetBoolean",
            ["decimal"] = "GetDecimal",
            ["numeric"] = "GetDecimal",
            ["float"] = "GetDouble",
            ["real"] = "GetFloat",
            ["money"] = "GetDecimal",
            ["smallmoney"] = "GetDecimal",
            ["char"] = "GetString",
            ["varchar"] = "GetString",
            ["text"] = "GetString",
            ["nchar"] = "GetString",
            ["nvarchar"] = "GetString",
            ["ntext"] = "GetString",
            ["datetime"] = "GetDateTime",
            ["smalldatetime"] = "GetDateTime",
            ["date"] = "GetDateTime",
            ["time"] = "GetTimeSpan",
            ["datetime2"] = "GetDateTime",
            ["datetimeoffset"] = "GetDateTimeOffset",
            ["uniqueidentifier"] = "GetGuid",
            ["binary"] = "GetBytes",
            ["varbinary"] = "GetBytes",
            ["image"] = "GetBytes",
            ["sql_variant"] = "GetValue"
        };
    }
}
