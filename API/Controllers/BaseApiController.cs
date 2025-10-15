using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
/// <summary>
/// Serves as the base class for all API controllers in the application.
/// </summary>
/// <remarks>
/// The <see cref="BaseApiController"/> centralizes common API controller configurations,  
/// such as routing and attribute inheritance (<see cref="ApiControllerAttribute"/> and <see cref="RouteAttribute"/>).  
/// 
/// All derived controllers automatically follow the route pattern <c>api/[controller]</c>  
/// and benefit from built-in model binding, validation, and error handling provided by ASP.NET Core.
/// </remarks>
[Route("api/[controller]")]
[ApiController]
public class BaseApiController : ControllerBase;
