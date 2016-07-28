using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Framework.EnumHelper;

namespace WYJK.Entity
{
    /// <summary>
    /// 借款审核表
    /// </summary>
    public class MemberLoanAudit
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
        /// 本次申请额度
        /// </summary>		
        public decimal ApplyAmount { get; set; }
        /// <summary>
        /// 还款期限
        /// </summary>		
        public string LoanTerm { get; set; }
        /// <summary>
        /// 还款方式
        /// </summary>		
        public string LoanMethod { get; set; }
        /// <summary>
        /// 状态
        /// </summary>		
        public string Status { get; set; }
        /// <summary>
        /// 申请日期
        /// </summary>		
        public DateTime ApplyDate { get; set; }
        /// <summary>
        /// 审核日期
        /// </summary>		
        public DateTime AuditDate { get; set; }
        /// <summary>
        /// 放款时间
        /// </summary>
        public DateTime AlreadyLoanDate { get; set; }

        /// <summary>
        /// 还款状态
        /// </summary>
        public string RepaymentStatus { get; set; }

        /// <summary>
        /// 借款余额
        /// </summary>
        public decimal LoanBalance { get; set; }
    }

    /// <summary>
    /// 借款参数
    /// </summary>
    public class MemberLoanAuditParameter
    {
        /// <summary>
        /// 用户ID
        /// </summary>		
        public int MemberID { get; set; }
        /// <summary>
        /// 本次申请额度
        /// </summary>		
        public decimal ApplyAmount { get; set; }
        /// <summary>
        /// 还款期限
        /// </summary>		
        public string LoanTerm { get; set; }
        /// <summary>
        /// 还款方式
        /// </summary>		
        public string LoanMethod { get; set; }
    }

    /// <summary>
    /// 用户借款审核列表
    /// </summary>
    public class MemberLoanAuditList : MemberLoanList
    {
        /// <summary>
        /// 本次申请额度
        /// </summary>		
        public decimal ApplyAmount { get; set; }
        /// <summary>
        /// 状态
        /// </summary>		
        public string Status { get; set; }
        /// <summary>
        /// 申请日期
        /// </summary>		
        public DateTime? ApplyDate { get; set; }
        /// <summary>
        /// 审核日期
        /// </summary>		
        public DateTime? AuditDate { get; set; }
        /// <summary>
        /// 放款时间
        /// </summary>
        public DateTime? AlreadyLoanDate { get; set; }
    }

    /// <summary>
    /// 用户借款审核参数
    /// </summary>
    public class MemberLoanAuditListParameter : PagedParameter
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string MemberName { get; set; }
        /// <summary>
        /// 状态
        /// </summary>		
        public string Status { get; set; }
    }
}
