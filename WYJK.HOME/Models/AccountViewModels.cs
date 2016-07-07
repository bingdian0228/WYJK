using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WYJK.Entity;

namespace WYJK.HOME.Models
{
    public class LoginViewModel
    {
        /// <summary>
        /// 用户ID
        /// </summary>		
        public int MemberID { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        [Display(Name = "用户名")]
        public string MemberName { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>		
        public string MemberPhone { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我?")]
        public bool RememberMe { get; set; }


        [Required]
        [Display(Name = "验证码")]
        public string CheckCode { get; set; }
    }

    public class PasswordViewModel
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        [Display(Name = "用户名")]
        public string MemberName { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        [Required]
        [Display(Name = "手机号")]
        public string MemberPhone { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "短信验证码")]
        public string SMSCheckCode { get; set; }

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

    public class InfoChangeViewModel
    {

        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        public int MemberID { get; set; }

        /// <summary>
        /// 是否补全信息 0：未补全，1：已补全
        /// </summary>
        [Display(Name = "是否补全信息")]
        public string IsComplete { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>		
        [Required]
        [Display(Name = "真实姓名")]
        public string TrueName { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>		
        [Required]
        [Display(Name = "证件类型")]
        public string CertificateType { get; set; }
        /// <summary>
        /// 证件号
        /// </summary>		
        [Required]
        [Display(Name = "证件号")]
        public string CertificateNo { get; set; }
        /// <summary>
        /// 政治面貌
        /// </summary>		
        [Required]
        [Display(Name = "政治面貌")]
        public string PoliticalStatus { get; set; }
        /// <summary>
        /// 学历
        /// </summary>		
        [Required]
        [Display(Name = "学历")]
        public string Education { get; set; }
        /// <summary>
        /// 生日
        /// </summary>		
        [Required]
        [Display(Name = "生日")]
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// 性别0：男，1：女
        /// </summary>		
        [Required]
        [Display(Name = "性别")]
        public string Sex { get; set; }
        /// <summary>
        /// 地址
        /// </summary>		
        [Required]
        [Display(Name = "地址")]
        public string Address { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>		
        [Required]
        [Display(Name = "手机号码")]
        public string Phone { get; set; }
        /// <summary>
        /// Email
        /// </summary>		
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
        /// <summary>
        /// QQ
        /// </summary>		
        [Required]
        [Display(Name = "QQ")]
        public string QQ { get; set; }
        /// <summary>
        /// 支付宝账号
        /// </summary>		
        [Required]
        [Display(Name = "支付宝账号")]
        public string Alipay { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>		
        [Required]
        [Display(Name = "银行卡号")]
        public string BankCardNo { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>		
        [Required]
        [Display(Name = "开户行")]
        public string BankAccount { get; set; }
        /// <summary>
        /// 开户人
        /// </summary>		
        [Required]
        [Display(Name = "开户人")]
        public string UserAccount { get; set; }
        /// <summary>
        /// 第二联系人
        /// </summary>		
        [Display(Name = "第二联系人")]
        public string SecondContact { get; set; }
        /// <summary>
        /// 第二联系人手机
        /// </summary>		
        [Display(Name = "第二联系人手机")]
        public string SecondContactPhone { get; set; }
        /// <summary>
        /// 投保地
        /// </summary>		
        [Required]
        [Display(Name = "投保地")]
        public string InsuranceArea { get; set; }
        /// <summary>
        /// 户口性质
        /// </summary>		
        [Required]
        [Display(Name = "户口性质")]
        public string HouseholdType { get; set; }
    }



}