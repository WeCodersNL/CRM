using CRM.Model.ApplicationModels;
using CRM.Model.IdentityModels;
using CRM.Model.InputModels;
using CRM.Model.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace CRM.Service.Identity
{
    public class UserService(UserManager<ApplicationUser> userManager) : IUserService
    {
        public async Task<ResponseModel<ApplicationUserProfileViewModel>> GetUserProfileAsync(ApplicationUserContext userContext)
        {
            ArgumentNullException.ThrowIfNull(userContext.UserId);

            var user = await userManager.FindByIdAsync(userContext.UserId) ?? throw new Exception("Unable to get user");
            return new ResponseModel<ApplicationUserProfileViewModel>
            {
                IsSuccess = true,
                Message = "User profile retrieved successfully",
                Data = new ApplicationUserProfileViewModel(user)
            };
        }

        public async Task<ResponseModel<bool>> UpdateUserProfileAsync(ApplicationUserProfileInputModel model, ApplicationUserContext userContext)
        {
            ArgumentNullException.ThrowIfNull(userContext.UserId);
            ArgumentNullException.ThrowIfNull(model.FirstName);
            ArgumentNullException.ThrowIfNull(model.LastName);

            var user = await userManager.FindByIdAsync(userContext.UserId) ?? throw new Exception("Unable to get user");
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            if (model.DateOfBirth.HasValue)
                user.DateOfBirth = model.DateOfBirth.Value;
            if (model.Gender.HasValue)
                user.Gender = model.Gender.Value;
            if (!string.IsNullOrEmpty(model.ImageName))
                user.ImageName = model.ImageName;


            var result = await userManager.UpdateAsync(user);
            return new ResponseModel<bool>
            {
                IsSuccess = result.Succeeded,
                Message = result.Succeeded ? "Profile updated successful" : "Profile update failed"
            };
        }

        public async Task<ResponseModel<bool>> ChangePasswordAsync(ApplicationUserChangePasswordInputModel model, ApplicationUserContext userContext)
        {
            ArgumentNullException.ThrowIfNull(userContext.UserId);
            ArgumentNullException.ThrowIfNull(model.NewPassword);
            ArgumentNullException.ThrowIfNull(model.CurrentPassword);

            var user = await userManager.FindByIdAsync(userContext.UserId) ?? throw new Exception("Unable to get user");

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            return new ResponseModel<bool>
            {
                IsSuccess = result.Succeeded,
                Message = result.Succeeded ? "Password changed successful" : "Password change failed"
            };
        }
    }
}
