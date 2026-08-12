using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MyJwtAuthService.Data;
using MyJwtAuthService.Exceptions;
using MyJwtAuthService.Extensions;
using MyJwtAuthService.Models;
using MyJwtAuthService.Requests;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;
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

            authGroup.MapPost("/login", async Task<Ok> ([FromBody] LoginRequest loginRequest,
                UserManager<ApplicationUser> userRepository, HttpContext httpContext, SignInManager<ApplicationUser> signInManager, IValidator<LoginRequest> validator) =>
            {
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

                var signInResult = await signInManager.PasswordSignInAsync(user, loginRequest.Password, loginRequest.RememberMe, true);

                if (signInResult.IsLockedOut)
                {
                    var lockoutExpireDate = await userRepository.GetLockoutEndDateAsync(user);
                    throw new UnathorizedException(lockoutExpireDate.HasValue && lockoutExpireDate.Value >= DateTimeOffset.Now ? $"You've been locked out. Time left: {(lockoutExpireDate - DateTimeOffset.UtcNow)?.ToString(@"hh\:mm\:ss")}." : "You've been locked out.");
                }

                if (!signInResult.Succeeded)
                {
                    throw new UnathorizedException();
                }
                await httpContext.ChallengeAsync(IdentityConstants.ApplicationScheme, new AuthenticationProperties() { });

                return TypedResults.Ok();
            });

            authGroup.MapDelete("/logout", async Task<NoContent> (HttpContext httpContext, SignInManager <ApplicationUser> signInManager) => 
            {
                await signInManager.SignOutAsync();

                return TypedResults.NoContent();
            }).RequireAuthorization();

            authGroup.MapDelete("/delete-account", async Task<NoContent> (UserManager<ApplicationUser> userManager, HttpContext httpContext, SignInManager < ApplicationUser > signInManager, AppIdentityDbContext dbContext) =>
            {
                string? rawUserId = httpContext.User.FindFirstValue("id");

                if (!Guid.TryParse(rawUserId, out Guid userId))
                {
                    throw new UnathorizedException();
                }
                ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    throw new NotFoundException("User not found.");
                }

                await signInManager.SignOutAsync();

                await userManager.DeleteAsync(user);

                return TypedResults.NoContent();
            }).RequireAuthorization();

            return authGroup;

        }
    }
}
