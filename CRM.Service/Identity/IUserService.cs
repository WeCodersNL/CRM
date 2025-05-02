using CRM.Model.ApplicationModels;
using CRM.Model.IdentityModels;
using CRM.Model.InputModels;
using CRM.Model.ViewModels;

namespace CRM.Service.Identity
{
    public interface IUserService
    {
        Task<ResponseModel<ApplicationUserProfileViewModel>> GetUserProfileAsync(ApplicationUserContext userContext);
        Task<ResponseModel<bool>> UpdateUserProfileAsync(ApplicationUserProfileInputModel model, ApplicationUserContext userContext);
        Task<ResponseModel<bool>> ChangePasswordAsync(ApplicationUserChangePasswordInputModel model, ApplicationUserContext userContext);
    }
}
