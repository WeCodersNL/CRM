using CRM.Model.ApplicationModels;
using CRM.Model.IdentityModels;
using CRM.Model.InputModels;
using CRM.Utility;
using CRM.Utility.IUtility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Claims;

namespace CRM.Service.Identity
{
    public class AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IApplicationEmailSender applicationEmailSender,
        ITokenHandler tokenHandler
        ) : IAuthenticationService
    {
        public async Task<ResponseModel<AuthenticationTokens>> LoginAsync(ApplicationUserLoginInputModel model)
        {
            ArgumentNullException.ThrowIfNull(model.Email);
            ArgumentNullException.ThrowIfNull(model.Password);

            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user!.IsActive == false)
                {
                    return new ResponseModel<AuthenticationTokens>
                    {
                        IsSuccess = false,
                        Message = "User is inactive"
                    };
                }

                var claims = new List<Claim>
                {
                    new(TokenParameters.UserId, user?.Id!),
                    new(TokenParameters.Email, user?.Email!)
                };
                var token = tokenHandler.GenerateJwtToken(claims);
                var refreshToken = tokenHandler.GenerateRefreshToken();

                user!.RefreshTokenAttemptCount = 0;
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(tokenHandler.GetRefreshTokenExpiryDays());
                await userManager.UpdateAsync(user);

                return new ResponseModel<AuthenticationTokens>
                {
                    IsSuccess = true,
                    Message = "Login successful",
                    Data = new AuthenticationTokens { AccessToken = token, RefreshToken = refreshToken, IsRefreshTokenValid = true }
                };
            }

            string errorMessage = result.IsLockedOut ? "User is locked out." :
                                  result.IsNotAllowed ? "Login is not allowed." :
                                  result.RequiresTwoFactor ? "Two-factor authentication is required." :
                                  "Invalid login attempt.";

            return new ResponseModel<AuthenticationTokens>
            {
                IsSuccess = false,
                Message = errorMessage
            };
        }

        public async Task<ResponseModel<bool>> RegisterAsync(ApplicationUserRegisterInputModel model)
        {
            ArgumentNullException.ThrowIfNull(model.Email);
            ArgumentNullException.ThrowIfNull(model.Password);

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                RegistrationDate = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = true,
                    Message = "User created successfully",
                    Data = true
                };
            }

            string errorMessage = result.Errors.Any()
                ? string.Join("; ", result.Errors.Select(e => e.Code))
                : "Unable to register user due to unknown errors.";

            return new ResponseModel<bool>
            {
                IsSuccess = false,
                Message = errorMessage,
                Data = false
            };
        }

        public async Task<ResponseModel<bool>> ConfirmEmailAsync(ApplicationUserConfirmEmailInputModel model)
        {
            ArgumentNullException.ThrowIfNull(model.Email);
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Message = "User not found",
                    Data = false
                };
            }

            if (user.EmailConfirmed)
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Message = "Email already confirmed",
                    Data = false
                };
            }

            user.VerificationCode = GenerateVerificationCode();
            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Message = "Failed to save verification code"
                };
            }

            // Send email with verification code
            model.Code = user.VerificationCode.ToString();
            model.FullName = $"{user.FirstName} {user.LastName}";
            await SendEmailConfirmationCodeAsync(model);
            return new ResponseModel<bool>
            {
                IsSuccess = true,
                Message = "Verification code sent successfully"
            };
        }

        public async Task<ResponseModel<bool>> ConfirmEmailVerifyCodeAsync(ApplicationUserConfirmEmailInputModel model)
        {
            ArgumentNullException.ThrowIfNull(model.Email);
            ArgumentNullException.ThrowIfNull(model.Code);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Message = "User not found"
                };
            }

            bool isCodeValid = user.VerificationCode != null && user.VerificationCode.ToString() == model.Code;
            if (isCodeValid)
            {
                user.EmailConfirmed = true;
                user.IsActive = true;
                var result = await userManager.UpdateAsync(user);
                return new ResponseModel<bool>
                {
                    IsSuccess = result.Succeeded,
                    Message = result.Succeeded ? "Email confirmed successfully" : "Email confirmation failed"
                };
            }
            else
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Message = "Invalid confirmation code"
                };
            }
        }

        public async Task<ResponseModel<bool>> ForgotPasswordAsync(ApplicationUserForgotPasswordInputModel model)
        {
            ArgumentNullException.ThrowIfNull(model.Email);
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Message = "User not found",
                    Data = false
                };
            }

            user.VerificationCode = GenerateVerificationCode();
            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Message = "Failed to save verification code"
                };
            }

            // Send email with verification code
            model.Code = user.VerificationCode.ToString();
            model.FullName = $"{user.FirstName} {user.LastName}";
            await SendEmailConfirmationCodeAsync(model);
            return new ResponseModel<bool>
            {
                IsSuccess = true,
                Message = "Verification code sent successfully"
            };
        }

        public async Task<ResponseModel<bool>> ResetPasswordAsync(ApplicationUserForgotPasswordInputModel model)
        {
            ArgumentNullException.ThrowIfNull(model.Email);
            ArgumentNullException.ThrowIfNull(model.Code);
            ArgumentNullException.ThrowIfNull(model.Password);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Message = "User not found"
                };
            }

            bool isCodeValid = user.VerificationCode != null && user.VerificationCode.ToString() == model.Code;
            if (isCodeValid)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, token, model.Password);
                return new ResponseModel<bool>
                {
                    IsSuccess = result.Succeeded,
                    Message = result.Succeeded ? "Password reset successful" : "Unable to reset password"
                };
            }
            else
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Message = "Invalid confirmation code"
                };
            }
        }

        public async Task<ResponseModel<AuthenticationTokens>> RefreshTokenAsync(AuthenticationTokens model)
        {
            ArgumentNullException.ThrowIfNull(model.AccessToken);
            ArgumentNullException.ThrowIfNull(model.RefreshToken);

            var principal = tokenHandler.GetPrincipalFromExpiredToken(model.AccessToken);
            var userEmail = principal?.Claims.FirstOrDefault(c => c.Type == TokenParameters.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return TokenRequestFailure("Invalid token");

            var user = await userManager.FindByEmailAsync(userEmail);
            if (user is null)
                return TokenRequestFailure("User not found");

            var maxRefreshTokenAttempts = tokenHandler.GetMaxRefreshTokenAttempts();
            if (user.RefreshTokenAttemptCount >= maxRefreshTokenAttempts)
            {
                user.RefreshTokenAttemptCount = 0;
                await userManager.UpdateAsync(user);
                return TokenRequestFailure("Refresh token limit exceeded");
            }

            if (string.IsNullOrEmpty(user.RefreshToken) || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                user.RefreshTokenAttemptCount += 1;
                await userManager.UpdateAsync(user);
                return TokenRequestFailure("Invalid refresh token");
            }

            var claims = new List<Claim>
            {
                new(TokenParameters.UserId, user?.Id!),
                new(TokenParameters.Email, user?.Email!)
            };
            var newToken = tokenHandler.GenerateJwtToken(claims);
            var newRefreshToken = tokenHandler.GenerateRefreshToken();

            user!.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(tokenHandler.GetRefreshTokenExpiryDays());
            user.RefreshTokenAttemptCount = 0;

            try
            {
                await userManager.UpdateAsync(user);
            }
            catch (DbUpdateConcurrencyException)
            {
                return TokenRequestFailure("Failed to update user");
            }

            return new ResponseModel<AuthenticationTokens>
            {
                IsSuccess = true,
                Message = "Token refreshed successfully",
                Data = new AuthenticationTokens { AccessToken = newToken, RefreshToken = newRefreshToken, IsRefreshTokenValid = true }
            };
        }

        private short GenerateVerificationCode()
        {
            Random random = new Random();
            return (short)random.Next(1000, 9999);
        }

        private async Task SendEmailConfirmationCodeAsync(ApplicationUserVerificationBaseInputModel model)
        {
            MailMessage mail = new();
            mail.To.Add(model.Email);
            mail.Subject = "CRM Application";

            var emailContent = model.EmailTemplate.Replace("{FullName}", model.FullName).Replace("{Code}", model.Code);
            var alternateView = AlternateView.CreateAlternateViewFromString(emailContent, null, MediaTypeNames.Text.Html);
            mail.AlternateViews.Add(alternateView);
            await applicationEmailSender.SendEmailAsync(mail);
        }

        private ResponseModel<AuthenticationTokens> TokenRequestFailure(string message) => new()
        {
            IsSuccess = false,
            Message = message,
            Data = new AuthenticationTokens { IsRefreshTokenValid = false }
        };
    }
}

