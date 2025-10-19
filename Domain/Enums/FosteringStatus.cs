namespace Domain.Enums
{

	/// <summary>
	/// Represents the fotering status of an animal.
	/// </summary>
	public enum FosteringStatus
	{
		/// <summary>
		/// The fostering is active.
		/// </summary>
		Active,
		/// <summary>
		/// The fostering was cancelled.
		/// </summary>
		Cancelled,
		/// <summary>
		/// The fostering was terminated because the animal died or left the shelter.
		/// </summary>
		Terminated
	}
}