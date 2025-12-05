namespace WebAPI.DTOs.Fostering
{
    /// <summary>
    /// Lightweight DTO containing only fostering and animal IDs.
    /// </summary>
    public class ResActiveFosteringIdDto
    {
        /// <summary>
        /// The unique identifier of the fostering record.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the fostered animal.
        /// </summary>
        public string AnimalId { get; set; } = string.Empty;
    }
}
