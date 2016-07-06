using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WYJK.Entity;

namespace WYJK.HOME.Models
{
    public class MemberLoanAuditViewModel
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        public int MemberID { get; set; }
        /// <summary>
        /// 本次申请额度
        /// </summary>
        [Required]
        public decimal ApplyAmount { get; set; }
        /// <summary>
        /// 还款期限
        /// </summary>
        [Required]
        public string LoanTerm { get; set; }
        /// <summary>
        /// 还款方式
        /// </summary>
        [Required]
        public string LoanMethod { get; set; }

        public AppayLoan AppayLoan { get; set; }

    }
}