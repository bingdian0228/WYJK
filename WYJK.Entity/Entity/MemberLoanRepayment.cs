using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Framework.EnumHelper;

namespace WYJK.Entity
{
    /// <summary>
    /// 还款
    /// </summary>
    public class MemberLoanRepayment
    {
        public int ID { get; set; }
        /// <summary>
        /// 还款总额
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 违约金
        /// </summary>
        public decimal WeiYueJin { get; set; }

        /// <summary>
        /// 还款时间
        /// </summary>
        public string RepaymentDt { get; set; }
        /// <summary>
        /// 还款类型 逾期还：1，正常还：2，提前还：3
        /// </summary>
        public string RepaymentType { get; set; }

        /// <summary>
        /// 借款方式
        /// </summary>
        public string LoanMethod { get; set; }

        /// <summary>
        /// 借款ID
        /// </summary>
        public int JieID { get; set; }

        /// <summary>
        /// 还款审核状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? AuditDt { get; set; }

        /// <summary>
        /// 还款类型
        /// </summary>
        public List<Property> RepaymentTypeList { get; set; } = new List<Property>();

        /// <summary>
        /// 还款详情列表
        /// </summary>
        public List<MemberLoanRepaymentDetail> DetailList { get; set; } = new List<MemberLoanRepaymentDetail>();

    }

    /// <summary>
    /// 我要还款展示
    /// </summary>
    public class MemberLoanRepaymentDetail
    {

        public int ID { get; set; }
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
        public string MonthStr { get; set; }

        public int HuanID { get; set; }

    }

    /// <summary>
    /// 还款
    /// </summary>
    public class MemberLoanRepaymentParameter
    {
        /// <summary>
        /// 借款ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 还款类型
        /// </summary>
        public string RepaymentType { get; set; }

    }

    /// <summary>
    /// 还款订单参数
    /// </summary>
    public class MemberLoanRepaymentOrderParameter
    {
        /// <summary>
        /// 借款ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 还款类型
        /// </summary>
        public string RepaymentType { get; set; }


        /// <summary>
        /// 平台类型
        /// </summary>
        public string PlatType { get; set; } = "1";
    }

    /// <summary>
    /// 还款订单
    /// </summary>
    public class MemberLoanRepaymentOrder
    {
        public int Id { get; set; }
        public int JieID { get; set; }
        public string RepaymentType { get; set; }

        public string Status { get; set; }

        public string HuanID { get; set; }
    }
}
