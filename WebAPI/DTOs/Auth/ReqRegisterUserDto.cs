using Domain;
using System;

namespace WebAPI.DTOs.Auth
{
    /// <summary>
    /// Request DTO used to register either a standard User or an AdminCAA account.
    /// </summary>
    public class ReqRegisterUserDto
    {
        // ----- USER PERSONAL DATA -----

        /// <summary>
        /// Full name of the user.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Birth date of the user.
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Street address of the user's residence.
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// City where the user resides.
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Postal/ZIP code of the user's address.
        /// </summary>
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Email address for login and identification.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password used for authentication.
        /// Must meet the password policy requirements.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Selected account role. 
        /// Allowed values: "User" (default) or "AdminCAA".
        /// Determines whether shelter data must be provided.
        /// </summary>
        public string SelectedRole { get; set; } = "User";


        // ----- SHELTER DATA (ONLY REQUIRED IF SelectedRole == "AdminCAA") -----

        /// <summary>
        /// Name of the shelter associated with an AdminCAA.
        /// Only required when <see cref="SelectedRole"/> is "AdminCAA".
        /// </summary>
        public string? ShelterName { get; set; } = string.Empty;

        /// <summary>
        /// Street address of the shelter.
        /// </summary>
        public string? ShelterStreet { get; set; } = string.Empty;

        /// <summary>
        /// City where the shelter is located.
        /// </summary>
        public string? ShelterCity { get; set; } = string.Empty;

        /// <summary>
        /// Postal/ZIP code of the shelter.
        /// </summary>
        public string? ShelterPostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Contact phone number of the shelter.
        /// </summary>
        public string? ShelterPhone { get; set; } = string.Empty;

        /// <summary>
        /// National tax identification number (NIF) of the shelter.
        /// </summary>
        public string? ShelterNIF { get; set; } = string.Empty;

        /// <summary>
        /// Opening time of the shelter (e.g., "09:00").
        /// </summary>
        public string? ShelterOpeningTime { get; set; } = string.Empty;

        /// <summary>
        /// Closing time of the shelter (e.g., "18:00").
        /// </summary>
        public string? ShelterClosingTime { get; set; } = string.Empty;

        /// <summary>
        /// Optional Shelter object reference.
        /// Typically set internally by the application flow and not by the client.
        /// </summary>
        public Domain.Shelter? Shelter { get; set; }
    }
}
