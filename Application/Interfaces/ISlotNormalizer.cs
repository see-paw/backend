using Application.Scheduling;
using Domain;

namespace Application.Interfaces;

/// <summary>
/// Defines the contract for normalizing scheduling slots within shelter operating hours.
/// </summary>
/// <remarks>
/// Implementations of this interface adjust, split, and filter <see cref="Slot"/> instances 
/// to ensure they align with valid daily time ranges (opening and closing hours).  
/// This normalization process is essential for generating consistent and accurate schedules.
/// </remarks>
public interface ISlotNormalizer
{
    /// <summary>
    /// Normalizes a collection of slots so that they fit within the shelter’s daily operating hours.
    /// </summary>
    /// <param name="slots">The collection of raw slots to normalize.</param>
    /// <param name="opening">The daily opening time of the shelter.</param>
    /// <param name="closing">The daily closing time of the shelter.</param>
    /// <returns>
    /// A <see cref="NormalizedSlots"/> object containing all valid, clamped, and split slots 
    /// within the provided time range.
    /// </returns>
    /// <remarks>
    /// The normalization process includes:
    /// <list type="bullet">
    /// <item>Splitting slots that span multiple days into single-day segments.</item>
    /// <item>Clamping slot times to respect the shelter’s opening and closing hours.</item>
    /// <item>Filtering out slots that fall completely outside of valid time ranges.</item>
    /// <item>Ensuring chronological ordering by date and start time.</item>
    /// </list>
    /// </remarks>
    public NormalizedSlots Normalize(
        IEnumerable<Slot> slots,
        TimeSpan opening,
        TimeSpan closing);
}