using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Models;
using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using MyJwtAuthService.Services.Authenticators;
using MyJwtAuthService.Services.RefreshTokenRepositories;
using MyJwtAuthService.Services.TokenValidators;
using System.Security.Claims;

namespace MyJwtAuthService.Endpoints
{
    public static class AuthenticationEndpoints
    {
        public static IEndpointRouteBuilder AddAuthenticationEndpoints(this IEndpointRouteBuilder app)
        {
            var authGroup = app.MapGroup("auth");

            authGroup.MapPost("/register", async Task<Results<Ok, BadRequest<ErrorResponse>, Conflict<ErrorResponse>>> ([FromBody] RegisterRequest registerRequest, UserManager<ApplicationUser> userRepository, IValidator<RegisterRequest> validator) => {
                var validationResult = validator.Validate(registerRequest);
                if (!validationResult.IsValid)
                {
                    return TypedResults.BadRequest(new ErrorResponse(validationResult.Errors.Select(x => x.ErrorMessage)));
                }

                if (registerRequest.Password != registerRequest.ConfirmPassword)
                {
                    return TypedResults.BadRequest(new ErrorResponse("Password does not match confirm password."));
                }

                ApplicationUser registrationUser = new ApplicationUser()
                {
                    Email = registerRequest.Email,
                    UserName = registerRequest.Username
                };

                IdentityResult result = await userRepository.CreateAsync(registrationUser, registerRequest.Password);
                if (!result.Succeeded)
                {
                    IdentityErrorDescriber errorDescriber = new();
                    IdentityError? primaryError = result.Errors.FirstOrDefault();

                    if (primaryError?.Code == nameof(errorDescriber.DuplicateEmail))
                    {
                        return TypedResults.Conflict(new ErrorResponse("Email already exists."));
                    }
                    else if (primaryError?.Code == nameof(errorDescriber.DuplicateUserName))
                    {
                        return TypedResults.Conflict(new ErrorResponse("Username already exists."));
                    }
                }

                return TypedResults.Ok();

            });

            authGroup.MapPost("/login", async Task<Results<Ok<AuthenticatedUserResponse>, BadRequest<ErrorResponse>, UnauthorizedHttpResult>> ([FromBody] LoginRequest loginRequest,
                UserManager<ApplicationUser> userRepository,
                Authenticator authenticator,
                IValidator<LoginRequest> validator) => {
                var validationResult = validator.Validate(loginRequest);
                if (!validationResult.IsValid)
                {
                    return TypedResults.BadRequest(new ErrorResponse(validationResult.Errors.Select(x => x.ErrorMessage)));
                }

                ApplicationUser? user = await userRepository.FindByNameAsync(loginRequest.Username);
                if (user == null)
                {
                    return TypedResults.Unauthorized();
                }

                bool isCorrectPassword = await userRepository.CheckPasswordAsync(user, loginRequest.Password);
                if (!isCorrectPassword)
                {
                    return TypedResults.Unauthorized();
                }

                AuthenticatedUserResponse response = await authenticator.Authenticate(user);

                return TypedResults.Ok<AuthenticatedUserResponse>(response);
            });

            authGroup.MapPost("/refresh", async Task<Results<Ok<AuthenticatedUserResponse>, BadRequest<ErrorResponse>, NotFound<ErrorResponse>>> ([FromBody] RefreshRequest refreshRequest,
                RefreshTokenValidator refreshTokenValidator,
                IRefreshTokenRepository refreshTokenRepository,
                UserManager<ApplicationUser> userRepository,
                Authenticator authenticator, IValidator<RefreshRequest> validator) => {
            
                var validationResult = validator.Validate(refreshRequest);
                if (!validationResult.IsValid)
                {
                    return TypedResults.BadRequest(new ErrorResponse(validationResult.Errors.Select(x=>x.ErrorMessage)));
                }
                
                bool isValidRefreshToken = refreshTokenValidator.Validate(refreshRequest.RefreshToken);
                if (!isValidRefreshToken)
                {
                    return TypedResults.BadRequest(new ErrorResponse("Invalid refresh token."));
                }

                RefreshToken? refreshTokenDTO = await refreshTokenRepository.GetByToken(refreshRequest.RefreshToken);
                if (refreshTokenDTO == null)
                {
                    return TypedResults.NotFound(new ErrorResponse("Invalid refresh token."));
                }

                await refreshTokenRepository.Delete(refreshTokenDTO.Id);

                ApplicationUser? user = await userRepository.FindByIdAsync(refreshTokenDTO.UserId.ToString());
                if (user == null)
                {
                    return TypedResults.NotFound(new ErrorResponse("User not found."));
                }

                AuthenticatedUserResponse response = await authenticator.Authenticate(user);

                return TypedResults.Ok(response);
            });

            authGroup.MapDelete("/logout", async Task<Results<NoContent, UnauthorizedHttpResult>> (HttpContext httpContext, IRefreshTokenRepository refreshTokenRepository) =>
            {
                string? rawUserId = httpContext.User.FindFirstValue("id");

                if (!Guid.TryParse(rawUserId, out Guid userId))
                {
                    return TypedResults.Unauthorized();
                }

                await refreshTokenRepository.DeleteAll(userId);

                return TypedResults.NoContent();
            }).RequireAuthorization();

            return authGroup;

        }
    }
}
