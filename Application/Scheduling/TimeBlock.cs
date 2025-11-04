namespace Application.Scheduling;

public sealed class TimeBlock
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateOnly Date { get; init; }
    public TimeSpan Start { get; init; }
    public TimeSpan End   { get; init; }
}