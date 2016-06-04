using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
namespace WYJK.Entity
{
    /// <summary>
    /// 公积金信息表
    /// </summary>
    public class AccumulationFund
    {

        /// <summary>
        /// 公积金ID
        /// </summary>		
        public int AccumulationFundID { get; set; }
        /// <summary>
        /// 参保人ID
        /// </summary>		
        public int SocialSecurityPeopleID { get; set; }
        /// <summary>
        /// 参保人名称
        /// </summary>
        public string SocialSecurityPeopleName { get; set; }

        /// <summary>
        /// 户口性质
        /// </summary>
        public string HouseholdProperty { get; set; }
        /// <summary>
        /// 参公积金地
        /// </summary>		
        public string AccumulationFundArea { get; set; }
        /// <summary>
        /// 公积金基数
        /// </summary>		
        public decimal AccumulationFundBase { get; set; }
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
        /// 备注
        /// </summary>		
        public string Note { get; set; }
        /// <summary>
        /// 状态
        /// </summary>		
        public string Status { get; set; }
        /// <summary>
        /// 公积金号
        /// </summary>
        public string AccumulationFundNo { get; set; }


        /// <summary>
        /// 业务办停时间
        /// </summary>
        public DateTime? StopDate { get; set; }
        /// <summary>
        /// 业务办理时间
        /// </summary>
        public DateTime? HandleDate { get; set; }

        /// <summary>
        /// 关联签约企业号
        /// </summary>
        public int RelationEnterprise { get; set; }

        /// <summary>
        /// 公积金类型
        /// </summary>
        public string AccumulationFundType { get; set; }
    }

    /// <summary>
    /// 公积金业务显示列表（Admin）
    /// </summary>
    public class AccumulationFundShowModel
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
        /// 公积金ID
        /// </summary>		
        public int AccumulationFundID { get; set; }
        /// <summary>
        /// 参保地
        /// </summary>		
        public string AccumulationFundArea { get; set; }
        /// <summary>
        /// 社保基数
        /// </summary>		
        public decimal AccumulationFundBase { get; set; }
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
        /// 申请停公积金时间
        /// </summary>
        public DateTime? ApplyStopDate { get; set; }
        /// <summary>
        /// 停公积金时间
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
    /// 公积金参数 (Admin)
    /// </summary>
    public class AccumulationFundParameter : PagedParameter
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
}

