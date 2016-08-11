using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 借款参数设置
    /// </summary>
    public class MemberLoanSetting
    {
        public decimal InThreeMonthsLiLv { get; set; }
        public decimal HalfYearLiLv { get; set; }
        public decimal OneYearPeriodLiLv { get; set; }
        public decimal ZhiNaJinPercent { get; set; }
        public decimal WeiYueJinPercent { get; set; }
    }
}
