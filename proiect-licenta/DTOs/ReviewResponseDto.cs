namespace proiect_licenta.DTOs;

public class ReviewResponseDto
{
    public int AppId { get; set; }
    public string Username { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public float Rating { get; set; }
    public DateTime PostDate { get; set; }
    public DateTime? EditDate { get; set; }
}