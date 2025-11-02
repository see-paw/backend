namespace WebAPI.DTOs.Fostering
{
    /// <summary>
    /// Represents an active fostering entry for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Each record includes details about the fostered animal and the monthly contribution
    /// made by the user to support its care.
    /// </remarks>
    public class ResCancelFosteringDto
    {
        /// <summary>
        /// The name of the fostered animal.
        /// </summary>
        public string AnimalName { get; set; } = string.Empty;

        /// <summary>
        /// The age of the animal, calculated in years.
        /// </summary>
        public int AnimalAge { get; set; }

        /// <summary>
        /// The monthly contribution amount made by the user.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The date when the fostering began.
        /// </summary>
        public DateTime StartDate { get; set; }


        /// <summary>
        /// The date when the fostering finished.
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}