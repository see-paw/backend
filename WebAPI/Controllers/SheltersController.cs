using Application.Core;
using Application.Shelters.Queries;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers;
using WebAPI.DTOs;

namespace API.Controllers
{
    public class SheltersController (IMapper mapper) : BaseApiController
    {

     
        [HttpGet("{shelterId}/animals")]
        public async Task<ActionResult<PagedList<Animal>>> GetAnimalsByShelter(string shelterId, [FromQuery] int pageNumber = 1)
        {
            if (pageNumber < 1)
                return BadRequest("Page number must be 1 or greater");

            const int pageSize = 20; // Default page size

            // Send the query to the Application layer through MediatR
            var result = await Mediator.Send(new GetAnimalsByShelter.Query
            {
                ShelterId = shelterId,
                PageNumber = pageNumber,
                PageSize = pageSize
            });

            // Return failure result if the handler reports an error
            if (!result.IsSuccess)
                return HandleResult(result);

            //Map Animal → ResGetAnimalsDto
            var dtoList = mapper.Map<List<ResAnimalDto>>(result.Value);

            var pagedDtoList = new PagedList<ResAnimalDto>(
                dtoList,
                result.Value.TotalCount,
                result.Value.CurrentPage,
                result.Value.PageSize
            );

            return HandleResult(Result<PagedList<ResAnimalDto>>.Success(pagedDtoList));
        }
    }
}
