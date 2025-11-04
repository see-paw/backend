public class ReqRegisterUserDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // "User" or "AdminCAA"
    public string SelectedRole { get; set; } = "User";
}