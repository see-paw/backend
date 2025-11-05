using Domain;

namespace Application.Scheduling;

public sealed class NormalizedSlots
{
    public IReadOnlyList<Slot> Slots { get; }
    
    public NormalizedSlots(IReadOnlyList<Slot> slots)
    {
        Slots = slots;
    }
}