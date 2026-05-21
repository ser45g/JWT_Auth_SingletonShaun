using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Data;
using Microsoft.AspNetCore.WebUtilities;
using MyJwtAuthService.Exceptions;
using MyJwtAuthService.Extensions;
using MyJwtAuthService.Models;
using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using MyJwtAuthService.Services.Authenticators;
using MyJwtAuthService.Services.EmailSenders;
using MyJwtAuthService.Services.RefreshTokenRepositories;
using MyJwtAuthService.Services.TokenValidators;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Timers;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using ValidationException = MyJwtAuthService.Exceptions.ValidationException;

namespace MyJwtAuthService.Endpoints
{
    public static class AuthenticationEndpoints
    {
        public static IEndpointRouteBuilder AddAuthenticationEndpoints(this IEndpointRouteBuilder app)
        {
            var authGroup = app.MapGroup("auth");

            authGroup.MapPost("/register", async Task<Ok> ([FromBody] RegisterRequest registerRequest, UserManager <ApplicationUser> userRepository,  IEmailSender<ApplicationUser> emailSender, IValidator<RegisterRequest> validator, HttpContext context, LinkGenerator linkGenerator) => {

                if (!userRepository.SupportsUserEmail)
                {
                    throw new NotSupportedException("MapIdentityApi requires a user store with email support.");
                }

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
                    UserName = registerRequest.Username,
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
                await SendConfirmationEmailAsync(registrationUser, userRepository, context, registerRequest.Email, "confirmEmail", linkGenerator, emailSender);

                return TypedResults.Ok();

            }).WithName("register").WithDescription("Allows registration for users using email verification.");

            authGroup.MapPost("/login", async Task<Ok<AuthenticatedUserResponse>> ([FromBody] LoginRequest loginRequest,
                UserManager<ApplicationUser> userRepository, Authenticator authenticator, SignInManager<ApplicationUser> signInManager, IValidator<LoginRequest> validator) =>
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
                bool isEmailConfirmed = await userRepository.IsEmailConfirmedAsync(user);

                if(!isEmailConfirmed){
                    throw new BadRequestException("Email must be confirmed");
                }

                var signInResult = await signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, true);

                if (signInResult.IsLockedOut)
                {
                    var lockoutExpireDate = await userRepository.GetLockoutEndDateAsync(user);
                    throw new UnathorizedException(lockoutExpireDate.HasValue && lockoutExpireDate.Value >= DateTimeOffset.Now ? $"You've been locked out. Time left: {(lockoutExpireDate - DateTimeOffset.UtcNow)?.ToString(@"hh\:mm\:ss")}." : "You've been locked out.");
                }

                if (!signInResult.Succeeded)
                {
                    throw new UnathorizedException();
                }

                AuthenticatedUserResponse response = await authenticator.Authenticate(user);

                return TypedResults.Ok(response);
            }).WithName("login").WithDescription("Allows users to sign in to their account by their Username and Password");

            authGroup.MapPost("/refresh", async Task<Ok<AuthenticatedUserResponse>> ([FromBody] RefreshRequest refreshRequest,
                RefreshTokenValidator refreshTokenValidator, IRefreshTokenRepository refreshTokenRepository, UserManager<ApplicationUser> userRepository, Authenticator authenticator, IValidator<RefreshRequest> validator) =>
            {

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
                bool isEmailConfirmed = await userRepository.IsEmailConfirmedAsync(user);

                if (!isEmailConfirmed)
                {
                    throw new BadRequestException("Email must be confirmed");
                }

                AuthenticatedUserResponse response = await authenticator.Authenticate(user);

                return TypedResults.Ok(response);
            }).WithName("refresh").WithDescription("Allows users to get a new short-lived access token by their long-lived refresh token.");
            authGroup.MapPost("/resendConfirmationEmail", async (ResendRequest resendRequest, HttpContext context, UserManager<ApplicationUser> userManager, IEmailSender<ApplicationUser> emailSender, LinkGenerator linkGenerator) => {
                ApplicationUser? val = await userManager.FindByEmailAsync(resendRequest.Email);
                if (val == null)
                {
                    return TypedResults.Ok();
                }

                await SendConfirmationEmailAsync(val, userManager, context, resendRequest.Email, "confirmEmail", linkGenerator, emailSender);
                return TypedResults.Ok();
            }).WithName("resendConfirmationEmail").WithDescription("To be able to sign in to a user's account, email confirmation is required. Such an email is sent during registration, but if it fails, you can always resend your confirmation email.");

            authGroup.MapPost("/forgotPassword", async (ForgotPasswordRequest forgotPasswordRequest, UserManager<ApplicationUser> userManager, IEmailSender<ApplicationUser> emailSender) =>
            {
                ApplicationUser? user = await userManager.FindByEmailAsync(forgotPasswordRequest.Email);
                bool flag = user != null;
                if (flag)
                {
                    flag = await userManager.IsEmailConfirmedAsync(user);
                }

                if (flag)
                {
                    string passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                    passwordResetToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(passwordResetToken));
                    await emailSender.SendPasswordResetCodeAsync(user, forgotPasswordRequest.Email, HtmlEncoder.Default.Encode(passwordResetToken));
                }

                return TypedResults.Ok();
            }).WithName("forgotPassword").WithDescription("Allows you to restore the access to your account. You get an email, in which you get a reset token. Then you need to pass that token to the reset password endpoint.");

            authGroup.MapPost("/resetPassword", async (ResetPasswordRequest resetRequest, UserManager<ApplicationUser> userManager, IEmailSender<ApplicationUser> emailSender) =>
            {

                ApplicationUser? user = await userManager.FindByEmailAsync(resetRequest.Email);
                bool flag = user == null;
                if (!flag)
                {
                    flag = !(await userManager.IsEmailConfirmedAsync(user));
                }

                if (flag)
                {
                    throw new BadRequestException("Invalid token");
                }

                IdentityResult identityResult;
                try
                {
                    string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
                    identityResult = await userManager.ResetPasswordAsync(user, token, resetRequest.NewPassword);
                }
                catch (FormatException)
                {
                    identityResult = IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
                }
                if (!identityResult.Succeeded)
                {
                    throw new BadRequestException("Invalid token");
                }

                return TypedResults.Ok();
            }).WithName("resetPassword").WithDescription("Allows you to reset your password. You need to get a reset token ");

            authGroup.MapGet("/confirmEmail", async ([FromQuery] string userId, [FromQuery] string code, [FromQuery] string? changedEmail, UserManager<ApplicationUser> userManager) =>
            {
                ApplicationUser? user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return TypedResults.Unauthorized();
                }

                try
                {
                    code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                }
                catch (FormatException)
                {
                    return TypedResults.Unauthorized();
                }

                IdentityResult identityResult;
                if (string.IsNullOrEmpty(changedEmail))
                {
                    identityResult = await userManager.ConfirmEmailAsync(user, code);
                }
                else
                {
                    identityResult = await userManager.ChangeEmailAsync(user, changedEmail, code);
                    if (identityResult.Succeeded)
                    {
                        identityResult = await userManager.SetUserNameAsync(user, changedEmail);
                    }
                }

                return (!identityResult.Succeeded) ? ((Results<ContentHttpResult, UnauthorizedHttpResult>)TypedResults.Unauthorized()) : ((Results<ContentHttpResult, UnauthorizedHttpResult>)TypedResults.Text("Thank you for confirming your email."));
            }).WithName("confirmEmail").WithDescription("After recieving a confirmation email, you must follow the link which leads here. That way a user confirms their email address.");


            authGroup.MapDelete("/logout", async Task<NoContent> (HttpContext httpContext, IRefreshTokenRepository refreshTokenRepository) => {
                string? rawUserId = httpContext.User.FindFirstValue("id");

                if (!Guid.TryParse(rawUserId, out Guid userId))
                {
                    throw new UnathorizedException();
                }

                await refreshTokenRepository.DeleteAll(userId);

                return TypedResults.NoContent();
            }).RequireAuthorization().WithName("logout").WithDescription("Allows users to log out of their account.");

            authGroup.MapDelete("/delete-account", async Task<NoContent> (UserManager<ApplicationUser> userManager, HttpContext httpContext, IRefreshTokenRepository refreshTokenRepository, AppIdentityDbContext dbContext) =>
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

                await userManager.DeleteAsync(user);

                return TypedResults.NoContent();
            }).RequireAuthorization();

            return authGroup;

        }

        public static async Task SendConfirmationEmailAsync(ApplicationUser user, UserManager<ApplicationUser> userManager, HttpContext context, string email, string confirmEmailEndpointName, LinkGenerator linkGenerator, IEmailSender<ApplicationUser> emailSender, bool isChange = false)
        {
            if (confirmEmailEndpointName == null)
            {
                throw new NotSupportedException("No email confirmation endpoint was registered!");
            }

            string text = ((!isChange) ? (await userManager.GenerateEmailConfirmationTokenAsync(user)) : (await userManager.GenerateChangeEmailTokenAsync(user, email)));
            string code = text;
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string value = await userManager.GetUserIdAsync(user);
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary
            {
                ["userId"] = value,
                ["code"] = code
            };
            if (isChange)
            {
                routeValueDictionary.Add("changedEmail", email);
            }

            string value2 = linkGenerator.GetUriByName(context, confirmEmailEndpointName, routeValueDictionary) ?? throw new NotSupportedException("Could not find endpoint named '" + confirmEmailEndpointName + "'.");
            await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(value2));
        }
    }
}

