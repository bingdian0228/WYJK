using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using WYJK.Entity;

namespace WYJK.Web.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "代码")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "记住此浏览器?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "电子邮件")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }
    }

    /// <summary>
    /// 员工的登录
    /// </summary>
    public class UserViewModel
    {
        [Display(Name = "账号")]
        [Required(ErrorMessage = "账号不能为空")]
        [StringLength(20, ErrorMessage = "账号太长了", MinimumLength = 0)]
        public string UserName { get; set; }

        [Display(Name = "密码")]
        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(20, ErrorMessage = "密码必须在6-20位之间", MinimumLength = 6)]
        public string Password { get; set; }

        [Display(Name = "验证码")]
        [Required(ErrorMessage = "验证码不能为空")]
        [StringLength(4, ErrorMessage = "验证码必须是4位数", MinimumLength = 4)]
        public string VerificationCode { get; set; }
    }


    //internal class EmployeeViewModelValidator : AbstractValidator<EmployeeViewModel>
    //{
    //    public EmployeeViewModelValidator()
    //    {
    //        RuleFor(entity => entity.Account).NotEmpty().WithMessage("账号不能为空").Length(0, 20).WithMessage("账号太长了");
    //        RuleFor(entity => entity.Password).NotEmpty().WithMessage("密码不能为空").Length(6, 20).WithMessage("密码必须在6-20字之间");
    //        RuleFor(entity => entity.VerificationCode).NotEmpty().WithMessage("验证码不能为空").Length(4).WithMessage("验证码必须是4个字");
    //    }
    //}


    /// <summary>
    /// 角色编辑
    /// </summary>
    public class RolesViewModel
    {
        public int RoleID { get; set; }
        //[System.Web.Mvc.Remote("CheckRoleName", "User", ErrorMessage = "角色名称已存在")]
        [Display(Name = "角色名称")]
        [Required(ErrorMessage = "角色不能为空")]
        public string RoleName { get; set; }
        [Display(Name = "描述")]
        public string Description { get; set; }

        /// <summary>
        /// 权限
        /// </summary>
        public string PermissionID { get; set; }


    }

    public static class RolesExtension
    {
        public static Roles ExtensionToModel(this RolesViewModel viewModel)
        {

            return new Roles()
            {
                RoleID = viewModel.RoleID,
                RoleName = viewModel.RoleName,
                Description = viewModel.Description
            };
        }
        public static RolesViewModel ExtensionToViewModel(this Roles model)
        {

            return new RolesViewModel()
            {
                RoleID = model.RoleID,
                RoleName = model.RoleName,
                Description = model.Description
            };
        }
    }
}
