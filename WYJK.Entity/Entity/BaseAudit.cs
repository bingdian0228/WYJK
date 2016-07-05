using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 基数审核
    /// </summary>
    public class BaseAudit
    {
        /// <summary>
		/// ID
        /// </summary>		
        public int ID { get; set; }
        /// <summary>
        /// 参保人ID
        /// </summary>		
        public int SocialSecurityPeopleID { get; set; }
        /// <summary>
        /// 当前基数
        /// </summary>
        public decimal CurrentBase { get; set; }
        /// <summary>
        /// 调整后基数
        /// </summary>		
        public decimal BaseAdjusted { get; set; }
        /// <summary>
        /// 申请时间
        /// </summary>		
        public DateTime ApplyDate { get; set; }
        /// <summary>
        /// 审核时间
        /// </summary>		
        public DateTime? AuditDate { get; set; }
        /// <summary>
        /// 审核状态
        /// </summary>		
        public string Status { get; set; }
        /// <summary>
        /// 类型（社保：0、公积金：1）
        /// </summary>		
        public string Type { get; set; }
    }

    /// <summary>
    /// 基数调整列表
    /// </summary>
    public class BaseAuditList : BaseAudit
    {
        /// <summary>
        /// 用户类型
        /// </summary>
        public string UserType { get; set; }
        public string MemberID { get; set; }
        /// <summary>
        /// 代理机构
        /// </summary>
        public string MemberName { get; set; }

        public string EnterpriseName { get; set; }

        public string BusinessName { get; set; }

        /// <summary>
        /// 参保人名称
        /// </summary>
        public string SocialSecurityPeopleName { get; set; }

        /// <summary>
        /// 当前基数
        /// </summary>
        public string CurrentBase { get; set; }


    }

    /// <summary>
    /// 基数审核参数
    /// </summary>
    public class BaseAuditParameter : PagedParameter
    {
        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 代理机构
        /// </summary>
        public string MemberID { get; set; }

        /// <summary>
        /// 参保人名称
        /// </summary>
        public string SocialSecurityPeopleName { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public string Status { get; set; }
    }

    /// <summary>
    /// 调整基数
    /// </summary>

    public class AdjustingBase
    {
        /// <summary>
        /// 参保人ID
        /// </summary>
        public int SocialSecurityPeopleID { get; set; }

        /// <summary>
        /// 是否正常缴纳社保
        /// </summary>
        public bool IsPaySocialSecurity { get; set; }
        /// <summary>
        /// 是否正常缴纳公积金
        /// </summary>
        public bool IsPayAccumulationFund { get; set; }
        /// <summary>
        /// 社保基数
        /// </summary>
        public decimal SocialSecurityBase { get; set; }
        /// <summary>
        /// 社保最小基数
        /// </summary>
        public decimal SocialSecurityMinBase { get; set; }
        /// <summary>
        /// 社保最大基数
        /// </summary>
        public decimal SocialSecurityMaxBase { get; set; }
        /// <summary>
        /// 公积金基数
        /// </summary>
        public decimal AccumulationFundBase { get; set; }

        /// <summary>
        /// 公积金最小基数
        /// </summary>
        public decimal AccumulationFundMinBase { get; set; }
        /// <summary>
        /// 公积金最大基数
        /// </summary>
        public decimal AccumulationFundMaxBase { get; set; }

    }

    /// <summary>
    /// 调整基数参数类
    /// </summary>
    public class AdjustingBaseParameter
    {
        /// <summary>
        /// 参保人ID
        /// </summary>
        public int SocialSecurityPeopleID { get; set; }
        /// <summary>
        /// 是否正常缴纳社保
        /// </summary>
        public bool IsPaySocialSecurity { get; set; }
        /// <summary>
        /// 是否正常缴纳公积金
        /// </summary>
        public bool IsPayAccumulationFund { get; set; }
        /// <summary>
        /// 调整后的社保基数
        /// </summary>
        public decimal SocialSecurityBaseAdjusted { get; set; }
        /// <summary>
        /// 社保调基备注
        /// </summary>
        public string SSAdjustingBaseNote { get; set; } = "";

        /// <summary>
        /// 调整后的公积金基数
        /// </summary>
        public decimal AccumulationFundBaseAdjusted { get; set; }
        /// <summary>
        /// 公积金调基备注
        /// </summary>
        public string AFAdjustingBaseNote { get; set; } = "";
        /// <summary>
        /// 平台类型
        /// </summary>
        public string PlatType { get; set; } = "1";
    }
}
