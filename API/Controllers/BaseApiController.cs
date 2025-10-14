using API.Validators;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected ActionResult ValidateId(string id)
        {

            if (id is null)
                return BadRequest("ID is required.");

            var validator = new GuidStringValidator();
            var result = validator.Validate(id);

            if (!result.IsValid)
            {
                return BadRequest(result.Errors.Select(error => error.ErrorMessage));
            }

            return null;
        }
    }

