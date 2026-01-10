using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using proiect_licenta.Contexts;
using proiect_licenta.DTOs;
using proiect_licenta.Models;

namespace proiect_licenta.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<MyUser> _userManager;
    private readonly SignInManager<MyUser> _signInManager;
        
    public AuthService(ApplicationDbContext context, UserManager<MyUser> userManager, SignInManager<MyUser> signInManager, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<string> Register(RegisterRequestDTO registerRequest)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerRequest.Email);
        if (existingUser != null)
            throw new Exception("User already exists");
            
        var user = new MyUser { UserName = registerRequest.UserName, Email = registerRequest.Email };
        var result = _userManager.CreateAsync(user, registerRequest.Password);
        if (!result.Result.Succeeded)
        {
            throw new Exception("Failed to create user");
        }

        if (registerRequest.IsDeveloper)
        {
            await _userManager.AddToRoleAsync(user, "Developer");
        }
        else
        {
            await _userManager.AddToRoleAsync(user, "User");
        }
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("name", user.UserName)
        };
            
        var key = new SymmetricSecurityKey(Convert.FromBase64String(Environment.GetEnvironmentVariable("JWT__KEY")!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("JWT__ISSUER"),
            audience: Environment.GetEnvironmentVariable("JWT__AUDIENCE"),
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
        
    public async Task<string> Login([FromBody] LoginRequest loginRequest)
    {
        var user = await _userManager.FindByEmailAsync(loginRequest.Email);
        if (user == null)
            throw new Exception("Invalid username or password");
            
        var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, false);
        if (!result.Succeeded)
            throw new Exception("Invalid username or password");

            
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("name", user.UserName!),
        };
            
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var key = new SymmetricSecurityKey(Convert.FromBase64String(Environment.GetEnvironmentVariable("JWT__KEY")!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("JWT__ISSUER"),
            audience: Environment.GetEnvironmentVariable("JWT__AUDIENCE"),
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}