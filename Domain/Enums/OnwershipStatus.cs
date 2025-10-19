namespace Domain.Enums
{
	/// <summary>
	/// Represents the ownership status of an animal.
	/// </summary>
	public enum OwnershipStatus
	{
		/// <summary>
		/// The ownership request was initialized.
		/// </summary>
		Pending,
		/// <summary>
		/// The ownership request is being analysed.
		/// </summary>
		Analysing,
		/// <summary>
		/// The ownership request was approved.
		/// </summary>
		Approved,
		/// <summary>
		/// The ownership request was rejected.
		/// </summary>
		Rejected
	}
}