using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 还款
    /// </summary>
    public class MemberLoanRepayment
    {
        public int ID { get; set; }
        public decimal BenJin { get; set; }
        public decimal LiXi { get; set; }
        public decimal ZhiNaJin { get; set; }
        public decimal WeiYueJin { get; set; }
        public int MonthCount { get; set; }
        public string MonthStr { get; set; }
        public string RepaymentDt { get; set; }
        /// <summary>
        /// 还款类型 正常还：1，提前还：2，逾期还：3
        /// </summary>
        public string RepaymentType { get; set; }
        /// <summary>
        /// 借款ID
        /// </summary>
        public int JieID { get; set; }

    }

    /// <summary>
    /// 我要还款展示
    /// </summary>
    public class Repayment
    {
        /// <summary>
        /// 还款方式
        /// </summary>
        public string LoanMethod { get; set; }

        /// <summary>
        /// 还款类型 正常还：1，提前还：2，逾期还：3
        /// </summary>
        public string RepaymentType { get; set; }

        /// <summary>
        /// 还款总额
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 违约金
        /// </summary>
        public decimal WeiYueJin { get; set; }

        /// <summary>
        /// 详情列表
        /// </summary>
        public List<RepaymentDetail> DetailList { get; set; }

    }

    /// <summary>
    /// 还款展示详情
    /// </summary>
    public class RepaymentDetail
    {

        /// <summary>
        /// 本金
        /// </summary>
        public decimal BenJin { get; set; }
        /// <summary>
        /// 利息
        /// </summary>
        public decimal LiXi { get; set; }
        /// <summary>
        /// 滞纳金
        /// </summary>
        public decimal ZhiNaJin { get; set; }

        /// <summary>
        /// 所还月份
        /// </summary>
        public string MonthDt { get; set; }
    }


}
