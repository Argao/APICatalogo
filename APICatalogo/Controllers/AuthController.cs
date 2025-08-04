using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using APICatalogo.DTO;
using APICatalogo.Models;
using APICatalogo.Services;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace APICatalogo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    
    public AuthController(ITokenService tokenService, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _tokenService = tokenService;
        _configuration = configuration;
        _userManager = userManager;
        _roleManager = roleManager;
    }


    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
    {
        var user = await _userManager.FindByNameAsync(loginDTO.UserName!);

        if (user is null || !await _userManager.CheckPasswordAsync(user, loginDTO.Password!)) return Unauthorized();
        
        var userRoles = await _userManager.GetRolesAsync(user);
        
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        authClaims.AddRange(userRoles.Select(userRole =>
            new Claim(ClaimTypes.Role, userRole)
        ));

        var token = _tokenService.GenerateAccessToken(authClaims, _configuration);
        var refreshToken = _tokenService.GenerateRefreshToken();
        _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out var refreshTokenValidityInMinutes);;
            
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(refreshTokenValidityInMinutes);
            
        await _userManager.UpdateAsync(user);
            
        return Ok(new
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken,
            Expiration = token.ValidTo
        });
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
    {
        var userExists = await _userManager.FindByNameAsync(registerDTO.UserName!);

        if (userExists != null)
        {
            return StatusCode(StatusCodes.Status409Conflict, new  ResponseDTO { Status = "Error", Message = "User already exists" });
        }

        ApplicationUser user = new()
        {
            Email = registerDTO.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = registerDTO.UserName,
        };
        
        var result = await _userManager.CreateAsync(user, registerDTO.Password!);
        
        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDTO { Status = "Error", Message = "Error creating user" });
        }
        return Ok(new ResponseDTO { Status = "Success", Message = "User created successfully" });
    }

    [HttpPost]
    [Route("refresh-token")]
    public async Task<IActionResult> RefreshToken(TokenDTO tokenDTO)
    {
        var accessToken = tokenDTO.AccessToken 
                          ?? throw new ArgumentNullException(nameof(tokenDTO));
        var refreshToken = tokenDTO.RefreshToken 
                          ?? throw new ArgumentNullException(nameof(tokenDTO));
        
        var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken, _configuration);
        
        var username = principal.Identity?.Name;
        
        var user = await _userManager.FindByNameAsync(username!);

        if (user is null || user.RefreshToken != refreshToken
                         || user.RefreshTokenExpiryTime < DateTime.UtcNow)
        {
            return Unauthorized();
        }
        
        var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims.ToList(), _configuration);
        
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        
        user.RefreshToken = newRefreshToken;
        await _userManager.UpdateAsync(user);

        return new ObjectResult(new
        {
            accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
            refreshToken = newRefreshToken
        });
    }

    [Authorize]
    [HttpPost]
    [Route("revoke/{username}")]
    public async Task<IActionResult> Revoke(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        
        if (user is null) return NotFound("Invalid username");
        
        user.RefreshToken = null;
        await _userManager.UpdateAsync(user);
        return NoContent();
    }
}