using CRM.Model.ApplicationModels;
using CRM.Model.IdentityModels;
using CRM.Model.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Service.Identity
{
    public interface IUserService
    {
        Task<ResponseModel<bool>> UpdateUserProfileAsync(ApplicationUserProfileInputModel model, ApplicationUserContext userContext);
        Task<ResponseModel<bool>> ChangePasswordAsync(ApplicationUserChangePasswordInputModel model, ApplicationUserContext userContext);
    }
}
