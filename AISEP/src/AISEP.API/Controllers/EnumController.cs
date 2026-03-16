using AISEP.BLL.Helpers;
using AISEP.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnumController : ControllerBase
    {

        [HttpGet("enums")]
        public IActionResult GetEnumByName([FromQuery] EnumTypeName enumName)
        {
            var enumType = Type.GetType($"AISEP.DAL.Enums.{enumName}, AISEP.DAL");
            if (enumType == null || !enumType.IsEnum)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Enum '{enumName}' not found.", $"Enum '{enumName}' not found.", 404));
            }

            var enumOptions = Enum.GetValues(enumType)
                .Cast<object>()
                .Select(e => new
                {
                    label = e.ToString(),
                    value = (int)e
                });

            return Ok(ApiResponse<object>.SuccessResponse(enumOptions, "Success"));
        }
    }
}

