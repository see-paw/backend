using Domain;
using WebAPI.DTOs.Images;

namespace WebAPI.DTOs.Animals
{
    /// <summary>
    /// Data Transfer Object (DTO) used to receive animal creation requests from the client.
    /// Maps directly to the <see cref="Animal"/> domain entity through AutoMapper.
    /// </summary>
    public class ReqCreateAnimalDto : BaseReqAnimalDto
    {
        /// <summary>
        /// A list of images associated with the animal at creation.
        /// </summary>
        public List<ReqCreateImageDto> Images { get; set; } = new();
    }
}
