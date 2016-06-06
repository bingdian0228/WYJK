using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using WYJK.Framework.Helpers;

namespace WYJK.Entity
{
    /// <summary>
    /// 注册用户
    /// </summary>
    public class Members : ExtensionInformation
    {
        /// <summary>
        /// 用户ID
        /// </summary>		
        public int MemberID { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>		
        public string MemberName { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>		
        public string MemberPhone { get; set; }
        /// <summary>
        /// 密码
        /// </summary>		
        public string Password { get; set; }
        /// <summary>
        /// 注册邀请码
        /// </summary>		
        public string InviteCode { get; set; }

        /// <summary>
        /// 传真
        /// </summary>		
        public string Fax { get; set; }
        /// <summary>
        /// 营业执照编码
        /// </summary>		
        public string BusinessCode { get; set; }
        /// <summary>
        /// 经营者身份证号
        /// </summary>		
        public string BusinessIdentityCardNo { get; set; }
        /// <summary>
        /// 营业执照名称
        /// </summary>		
        public string BusinessName { get; set; }
        /// <summary>
        /// 经营者姓名
        /// </summary>		
        public string BusinessUser { get; set; }
        /// <summary>
        /// 经营者身份证正反面
        /// </summary>		
        public string BusinessIdentityPhoto { get; set; }
        /// <summary>
        /// 营业执照照片
        /// </summary>		
        public string BusinessLicensePhoto { get; set; }
        /// <summary>
        /// 企业申请人姓名
        /// </summary>		
        public string EnterpriseApplyName { get; set; }
        /// <summary>
        /// 申请人身份证号
        /// </summary>		
        public string EnterpriseApplyIdentityCardNo { get; set; }
        /// <summary>
        /// 申请人身份证照片正反面
        /// </summary>		
        public string EnterpriseApplyIdentityPhoto { get; set; }
        /// <summary>
        /// 企业申请人手机
        /// </summary>		
        public string EnterpriseApplyUserPhone { get; set; }
        /// <summary>
        /// 申请人邮箱
        /// </summary>		
        public string EnterpriseApplyUserEmail { get; set; }
        /// <summary>
        /// 授权协议数照片
        /// </summary>		
        public string EnterpriseLicensePhone { get; set; }
        /// <summary>
        /// 企业名称
        /// </summary>		
        public string EnterpriseName { get; set; }
        /// <summary>
        /// 行业类型
        /// </summary>		
        public string EnterpriseType { get; set; }
        /// <summary>
        /// 所在城市
        /// </summary>		
        public string EnterpriseArea { get; set; }
        /// <summary>
        /// 法人代表
        /// </summary>		
        public string EnterpriseLegal { get; set; }
        /// <summary>
        /// 法人代表身份证号
        /// </summary>		
        public string EnterpriseLegalIdentityCardNo { get; set; }
        /// <summary>
        /// 企业人数范围
        /// </summary>		
        public string EnterprisePeopleNum { get; set; }
        /// <summary>
        /// 社会信用代码
        /// </summary>
        public string SocialSecurityCreditCode { get; set; }
        /// <summary>
        /// 是否三证合一
        /// </summary>		
        public string EnterpriseIsThreeInOne { get; set; }
        /// <summary>
        /// 营业执照
        /// </summary>		
        public string EnterpriseBusinessLicense { get; set; }
        /// <summary>
        /// 税务登记证
        /// </summary>		
        public string EnterpriseTaxRegistrationCertificate { get; set; }
        /// <summary>
        /// 组织机构代码证
        /// </summary>		
        public string EnterpriseOrganizationCodeCertificate { get; set; }
        /// <summary>
        /// 企业固定电话
        /// </summary>		
        public string EnterpriseFixedTelePhone { get; set; }
        /// <summary>
        /// 是否已认证 0:未认证、1：已认证
        /// </summary>		
        public string IsAuthentication { get; set; }
        /// <summary>
        /// 用户类型 0：普通用户、1：企业用户、2：个体用户
        /// </summary>		
        public string UserType { get; set; }
        /// <summary>
        /// 纳税人名称
        /// </summary>		
        public string TaxpayerName { get; set; }
        /// <summary>
        /// 税号
        /// </summary>		
        public string TaxNumber { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>		
        public string ContactUser { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>		
        public string ContactUserPhone { get; set; }
        /// <summary>
        /// 开户电话
        /// </summary>		
        public string OpenAccountPhone { get; set; }

        /// <summary>
		/// 账户余额
        /// </summary>		
        public decimal Account { get; set; }
        /// <summary>
        /// 社保账户金额
        /// </summary>		
        public decimal SocialSecurityAmount { get; set; }
        /// <summary>
        /// 公积金账户金额
        /// </summary>		
        public decimal AccumulationFundAmount { get; set; }
        /// <summary>
        /// 服务费
        /// </summary>		
        public decimal ServiceCost { get; set; }
        /// <summary>
        /// 第一次代办费
        /// </summary>		
        public decimal FirstBacklogCost { get; set; }
        /// <summary>
        /// 补差（冻结金额）
        /// </summary>		
        public decimal Bucha { get; set; }

    }

    /// <summary>
    /// 用户简单信息
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class MemberSimpleModel
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int MemberID { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>		
        public string MemberName { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>		
        public string MemberPhone { get; set; }
        /// <summary>
        /// 密码
        /// </summary>		
        public string Password { get; set; }
        /// <summary>
        /// 加密后的密码
        /// </summary>
        [JsonIgnore]
        public string HashPassword
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Password))
                {
                    return SecurityHelper.HashPassword(Password, Password);
                }
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// 注册用户
    /// </summary>
    public class MemberRegisterModel : MemberSimpleModel
    {
        /// <summary>
        /// 注册邀请码
        /// </summary>		
        public string InviteCode { get; set; }
    }

    /// <summary>
    /// 忘记密码
    /// </summary>
    public class MemberForgetPasswordModel : MemberSimpleModel
    {

    }

    /// <summary>
    /// APP修改密码(Mobile)
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class MemberMidifyPassword
    {
        /// <summary>
        /// 用户ID
        /// </summary>		
        public int MemberID { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        public string MemberName { get; set; }
        /// <summary>
        /// 原密码
        /// </summary>
        [Required(ErrorMessage = "原密码不能为空")]
        public string OldPassword { get; set; }
        /// <summary>
        /// 原密码加密后的密码
        /// </summary>
        [JsonIgnore]
        public string HashOldPassword
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(OldPassword))
                {
                    return SecurityHelper.HashPassword(OldPassword, OldPassword);
                }
                return string.Empty;
            }
        }
        /// <summary>
        /// 新密码
        /// </summary>
        [Required(ErrorMessage = "新密码不能为空")]
        public string NewPassword { get; set; }
        /// <summary>
        /// 确认密码
        /// </summary>
        [Compare("NewPassword", ErrorMessage = "新密码和确认密码不匹配")]
        [Required(ErrorMessage = "确认密码不能为空")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// 新密码加密后的密码
        /// </summary>
        [JsonIgnore]
        public string HashNewPassword
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(NewPassword))
                {
                    return SecurityHelper.HashPassword(NewPassword, NewPassword);
                }
                return string.Empty;
            }
        }

    }

    /// <summary>
    /// 登录用户(Mobile)
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class MemberLoginModel
    {
        /// <summary>
        /// 用户ID
        /// </summary>		
        public int MemberID { get; set; }
        /// <summary>
        /// 用户名或手机号
        /// </summary>		
        public string Account { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>		
        public string MemberName { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>		
        public string MemberPhone { get; set; }
        /// <summary>
        /// 密码
        /// </summary>		
        public string Password { get; set; }
        /// <summary>
        /// 加密后的密码
        /// </summary>
        [JsonIgnore]
        public string HashPassword
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Password))
                {
                    return SecurityHelper.HashPassword(Password, Password);
                }
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// 企业资质认证(Mobile)
    /// </summary>
    public class EnterpriseCertification
    {

        /// <summary>
        /// 用户ID
        /// </summary>
        public int MemberID { get; set; }
        /// <summary>
        /// 企业名称
        /// </summary>		
        public string EnterpriseName { get; set; }
        /// <summary>
        /// 企业税号
        /// </summary>
        public string EnterpriseTax { get; set; }
        /// <summary>
        /// 行业类型
        /// </summary>		
        public string EnterpriseType { get; set; }
        /// <summary>
        /// 所在城市
        /// </summary>		
        public string EnterpriseArea { get; set; }
        /// <summary>
        /// 法人代表
        /// </summary>		
        public string EnterpriseLegal { get; set; }
        /// <summary>
        /// 法人代表身份证号
        /// </summary>		
        public string EnterpriseLegalIdentityCardNo { get; set; }
        /// <summary>
        /// 企业人数范围
        /// </summary>		
        public string EnterprisePeopleNum { get; set; }
        /// <summary>
        /// 社会信用代码
        /// </summary>
        public string SocialSecurityCreditCode { get; set; }
        /// <summary>
        /// 营业执照
        /// </summary>		
        public string EnterpriseBusinessLicense { get; set; }
    }

    /// <summary>
    /// 个体认证(Mobile)
    /// </summary>
    public class IndividualCertification
    {

        /// <summary>
        /// 用户ID
        /// </summary>
        public int MemberID { get; set; }
        /// <summary>
        /// 经营者身份证号
        /// </summary>		
        public string BusinessIdentityCardNo { get; set; }
        /// <summary>
        /// 营业执照名称
        /// </summary>		
        public string BusinessName { get; set; }
        /// <summary>
        /// 经营者姓名
        /// </summary>		
        public string BusinessUser { get; set; }
        /// <summary>
        /// 经营者身份证正反面 多张照片用；隔开
        /// </summary>		
        public string BusinessIdentityPhoto { get; set; }
        /// <summary>
        /// 营业执照照片 
        /// </summary>		
        public string BusinessLicensePhoto { get; set; }
    }

    /// <summary>
    /// 账户信息(Mobile)
    /// </summary>
    public class AccountInfo
    {
        /// <summary>
        /// 用户名
        /// </summary>		
        public string MemberName { get; set; }
        /// <summary>
		/// 账户余额
        /// </summary>		
        public decimal Account { get; set; }

        /// <summary>
        /// 补差（冻结金额）
        /// </summary>		
        public decimal Bucha { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string HeadPortrait { get; set; }
    }


    /// <summary>
    /// 信息补充(Mobile)
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class ExtensionInformation
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [JsonIgnore]
        public string MemberName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [JsonIgnore]
        public string Password { get; set; }
        /// <summary>
        /// 用户手机号
        /// </summary>
        [JsonIgnore]
        public string MemberPhone { get; set; }
        /// <summary>
        /// 注册邀请码
        /// </summary>		
        [JsonIgnore]
        public string InviteCode { get; set; }


        /// <summary>
        /// 是否补全信息 0：未补全，1：已补全
        /// </summary>
        [JsonIgnore]
        public string IsComplete { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>		
        public string TrueName { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>		
        public string CertificateType { get; set; }
        /// <summary>
        /// 证件号
        /// </summary>		
        public string CertificateNo { get; set; }
        /// <summary>
        /// 政治面貌
        /// </summary>		
        public string PoliticalStatus { get; set; }
        /// <summary>
        /// 学历
        /// </summary>		
        public string Education { get; set; }
        /// <summary>
        /// 生日
        /// </summary>		
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// 性别0：男，1：女
        /// </summary>		
        public string Sex { get; set; }
        /// <summary>
        /// 地址
        /// </summary>		
        public string Address { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>		
        public string Phone { get; set; }
        /// <summary>
        /// Email
        /// </summary>		
        public string Email { get; set; }
        /// <summary>
        /// QQ
        /// </summary>		
        public string QQ { get; set; }
        /// <summary>
        /// 支付宝账号
        /// </summary>		
        public string Alipay { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>		
        public string BankCardNo { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>		
        public string BankAccount { get; set; }
        /// <summary>
        /// 开户人
        /// </summary>		
        public string UserAccount { get; set; }
        /// <summary>
        /// 第二联系人
        /// </summary>		
        public string SecondContact { get; set; }
        /// <summary>
        /// 第二联系人手机
        /// </summary>		
        public string SecondContactPhone { get; set; }
        /// <summary>
        /// 投保地
        /// </summary>		
        public string InsuranceArea { get; set; }
        /// <summary>
        /// 户口性质
        /// </summary>		
        public string HouseholdType { get; set; }
    }

    /// <summary>
    /// 补充信息参数(Mobile)
    /// </summary>
    public class ExtensionInformationParameter : ExtensionInformation
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int MemberID { get; set; }
    }

    /// <summary>
    /// 后台客户信息统计
    /// </summary>
    public class MembersStatistics
    {
        public int MemberID { get; set; }
        public string UserType { get; set; }
        public string MemberName { get; set; }

        public string EnterpriseName { get; set; }

        public string BusinessName { get; set; }
        public string MemberPhone { get; set; }
        /// <summary>
        /// 人数
        /// </summary>
        public string SocialSecurityPeopleCount { get; set; }
        /// <summary>
        /// 账户金额
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 冻结费
        /// </summary>
        public string Bucha { get; set; }
        /// <summary>
        /// 账户状态
        /// </summary>
        public string AccountStatus { get; set; }
        /// <summary>
        /// 是否欠费
        /// </summary>
        public string IsArrears { get; set; }

    }


    /// <summary>
    /// 会员参数
    /// </summary>
    public class MembersParameters : PagedParameter
    {
        /// <summary>
        /// 用户类型
        /// </summary>
        public string UserType { get; set; }
        /// <summary>
        /// 注册用户ID（代理机构）
        /// </summary>
        public string MemberID { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string SocialSecurityPeopleName { get; set; }
    }


    /// <summary>
    /// 续费服务参数
    /// </summary>
    public class RenewalServiceParameters
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int MemberID { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public string PayMethod { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        ///服务月数
        /// </summary>
        public int MonthCount { get; set; }
    }

    /// <summary>
    /// 充值服务参数
    /// </summary>
    public class RechargeParameters
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int MemberID { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public string PayMethod { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// 头像
    /// </summary>
    public class HeadPortraitParameters
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int MemberID { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string HeadPortrait { get; set; }
    }
}

