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
}
