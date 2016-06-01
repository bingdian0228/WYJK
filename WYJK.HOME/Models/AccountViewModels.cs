using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WYJK.HOME.Models
{
    public class LoginViewModel 
    {
        [Required]
        [Display(Name = "用户名")]
        //[EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我?")]
        public bool RememberMe { get; set; }


        [Required]
        [Display(Name = "验证码")]
        //[EmailAddress]
        public string CheckCode { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "用户名")]
        public string MemberName { get; set; }

        [Required]
        [Display(Name = "手机号")]
        public string MemberPhone { get; set; }

        [Required]
        [Display(Name = "密码")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "邀请码")]
        public string InviteCode { get; set; }

        [Display(Name = "短信验证码")]
        public string SMSCheckCode { get; set; }

        [Required]
        [Display(Name = "验证码")]
        public string CheckCode { get; set; }

        [Required]
        [Display(Name = "无忧借款服务协议")]
        public bool Agreement { get; set; }


    }
}