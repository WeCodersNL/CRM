using CRM.Model.ApplicationModels;
using CRM.Model.InputModels;

namespace CRM.WebBlazor.Service
{
    public interface IAuthenticationService
    {
        Task<ResponseModel<bool>> ConfirmEmailAsync(ApplicationUserConfirmEmailInputModel model);
        Task<ResponseModel<bool>> VerifyEmailCodeAsync(ApplicationUserConfirmEmailInputModel model);
    }
}
