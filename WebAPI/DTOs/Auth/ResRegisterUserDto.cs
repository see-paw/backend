using WebAPI.DTOs.Auth;

public class ResRegisterUserDto
{
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime BirthDate { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Role { get; set; }

    //only if Role is AdminCAA
    public ResRegisterShelterDto? Shelter { get; set; }
}