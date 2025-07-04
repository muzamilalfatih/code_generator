using code_generator_business;
using Microsoft.AspNetCore.Mvc;
using shared_classes;

namespace code_generator_API.Controllers
{
    [Route("api/code-write")]
    [ApiController]  
    public class CodeWriterController: Controller
    {

        private readonly IWebHostEnvironment _env;

        public CodeWriterController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost(Name = "GenerateCode")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> GenerateCode([FromBody] GenerateCodeDTO generateCodeDTO)
        {
            if (generateCodeDTO.projectName == "")
            {
                return BadRequest("Invalid project name!");
            }
            Result<bool> existanceResult = clsDatabaseReader.IsDatabaseExist(generateCodeDTO.databaseName);
            if (!existanceResult.data)
            {
                return BadRequest("Database is not exist!");
            }
            Result<string> generationResult = clsCodeWriter.GenerateCode(generateCodeDTO.databaseName, generateCodeDTO.projectName);
            if (!generationResult.success)
                return StatusCode(generationResult.errorCode, generationResult.message);

            string basePath = _env.ContentRootPath;

            string GeneratedProjectPath = Path.Combine(basePath, generationResult.data);

            return Created("", GeneratedProjectPath);
        }

    }
}
