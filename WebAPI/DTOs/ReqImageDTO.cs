namespace WebAPI.DTOs;

public class ReqImageDTO
{
    public bool isPrincipal { get; set; }
    public string Url { get; set; } = null!;
    public string? Description { get; set; } 
}
