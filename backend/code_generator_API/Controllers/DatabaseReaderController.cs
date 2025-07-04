using Microsoft.AspNetCore.Mvc;
using code_generator_business;
using shared_classes;

namespace code_generator_API.Controllers
{
    [Route("api/databases")]
    [ApiController]
    public class DatabaseReaderController : Controller
    {
        [HttpGet("All", Name = "GetDatabases")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<string>>> GetAllDatabases()
        {
            Result<List<string>> result = clsDatabaseReader.GetDatabases();

            if (result.success)
            {
                return Ok(result.data);
            }
            return StatusCode(result.errorCode, result.message);
        }
    }
}
