using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using shared_classes;

namespace code_generator_business
{
    internal class clsAPIGenerator
    {
        public static void GenerateAPI(IGrouping<string, TableColumnInfoDTO> table, IGrouping<string, viewInfoDTO>? view )
        {
            string className;
            if (table.Key.Equals("People", StringComparison.OrdinalIgnoreCase))
                className = "Person";
            else
                className = table.Key.Substring(0, table.Key.Length - 1);

            _GenerateControllers(table, className, view);
        }

        private static void _GenerateControllers(IGrouping<string, TableColumnInfoDTO> tabe, string className, IGrouping<string, viewInfoDTO>? view)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using SharedClasses;");
            sb.AppendLine($"using {clsUtil.BussiessProjectName};");
            sb.AppendLine($"namespace {clsUtil.APIProjectName}.Controllers");
            sb.AppendLine("{");
            sb.AppendLine($"    [Route(\"api/{clsUtil.ToCamel(tabe.Key)}\")]");
            sb.AppendLine("     [ApiController]");
            if (tabe.Key.Equals("People",StringComparison.OrdinalIgnoreCase))
                sb.AppendLine($"    public class {tabe.Key}Controller : ControllerBase");
            else
                sb.AppendLine($"    public class {className}sController : ControllerBase");
            sb.AppendLine("     {");
            string crud = _GenerateCrud(tabe, className, view);
            sb.AppendLine(crud);
            sb.AppendLine("     }");
            sb.AppendLine("}");

            File.WriteAllText($"{clsUtil.APIProjectName}/Controllers/{className}sController.cs", sb.ToString());
        }
        private static string _GenerateCrud(IGrouping<string, TableColumnInfoDTO> table,  string className, IGrouping<string,viewInfoDTO>? view)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(_GenrateGetByIDMethod(className));
            sb.AppendLine(_GenerateAddNewMethod(className));
            sb.AppendLine(_GenerateUpdateMethod(className, table));
            sb.AppendLine(_GenerateDeleteMethod(className));
            if (view != null) 
                sb.AppendLine(_GenerateGetAllMethod(className));

            return sb.ToString();   
        }
        private static string _GenrateGetByIDMethod(string className)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"        [HttpGet(\"{{id}}\", Name = \"Get{className}ByID\")]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status200OK)]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status400BadRequest)]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status404NotFound)]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status500InternalServerError)]");
            sb.AppendLine($"        public async Task<ActionResult<{className}DTO>> Get{className}ByID(int id)");
            sb.AppendLine("        {");
            sb.AppendLine($"            Result<cls{className}> result = await cls{className}.FindAsync(id);");
            sb.AppendLine("            if (result.success)");
            sb.AppendLine("            {");
            sb.AppendLine($"                return Ok(result.data.{className.Substring(0, 1)}DTO);");
            sb.AppendLine("            }");
            sb.AppendLine("            return result.errorCode == 400 ? BadRequest(result.message) : NotFound(result.message);");
            sb.AppendLine("        }");

            return sb.ToString() ;
        }
        private static string _GenerateGetAllMethod(string className)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"        [HttpGet(\"All\", Name = \"Get{className}s\")]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status200OK)]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status404NotFound)]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status500InternalServerError)]");
            sb.AppendLine($"        public async Task<ActionResult<IEnumerable<{className}ViewDTO>>> GetAll{className}s()");
            sb.AppendLine("        {");
            sb.AppendLine($"            Result<List<{className}ViewDTO>> result = await cls{className}.GetAll{className}sAsync();");
            sb.AppendLine("            if (result.success)");
            sb.AppendLine("            {");
            sb.AppendLine($"                return Ok(result.data);");
            sb.AppendLine("            }");
            sb.AppendLine("            return StatusCode(result.errorCode, result.message);");
            sb.AppendLine("        }");

            return sb.ToString();
        }
        private static string _GenerateDeleteMethod(string className)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"        [HttpDelete(\"{{id}}\", Name = \"Delete{className}\")]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status200OK)]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status400BadRequest)]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status404NotFound)]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status500InternalServerError)]");
            sb.AppendLine($"        public async Task<ActionResult> Delete{className}(int id)");
            sb.AppendLine("        {");
            sb.AppendLine($"            Result<bool> result = await cls{className}.Delete{className}Async(id);");
            sb.AppendLine("            if (result.success)");
            sb.AppendLine("            {");
            sb.AppendLine($"                return Ok($\"{className} with ID {{id}} has been deleted.\");");
            sb.AppendLine("            }");
            sb.AppendLine("            return StatusCode(result.errorCode, result.message);");
            sb.AppendLine("        }");

            return sb.ToString();
        }
        private static string _GenerateUpdateMethod(string className, IGrouping<string,TableColumnInfoDTO> table)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"        [HttpPut(\"{{id}}\", Name = \"Update{className}\")]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status200OK)]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status400BadRequest)]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status404NotFound)]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status500InternalServerError)]");
            sb.AppendLine($"        public async Task<ActionResult<{className}DTO>> Update{className}(int id, [FromBody] {className}DTO {clsUtil.ToCamel(className)}DTO)");
            sb.AppendLine("        {");
            sb.AppendLine($"            Result<cls{className}> result = await cls{className}.FindAsync(id);");
            sb.AppendLine("            if (!result.success)");
            sb.AppendLine("            {");
            sb.AppendLine($"                return StatusCode(result.errorCode, result.message);");
            sb.AppendLine("            }");
            foreach (var c in table)
            {
                if (!c.columnName.Contains("id",StringComparison.OrdinalIgnoreCase))
                    sb.AppendLine($"                result.data.{clsUtil.ToCamel(c.columnName)} = {clsUtil.ToCamel(className)}DTO.{clsUtil.ToCamel(c.columnName)};");
            }
            sb.AppendLine("            Result<int> savingResult = await result.data.SaveAsync();");
            sb.AppendLine("            if (savingResult.success)");
            sb.AppendLine($"                return Ok(result.data.{className.Substring(0, 1)}DTO);");
            sb.AppendLine("           return StatusCode(savingResult.errorCode, savingResult.message);");
            sb.AppendLine("        }");


            return sb.ToString();
        }
        private static string _GenerateAddNewMethod(string className)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"        [HttpPost(Name = \"Add{className}\")]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status201Created)]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status400BadRequest)]");
            sb.AppendLine("        [ProducesResponseType(StatusCodes.Status500InternalServerError)]");
            sb.AppendLine($"        public async Task<ActionResult<{className}DTO>> Add{className}({className}DTO {clsUtil.ToCamel(className)}DTO)");
            sb.AppendLine("        {");
            sb.AppendLine($"            cls{className} new{className} = new cls{className}({clsUtil.ToCamel(className)}DTO, cls{className}.enMode.AddNew);");
            sb.AppendLine($"            Result<int> result = await new{className}.SaveAsync();  ");
            sb.AppendLine("            if (result.success)");
            sb.AppendLine("            {");
            sb.AppendLine($"                return CreatedAtRoute(\"Get{className}ByID\", new {{ id = new{className}.id }}, new{className}.{className.Substring(0, 1)}DTO);");
            sb.AppendLine("            }");
            sb.AppendLine("                return StatusCode(result.errorCode, result.message);");
            sb.AppendLine("        }");

            return sb.ToString();
        }
    }
}
