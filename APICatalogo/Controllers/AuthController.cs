
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using APICatalogo.DTO;
using APICatalogo.Models;
using APICatalogo.Services;
using Azure;
using Azure.Identity;
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
    private readonly ILogger<AuthController> _logger;
    
    
    public AuthController(ITokenService tokenService, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _configuration = configuration;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }


    [HttpPost]
    [Route("CreateRole")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        var roleExists = await _roleManager.RoleExistsAsync(roleName);

        if (roleExists)
            return StatusCode(StatusCodes.Status400BadRequest,
                new ResponseDTO { Status = "Error", Message = "Role already exists" });
        
        var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));

        if (roleResult.Succeeded)
        {
            _logger.LogInformation($"Role {roleName} created successfully");
            return StatusCode(StatusCodes.Status201Created,
                new ResponseDTO { Status = "Success", Message = $"Role {roleName} created successfully" });
        }
            
        _logger.LogInformation(2, "Error");
            
        return StatusCode(StatusCodes.Status500InternalServerError,
            new ResponseDTO { Status = "Error", Message = "Error creating role" });
    }

    [HttpPost]
    [Route("AddUserToRole")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> AddUserToRole(string userEmail, string roleName)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);
        
        if (user is null) return NotFound("User not found");

        if (!await  _roleManager.RoleExistsAsync(roleName)) return NotFound("Role not found");
        
        var result = await _userManager.AddToRoleAsync(user, roleName);

        if (result.Succeeded)
        {
            _logger.LogInformation($"User {user.UserName} added to role {roleName}");
            return StatusCode(StatusCodes.Status200OK, 
                new ResponseDTO { Status = "Success", Message = $"User {user.UserName} added to role {roleName}" });
        }
        
        _logger.LogInformation(1, $"Error: Unable to add user {user.UserName} to role {roleName}");
        return StatusCode(StatusCodes.Status400BadRequest,
            $"Error: Unable to add user {user.UserName} to role {roleName}");
    }


    /// <summary>
    /// Authenticates a user and generates access and refresh tokens upon successful login.
    /// </summary>
    /// <param name="loginDTO">An object containing the login credentials, including user name and password.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> containing the access token, refresh token, and token expiration time
    /// if authentication is successful. Returns an Unauthorized status if authentication fails.
    /// </returns>
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
            new Claim("id", user.UserName!),
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


    /// <summary>
    /// Registers a new user in the system with the provided credentials.
    /// </summary>
    /// <param name="registerDTO">An object containing the registration details such as user name, email, and password.</param>
    /// <returns>
    /// Returns an <see cref="IActionResult"/> indicating the success or failure of the registration process.
    /// On success, returns a message confirming the user creation.
    /// On failure, returns an appropriate status code and error message (e.g., user or email already exists, internal server error).
    /// </returns>
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
    {
        var userExists = await _userManager.FindByNameAsync(registerDTO.UserName!);
        if (userExists != null) return StatusCode(StatusCodes.Status409Conflict, new  ResponseDTO { Status = "Error", Message = "User already exists" });
        
        var emailExists = await _userManager.FindByEmailAsync(registerDTO.Email!);
        if (emailExists != null) return StatusCode(StatusCodes.Status409Conflict, new ResponseDTO { Status = "Error", Message = "Email already exists" });
        

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
    
    [HttpPost]
    [Route("revoke/{username}")]
    [Authorize(Policy = "ExclusiveOnly")]
    public async Task<IActionResult> Revoke(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        
        if (user is null) return NotFound("Invalid username");
        
        user.RefreshToken = null;
        await _userManager.UpdateAsync(user);
        return NoContent();
    }
}