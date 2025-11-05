namespace Application.Scheduling;

/// <summary>
/// Represents a continuous block of time within a specific date.
/// </summary>
public sealed class TimeBlock
{
    /// <summary>
    /// Unique identifier of the time block (GUID).
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// The calendar date associated with this time block.
    /// </summary>
    public DateOnly Date { get; init; }
    
    /// <summary>
    /// The start time of the block within the given <see cref="Date"/>.
    /// </summary>
    public TimeSpan Start { get; init; }
    
    
    /// <summary>
    /// The end time of the block within the given <see cref="Date"/>.
    /// </summary>
    public TimeSpan End   { get; init; }
}