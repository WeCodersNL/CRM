using CRM.Model.InputModels;
using CRM.Service.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace CRM.Api.Areas.Identity
{
    [Area("Identity")]
    [DisplayName("Authentication Controller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class AuthenticationController(IAuthenticationService authenticationService) : ControllerBase
    {
        [HttpPost("login")]
        [DisplayName("Login")]
        public async Task<IActionResult> Login([FromBody] ApplicationUserLoginInputModel model)
        {
            var response = await authenticationService.LoginAsync(model);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("register")]
        [DisplayName("Register")]
        public async Task<IActionResult> Register([FromBody] ApplicationUserRegisterInputModel model)
        {
            var response = await authenticationService.RegisterAsync(model);
            return response.IsSuccess ? Ok() : BadRequest(response);
        }

        [HttpPost("confirm-email")]
        [DisplayName("Confirm Email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ApplicationUserConfirmEmailInputModel model)
        {
            var response = await authenticationService.ConfirmEmailAsync(model);
            return response.IsSuccess ? Ok() : BadRequest(response);
        }
        
        [HttpPost("confirm-email-verify-code")]
        [DisplayName("Confirm Email Verify Code")]
        public async Task<IActionResult> ConfirmEmailVerifyCode([FromBody] ApplicationUserConfirmEmailInputModel model)
        {
            var response = await authenticationService.ConfirmEmailVerifyCodeAsync(model);
            return response.IsSuccess ? Ok() : BadRequest(response);
        }

        [HttpPost("forgot-password")]
        [DisplayName("Forgot Password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ApplicationUserForgotPasswordInputModel model)
        {
            var response = await authenticationService.ForgotPasswordAsync(model);
            return response.IsSuccess ? Ok() : StatusCode(500);
        }
        
        [HttpPost("reset-password")]
        [DisplayName("Reset Password")]
        public async Task<IActionResult> ResetPassword([FromBody] ApplicationUserForgotPasswordInputModel model)
        {
            var response = await authenticationService.ResetPasswordAsync(model);
            return response.IsSuccess ? Ok() : StatusCode(500);
        }

        [HttpPost("refresh-token")]
        [DisplayName("Refresh Token")]
        public async Task<IActionResult> RefreshToken([FromBody] ApplicationUserRegisterInputModel model)
        {
            var response = await authenticationService.RefreshTokenAsync(model);
            return response ? Ok() : StatusCode(500);
        }
    }
}
