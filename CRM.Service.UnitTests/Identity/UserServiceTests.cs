using System;
using System.Reflection;
using System.Threading.Tasks;
using CRM.Model.ApplicationModels;
using CRM.Model.Enums;
using CRM.Model.IdentityModels;
using CRM.Model.InputModels;
using CRM.Model.ViewModels;
using CRM.Service.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace CRM.Service.UnitTests.Identity
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            // Provide default values for non-nullable parameters
            var options = Mock.Of<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();
            var passwordHasher = Mock.Of<IPasswordHasher<ApplicationUser>>();
            var userValidators = new IUserValidator<ApplicationUser>[0];
            var passwordValidators = new IPasswordValidator<ApplicationUser>[0];
            var keyNormalizer = Mock.Of<ILookupNormalizer>();
            var errors = Mock.Of<IdentityErrorDescriber>();
            var services = Mock.Of<IServiceProvider>();
            var logger = Mock.Of<Microsoft.Extensions.Logging.ILogger<UserManager<ApplicationUser>>>();

            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, options, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger
            );
            _userService = new UserService(_userManagerMock.Object);
        }

        #region GetUserProfileAsync

        [Fact]
        public async Task GetUserProfileAsync_ReturnsProfile_WhenUserExists()
        {
            var userId = "user1";
            var user = new ApplicationUser { Id = userId, FirstName = "John", LastName = "Doe" };
            var context = new ApplicationUserContext { UserId = userId };

            _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

            var result = await _userService.GetUserProfileAsync(context);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("User profile retrieved successfully", result.Message);
            Assert.Equal(user.FirstName, result.Data.FirstName);
        }

        [Fact]
        public async Task GetUserProfileAsync_ThrowsArgumentNullException_WhenUserIdIsNull()
        {
            var context = new ApplicationUserContext { UserId = null };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.GetUserProfileAsync(context));
        }

        [Fact]
        public async Task GetUserProfileAsync_ThrowsException_WhenUserNotFound()
        {
            var context = new ApplicationUserContext { UserId = "notfound" };
            _userManagerMock.Setup(m => m.FindByIdAsync("notfound")).ReturnsAsync((ApplicationUser?)null);

            await Assert.ThrowsAsync<Exception>(() => _userService.GetUserProfileAsync(context));
        }

        #endregion

        #region UpdateUserProfileAsync

        [Fact]
        public async Task UpdateUserProfileAsync_UpdatesProfile_WhenValid()
        {
            var userId = "user1";
            var user = new ApplicationUser { Id = userId };
            var context = new ApplicationUserContext { UserId = userId };
            var model = new ApplicationUserProfileInputModel
            {
                FirstName = "Jane",
                LastName = "Smith",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = Gender.Female,
                ImageName = "img.png",
                ProfileDescription = "desc"
            };

            _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _userService.UpdateUserProfileAsync(model, context);

            Assert.True(result.IsSuccess);
            Assert.Equal("Profile updated successful", result.Message);
            Assert.Equal(model.FirstName, user.FirstName);
            Assert.Equal(model.LastName, user.LastName);
            Assert.Equal(model.DateOfBirth, user.DateOfBirth);
            Assert.Equal(model.Gender, user.Gender);
            Assert.Equal(model.ImageName, user.ImageName);
            Assert.Equal(model.ProfileDescription, user.ProfileDescription);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ThrowsArgumentNullException_WhenUserIdIsNull()
        {
            var model = new ApplicationUserProfileInputModel { FirstName = "A", LastName = "B" };
            var context = new ApplicationUserContext { UserId = null };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.UpdateUserProfileAsync(model, context));
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ThrowsArgumentNullException_WhenFirstNameIsNull()
        {
            var model = new ApplicationUserProfileInputModel { FirstName = string.Empty, LastName = "B" };
            var context = new ApplicationUserContext { UserId = "user1" };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.UpdateUserProfileAsync(
                new ApplicationUserProfileInputModel { FirstName = null!, LastName = "B" }, context));
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ThrowsArgumentNullException_WhenLastNameIsNull()
        {
            var model = new ApplicationUserProfileInputModel { FirstName = "A", LastName = string.Empty };
            var context = new ApplicationUserContext { UserId = "user1" };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.UpdateUserProfileAsync(
                new ApplicationUserProfileInputModel { FirstName = "A", LastName = null! }, context));
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ThrowsException_WhenUserNotFound()
        {
            var model = new ApplicationUserProfileInputModel { FirstName = "A", LastName = "B" };
            var context = new ApplicationUserContext { UserId = "notfound" };
            _userManagerMock.Setup(m => m.FindByIdAsync("notfound")).ReturnsAsync((ApplicationUser?)null);

            await Assert.ThrowsAsync<Exception>(() => _userService.UpdateUserProfileAsync(model, context));
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ReturnsFailure_WhenUpdateFails()
        {
            var userId = "user1";
            var user = new ApplicationUser { Id = userId };
            var context = new ApplicationUserContext { UserId = userId };
            var model = new ApplicationUserProfileInputModel { FirstName = "A", LastName = "B" };

            _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Failed());

            var result = await _userService.UpdateUserProfileAsync(model, context);

            Assert.False(result.IsSuccess);
            Assert.Equal("Profile update failed", result.Message);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_DoesNotUpdateOptionalFields_WhenNotProvided()
        {
            var userId = "user1";
            var user = new ApplicationUser { Id = userId };
            var context = new ApplicationUserContext { UserId = userId };
            var model = new ApplicationUserProfileInputModel { FirstName = "A", LastName = "B" };

            _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _userService.UpdateUserProfileAsync(model, context);

            Assert.True(result.IsSuccess);
            // Optional fields remain unchanged (null)
            Assert.Null(user.DateOfBirth);
            Assert.Null(user.Gender);
            Assert.Null(user.ImageName);
            Assert.Null(user.ProfileDescription);
        }

        #endregion

        #region ChangePasswordAsync

        [Fact]
        public async Task ChangePasswordAsync_ChangesPassword_WhenValid()
        {
            var userId = "user1";
            var user = new ApplicationUser { Id = userId };
            var context = new ApplicationUserContext { UserId = userId };
            var model = new ApplicationUserChangePasswordInputModel
            {
                CurrentPassword = "oldpass",
                NewPassword = "newpass"
            };

            _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _userService.ChangePasswordAsync(model, context);

            Assert.True(result.IsSuccess);
            Assert.Equal("Password changed successful", result.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_ThrowsArgumentNullException_WhenUserIdIsNull()
        {
            var model = new ApplicationUserChangePasswordInputModel { CurrentPassword = "a", NewPassword = "b" };
            var context = new ApplicationUserContext { UserId = null };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.ChangePasswordAsync(model, context));
        }

        [Fact]
        public async Task ChangePasswordAsync_ThrowsArgumentNullException_WhenNewPasswordIsNull()
        {
            var context = new ApplicationUserContext { UserId = "user1" };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.ChangePasswordAsync(
                new ApplicationUserChangePasswordInputModel { CurrentPassword = "a", NewPassword = null! }, context));
        }

        [Fact]
        public async Task ChangePasswordAsync_ThrowsArgumentNullException_WhenCurrentPasswordIsNull()
        {
            var context = new ApplicationUserContext { UserId = "user1" };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.ChangePasswordAsync(
                new ApplicationUserChangePasswordInputModel { CurrentPassword = null!, NewPassword = "b" }, context));
        }

        [Fact]
        public async Task ChangePasswordAsync_ThrowsException_WhenUserNotFound()
        {
            var model = new ApplicationUserChangePasswordInputModel { CurrentPassword = "a", NewPassword = "b" };
            var context = new ApplicationUserContext { UserId = "notfound" };
            _userManagerMock.Setup(m => m.FindByIdAsync("notfound")).ReturnsAsync((ApplicationUser?)null);

            await Assert.ThrowsAsync<Exception>(() => _userService.ChangePasswordAsync(model, context));
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnsFailure_WhenChangeFails()
        {
            var userId = "user1";
            var user = new ApplicationUser { Id = userId };
            var context = new ApplicationUserContext { UserId = userId };
            var model = new ApplicationUserChangePasswordInputModel { CurrentPassword = "a", NewPassword = "b" };

            _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword))
                .ReturnsAsync(IdentityResult.Failed());

            var result = await _userService.ChangePasswordAsync(model, context);

            Assert.False(result.IsSuccess);
            Assert.Equal("Password change failed", result.Message);
        }

        #endregion
    }
}
