using Domain;

namespace Application.Scheduling;

public sealed class NormalizedSlots
{
    public IReadOnlyList<Slot> Slots { get; }
    
    internal NormalizedSlots(IReadOnlyList<Slot> slots)
    {
        Slots = slots;
    }
}