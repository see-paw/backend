using Application.Core;
using Application.OwnershipRequests.Commands;
using Application.OwnershipRequests.Queries;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.DTOs.Ownership;

namespace WebAPI.Controllers;

/// <summary>
/// API controller responsible for handling ownership request operations.
/// Provides endpoints for creating, updating status, approving and rejecting ownership requests.
/// </summary>
[Authorize]
public class OwnershipRequestsController(IMapper mapper) : BaseApiController
{
    /// <summary>
    /// Retrieves all ownership requests associated with animals 
    /// belonging to the authenticated admin's shelter.
    /// </summary>
    /// <param name="pageNumber">
    /// The current page number for pagination. Defaults to <c>1</c>.
    /// </param>
    /// <returns>
    /// A paginated <see cref="PagedList{T}"/> of <see cref="ResOwnershipRequestDto"/> objects 
    /// representing ownership requests related to the admin's shelter.
    /// </returns>
    /// <response code="200">Returns a paginated list of ownership requests.</response>
    /// <response code="401">Unauthorized — if the user is not authenticated.</response>
    /// <response code="403">Forbidden — if the user does not have permission to access this data.</response>
    /// <response code="500">Internal server error — if an unexpected error occurs.</response>
    [HttpGet]
    public async Task<ActionResult> GetOwnershipRequests([FromQuery] int pageNumber = 1)
    {
        var result = await Mediator.Send(new GetOwnershipRequestsByShelter.Query
        {
            PageNumber = pageNumber
        });

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var dtoList = mapper.Map<List<ResOwnershipRequestDto>>(result.Value);

        // Create a new paginated list with the DTOs
        var dtoPagedList = new PagedList<ResOwnershipRequestDto>(
            dtoList,
            result.Value.TotalCount,
            result.Value.CurrentPage,
            result.Value.PageSize
        );

        // Return the successful paginated result
        return HandleResult(Result<PagedList<ResOwnershipRequestDto>>.Success(dtoPagedList, 200));
    }

    /// <summary>
    /// Creates a new ownership request for an animal.
    /// </summary>
    /// <param name="dto">The request DTO containing animal ID.</param>
    /// <returns>The created ownership request object.</returns>
    [HttpPost]
    public async Task<ActionResult<ResOwnershipRequestDto>> CreateOwnershipRequest([FromBody] ReqCreateOwnershipRequestDto dto)
    {
        var command = new CreateOwnershipRequest.Command
        {
            AnimalID = dto.AnimalId,
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        var responseDto = mapper.Map<ResOwnershipRequestDto>(result.Value);

        return HandleResult(Result<ResOwnershipRequestDto>.Success(responseDto, 200));
    }

    /// <summary>
    /// Updates an ownership request to "Analysing" status.
    /// </summary>
    /// <param name="id">The ownership request ID.</param>
    /// <param name="dto">Optional request info to update.</param>
    /// <returns>The updated ownership request object.</returns>
    [HttpPut("analysing/{id}")]
    public async Task<ActionResult<ResOwnershipRequestDto>> UpdateStatus(string id, [FromBody] ReqUpdateOwnershipStatusDto dto)
    {
        var command = new UpdateOwnershipRequestStatus.Command
        {
            OwnershipRequestId = id,
            RequestInfo = dto.RequestInfo
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        var responseDto = mapper.Map<ResOwnershipRequestDto>(result.Value);

        return HandleResult(Result<ResOwnershipRequestDto>.Success(responseDto, 200));
    }

    /// <summary>
    /// Approves an ownership request, updates the animal state and cancels active fosterings.
    /// </summary>
    /// <param name="id">The ownership request ID.</param>
    /// <returns>The approved ownership request object.</returns>
    [HttpPut("approve/{id}")]
    public async Task<ActionResult<ResOwnershipRequestDto>> ApproveRequest(string id)
    {
        var command = new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = id
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        var responseDto = mapper.Map<ResOwnershipRequestDto>(result.Value);

        return HandleResult(Result<ResOwnershipRequestDto>.Success(responseDto, 200));
    }

    /// <summary>
    /// Rejects an ownership request with an optional reason.
    /// </summary>
    /// <param name="id">The ownership request ID.</param>
    /// <param name="dto">The rejection reason.</param>
    /// <returns>The rejected ownership request object.</returns>
    [HttpPut("reject/{id}")]
    public async Task<ActionResult<ResOwnershipRequestDto>> RejectRequest(string id, [FromBody] ReqRejectOwnershipRequestDto dto)
    {
        var command = new RejectOwnershipRequest.Command
        {
            OwnershipRequestId = id,
            RejectionReason = dto.RejectionReason
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        var responseDto = mapper.Map<ResOwnershipRequestDto>(result.Value);

        return HandleResult(Result<ResOwnershipRequestDto>.Success(responseDto, 200));
    }
}