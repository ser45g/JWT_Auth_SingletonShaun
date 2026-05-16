using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Models;
using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using MyJwtAuthService.Services.Authenticators;
using MyJwtAuthService.Services.RefreshTokenRepositories;
using MyJwtAuthService.Services.TokenValidators;
using System.Security.Claims;


namespace MyJwtAuthService.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthenticationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userRepository;
        private readonly Authenticator _authenticator;
        private readonly RefreshTokenValidator _refreshTokenValidator;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthenticationController(UserManager<ApplicationUser> userRepository, 
            Authenticator authenticator,
            RefreshTokenValidator refreshTokenValidator,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _authenticator = authenticator;
            _refreshTokenValidator = refreshTokenValidator;
            _refreshTokenRepository = refreshTokenRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (registerRequest.Password != registerRequest.ConfirmPassword)
            {
                return BadRequest(new ErrorResponse("Password does not match confirm password."));
            }

            ApplicationUser registrationUser = new ApplicationUser()
            {
                Email = registerRequest.Email,
                UserName = registerRequest.Username
            };
            
            IdentityResult result = await _userRepository.CreateAsync(registrationUser, registerRequest.Password);
            if(!result.Succeeded)
            {
                IdentityErrorDescriber errorDescriber = new();
                IdentityError? primaryError = result.Errors.FirstOrDefault();
                
                if(primaryError?.Code == nameof(errorDescriber.DuplicateEmail))
                {
                    return Conflict(new ErrorResponse("Email already exists."));
                }
                else if (primaryError?.Code == nameof(errorDescriber.DuplicateUserName))
                {
                    return Conflict(new ErrorResponse("Username already exists."));
                }
            }

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApplicationUser? user = await _userRepository.FindByNameAsync(loginRequest.Username);
            if(user == null)
            {
                return Unauthorized();
            }

            bool isCorrectPassword = await _userRepository.CheckPasswordAsync(user, loginRequest.Password);
            if(!isCorrectPassword)
            {
                return Unauthorized();
            }

            AuthenticatedUserResponse response = await _authenticator.Authenticate(user);

            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest refreshRequest)
        {
            Console.WriteLine(refreshRequest.RefreshToken);
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool isValidRefreshToken = _refreshTokenValidator.Validate(refreshRequest.RefreshToken);
            if(!isValidRefreshToken)
            {
                return BadRequest(new ErrorResponse("Invalid refresh token."));
            }

            RefreshToken? refreshTokenDTO = await _refreshTokenRepository.GetByToken(refreshRequest.RefreshToken);
            if(refreshTokenDTO == null)
            {
                return NotFound(new ErrorResponse("Invalid refresh token."));
            }

            await _refreshTokenRepository.Delete(refreshTokenDTO.Id);

            ApplicationUser? user = await _userRepository.FindByIdAsync(refreshTokenDTO.UserId.ToString());
            if(user == null)
            {
                return NotFound(new ErrorResponse("User not found."));
            }

            AuthenticatedUserResponse response = await _authenticator.Authenticate(user);

            return Ok(response);
        }
        [Authorize(Roles = "Admin,User")]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            return Ok("Authorized endpoint \"profile\"");
        }

        [Authorize]
        [HttpDelete("logout")]
        public async Task<IActionResult> Logout()
        {
            string? rawUserId = HttpContext.User.FindFirstValue("id");

            if(!Guid.TryParse(rawUserId, out Guid userId))
            {
                return Unauthorized();
            }

            await _refreshTokenRepository.DeleteAll(userId);

            return NoContent();
        }
    }
}
