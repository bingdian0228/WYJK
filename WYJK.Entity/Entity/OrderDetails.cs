using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 从订单
    /// </summary>
    public class OrderDetails
    {
        /// <summary>
        /// OrderDetailID
        /// </summary>		
        public int OrderDetailID { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>		
        public string OrderCode { get; set; }
        /// <summary>
        /// 参保人ID
        /// </summary>		
        public int SocialSecurityPeopleID { get; set; }
        /// <summary>
        /// 参保人名称
        /// </summary>		
        public string SocialSecurityPeopleName { get; set; }
        /// <summary>
        /// 社保每月金额
        /// </summary>		
        public decimal SocialSecurityAmount { get; set; }
        /// <summary>
        /// 社保缴费月数
        /// </summary>		
        public int SocialSecuritypayMonth { get; set; }
        /// <summary>
        /// 社保服务费
        /// </summary>		
        public decimal SocialSecurityServiceCost { get; set; }
        /// <summary>
        /// 社保第一次代办费
        /// </summary>		
        public decimal SocialSecurityFirstBacklogCost { get; set; }
        /// <summary>
        /// 社保补差
        /// </summary>		
        public decimal SocialSecurityBuCha { get; set; }
        /// <summary>
        /// 公积金每月金额
        /// </summary>		
        public decimal AccumulationFundAmount { get; set; }
        /// <summary>
        /// 公积金缴费月数
        /// </summary>		
        public int AccumulationFundpayMonth { get; set; }
        /// <summary>
        /// 公积金服务费
        /// </summary>		
        public decimal AccumulationFundServiceCost { get; set; }
        /// <summary>
        /// 公积金第一次代办费
        /// </summary>		
        public decimal AccumulationFundFirstBacklogCost { get; set; }
        /// <summary>
        /// 是否缴纳社保
        /// </summary>
        public bool IsPaySocialSecurity { get; set; }
        /// <summary>
        /// 是否缴纳公积金
        /// </summary>
        public bool IsPayAccumulationFund { get; set; }
    }
}
