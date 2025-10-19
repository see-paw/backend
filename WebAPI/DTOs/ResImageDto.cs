namespace WebAPI.DTOs;
public class ResImageDto
{
    public required string Id { get; init; }
    public bool IsPrincipal { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

