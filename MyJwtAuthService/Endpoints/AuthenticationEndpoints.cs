using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using MyJwtAuthServer.Contracts.Events;
using MyJwtAuthServer.Contracts.Events.BusinessEvents;
using MyJwtAuthService.Data;
using MyJwtAuthService.Exceptions;
using MyJwtAuthService.Extensions;
using MyJwtAuthService.Helpers;
using MyJwtAuthService.Models;
using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using MyJwtAuthService.Services.Authenticators;
using MyJwtAuthService.Services.EmailSenders;
using MyJwtAuthService.Services.RefreshTokenRepositories;
using MyJwtAuthService.Services.TokenValidators;
using System.Security.Claims;
using System.Text;
using ValidationException = MyJwtAuthService.Exceptions.ValidationException;

namespace MyJwtAuthService.Endpoints
{
    public static class AuthenticationEndpoints
    {
        public static IEndpointRouteBuilder AddAuthenticationEndpoints(this IEndpointRouteBuilder app, string confirmEmailEndpointName="confirmEmail") {
            var authGroup = app.MapGroup("auth");

            authGroup.MapPost("/register", async Task<Ok> (
                [FromBody] RegisterRequest registerRequest, UserManager<ApplicationUser> userRepository,
                AppIdentityDbContext dbContext,
                HttpContext context,
                IApplicationLinkGenerator applicationLinkGenerator,
                IValidator<RegisterRequest> validator,
                CancellationToken cancellationToken) => {

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
                
                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

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
                    else
                    {
                        throw new ValidationException(result.GetValidationErrors());
                    }
                }

                string? link = await applicationLinkGenerator.GetEmailConfirmationLink(registrationUser, registerRequest.Email, context, confirmEmailEndpointName);

                if(link is null)
                {
                    throw new InternalServerErrorException("Failed to generate email confirmation link.");
                }

                await dbContext.InsertOutboxMessages(cancellationToken,
                    new RegistrationEmailConfirmationSentEvent(registerRequest.Email, link),
                    new UserRegisteredEvent(registrationUser.UserName, registrationUser.Email, DateTime.UtcNow));
                    
                await transaction.CommitAsync(cancellationToken);
                
                return TypedResults.Ok();

            }).RequireRateLimiting(RateLimitingPolicyNames.IpLimiter).WithName("register").WithDescription("Allows registration for users using email verification.");

            authGroup.MapPost("/login", async Task<Ok<AuthenticatedUserResponse>> (
                [FromBody] LoginRequest loginRequest,
                UserManager<ApplicationUser> userRepository,
                Authenticator authenticator,
                SignInManager<ApplicationUser> signInManager,
                IValidator<LoginRequest> validator,
                IPublishEndpoint publishEndpoint,
                AppIdentityDbContext dbContext,
                CancellationToken cancellationToken) =>
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
                using var trans = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                AuthenticatedUserResponse response = await authenticator.Authenticate(user);

                await dbContext.InsertOutboxMessage(new UserLoggedInEvent(user.UserName!, user.Email!, DateTime.UtcNow), cancellationToken: cancellationToken);

                await trans.CommitAsync(cancellationToken);

                return TypedResults.Ok(response);
               
                throw new InternalServerErrorException("An error occurred during the login process.");
            }).WithName("login").WithDescription("Allows users to sign in to their account by their Username and Password");

            authGroup.MapPost("/refresh", async Task<Ok<AuthenticatedUserResponse>> (
                [FromBody] RefreshRequest refreshRequest,
                RefreshTokenValidator refreshTokenValidator,
                IRefreshTokenRepository refreshTokenRepository,
                UserManager<ApplicationUser> userRepository,
                Authenticator authenticator,
                AppIdentityDbContext dbContext,
                IValidator<RefreshRequest> validator,
                CancellationToken cancellationToken) => 
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

                RefreshToken? refreshToken = await refreshTokenRepository.GetByToken(refreshRequest.RefreshToken);
                if (refreshToken == null)
                {
                    throw new NotFoundException("Invalid refresh token.");
                }

                ApplicationUser? user = await userRepository.FindByIdAsync(refreshToken.UserId.ToString());
                if (user == null)
                {
                    throw new NotFoundException("User not found.");
                }
                bool isEmailConfirmed = await userRepository.IsEmailConfirmedAsync(user);

                if (!isEmailConfirmed)
                {
                    throw new BadRequestException("Email must be confirmed");
                }
                await using var trans = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                AuthenticatedUserResponse response = await authenticator.Authenticate(user);

                dbContext.RefreshTokens.Remove(refreshToken);

                dbContext.InsertOutboxMessagesWithoutSaveChanges(new UserRefreshTokenEvent(user.UserName!, user.Email!, refreshToken.Token, response.RefreshToken, DateTime.UtcNow));

                await dbContext.SaveChangesAsync(cancellationToken);

                await trans.CommitAsync(cancellationToken);

                return TypedResults.Ok(response);   

            }).RequireRateLimiting(RateLimitingPolicyNames.IpLimiter).WithName("refresh").WithDescription("Allows users to get a new short-lived access token by their long-lived refresh token.");

            authGroup.MapPost("/resendConfirmationEmail", async Task<Ok> (
                ResendRequest resendRequest,
                HttpContext context,
                UserManager<ApplicationUser> userManager,
                IPublishEndpoint publishEndpoint,
                IApplicationLinkGenerator applicationLinkGenerator,
                IValidator<ResendRequest> validator,
                CancellationToken cancellationToken) => 
            {

                var validationResult = validator.Validate(resendRequest);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.GetValidationErrors());
                }

                ApplicationUser? user = await userManager.FindByEmailAsync(resendRequest.Email);
                if (user != null)
                {
                    string? link = await applicationLinkGenerator.GetEmailConfirmationLink(user, resendRequest.Email, context, confirmEmailEndpointName);

                    if (link is null)
                    {
                        throw new InternalServerErrorException("Failed to generate email confirmation link.");
                    }

                    await publishEndpoint.Publish<RegistrationEmailConfirmationSentEvent>(new RegistrationEmailConfirmationSentEvent(resendRequest.Email, link), cancellationToken: cancellationToken);
                }
                return TypedResults.Ok();

            }).RequireRateLimiting(RateLimitingPolicyNames.IpLimiter).WithName("resendConfirmationEmail").WithDescription("To be able to sign in to a user's account, email confirmation is required. Such an email is sent during registration, but if it fails, you can always resend your confirmation email.");

            authGroup.MapPost("/forgotPassword", async Task<Ok> (
                ForgotPasswordRequest forgotPasswordRequest,
                UserManager<ApplicationUser> userManager,
                IPublishEndpoint publishEndpoint,
                IEmailSender<ApplicationUser> emailSender,
                IValidator<ForgotPasswordRequest> validator,
                CancellationToken cancellationToken) => 
            {
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

                        await publishEndpoint.Publish(new PasswordResetCodeConfirmationSentEvent(forgotPasswordRequest.Email, passwordResetToken), cancellationToken);
                    }
                }

                return TypedResults.Ok();
            }).RequireRateLimiting(RateLimitingPolicyNames.IpLimiter).WithName("forgotPassword").WithDescription("Allows you to restore the access to your account. You get an email, in which you get a reset token. Then you need to pass that token to the reset password endpoint.");

            authGroup.MapPost("/resetPassword", async Task<Ok> (
                ResetPasswordRequest resetRequest,
                UserManager<ApplicationUser> userManager,
                IValidator<ResetPasswordRequest> validator,
                AppIdentityDbContext dbContext,
                CancellationToken cancellationToken) => 
            {

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

                using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                
                string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));

                IdentityResult identityResult = await userManager.ResetPasswordAsync(user, token, resetRequest.NewPassword);

                await dbContext.InsertOutboxMessage(new UserPasswordResetEvent(user.UserName!, user.Email!, token, DateTime.UtcNow));

                await transaction.CommitAsync(cancellationToken);
              
                return TypedResults.Ok();
            }).RequireRateLimiting(RateLimitingPolicyNames.IpLimiter).WithName("resetPassword").WithDescription("Allows you to reset your password. You need to get a reset token ");

            authGroup.MapPost("/changeEmail", async Task<Ok> (
                ChangeEmailRequest changeEmailRequest,
                HttpContext context,
                IValidator <ChangeEmailRequest> validator,
                AppIdentityDbContext dbContext,
                IApplicationLinkGenerator applicationLinkGenerator,
                UserManager<ApplicationUser> userManager) =>
            {
                var validationResult = validator.Validate(changeEmailRequest);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.GetValidationErrors());
                }

                string? rawUserId = context.User.FindFirstValue("id");

                if (!Guid.TryParse(rawUserId, out Guid userId))
                {
                    throw new UnathorizedException();
                }

                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    throw new NotFoundException("User not found.");
                }

                string? link = await applicationLinkGenerator.GetEmailConfirmationLink(user, changeEmailRequest.NewEmail, context, confirmEmailEndpointName, isEmailChanged:true);

                if (link is null)
                {
                    throw new InternalServerErrorException("Failed to generate email confirmation link.");
                }

                await dbContext.InsertOutboxMessage(new ChangeEmailConfirmationSentEvent(changeEmailRequest.NewEmail, link));

                return TypedResults.Ok();

            }).RequireAuthorization().WithName("changeEmail").WithDescription("Allows users to change their email for a new one.");

            authGroup.MapGet($"/{confirmEmailEndpointName}", async Task<ContentHttpResult> (
                [FromQuery] string userId,
                [FromQuery] string code,
                [FromQuery] string? changedEmail,
                UserManager<ApplicationUser> userManager,
                AppIdentityDbContext dbContext,
                CancellationToken cancellationToken) =>
            {
                var user = await userManager.FindByIdAsync(userId);
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

                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                IdentityResult identityResult;
                if (string.IsNullOrEmpty(changedEmail))
                {
                    identityResult = await userManager.ConfirmEmailAsync(user, code);
                    if (identityResult.Succeeded)
                    {
                        await dbContext.InsertOutboxMessage(new UserEmailConfirmedEvent(user.UserName!, user.Email!, DateTime.UtcNow), cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    identityResult = await userManager.ChangeEmailAsync(user, changedEmail, code);
                    if (identityResult.Succeeded)
                    {
                        identityResult = await userManager.SetUserNameAsync(user, changedEmail);
                    }

                    if (identityResult.Succeeded)
                    {
                        identityResult = await userManager.UpdateSecurityStampAsync(user);
                    }
                    if (identityResult.Succeeded)
                    {
                        await dbContext.InsertOutboxMessage(new UserEmailChangedEvent(user.UserName!, user.Email!, changedEmail, DateTime.UtcNow), cancellationToken: cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                    }
                }
                if (!identityResult.Succeeded) {
                    throw new UnathorizedException();
                }

                return TypedResults.Text("Thank you for confirming your email.");

            }).RequireRateLimiting(RateLimitingPolicyNames.IpLimiter).WithName(confirmEmailEndpointName).WithDescription("After recieving a confirmation email, you must follow the link which leads here. That way a user confirms their email address.");


            authGroup.MapDelete("/logout", async Task<NoContent> (
                HttpContext httpContext,
                IRefreshTokenRepository refreshTokenRepository,
                UserManager<ApplicationUser> userManager,
                AppIdentityDbContext dbContext,
                CancellationToken cancellationToken) => 
            {
                string? rawUserId = httpContext.User.FindFirstValue("id");

                if (!Guid.TryParse(rawUserId, out Guid userId))
                {
                    throw new UnathorizedException();
                }
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    throw new UnathorizedException();
                }

                using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                await dbContext.RefreshTokens.Where(rt => rt.UserId == userId).ExecuteDeleteAsync(cancellationToken);

                await dbContext.InsertOutboxMessage(new UserLoggedOutEvent(user.UserName!, user.Email!, DateTime.UtcNow), cancellationToken: cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return TypedResults.NoContent();
            }).RequireAuthorization().WithName("logout").WithDescription("Allows users to log out of their account.");

            authGroup.MapDelete("/delete-account", async Task<NoContent> (
                UserManager<ApplicationUser> userManager,
                HttpContext httpContext,
                AppIdentityDbContext dbContext, 
                CancellationToken cancellationToken) =>
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
                using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                await userManager.DeleteAsync(user);

                await dbContext.InsertOutboxMessage(new UserDeletedEvent(user.UserName!, user.Email!, DateTime.UtcNow), cancellationToken: cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return TypedResults.NoContent();
            }).RequireAuthorization().WithName("delete-account").WithDescription("Allows users to delete their account if they want.");


            authGroup.MapGet("/account-info", async Task<Ok<UserInfoResponse>> (
                UserManager<ApplicationUser> userManager,
                HttpContext httpContext) =>
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
                var roles = await userManager.GetRolesAsync(user);

                var userResponse = new UserInfoResponse(user.Id, user.UserName, user.Email, user.EmailConfirmed, roles);

                return TypedResults.Ok(userResponse);
            }).RequireAuthorization().WithName("account-info").WithDescription("Allows users to get their account information");

            return authGroup;
        }
    }
}

