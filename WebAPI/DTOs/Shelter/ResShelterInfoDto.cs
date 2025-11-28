namespace WebAPI.DTOs.Shelter;


    /// <summary>
    /// Detailed shelter information returned by API endpoints.
    /// </summary>
    public class ResShelterInfoDto
{
    public string Id { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string Street { get; set; } = default!;

    public string City { get; set; } = default!;

    public string PostalCode { get; set; } = default!;

    public string Phone { get; set; } = default!;

    public string Nif { get; set; } = default!;

    /// <summary>
    /// Opening time in HH:mm format.
    /// </summary>
    public string OpeningTime { get; set; } = default!;

    /// <summary>
    /// Closing time in HH:mm format.
    /// </summary>
    public string ClosingTime { get; set; } = default!;
}
