using System;

namespace WYJK.Entity
{
    /// <summary>
    /// 基数订单
    /// </summary>
    public class BaseOrders
    {
        /// <summary>
        /// OrderID
        /// </summary>		
        public int OrderID { get; set; }
        /// <summary>
        /// OrderCode
        /// </summary>		
        public string OrderCode { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>		
        public int MemberID { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string MemberName { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>		
        public string PaymentMethod { get; set; }
        /// <summary>
        /// 生成时间
        /// </summary>		
        public DateTime GenerateDate { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>		
        public string Status { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>		
        public DateTime PayTime { get; set; }
        /// <summary>
        /// 审核时间
        /// </summary>		
        public DateTime AuditTime { get; set; }
        /// <summary>
        /// 参保人ID
        /// </summary>		
        public int SocialSecurityPeopleID { get; set; }
        /// <summary>
        /// IsPaySocialSecurity
        /// </summary>		
        public bool IsPaySocialSecurity { get; set; }
        /// <summary>
        /// 当前基数
        /// </summary>		
        public decimal SSCurrentBase { get; set; }
        /// <summary>
        /// 调整后基数
        /// </summary>		
        public decimal SSBaseAdjusted { get; set; }

        public string SSAdjustingBaseNote { get; set; }

        /// <summary>
        /// SSBaseServiceCharge
        /// </summary>		
        public decimal SSBaseServiceCharge { get; set; }
        /// <summary>
        /// IsPayAccumulationFund
        /// </summary>		
        public bool IsPayAccumulationFund { get; set; }
        /// <summary>
        /// 当前基数
        /// </summary>		
        public decimal AFCurrentBase { get; set; }
        /// <summary>
        /// 调整后基数
        /// </summary>		
        public decimal AFBaseAdjusted { get; set; }

        public string AFAdjustingBaseNote { get; set; }
        /// <summary>
        /// AFBaseServiceCharge
        /// </summary>		
        public decimal AFBaseServiceCharge { get; set; }
    }


    /// <summary>
    /// 调基分页查询类
    /// </summary>
    public class BaseOrdersParameter : PagedParameter
    {
        /// <summary>
        /// 客户姓名
        /// </summary>
        public string SocialSecurityPeopleName { get; set; }


        /// <summary>
        /// 审核状态
        /// </summary>
        public int Status { get; set; }

    }
}
