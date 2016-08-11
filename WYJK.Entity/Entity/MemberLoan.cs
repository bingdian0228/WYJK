using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 用户借款
    /// </summary>
    public class MemberLoan
    {
        /// <summary>
        /// ID
        /// </summary>		
        public int ID { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>		
        public int MemberID { get; set; }
        /// <summary>
        /// 总额度
        /// </summary>		
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// 已用额度
        /// </summary>		
        public decimal AlreadyUsedAmount { get; set; }
        /// <summary>
        /// 可用额度
        /// </summary>		
        public decimal AvailableAmount { get; set; }

    }

    /// <summary>
    /// 用户借款列表
    /// </summary>
    public class MemberLoanList : MemberLoan
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string MemberName { get; set; }

        public string UserType { get; set; }

        public string EnterpriseName { get; set; }

        public string BusinessName { get; set; }

        /// <summary>
        /// 用户电话
        /// </summary>
        public string MemberPhone { get; set; }
    }

    /// <summary>
    /// 用户借款参数
    /// </summary>
    public class MemberLoanParameter : PagedParameter
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string MemberName { get; set; }
    }

    /// <summary>
    /// 申请借款
    /// </summary>
    public class AppayLoan
    {
        /// <summary>
        /// 总额度
        /// </summary>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// 已借额度
        /// </summary>
        public decimal AlreadyUsedAmount { get; set; }
        /// <summary>
        /// 可借额度
        /// </summary>
        public decimal AvailableAmount { get; set; }

        /// <summary>
        /// 冻结额度
        /// </summary>
        public decimal FreezingAmount { get; set; }
    }
}
