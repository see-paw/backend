using Application.Animals.Commands;
using Application.Animals.Filters;
using Application.Animals.Queries;
using Application.Core;
using Application.Fosterings.Commands;
using Application.Images.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using Boolean = System.Boolean;

using WebAPI.DTOs.Animals;
using WebAPI.DTOs.Fostering;
using WebAPI.DTOs.Images;

namespace WebAPI.Controllers;

/// <summary>
/// Handles all API operations related to animals, including creation, updates, image management, and deactivation.
/// </summary>
/// <remarks>
/// Provides endpoints for public users to view animals and for shelter administrators (<c>AdminCAA</c> role)
/// to manage animal data and images.  
/// Uses MediatR to delegate business logic to the Application layer and AutoMapper for DTO mapping.
/// </remarks>
public class AnimalsController(IMapper mapper, IUserAccessor userAccessor) : BaseApiController
{
    /// <summary>
    /// Retrieves a paginated list of animals that are available or partially fostered.
    /// </summary>
    /// <param name="animalFilters">Filter criteria for searching animals</param>
    /// <param name="sortBy">Field to sort by: "name", "age", or "created"</param>
    /// <param name="order">Sort direction: "asc" or "desc"</param>
    /// <param name="pageNumber">The page number to retrieve. Defaults to 1.</param>
    /// <returns>
    /// A paginated <see cref="PagedList{T}"/> of <see cref="ResAnimalDto"/> objects representing the animals.
    /// Returns <c>400</c> if the page number is invalid or an appropriate error message on failure.
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<PagedList<ResAnimalDto>>> GetAnimals(
        [FromQuery] AnimalFilterDto animalFilters,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? order = null, 
        [FromQuery] int pageNumber = 1)
    {
        
        var filterModel = mapper.Map<AnimalFilterModel>(animalFilters);
        
        var result = await Mediator.Send(new GetAnimalList.Query
        {
            PageNumber = pageNumber,
            SortBy = sortBy,
            Order = order,
            Filters = filterModel
        });

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var dtoList = mapper.Map<List<ResAnimalDto>>(result.Value.Items);

        // Create a new paginated list with the DTOs
        var dtoPagedList = new PagedList<ResAnimalDto>(
            dtoList,
            result.Value.TotalCount,
            result.Value.CurrentPage,
            result.Value.PageSize
        );

        // Return the successful paginated result
        return HandleResult(Result<PagedList<ResAnimalDto>>.Success(dtoPagedList, 200));
    }

    /// <summary>
    /// Retrieves detailed information about a specific animal by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the animal.</param>
    /// <returns>
    /// A <see cref="ResAnimalDto"/> containing the animal details if found; otherwise, an appropriate error response.
    /// </returns>
    /// <remarks>
    /// Sends a <see cref="GetAnimalDetails.Query"/> through MediatR to fetch the animal data.
    /// If the animal is not found or unavailable, returns a standardized error response.
    /// Successfully retrieved entities are mapped to <see cref="ResAnimalDto"/> using AutoMapper.
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<ActionResult<ResAnimalDto>> GetAnimalDetails(string id)
    {
        var result = await Mediator.Send(new GetAnimalDetails.Query() { Id = id });

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var animalDto = mapper.Map<ResAnimalDto>(result.Value);

        return HandleResult(Result<ResAnimalDto>.Success(animalDto, 200));
    }
    
    /// <summary>
    /// Creates a new animal and uploads its associated images.
    /// </summary>
    /// <param name="reqAnimalDto">The data and images required to create the animal.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing the ID of the created animal if successful,  
    /// or an error message with the corresponding status code otherwise.
    /// </returns>
    /// <remarks>
    /// Accessible only to users with the <c>AdminCAA</c> role.  
    /// Accepts multipart form data to include both animal information and image files.  
    /// Uses the <see cref="CreateAnimal"/> command via MediatR to handle the creation.
    /// </remarks>
    [Authorize(Roles = "AdminCAA")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<string>> CreateAnimal([FromForm] ReqCreateAnimalDto reqAnimalDto)
    {
            var user = await userAccessor.GetUserAsync();
            var shelterId = user.ShelterId;
        
            if (string.IsNullOrEmpty(shelterId))
                return Unauthorized("Invalid shelter token");
        
            if (reqAnimalDto.Images.Count == 0)
                return BadRequest("At least one image is required when creating an animal.");
        
            var invalidFile = reqAnimalDto.Images.Any(i => i.File.Length == 0);
            if (invalidFile)
                return BadRequest("Each image must include a valid file.");

            // Map the validated DTO to the domain entity
            var animal = mapper.Map<Animal>(reqAnimalDto);
            var imageEntities = mapper.Map<List<Image>>(reqAnimalDto.Images);
        
            // Build the command to send to the Application layer
            var command = new CreateAnimal.Command
            {
                Animal = animal,
                ShelterId = shelterId,
                Images = imageEntities,
                Files = reqAnimalDto.Images.Select(i => i.File).ToList()
            };

            // Centralized result handling (200, 400, 404, etc.)
            return HandleResult(await Mediator.Send(command));
    }


    /// <summary>
    /// Updates an existing animal with new information.
    /// </summary>
    /// <param name="id">The ID of the animal to update.</param>
    /// <param name="reqEditAnimalDto">The data containing the updated animal details.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing the updated <see cref="ResAnimalDto"/> if successful,  
    /// or an error message with the corresponding status code otherwise.
    /// </returns>
    /// <remarks>
    /// Accessible only to users with the <c>AdminCAA</c> role.  
    /// Uses the <see cref="EditAnimal"/> command via MediatR to perform the update.
    /// </remarks>
    [Authorize(Roles = "AdminCAA")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ResAnimalDto>> EditAnimal(string id, [FromBody] ReqEditAnimalDto reqEditAnimalDto)
    {

        var animal = mapper.Map<Animal>(reqEditAnimalDto);

        animal.Id = id;

        var command = new EditAnimal.Command { Animal = animal };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        var animalDto = mapper.Map<ResAnimalDto>(result.Value);

        return HandleResult(Result<ResAnimalDto>.Success(animalDto, 200));
    }

    /// <summary>
    /// Adds one or more images to an existing animal.
    /// </summary>
    /// <param name="id">The unique identifier of the animal.</param>
    /// <param name="reqAddImagesDto">DTO containing the list of images to add.</param>
    /// <returns>
    /// A list of <see cref="ResImageDto"/> representing the newly added images on success,
    /// or an appropriate error response on failure.
    /// </returns>
    [Authorize(Roles = "AdminCAA")]
    [HttpPost("{id}/images")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<List<ResImageDto>>> AddImagesToAnimal(string id, [FromForm] ReqAddImagesDto reqAddImagesDto)
    {
        var imageEntities = mapper.Map<List<Image>>(reqAddImagesDto.Images);
    
        var command = new AddImagesAnimal.Command
        {
            AnimalId = id,
            Images = imageEntities,
            Files = reqAddImagesDto.Images.Select(i => i.File).ToList()
        };
    
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        var imageDtos = mapper.Map<List<ResImageDto>>(result.Value);

        return HandleResult(Result<List<ResImageDto>>.Success(imageDtos, 201));
    }
    
    /// <summary>
    /// Deletes an image from a specific animal.
    /// </summary>
    /// <param name="animalId">The ID of the animal that owns the image.</param>
    /// <param name="imageId">The ID of the image to delete.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> indicating success or failure of the deletion.  
    /// Returns a success result if the image was removed successfully,  
    /// or an error message with the corresponding status code otherwise.
    /// </returns>
    /// <remarks>
    /// Accessible only to users with the <c>AdminCAA</c> role.  
    /// Uses the <see cref="DeleteAnimalImage"/> command via MediatR to handle the deletion.
    /// </remarks>
    [Authorize(Roles = "AdminCAA")]
    [HttpDelete("{animalId}/images/{imageId}")]
    public async Task<ActionResult<Unit>> DeleteAnimalImage(string animalId, string imageId)
    {
        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Sets the main image for a specific animal.
    /// </summary>
    /// <param name="animalId">The ID of the animal whose main image will be updated.</param>
    /// <param name="imageId">The ID of the image to set as the main image.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing the result of the operation.  
    /// Returns a success result if the image was updated successfully,  
    /// or an error message with the corresponding status code otherwise.
    /// </returns>
    /// <remarks>
    /// Accessible only to users with the <c>AdminCAA</c> role.  
    /// Uses the <see cref="SetAnimalPrincipalImage"/> command via MediatR to handle the update.
    /// </remarks>
    [Authorize(Roles = "AdminCAA")]
    [HttpPut("{animalId}/images/{imageId}/set-principal")]
    public async Task<ActionResult<Unit>> SetAnimalPrincipalImage(string animalId, string imageId)
    {
        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };
        
        return HandleResult(await Mediator.Send(command));
    }
    
    
    /// <summary>
    /// Deactivates an existing <see cref="Animal"/> entity within the shelter context,
    /// changing its <see cref="Domain.Enums.AnimalState"/> to <c>Inactive</c> instead of deleting it.
    /// </summary>
    /// <para>
    /// The operation is only permitted when:
    /// <list type="bullet">
    ///   <item><description>The animal belongs to the authenticated shelter administrator (CAA).</description></item>
    ///   <item><description>The animal has no active <see cref="Domain.Fostering"/> or <see cref="Domain.OwnershipRequest"/> associations.</description></item>
    ///   <item><description>The authenticated user has the <c>AdminCAA</c> role.</description></item>
    /// </list>
    /// </para>
    /// <param name="id">The unique identifier of the animal to be deactivated.</param>
    [Authorize(Roles = "AdminCAA")]
    [HttpPatch("{id}/deactivate")]
    public async Task<ActionResult> DeactivateAnimal(string id)
    {
        var user = await userAccessor.GetUserAsync();
        var shelterId = user.ShelterId;

        if (string.IsNullOrEmpty(shelterId))
            return Unauthorized("Invalid shelter token");

        var command = new DeactivateAnimal.Command
        {
            AnimalId = id,
            ShelterId = shelterId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        var animalDto = mapper.Map<ResAnimalDto>(result.Value);
        return HandleResult(Result<ResAnimalDto>.Success(animalDto, 200));
    }
    
    /// <summary>
    /// Creates a new fostering record for the specified animal, allowing the authenticated user
    /// to sponsor it with a monthly contribution.
    /// </summary>
    /// <param name="animalId">The unique identifier (GUID) of the animal to be fostered.</param>
    /// <param name="reqAddFosteringDto">The data transfer object containing the monthly contribution value.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="ResActiveFosteringDto"/> that represents the created fostering record.  
    /// Returns an appropriate error response if the operation fails.
    /// <list type="bullet">
    /// <item><description><c>201 Created</c> – fostering created successfully.</description></item>
    /// <item><description><c>404 Not Found</c> – the specified animal does not exist.</description></item>
    /// <item><description><c>409 Conflict</c> – the animal is in an invalid state or already fostered by the same user.</description></item>
    /// <item><description><c>422 Unprocessable Entity</c> – the monthly value surpasses the animal’s cost.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This endpoint is accessible only to authenticated users with the <c>User</c> role.
    /// </remarks>
    [Authorize(Roles = "User")]
    [HttpPost("{animalId}/fosterings")]
    public async Task<ActionResult<ResActiveFosteringDto>> AddFostering(string animalId, [FromBody] ReqAddFosteringDto reqAddFosteringDto)
    {
        var result = await Mediator.Send(new AddFostering.Command
        {
            AnimalId = animalId,
            MonthValue = reqAddFosteringDto.MonthValue
            
        });
        
        return HandleResult(result);
    }

    /// <summary>
    /// Checks whether a given animal is eligible to be associated with an Ownership.
    /// </summary>
    /// <param name="id">The unique identifier of the animal to verify.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> containing:
    /// <list type="bullet">
    /// <item><description><c>200 OK</c> with <c>true</c> if the animal is eligible for ownership.</description></item>
    /// <item><description><c>400 Bad Request</c> if the animal exists but is not eligible (e.g., already adopted or inactive).</description></item>
    /// <item><description><c>404 Not Found</c> if the animal does not exist in the database.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This endpoint delegates validation to the <see cref="CheckAnimalEligibilityForOwnership"/> query handler
    /// in the <c>Application</c> layer, ensuring centralized business logic and consistent results.
    /// <para>
    /// **Route:** <c>GET /api/ownershiprequests/check-eligibility/{id}</c>
    /// </para>
    /// </remarks>
    [HttpGet("check-eligibility/{id}")]
    public async Task<ActionResult> CheckEligibility([FromRoute] string id)
    {
        // Send the eligibility check query via Mediator
        var result = await Mediator.Send(new CheckAnimalEligibilityForOwnership.Query
        {
            AnimalId = id
        });
        
        // If the query result indicates failure, return the corresponding HTTP status and message
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }
        
        // Map the boolean value and return 200 OK with eligibility result
        var isPossibleToOwnership = mapper.Map<Boolean>(result.Value);
        return HandleResult(Result<Boolean>.Success(isPossibleToOwnership, 200));
    }
}