using Domain.Enums;

namespace WebAPI.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) used to receive animal editing requests from the client.
    /// </summary>
    /// <remarks>
    /// Inherits from <see cref="ReqCreateAnimalDto"/> to reuse all creation fields, and 
    /// adds the <see cref="AnimalState"/> property that represents the current state of the animal.
    /// <para>
    /// This DTO is typically used in <c>PUT</c> requests to update an existing animal, 
    /// allowing changes to biological data, adoption details, and its current state.
    /// </para>
    /// </remarks>
    public class ReqEditAnimalDto : BaseReqAnimalDto
    {
        /// <summary>
        /// The animal state of the animal (e.g.,Available, PartiallyFostered, TotallyFostered, HasOwner and Inactive).
        /// </summary>
        public AnimalState AnimalState { get; set; }
    }
}