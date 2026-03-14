using AISEP.BLL.Helpers;
using AISEP.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/common")]
    public class CommonController : ControllerBase
    {
        [HttpGet("industries")]
        [AllowAnonymous]
        public IActionResult GetIndustries()
        {
            var industries = GetEnumItems(typeof(Industry));

            return Ok(ApiResponse<object>.SuccessResponse(industries, "Success"));
        }

        [HttpGet("enums/{enumName}")]
        [AllowAnonymous]
        public IActionResult GetEnumByName(string enumName)
        {
            var enumType = GetEnumTypes()
                .FirstOrDefault(type => string.Equals(type.Name, enumName, StringComparison.OrdinalIgnoreCase));

            if (enumType is null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Enum '{enumName}' was not found.", "Not Found"));
            }

            var items = GetEnumItems(enumType);
            return Ok(ApiResponse<object>.SuccessResponse(items, "Success"));
        }

        [HttpGet("enums")]
        [AllowAnonymous]
        public IActionResult GetAllEnums()
        {
            var enumData = GetEnumTypes()
                .ToDictionary(
                    enumType => enumType.Name,
                    enumType => GetEnumItems(enumType));

            return Ok(ApiResponse<object>.SuccessResponse(enumData, "Success"));
        }

        private static List<object> GetEnumItems(Type enumType)
        {
            return Enum.GetValues(enumType)
                .Cast<object>()
                .Select(value =>
                {
                    var name = value.ToString() ?? string.Empty;
                    return new
                    {
                        value = Convert.ToInt32(value),
                        name,
                        label = ToLabel(name)
                    };
                })
                .Cast<object>()
                .ToList();
        }

        private static IEnumerable<Type> GetEnumTypes()
        {
            return typeof(Industry).Assembly
                .GetTypes()
                .Where(type => type.IsEnum && type.Namespace == "AISEP.DAL.Enums")
                .OrderBy(type => type.Name);
        }

        private static string ToLabel(string enumName)
        {
            var withSpaces = enumName.Replace("_", " ");
            return Regex.Replace(withSpaces, "([a-z])([A-Z])", "$1 $2");
        }
    }
}