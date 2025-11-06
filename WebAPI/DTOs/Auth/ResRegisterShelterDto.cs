namespace WebAPI.DTOs.Auth;

public class ResRegisterShelterDto
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Street { get; init; } = null!;
    public string City { get; init; } = null!;
    public string PostalCode { get; init; } = null!;
    public string Phone { get; init; } = null!;
    public string NIF { get; init; } = null!;
    public string OpeningTime { get; init; } = null!;
    public string ClosingTime { get; init; } = null!;
}