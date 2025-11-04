using Application.Scheduling;
using Domain;

namespace Application.Interfaces;

public interface ISlotNormalizer
{
    public NormalizedSlots Normalize(
        IEnumerable<Slot> slots,
        TimeSpan opening,
        TimeSpan closing);
}