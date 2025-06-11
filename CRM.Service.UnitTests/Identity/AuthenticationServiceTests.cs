using System.Net.Mail;
using System.Security.Claims;
using CRM.Model.ApplicationModels;
using CRM.Model.IdentityModels;
using CRM.Model.InputModels;
using CRM.Service.Identity;
using CRM.Utility;
using CRM.Utility.IUtility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CRM.Service.UnitTests.Identity
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly Mock<IApplicationEmailSender> _emailSenderMock;
        private readonly Mock<ITokenHandler> _tokenHandlerMock;
        private readonly Service.Identity.AuthenticationService _service;
        private readonly string _emailTemplate = "Hello {FullName}, your code is {Code}";

        public AuthenticationServiceTests()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
            var userValidators = new List<IUserValidator<ApplicationUser>>();
            var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
            var keyNormalizer = new Mock<ILookupNormalizer>();
            var errors = new Mock<IdentityErrorDescriber>();
            var services = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStore.Object,
                options.Object,
                passwordHasher.Object,
                userValidators,
                passwordValidators,
                keyNormalizer.Object,
                errors.Object,
                services.Object,
                logger.Object);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var signInOptions = new Mock<IOptions<IdentityOptions>>();
            var signInLogger = new Mock<ILogger<SignInManager<ApplicationUser>>>();
            var authenticationSchemeProvider = new Mock<IAuthenticationSchemeProvider>();

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                contextAccessor.Object,
                userPrincipalFactory.Object,
                signInOptions.Object,
                signInLogger.Object,
                authenticationSchemeProvider.Object)
                { CallBase = true };

            _signInManager = _signInManagerMock.Object;

            _emailSenderMock = new Mock<IApplicationEmailSender>();
            _tokenHandlerMock = new Mock<ITokenHandler>();
            _service = new Service.Identity.AuthenticationService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _emailSenderMock.Object,
                _tokenHandlerMock.Object
            );
        }

        #region LoginAsync

        [Fact]
        public async Task LoginAsync_ReturnsTokens_WhenCredentialsAreValid_AndUserIsActive()
        {
            var model = new ApplicationUserLoginInputModel { Email = "test@example.com", Password = "Password1!" };
            var user = new ApplicationUser { Email = model.Email, Id = "1", IsActive = true };
            var signInResult = SignInResult.Success;

            _signInManagerMock.Setup(s => s.PasswordSignInAsync(model.Email, model.Password, false, false))
                .ReturnsAsync(signInResult);
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _tokenHandlerMock.Setup(t => t.GenerateJwtToken(It.IsAny<List<Claim>>())).Returns("jwt-token");
            _tokenHandlerMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");
            _tokenHandlerMock.Setup(t => t.GetRefreshTokenExpiryDays()).Returns(7);
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _service.LoginAsync(model);

            Assert.True(result.IsSuccess);
            Assert.Equal("Login successful", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal("jwt-token", result.Data.AccessToken);
            Assert.Equal("refresh-token", result.Data.RefreshToken);
            Assert.True(result.Data.IsRefreshTokenValid);
        }

        [Fact]
        public async Task LoginAsync_ThrowsArgumentNullException_WhenEmailIsNull()
        {
            var model = new ApplicationUserLoginInputModel { Email = null!, Password = "Password1!" };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.LoginAsync(model));
        }

        [Fact]
        public async Task LoginAsync_ThrowsArgumentNullException_WhenPasswordIsNull()
        {
            var model = new ApplicationUserLoginInputModel { Email = "test@example.com", Password = null! };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.LoginAsync(model));
        }

        [Fact]
        public async Task LoginAsync_ReturnsFailure_WhenUserIsInactive()
        {
            var model = new ApplicationUserLoginInputModel { Email = "test@example.com", Password = "Password1!" };
            var user = new ApplicationUser { Email = model.Email, Id = "1", IsActive = false };
            var signInResult = SignInResult.Success;

            _signInManagerMock.Setup(s => s.PasswordSignInAsync(model.Email, model.Password, false, false))
                .ReturnsAsync(signInResult);
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(user);

            var result = await _service.LoginAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("User is inactive", result.Message);
        }

        [Theory]
        [InlineData(true, false, false, "User is locked out.")]
        [InlineData(false, true, false, "Login is not allowed.")]
        [InlineData(false, false, true, "Two-factor authentication is required.")]
        [InlineData(false, false, false, "Invalid login attempt.")]
        public async Task LoginAsync_ReturnsFailure_ForSignInFailures(
            bool isLockedOut, bool isNotAllowed, bool requiresTwoFactor, string expectedMessage)
        {
            var model = new ApplicationUserLoginInputModel { Email = "test@example.com", Password = "Password1!" };
            var signInResult = SignInResult.Failed;
            typeof(SignInResult)
                .GetProperty(nameof(SignInResult.IsLockedOut))!
                .SetValue(signInResult, isLockedOut);
            typeof(SignInResult)
                .GetProperty(nameof(SignInResult.IsNotAllowed))!
                .SetValue(signInResult, isNotAllowed);
            typeof(SignInResult)
                .GetProperty(nameof(SignInResult.RequiresTwoFactor))!
                .SetValue(signInResult, requiresTwoFactor);

            _signInManagerMock.Setup(s => s.PasswordSignInAsync(model.Email, model.Password, false, false))
                .ReturnsAsync(signInResult);

            var result = await _service.LoginAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal(expectedMessage, result.Message);
        }

        #endregion

        #region RegisterAsync

        [Fact]
        public async Task RegisterAsync_ReturnsSuccess_WhenRegistrationSucceeds()
        {
            var model = new ApplicationUserRegisterInputModel
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe"
            };
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _service.RegisterAsync(model);

            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
            Assert.Equal("User created successfully", result.Message);
        }

        [Fact]
        public async Task RegisterAsync_ThrowsArgumentNullException_WhenEmailIsNull()
        {
            var model = new ApplicationUserRegisterInputModel
            {
                Email = null!,
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe"
            };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.RegisterAsync(model));
        }

        [Fact]
        public async Task RegisterAsync_ThrowsArgumentNullException_WhenPasswordIsNull()
        {
            var model = new ApplicationUserRegisterInputModel
            {
                Email = "test@example.com",
                Password = null!,
                FirstName = "John",
                LastName = "Doe"
            };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.RegisterAsync(model));
        }

        [Fact]
        public async Task RegisterAsync_ReturnsFailure_WithErrorCodes_WhenRegistrationFails()
        {
            var model = new ApplicationUserRegisterInputModel
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe"
            };
            var errors = new[] { new IdentityError { Code = "DuplicateEmail" }, new IdentityError { Code = "PasswordTooShort" } };
            var resultFail = IdentityResult.Failed(errors);

            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(resultFail);

            var result = await _service.RegisterAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Contains("DuplicateEmail", result.Message);
            Assert.Contains("PasswordTooShort", result.Message);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsFailure_WithGenericMessage_WhenNoErrorCodes()
        {
            var model = new ApplicationUserRegisterInputModel
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe"
            };
            var resultFail = IdentityResult.Failed();

            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(resultFail);

            var result = await _service.RegisterAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("Unable to register user due to unknown errors.", result.Message);
            Assert.False(result.Data);
        }

        #endregion

        #region ConfirmEmailAsync

        [Fact]
        public async Task ConfirmEmailAsync_SendsCode_WhenUserExists_AndNotConfirmed()
        {
            var model = new ApplicationUserConfirmEmailInputModel
            {
                Email = "test@example.com",
                EmailTemplate = "Hello {FullName}, your code is {Code}"
            };
            var user = new ApplicationUser { Email = model.Email, FirstName = "John", LastName = "Doe", EmailConfirmed = false };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _emailSenderMock.Setup(e => e.SendEmailAsync(It.IsAny<MailMessage>())).Returns(Task.CompletedTask);

            var result = await _service.ConfirmEmailAsync(model);

            Assert.True(result.IsSuccess);
            Assert.Equal("Verification code sent successfully", result.Message);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ThrowsArgumentNullException_WhenEmailIsNull()
        {
            var model = new ApplicationUserConfirmEmailInputModel { Email = null!, EmailTemplate = _emailTemplate };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ConfirmEmailAsync(model));
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsFailure_WhenUserNotFound()
        {
            var model = new ApplicationUserConfirmEmailInputModel { Email = "notfound@example.com", EmailTemplate = _emailTemplate };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser?)null);

            var result = await _service.ConfirmEmailAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsFailure_WhenEmailAlreadyConfirmed()
        {
            var model = new ApplicationUserConfirmEmailInputModel { Email = "test@example.com", EmailTemplate = _emailTemplate };
            var user = new ApplicationUser { Email = model.Email, EmailConfirmed = true };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(user);

            var result = await _service.ConfirmEmailAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("Email already confirmed", result.Message);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsFailure_WhenUpdateFails()
        {
            var model = new ApplicationUserConfirmEmailInputModel { Email = "test@example.com", EmailTemplate = _emailTemplate };
            var user = new ApplicationUser { Email = model.Email, EmailConfirmed = false };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Failed());

            var result = await _service.ConfirmEmailAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("Failed to save verification code", result.Message);
        }

        #endregion

        #region ConfirmEmailVerifyCodeAsync

        [Fact]
        public async Task ConfirmEmailVerifyCodeAsync_ConfirmsEmail_WhenCodeMatches()
        {
            var model = new ApplicationUserConfirmEmailInputModel { Email = "test@example.com", Code = "1234", EmailTemplate = _emailTemplate };
            var user = new ApplicationUser { Email = model.Email, VerificationCode = 1234, EmailConfirmed = false };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _service.ConfirmEmailVerifyCodeAsync(model);

            Assert.True(result.IsSuccess);
            Assert.Equal("Email confirmed successfully", result.Message);
        }

        [Fact]
        public async Task ConfirmEmailVerifyCodeAsync_ThrowsArgumentNullException_WhenEmailIsNull()
        {
            var model = new ApplicationUserConfirmEmailInputModel { Email = null!, Code = "1234", EmailTemplate = _emailTemplate };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ConfirmEmailVerifyCodeAsync(model));
        }

        [Fact]
        public async Task ConfirmEmailVerifyCodeAsync_ThrowsArgumentNullException_WhenCodeIsNull()
        {
            var model = new ApplicationUserConfirmEmailInputModel { Email = "test@example.com", Code = null, EmailTemplate = _emailTemplate };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ConfirmEmailVerifyCodeAsync(model));
        }

        [Fact]
        public async Task ConfirmEmailVerifyCodeAsync_ReturnsFailure_WhenUserNotFound()
        {
            var model = new ApplicationUserConfirmEmailInputModel { Email = "notfound@example.com", Code = "1234", EmailTemplate = _emailTemplate };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser?)null);

            var result = await _service.ConfirmEmailVerifyCodeAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task ConfirmEmailVerifyCodeAsync_ReturnsFailure_WhenCodeDoesNotMatch()
        {
            var model = new ApplicationUserConfirmEmailInputModel { Email = "test@example.com", Code = "9999" , EmailTemplate = _emailTemplate };
            var user = new ApplicationUser { Email = model.Email, VerificationCode = 1234 };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(user);

            var result = await _service.ConfirmEmailVerifyCodeAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid confirmation code", result.Message);
        }

        [Fact]
        public async Task ConfirmEmailVerifyCodeAsync_ReturnsFailure_WhenUpdateFails()
        {
            var model = new ApplicationUserConfirmEmailInputModel { Email = "test@example.com", Code = "1234" , EmailTemplate = _emailTemplate };
            var user = new ApplicationUser { Email = model.Email, VerificationCode = 1234 };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Failed());

            var result = await _service.ConfirmEmailVerifyCodeAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("Email confirmation failed", result.Message);
        }

        #endregion

        #region ForgotPasswordAsync

        [Fact]
        public async Task ForgotPasswordAsync_SendsCode_WhenUserExists()
        {
            var model = new ApplicationUserForgotPasswordInputModel { Email = "test@example.com", EmailTemplate = _emailTemplate };
            var user = new ApplicationUser { Email = model.Email };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _emailSenderMock.Setup(e => e.SendEmailAsync(It.IsAny<MailMessage>())).Returns(Task.CompletedTask);

            var result = await _service.ForgotPasswordAsync(model);

            Assert.True(result.IsSuccess);
            Assert.Equal("Verification code sent successfully", result.Message);
        }

        [Fact]
        public async Task ForgotPasswordAsync_ThrowsArgumentNullException_WhenEmailIsNull()
        {
            var model = new ApplicationUserForgotPasswordInputModel { Email = null!, EmailTemplate = _emailTemplate };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ForgotPasswordAsync(model));
        }

        [Fact]
        public async Task ForgotPasswordAsync_ReturnsFailure_WhenUserNotFound()
        {
            var model = new ApplicationUserForgotPasswordInputModel { Email = "notfound@example.com", EmailTemplate = _emailTemplate };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser?)null);

            var result = await _service.ForgotPasswordAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task ForgotPasswordAsync_ReturnsFailure_WhenUpdateFails()
        {
            var model = new ApplicationUserForgotPasswordInputModel { Email = "test@example.com", EmailTemplate = _emailTemplate };
            var user = new ApplicationUser { Email = model.Email };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Failed());

            var result = await _service.ForgotPasswordAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("Failed to save verification code", result.Message);
        }

        #endregion

        #region ResetPasswordAsync

        [Fact]
        public async Task ResetPasswordAsync_ResetsPassword_WhenCodeMatches()
        {
            var model = new ApplicationUserForgotPasswordInputModel
            {
                Email = "test@example.com",
                Code = "1234",
                Password = "NewPassword1!",
                EmailTemplate = _emailTemplate
            };
            var user = new ApplicationUser { Email = model.Email, VerificationCode = 1234 };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
            _userManagerMock.Setup(u => u.ResetPasswordAsync(user, "reset-token", model.Password)).ReturnsAsync(IdentityResult.Success);

            var result = await _service.ResetPasswordAsync(model);

            Assert.True(result.IsSuccess);
            Assert.Equal("Password reset successful", result.Message);
        }

        [Fact]
        public async Task ResetPasswordAsync_ThrowsArgumentNullException_WhenEmailIsNull()
        {
            var model = new ApplicationUserForgotPasswordInputModel { Email = null!, Code = "1234", Password = "P", EmailTemplate = _emailTemplate };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ResetPasswordAsync(model));
        }

        [Fact]
        public async Task ResetPasswordAsync_ThrowsArgumentNullException_WhenCodeIsNull()
        {
            var model = new ApplicationUserForgotPasswordInputModel { Email = "test@example.com", Code = null, Password = "P", EmailTemplate = _emailTemplate };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ResetPasswordAsync(model));
        }

        [Fact]
        public async Task ResetPasswordAsync_ThrowsArgumentNullException_WhenPasswordIsNull()
        {
            var model = new ApplicationUserForgotPasswordInputModel { Email = "test@example.com", Code = "1234", Password = null, EmailTemplate = _emailTemplate };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ResetPasswordAsync(model));
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFailure_WhenUserNotFound()
        {
            var model = new ApplicationUserForgotPasswordInputModel { Email = "notfound@example.com", Code = "1234", Password = "P", EmailTemplate = _emailTemplate };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser?)null);

            var result = await _service.ResetPasswordAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFailure_WhenCodeDoesNotMatch()
        {
            var model = new ApplicationUserForgotPasswordInputModel { Email = "test@example.com", Code = "9999", Password = "P", EmailTemplate = _emailTemplate };
            var user = new ApplicationUser { Email = model.Email, VerificationCode = 1234 };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(user);

            var result = await _service.ResetPasswordAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid confirmation code", result.Message);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFailure_WhenResetFails()
        {
            var model = new ApplicationUserForgotPasswordInputModel { Email = "test@example.com", Code = "1234", Password = "P", EmailTemplate = _emailTemplate };
            var user = new ApplicationUser { Email = model.Email, VerificationCode = 1234 };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
            _userManagerMock.Setup(u => u.ResetPasswordAsync(user, "reset-token", model.Password)).ReturnsAsync(IdentityResult.Failed());

            var result = await _service.ResetPasswordAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("Unable to reset password", result.Message);
        }

        #endregion

        #region RefreshTokenAsync

        [Fact]
        public async Task RefreshTokenAsync_ReturnsNewTokens_WhenValid()
        {
            var model = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
            var claims = new List<Claim> { new Claim(TokenParameters.Email, "test@example.com"), new Claim(TokenParameters.UserId, "1") };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var user = new ApplicationUser
            {
                Email = "test@example.com",
                Id = "1",
                RefreshToken = "refresh",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1),
                RefreshTokenAttemptCount = 0,
                IsActive = true
            };

            _tokenHandlerMock.Setup(t => t.GetPrincipalFromExpiredToken(model.AccessToken)).Returns(principal);
            _userManagerMock.Setup(u => u.FindByEmailAsync("test@example.com")).ReturnsAsync(user);
            _tokenHandlerMock.Setup(t => t.GetMaxRefreshTokenAttempts()).Returns(5);
            _tokenHandlerMock.Setup(t => t.GenerateJwtToken(It.IsAny<List<Claim>>())).Returns("new-access");
            _tokenHandlerMock.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh");
            _tokenHandlerMock.Setup(t => t.GetRefreshTokenExpiryDays()).Returns(7);
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _service.RefreshTokenAsync(model);

            Assert.NotNull(result.Data);
            Assert.True(result.IsSuccess);
            Assert.Equal("Token refreshed successfully", result.Message);
            Assert.Equal("new-access", result.Data.AccessToken);
            Assert.Equal("new-refresh", result.Data.RefreshToken);
            Assert.True(result.Data.IsRefreshTokenValid);
        }

        [Fact]
        public async Task RefreshTokenAsync_ThrowsArgumentNullException_WhenAccessTokenIsNull()
        {
            var model = new AuthenticationTokens { AccessToken = null, RefreshToken = "refresh" };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.RefreshTokenAsync(model));
        }

        [Fact]
        public async Task RefreshTokenAsync_ThrowsArgumentNullException_WhenRefreshTokenIsNull()
        {
            var model = new AuthenticationTokens { AccessToken = "access", RefreshToken = null };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.RefreshTokenAsync(model));
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsFailure_WhenPrincipalIsInvalid()
        {
            var model = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
            _tokenHandlerMock.Setup(t => t.GetPrincipalFromExpiredToken(model.AccessToken)).Returns((ClaimsPrincipal?)null);

            var result = await _service.RefreshTokenAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid token", result.Message);
            Assert.NotNull(result.Data);
            Assert.False(result.Data.IsRefreshTokenValid);
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsFailure_WhenUserNotFound()
        {
            var model = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
            var claims = new List<Claim> { new Claim(TokenParameters.Email, "notfound@example.com") };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            _tokenHandlerMock.Setup(t => t.GetPrincipalFromExpiredToken(model.AccessToken)).Returns(principal);
            _userManagerMock.Setup(u => u.FindByEmailAsync("notfound@example.com")).ReturnsAsync((ApplicationUser?)null);

            var result = await _service.RefreshTokenAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("User not found", result.Message);
            Assert.NotNull(result.Data);
            Assert.False(result.Data.IsRefreshTokenValid);
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsFailure_AndDisablesUser_WhenAttemptsExceeded()
        {
            var model = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
            var claims = new List<Claim> { new Claim(TokenParameters.Email, "test@example.com") };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var user = new ApplicationUser
            {
                Email = "test@example.com",
                RefreshTokenAttemptCount = 5,
                IsActive = true
            };
            _tokenHandlerMock.Setup(t => t.GetPrincipalFromExpiredToken(model.AccessToken)).Returns(principal);
            _userManagerMock.Setup(u => u.FindByEmailAsync("test@example.com")).ReturnsAsync(user);
            _tokenHandlerMock.Setup(t => t.GetMaxRefreshTokenAttempts()).Returns(5);
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _service.RefreshTokenAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("Refresh token limit exceeded", result.Message);
            Assert.NotNull(result.Data);
            Assert.False(result.Data.IsRefreshTokenValid);
            Assert.False(user.IsActive);
            Assert.Equal((byte)0, user.RefreshTokenAttemptCount);
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsFailure_WhenRefreshTokenIsInvalidOrExpired()
        {
            var model = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
            var claims = new List<Claim> { new Claim(TokenParameters.Email, "test@example.com") };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var user = new ApplicationUser
            {
                Email = "test@example.com",
                RefreshToken = "other-token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1),
                RefreshTokenAttemptCount = 0
            };
            _tokenHandlerMock.Setup(t => t.GetPrincipalFromExpiredToken(model.AccessToken)).Returns(principal);
            _userManagerMock.Setup(u => u.FindByEmailAsync("test@example.com")).ReturnsAsync(user);
            _tokenHandlerMock.Setup(t => t.GetMaxRefreshTokenAttempts()).Returns(5);
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _service.RefreshTokenAsync(model);

            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid refresh token", result.Message);
            Assert.NotNull(result.Data);
            Assert.False(result.Data.IsRefreshTokenValid);
            Assert.Equal((byte)1, user.RefreshTokenAttemptCount);
        }

        #endregion
    }
}
