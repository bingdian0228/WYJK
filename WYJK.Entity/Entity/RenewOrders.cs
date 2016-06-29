using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 续费订单
    /// </summary>
    public class RenewOrders
    {
        public int OrderID { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>		
        public string OrderCode { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>		
        public int MemberID { get; set; }
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
        /// 订单状态 0:待付款,1:审核中，2：已完成
        /// </summary>		
        public string Status { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? AuditTime { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Money { get; set; }
    }

    /// <summary>
    /// 续费分页查询类
    /// </summary>
    public class RenewOrdersParameter : PagedParameter
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
