using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WYJK.HOME.Models
{
    public class DrawCash
    {
        public int DrawCashId { get; set; }

        public int MemberId { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        [Required]
        [Range(1,100000000)]
        public decimal Money { get; set; }

        public decimal FrozenMoney { get; set; }

        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime ApplyTime { get; set; }

        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public int ApplyStatus { get; set; }

        public DateTime AgreeTime { get; set; }

        public string PaySn { get; set; }

        public decimal LeftAccount { get; set; }


    }
}