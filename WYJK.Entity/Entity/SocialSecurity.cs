using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
namespace WYJK.Entity
{
    /// <summary>
    /// 参保人的社保信息
    /// </summary>
    public class SocialSecurity
    {

        /// <summary>
        /// 社保ID
        /// </summary>		
        public int SocialSecurityID { get; set; }
        /// <summary>
        /// 参保人ID
        /// </summary>		
        public int SocialSecurityPeopleID { get; set; }

        /// <summary>
        /// 户口性质
        /// </summary>
        public string HouseholdProperty { get; set; }
        /// <summary>
        /// 参保人姓名
        /// </summary>
        public string SocialSecurityPeopleName { get; set; }
        /// <summary>
        /// 参保地
        /// </summary>		
        public string InsuranceArea { get; set; }
        /// <summary>
        /// 社保基数
        /// </summary>		
        public decimal SocialSecurityBase { get; set; }
        /// <summary>
        /// 缴费比例
        /// </summary>		
        public decimal PayProportion { get; set; }
        /// <summary>
        /// 起缴时间
        /// </summary>		
        public DateTime PayTime { get; set; }
        /// <summary>
        /// 缴费月数
        /// </summary>		
        public int PayMonthCount { get; set; }
        /// <summary>
        /// 已投月数
        /// </summary>
        public int AlreadyPayMonthCount { get; set; }

        /// <summary>
        /// 剩余月数
        /// </summary>
        public int RemainingMonths { get; set; }
        /// <summary>
        /// 补交月数
        /// </summary>		
        public int PayBeforeMonthCount { get; set; }
        /// <summary>
        /// 银行缴费月数
        /// </summary>		
        public int BankPayMonth { get; set; }
        /// <summary>
        /// 企业缴费月数
        /// </summary>		
        public int EnterprisePayMonth { get; set; }
        /// <summary>
        /// 备注
        /// </summary>		
        public string Note { get; set; }
        /// <summary>
        /// 状态
        /// </summary>		
        public string Status { get; set; }

        /// <summary>
        /// 社保号
        /// </summary>
        public string SocialSecurityNo { get; set; }

        /// <summary>
        /// 关联签约企业
        /// </summary>
        public int RelationEnterprise { get; set; }


        /// <summary>
        /// 业务办停时间
        /// </summary>
        public DateTime? StopDate { get; set; }


        /// <summary>
        /// 业务办理时间
        /// </summary>
        public DateTime? HandleDate { get; set; }

    }

    /// <summary>
    /// 社保业务显示列表(Admin)
    /// </summary>
    public class SocialSecurityShowModel
    {
        /// <summary>
        /// 客户类型
        /// </summary>
        public string UserType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MemberName { get; set; }
        /// <summary>
        /// 参保人ID
        /// </summary>
        public int SocialSecurityPeopleID { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>		
        public string SocialSecurityPeopleName { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>		
        public string IdentityCard { get; set; }
        /// <summary>
        /// 户口性质
        /// </summary>		
        public string HouseholdProperty { get; set; }

        /// <summary>
        /// 社保ID
        /// </summary>		
        public int SocialSecurityID { get; set; }
        /// <summary>
        /// 参保地
        /// </summary>		
        public string InsuranceArea { get; set; }
        /// <summary>
        /// 社保基数
        /// </summary>		
        public decimal SocialSecurityBase { get; set; }
        /// <summary>
        /// 缴费比例
        /// </summary>		
        public decimal PayProportion { get; set; }
        /// <summary>
        /// 起缴时间
        /// </summary>		
        public DateTime PayTime { get; set; }
        /// <summary>
        /// 缴费月数
        /// </summary>		
        public int PayMonthCount { get; set; }
        /// <summary>
        /// 补交月数
        /// </summary>		
        public int PayBeforeMonthCount { get; set; }
        /// <summary>
        /// 银行缴费月数
        /// </summary>		
        public int BankPayMonth { get; set; }
        /// <summary>
        /// 企业缴费月数
        /// </summary>		
        public int EnterprisePayMonth { get; set; }

        /// <summary>
        /// 停保原因
        /// </summary>
        public string StopReason { get; set; }
        /// <summary>
        /// 申请停保时间
        /// </summary>
        public DateTime? ApplyStopDate { get; set; }
        /// <summary>
        /// 停保原因
        /// </summary>
        public DateTime? StopDate { get; set; }

        /// <summary>
        /// 是否欠费
        /// </summary>
        public bool IsArrears { get; set; }

        /// <summary>
        /// 状态
        /// </summary>		
        public string Status { get; set; }
    }

    /// <summary>
    /// 社保参数(Admin)
    /// </summary>
    public class SocialSecurityParameter : PagedParameter
    {
        /// <summary>
        /// 客户姓名
        /// </summary>
        public string SocialSecurityPeopleName { get; set; }
        /// <summary>
        /// 身份证号码
        /// </summary>
        public string IdentityCard { get; set; }
        /// <summary>
        /// 客户类型 --用户
        /// </summary>
        public string UserType { get; set; }

        /// <summary>
        /// 社保状态
        /// </summary>
        public string Status { get; set; }


    }

    /// <summary>
    /// 社保与公积金详情(Mobile)
    /// </summary>
    public class SocialSecurityDetail
    {
        /// <summary>
        /// 是否交社保
        /// </summary>
        public bool IsSocialSecurity { get; set; }
        /// <summary>
        /// 是否交公积金
        /// </summary>
        public bool IsAccumulationFund { get; set; }
        /// <summary>
        /// 参保人姓名
        /// </summary>
        public string SocialSecurityPeopleName { get; set; }
        /// <summary>
        /// 社保基数
        /// </summary>
        public string SocialSecurityBase { get; set; }
        /// <summary>
        /// 投保地区
        /// </summary>
        public string InsuranceArea { get; set; }
        /// <summary>
        /// 投保时间
        /// </summary>
        public DateTime SSPayTime { get; set; }

        /// <summary>
        /// 社保已交月数
        /// </summary>
        public int SSAlreadyPayMonthCount { get; set; }
        /// <summary>
        /// 社保剩余月数
        /// </summary>
        public int SSRemainingMonths { get; set; }
        /// <summary>
        /// 公积金基数
        /// </summary>
        public string AccumulationFundBase { get; set; }
        /// <summary>
        /// 公积金地区
        /// </summary>
        public string AccumulationFundArea { get; set; }
        /// <summary>
        /// 公积金投缴时间
        /// </summary>
        public DateTime AFPayTime { get; set; }
        /// <summary>
        /// 公积金已投月数
        /// </summary>
        public int AFAlreadyPayMonthCount { get; set; }
        /// <summary>
        /// 公积金剩余月数
        /// </summary>
        public int AFRemainingMonths { get; set; }
    }
}

