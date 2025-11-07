using System;

using WebAPI.DTOs.Auth;

namespace WebAPI.DTOs.Auth
{
    /// <summary>
    /// Response DTO returned upon successful user registration.
    /// </summary>
    public class ResRegisterUserDto
    {
        /// <summary>
        /// Email address of the newly registered user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Full name of the user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Birth date of the user.
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Street address associated with the user.
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// City where the user resides.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Postal/ZIP code of the user's location.
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Role assigned to the user ("User" or "AdminCAA").
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Shelter information, when the registered user corresponds to an AdminCAA.
        /// Only populated when <see cref="Role"/> is AdminCAA.
        /// </summary>
        public ResRegisterShelterDto? Shelter { get; set; }
    }
}
