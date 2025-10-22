using Application.Core;
using Application.OwnershipRequests.Commands;
using Domain;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;

namespace WebAPI.Controllers;

/// <summary>
/// API controller responsible for handling ownership request operations.
/// Provides endpoints for creating, updating status, approving and rejecting ownership requests.
/// </summary>
[ApiController]
[Route("api/ownership-requests")]
public class OwnershipRequestsController : BaseApiController
{
    /// <summary>
    /// Creates a new ownership request for an animal.
    /// </summary>
    /// <param name="dto">The request DTO containing animal ID and optional request info.</param>
    /// <returns>The created ownership request object.</returns>
    [HttpPost]
    public async Task<ActionResult<ResOwnershipRequestDto>> CreateOwnershipRequest([FromBody] ReqCreateOwnershipRequestDto dto)
    {
        // TODO: Get userId from JWT token when authentication is implemented
        var userId = "temporary-user-id";

        var command = new CreateOwnershipRequest.Command
        {
            AnimalID = dto.AnimalId,
            UserId = userId,
            RequestInfo = dto.RequestInfo
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        var responseDto = Mapper.Map<ResOwnershipRequestDto>(result.Value);

        return HandleResult(Result<ResOwnershipRequestDto>.Success(responseDto));
    }

    /// <summary>
    /// Updates an ownership request from Pending to Analysing status.
    /// </summary>
    /// <param name="id">The ownership request ID.</param>
    /// <param name="dto">Optional request info to update.</param>
    /// <returns>The updated ownership request object.</returns>
    [HttpPut("{id}/status")]
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

        var responseDto = Mapper.Map<ResOwnershipRequestDto>(result.Value);

        return HandleResult(Result<ResOwnershipRequestDto>.Success(responseDto));
    }

    /// <summary>
    /// Approves an ownership request, updates the animal state and cancels active fosterings.
    /// </summary>
    /// <param name="id">The ownership request ID.</param>
    /// <returns>The approved ownership request object.</returns>
    [HttpPut("{id}/approve")]
    public async Task<ActionResult<ResOwnershipRequestDto>> ApproveRequest(string id)
    {
        var command = new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = id
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        var responseDto = Mapper.Map<ResOwnershipRequestDto>(result.Value);

        return HandleResult(Result<ResOwnershipRequestDto>.Success(responseDto));
    }

    /// <summary>
    /// Rejects an ownership request with an optional reason.
    /// </summary>
    /// <param name="id">The ownership request ID.</param>
    /// <param name="dto">The rejection reason.</param>
    /// <returns>The rejected ownership request object.</returns>
    [HttpPut("{id}/reject")]
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

        var responseDto = Mapper.Map<ResOwnershipRequestDto>(result.Value);

        return HandleResult(Result<ResOwnershipRequestDto>.Success(responseDto));
    }
}