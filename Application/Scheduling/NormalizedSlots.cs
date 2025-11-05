using Domain;

namespace Application.Scheduling;

/// <summary>
/// Represents a normalized collection of <see cref="Slot"/> instances adjusted to valid shelter hours.
/// </summary>
public sealed class NormalizedSlots
{
    /// <summary>
    /// Gets the read-only collection of normalized <see cref="Slot"/> objects.
    /// </summary>
    public IReadOnlyList<Slot> Slots { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="NormalizedSlots"/> class with the provided list of slots.
    /// </summary>
    /// <param name="slots">
    /// The collection of <see cref="Slot"/> objects that have been normalized and validated.
    /// </param>
    public NormalizedSlots(IReadOnlyList<Slot> slots)
    {
        Slots = slots;
    }
}