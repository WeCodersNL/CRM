using CRM.Model.IdentityModels;
using CRM.Model.InputModels;
using CRM.Service.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace CRM.Api.Areas.Identity
{
    [Area("Identity")]
    [DisplayName("User Controller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpPost("update-user-profile")]
        [DisplayName("Update User Profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] ApplicationUserProfileInputModel model)
        {
            var userContext = new ApplicationUserContext()
            {
                UserId = User.FindFirst("UserId")?.Value,
                Email = User.FindFirst("Email")?.Value,
            };

            var response = await userService.UpdateUserProfileAsync(model, userContext);
            return response.IsSuccess ? Ok() : BadRequest(response);
        }

        [HttpPost("change-password")]
        [DisplayName("Change Password")]
        public async Task<IActionResult> ChangePassword([FromBody] ApplicationUserChangePasswordInputModel model)
        {
            var userContext = new ApplicationUserContext()
            {
                UserId = User.FindFirst("UserId")?.Value,
                Email = User.FindFirst("Email")?.Value,
            };

            var response = await userService.ChangePasswordAsync(model, userContext);
            return response.IsSuccess ? Ok() : BadRequest(response);
        }
    }
}
