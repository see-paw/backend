namespace WebAPI.DTOs;

public class ResImageDto
{
	public string ImageId { get; set; } = null!;
    public bool isPrincipal { get; set; }
	public string Url { get; set; } = null!;
	public string? Description { get; set; }
}
