using shared_classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace code_generator_business
{
    internal partial class clsDataAccessGenerator
    {
        private enum enQueryType { add,update};

        public static void GenerateDataAccess(IGrouping<string, TableColumnInfoDTO> table,  IEnumerable<IGrouping<string, ProcedureInfoDTO>> procedures,
                                                     IGrouping<string, viewInfoDTO>? view)
        {
            string className;
            if (table.Key.Equals("People", StringComparison.OrdinalIgnoreCase))
                className = "Person";
            else
                className = table.Key.Substring(0, table.Key.Length - 1);

            IEnumerable<IGrouping<string, ProcedureInfoDTO>> relatedProcedure = procedures.Where(p => p.Key.Contains(className, StringComparison.OrdinalIgnoreCase));
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"using Microsoft.Data.SqlClient;");
            sb.AppendLine($"using SharedClasses;");
            sb.AppendLine($"using System.Data;");
            sb.AppendLine($"namespace {clsUtil.DataAcessProjectName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class cls{className}Data");
            sb.AppendLine("     {");
            string CRUD = _GenerateCRUD(relatedProcedure, table,  view);
            sb.AppendLine(CRUD);
            sb.AppendLine("     }");
            sb.AppendLine("}");
            File.WriteAllText($"{clsUtil.DataAcessProjectName}/cls{className}Data.cs", sb.ToString());        
        }

        private static string _GenerateCRUD(IEnumerable<IGrouping<string, ProcedureInfoDTO>> procedures,
                                    IGrouping<string, TableColumnInfoDTO> table,
    
                                    IGrouping<string, viewInfoDTO>? view)
        {
            string className;
            if (table.Key.Contains("people", StringComparison.OrdinalIgnoreCase))
                className = "Person";
            else 
                className = table.Key.Substring(0, table.Key.Length - 1);
            StringBuilder sb = new StringBuilder();

            bool hasGetAll = false;
            bool hasGetById = false;
            bool hasAdd = false;
            bool hasUpdate = false;
            bool hasDelete = false;

            foreach (var procedure in procedures)
            {
                if (procedure.Key.Contains("GetAll", StringComparison.OrdinalIgnoreCase) && !className.Contains("person",StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine(_GenerateGetAllMethod(procedure,className, view));
                    hasGetAll = true;
                }
                else if (procedure.Key.Contains($"Get{className}", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine(_GenerateGetInfoByIDMethod(procedure, className, table));
                    hasGetById = true;
                }
                else if (procedure.Key.Contains("AddNew", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine(_GenerateAddNewMethod(procedure, className));
                    hasAdd = true;
                }
                else if (procedure.Key.Contains("Update", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine(_GenerateUpdateMethod(procedure, className));
                    hasUpdate = true;
                }
                else if (procedure.Key.Contains("Delete", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine(_GenerateDeleteMethod(procedure, className));
                    hasDelete = true;
                }
                else
                {
                    //sb.AppendLine(_GenerateProcedureMethod(procedure.Key, className));
                }
            }

            if (!hasGetAll && view != null && !className.Contains("person",StringComparison.OrdinalIgnoreCase))
                sb.AppendLine(_GenerateRawGetAllMethod(view, className));

            if (!hasGetById)
                sb.AppendLine(_GenerateRawGetByIdMethod(className,table));

            if (!hasAdd)
                sb.AppendLine(_GenerateRawAddNewMethod(className,table));

            if (!hasUpdate)
                sb.AppendLine(_GenerateRawUpdateMethod(className,table));

            if (!hasDelete)
                sb.AppendLine(_GenerateRawDeleteMethod(className,table));

            return sb.ToString();
        }
        
    }
}
