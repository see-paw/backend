using Domain;

public class ReqRegisterUserDto
{
    // ----- USER PERSONAL DATA -----
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // "User" or "AdminCAA"
    public string SelectedRole { get; set; } = "User";

    // ----- SHELTER DATA (ONLY REQUIRED IF SelectedRole == "AdminCAA") -----
    public string? ShelterName { get; set; } = string.Empty;
    public string? ShelterStreet { get; set; } = string.Empty;
    public string? ShelterCity { get; set; } = string.Empty;
    public string? ShelterPostalCode { get; set; } = string.Empty;
    public string? ShelterPhone { get; set; } = string.Empty;
    public string? ShelterNIF { get; set; } = string.Empty;
    public String? ShelterOpeningTime { get; set; } = string.Empty;
    public String? ShelterClosingTime { get; set; } = string.Empty;
    public Shelter? Shelter { get; set; }
}