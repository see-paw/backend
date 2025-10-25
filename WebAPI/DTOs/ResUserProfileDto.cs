namespace WebAPI.DTOs
{
    /// <summary>
    /// Response DTO returned when fetching the authenticated user's profile.
    /// </summary>
    /// <remarks>
    /// Mirrors the public profile information without exposing sensitive Identity data.
    /// </remarks>
    public class ResUserProfileDto
    {
        
        /// <summary>
        /// Email address used for authentication and contact.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Full name of the user.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Birth date of the user (UTC).
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Street and door number.
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// City of residence.
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// ZIP code.
        /// </summary>
        public string PostalCode { get; set; } = string.Empty;

    }
}
