using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 主订单
    /// </summary>
    public class Order
    {
        /// <summary>
        /// 订单编号
        /// </summary>		
        public string OrderCode { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>		
        public int MemberID { get; set; }
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

    }

    /// <summary>
    /// 财务主订单(Admin)
    /// </summary>
    public class FinanceOrder
    {
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime PayTime { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 用户类型
        /// </summary>
        public string UserType { get; set; }
        /// <summary>
        /// 代理机构名称
        /// </summary>
        public string MemberName { get; set; }

        public string MemberID { get; set; }
        /// <summary>
        /// 代理机构ID
        /// </summary>
        public string EnterpriseName { get; set; }

        public string BusinessName { get; set; }
        /// <summary>
        /// 用户支付数
        /// </summary>
        public int payUserCount { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public string PaymentMethod { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal Amounts { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public string Status { get; set; }
    }

    /// <summary>
    /// 财务从订单
    /// </summary>
    public class FinanceSubOrder
    {
        public string OrderCode { get; set; }
        public string Status { get; set; }
        public string SocialSecurityPeopleName { get; set; }
        public string HouseholdProperty { get; set; }
        public DateTime? ssStartTime { get; set; }
        public DateTime? ssEndTime { get; set; }
        public string SocialSecurityBase { get; set; }
        public decimal ssAmount { get; set; }
        public decimal SocialSecurityServiceCost { get; set; }
        public decimal SocialSecurityFirstBacklogCost { get; set; }
        public decimal SocialSecurityBuCha { get; set; }
        public DateTime? afStartTime { get; set; }
        public DateTime? afEndTime { get; set; }
        public decimal afAmount { get; set; }
        public decimal AccumulationFundServiceCost { get; set; }
        public decimal AccumulationFundFirstBacklogCost { get; set; }
        public decimal totalAmount { get; set; }
    }

    /// <summary>
    /// 财务订单参数
    /// </summary>
    public class FinanceOrderParameter : PagedParameter
    {

        public string MemberID { get; set; }

        public string PaymentMethod { get; set; }

        public string OrderCode { get; set; }
        public string Status { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }



    }

    /// <summary>
    /// 生成订单参数
    /// </summary>
    public class GenerateOrderParameter
    {
        /// <summary>
        /// 参保人ID数组
        /// </summary>
        public int[] SocialSecurityPeopleIDS { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public int MemberID { get; set; }
    }


    /// <summary>
    /// 移动端订单列表
    /// </summary>
    public class OrderListForMobile
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 人员集合
        /// </summary>
        public string Names { get; set; }
        /// <summary>
        /// 总额
        /// </summary>
        public string Amounts { get; set; }
    }

    /// <summary>
    /// 移动端订单详情
    /// </summary>
    public class OrderDetailForMobile
    {
        public int OrderID { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 人员集合
        /// </summary>
        public string Names { get; set; }
        /// <summary>
        /// 社保总金额
        /// </summary>
        public decimal SocialSecurityTotalAmount { get; set; }
        /// <summary>
        /// 公积金总金额
        /// </summary>
        public decimal AccumulationFundTotalAmount { get; set; }
        /// <summary>
        /// 总服务费
        /// </summary>
        public decimal ServiceCost { get; set; }
        /// <summary>
        /// 第一次代办费
        /// </summary>
        public decimal FirstBacklogCost { get; set; }
        /// <summary>
        /// 社保补差
        /// </summary>
        public decimal SocialSecurityBuCha { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public string PaymentMethod { get; set; }

    }

    /// <summary>
    /// 订单编号数组类
    /// </summary>
    public class OrderCodeArrayParameter
    {
        /// <summary>
        /// 订单编号数组
        /// </summary>
        public string[] OrderCode { get; set; }
    }



    /// <summary>
    /// 缴费列表参数
    /// </summary>
    public class PayParameters : PagedParameter
    {
        /// <summary>
        /// 用户类型
        /// </summary>
        public string UserType { get; set; }
        /// <summary>
        /// 注册用户ID（代理机构）
        /// </summary>
        public string MemberID { get; set; }

    }

    public class OrderPayParameter
    {
        public int OrderID { get; set; }
        /// <summary>
        /// 平台类型
        /// </summary>
        public string PlatType { get; set; } = "1";
    }


}
