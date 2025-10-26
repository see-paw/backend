namespace WebAPI.DTOs
{
    /// <summary>
    /// Request DTO used to update the authenticated user's profile.
    /// </summary>
    /// <remarks>
    /// This DTO is intentionally limited to editable personal fields.
    /// Authentication fields managed by ASP.NET Identity (e.g., password, security stamps)
    /// are not exposed here.
    /// </remarks>
    public class ReqUserProfileDto
    {
        /// <summary>
        /// Full name of the user.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Birth date of the user.
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

        /// <summary>
        /// Phone number.
        /// </summary>
        public string PhoneNumber { get; set; }
    }
}