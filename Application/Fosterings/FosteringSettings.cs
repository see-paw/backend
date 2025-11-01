namespace Application.Fosterings;

/// <summary>
/// Represents configuration settings related to the fostering process.
/// </summary>
public class FosteringSettings
{
    /// <summary>
    /// The minimum monthly contribution amount allowed for a fostering.
    /// Used to validate user-provided values during the creation of a fostering record.
    /// </summary>
    public decimal MinMonthlyValue { get; set; }
}