namespace proiect_licenta.DTOs;

public class RegisterRequestDTO
{
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public bool IsDeveloper { get; set; }
}