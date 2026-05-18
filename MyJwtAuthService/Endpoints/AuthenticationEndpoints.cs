using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Exceptions;
using MyJwtAuthService.Extensions;
using MyJwtAuthService.Models;
using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using MyJwtAuthService.Services.Authenticators;
using MyJwtAuthService.Services.RefreshTokenRepositories;
using MyJwtAuthService.Services.TokenValidators;
using System.Security.Claims;
using ValidationException = MyJwtAuthService.Exceptions.ValidationException;

namespace MyJwtAuthService.Endpoints
{
    public static class AuthenticationEndpoints
    {
        public static IEndpointRouteBuilder AddAuthenticationEndpoints(this IEndpointRouteBuilder app)
        {
            var authGroup = app.MapGroup("auth");

            authGroup.MapPost("/register", async Task<Ok> ([FromBody] RegisterRequest registerRequest, UserManager<ApplicationUser> userRepository, IValidator<RegisterRequest> validator) => {
                var validationResult = validator.Validate(registerRequest);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.GetValidationErrors());
                }

                if (registerRequest.Password != registerRequest.ConfirmPassword)
                {
                    throw new BadRequestException("Password does not match confirm password.");
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
                        throw new ConflictException("Email already exists.");
                    }
                    else if (primaryError?.Code == nameof(errorDescriber.DuplicateUserName))
                    {
                        throw new ConflictException("Username already exists.");
                    }
                }

                return TypedResults.Ok();

            });

            authGroup.MapPost("/login", async Task<Ok<AuthenticatedUserResponse>> ([FromBody] LoginRequest loginRequest,
                UserManager<ApplicationUser> userRepository,
                Authenticator authenticator,
                IValidator<LoginRequest> validator) => {

                ValidationResult validationResult = validator.Validate(loginRequest);
                if (!validationResult.IsValid)
                {             
                    throw new ValidationException(validationResult.GetValidationErrors());
                }

                ApplicationUser? user = await userRepository.FindByNameAsync(loginRequest.Username);
                if (user == null)
                {
                    throw new UnathorizedException();
                }

                bool isCorrectPassword = await userRepository.CheckPasswordAsync(user, loginRequest.Password);
                if (!isCorrectPassword)
                {
                    throw new UnathorizedException();
                }

                AuthenticatedUserResponse response = await authenticator.Authenticate(user);

                return TypedResults.Ok<AuthenticatedUserResponse>(response);
            });

            authGroup.MapPost("/refresh", async Task<Ok<AuthenticatedUserResponse>> ([FromBody] RefreshRequest refreshRequest,
                RefreshTokenValidator refreshTokenValidator,
                IRefreshTokenRepository refreshTokenRepository,
                UserManager<ApplicationUser> userRepository,
                Authenticator authenticator, IValidator<RefreshRequest> validator) => {
            
                var validationResult = validator.Validate(refreshRequest);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.GetValidationErrors());
                }
                
                bool isValidRefreshToken = refreshTokenValidator.Validate(refreshRequest.RefreshToken);
                if (!isValidRefreshToken)
                {
                    throw new BadRequestException("Invalid refresh token.");
                }

                RefreshToken? refreshTokenDTO = await refreshTokenRepository.GetByToken(refreshRequest.RefreshToken);
                if (refreshTokenDTO == null)
                {
                    throw new NotFoundException("Invalid refresh token.");
                }

                await refreshTokenRepository.Delete(refreshTokenDTO.Id);

                ApplicationUser? user = await userRepository.FindByIdAsync(refreshTokenDTO.UserId.ToString());
                if (user == null)
                {
                    throw new NotFoundException("User not found.");
                }

                AuthenticatedUserResponse response = await authenticator.Authenticate(user);

                return TypedResults.Ok(response);
            });

            authGroup.MapDelete("/logout", async Task<NoContent> (HttpContext httpContext, IRefreshTokenRepository refreshTokenRepository) => {
                string? rawUserId = httpContext.User.FindFirstValue("id");

                if (!Guid.TryParse(rawUserId, out Guid userId))
                {
                    throw new UnathorizedException();
                }

                await refreshTokenRepository.DeleteAll(userId);

                return TypedResults.NoContent();
            }).RequireAuthorization();

            return authGroup;

        }
    }
}
