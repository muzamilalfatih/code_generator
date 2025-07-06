using code_generator_business;
using shared_classes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using code_generator_dataaccess;

namespace code_generator_business
{
    public  class clsCodeWriter
    {   
        public static Result<string> GenerateCode(string database, string pojectName)
        {
            clsUtil.projectName = pojectName;

            Directory.CreateDirectory(clsUtil.projectPath);
            clsSolutionGeneratror.GenerateSolution();
            clsUtil.IntialSettings(database);

            
            Result<List<TableColumnInfoDTO>> tablesResult = clsDatabaseReaderData.GetTablesAndColumns(database);
            if (!tablesResult.success)
                return new Result<string>(tablesResult.success, tablesResult.message, "", tablesResult.errorCode);
            Result<List<ProcedureInfoDTO>> proceduresReuslt = clsDatabaseReaderData.GetStoredProcedureInfo(database);
            if (!proceduresReuslt.success)
                return new Result<string>(proceduresReuslt.success, proceduresReuslt.message, "", proceduresReuslt.errorCode);
            Result<List<viewInfoDTO>> viewsResult = clsDatabaseReaderData.GetViewInfo(database);
            if (!viewsResult.success)
                return new Result<string>(viewsResult.success, viewsResult.message, "", viewsResult.errorCode);


            var groupedTables = tablesResult.data.GroupBy(t => t.tableName);
            var groubedProcedure = proceduresReuslt.data.GroupBy(t => t.procedureName);
            var groupedViews = viewsResult.data.GroupBy(t => t.viewName);
            clsSharedClassessGenerator.GenerateResultClass();
            clsBussinessGenerator.GenerateUtilClass();
            foreach ( IGrouping<string, TableColumnInfoDTO> tabe in groupedTables )
            {
                IEnumerable<IGrouping<string, viewInfoDTO>> tableViews = groupedViews.Where(v => v.Key.Contains(tabe.Key.Substring(0, tabe.Key.Length - 1), StringComparison.OrdinalIgnoreCase));
                clsSharedClassessGenerator.GenerateSharedClasses(tabe, tableViews);
                clsDataAccessGenerator.GenerateDataAccess(tabe, groubedProcedure, tableViews.FirstOrDefault());
                clsBussinessGenerator.GenerateBussiness(tabe , tableViews.FirstOrDefault());
                clsAPIGenerator.GenerateAPI(tabe, tableViews.FirstOrDefault());
            }
            return new Result<string>(true, "Code generated successfully!", clsUtil.projectPath);
        }
    }
}
