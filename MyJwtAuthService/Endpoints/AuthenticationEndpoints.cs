using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MyJwtAuthService.Data;
using MyJwtAuthService.Exceptions;
using MyJwtAuthService.Extensions;
using MyJwtAuthService.Models;
using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using MyJwtAuthService.Services.Authenticators;
using MyJwtAuthService.Services.EmailSenders;
using MyJwtAuthService.Services.RefreshTokenRepositories;
using MyJwtAuthService.Services.TokenValidators;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using ValidationException = MyJwtAuthService.Exceptions.ValidationException;

namespace MyJwtAuthService.Endpoints
{
    public static class AuthenticationEndpoints
    {
        public static IEndpointRouteBuilder AddAuthenticationEndpoints(this IEndpointRouteBuilder app, string confirmEmailEndpointName="confirmEmail") {
            var authGroup = app.MapGroup("auth");

            authGroup.MapPost("/register", async Task<Ok> ([FromBody] RegisterRequest registerRequest, UserManager <ApplicationUser> userRepository, HttpContext context,  IConfirmationLinkEmailSender confirmationEmailSender, IValidator<RegisterRequest> validator) => {

                if (!userRepository.SupportsUserEmail)
                {
                    throw new NotSupportedException("Requires a user store with email support.");
                }

                var validationResult = validator.Validate(registerRequest);

                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.GetValidationErrors());
                }
              
                ApplicationUser registrationUser = new ApplicationUser()
                {
                    Email = registerRequest.Email,
                    UserName = registerRequest.Email,
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
                await confirmationEmailSender.SendConfirmationEmailAsync(registrationUser,registerRequest.Email, context, confirmEmailEndpointName);

                return TypedResults.Ok();

            }).WithName("register").WithDescription("Allows registration for users using email verification.");

            authGroup.MapPost("/login", async Task<Ok<AuthenticatedUserResponse>> ([FromBody] LoginRequest loginRequest,
                UserManager<ApplicationUser> userRepository, Authenticator authenticator, SignInManager<ApplicationUser> signInManager, IValidator<LoginRequest> validator) =>
            {
                var validationResult = validator.Validate(loginRequest);
                if (!validationResult.IsValid)
                {             
                    throw new ValidationException(validationResult.GetValidationErrors());
                }

                ApplicationUser? user = await userRepository.FindByNameAsync(loginRequest.Email);
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
                RefreshTokenValidator refreshTokenValidator, IRefreshTokenRepository refreshTokenRepository, UserManager<ApplicationUser> userRepository, Authenticator authenticator, IValidator<RefreshRequest> validator) => {

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

                await refreshTokenRepository.Delete(refreshTokenDTO.Id);

                AuthenticatedUserResponse response = await authenticator.Authenticate(user);

                return TypedResults.Ok(response);
            }).WithName("refresh").WithDescription("Allows users to get a new short-lived access token by their long-lived refresh token.");

            authGroup.MapPost("/resendConfirmationEmail", async Task<Ok> (ResendRequest resendRequest, HttpContext context, UserManager<ApplicationUser> userManager, IConfirmationLinkEmailSender confirmationEmailSender, IValidator<ResendRequest> validator) => {

                var validationResult = validator.Validate(resendRequest);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.GetValidationErrors());
                }

                ApplicationUser? user = await userManager.FindByEmailAsync(resendRequest.Email);
                if (user != null)
                {
                    await confirmationEmailSender.SendConfirmationEmailAsync(user, resendRequest.Email, context, confirmEmailEndpointName);
                }
                return TypedResults.Ok();

            }).WithName("resendConfirmationEmail").WithDescription("To be able to sign in to a user's account, email confirmation is required. Such an email is sent during registration, but if it fails, you can always resend your confirmation email.");

            authGroup.MapPost("/forgotPassword", async Task<Ok> (ForgotPasswordRequest forgotPasswordRequest, UserManager<ApplicationUser> userManager, IEmailSender<ApplicationUser> emailSender, IValidator<ForgotPasswordRequest> validator) => {

                var validationResult = validator.Validate(forgotPasswordRequest);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.GetValidationErrors());
                }

                ApplicationUser? user = await userManager.FindByEmailAsync(forgotPasswordRequest.Email);
                bool isEmailConfirmed = false;

                if (user != null)
                {
                    isEmailConfirmed = await userManager.IsEmailConfirmedAsync(user);
                    if (isEmailConfirmed)
                    {
                        string passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);

                        passwordResetToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(passwordResetToken));

                        await emailSender.SendPasswordResetCodeAsync(user, forgotPasswordRequest.Email, HtmlEncoder.Default.Encode(passwordResetToken));
                    }
                }

                return TypedResults.Ok();
            }).WithName("forgotPassword").WithDescription("Allows you to restore the access to your account. You get an email, in which you get a reset token. Then you need to pass that token to the reset password endpoint.");

            authGroup.MapPost("/resetPassword", async Task<Ok> (ResetPasswordRequest resetRequest, UserManager<ApplicationUser> userManager, IValidator<ResetPasswordRequest> validator) => {

                var validationResult = validator.Validate(resetRequest);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.GetValidationErrors());
                }

                ApplicationUser? user = await userManager.FindByEmailAsync(resetRequest.Email);
                bool isEmailConfirmed = false;
                if (user!=null)
                {
                    isEmailConfirmed = await userManager.IsEmailConfirmedAsync(user);
                }

                if (user==null || !isEmailConfirmed)
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

            authGroup.MapGet($"/{confirmEmailEndpointName}", async Task<ContentHttpResult> ([FromQuery] string userId, [FromQuery] string code, [FromQuery] string? changedEmail, UserManager<ApplicationUser> userManager) =>
            {
                ApplicationUser? user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new UnathorizedException();
                }

                try
                {
                    code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                }
                catch (FormatException)
                {
                    throw new UnathorizedException();
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
                if (!identityResult.Succeeded) {
                    throw new UnathorizedException();
                }

                return TypedResults.Text("Thank you for confirming your email.");

            }).WithName(confirmEmailEndpointName).WithDescription("After recieving a confirmation email, you must follow the link which leads here. That way a user confirms their email address.");


            authGroup.MapDelete("/logout", async Task<NoContent> (HttpContext httpContext, IRefreshTokenRepository refreshTokenRepository) => {
                string? rawUserId = httpContext.User.FindFirstValue("id");

                if (!Guid.TryParse(rawUserId, out Guid userId))
                {
                    throw new UnathorizedException();
                }

                await refreshTokenRepository.DeleteAll(userId);

                return TypedResults.NoContent();
            }).RequireAuthorization().WithName("logout").WithDescription("Allows users to log out of their account.");

            authGroup.MapDelete("/delete-account", async Task<NoContent> (UserManager<ApplicationUser> userManager, HttpContext httpContext) =>
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
            }).RequireAuthorization().WithName("delete-account").WithDescription("Allows users to delete their account if they want.");

            return authGroup;
        }

       
    }
}

