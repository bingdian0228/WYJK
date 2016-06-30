using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 费用参数设置
    /// </summary>
    public class CostParameterSetting
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 基数服务费
        /// </summary>
        public decimal BaseServiceCharge { get; set; }

        /// <summary>
        /// 代办费
        /// </summary>

        public decimal BacklogCost { get; set; } = 0.00M;
        /// <summary>
        /// 冻结费
        /// </summary>
        public decimal FreezingAmount { get; set; } = 0.00M;
        /// <summary>
        /// 补办服务费
        /// </summary>
        public decimal PayBeforeServiceCost { get; set; } = 0.00M;
        /// <summary>
        /// 续费服务费
        /// </summary>
        public string RenewServiceCost { get; set; }

        ///// <summary>
        ///// 续费服务费
        ///// </summary>
        //public List<RenewServiceCost> RenewServiceCostList { get; set; }
        /// <summary>
        /// 起始时间
        /// </summary>
        public int[] StartTime { get; set; }
        /// <summary>
        /// 终止时间
        /// </summary>
        public int[] EndTime { get; set; }

        /// <summary>
        /// 服务费
        /// </summary>
        public decimal[] ServiceCost { get; set; }

        /// <summary>
        /// 缴费类型
        /// </summary>
        public int Status { get; set; }

    }

    /// <summary>
    /// 续费服务费
    /// </summary>
    public class RenewServiceCost {
        /// <summary>
        /// 起始时间
        /// </summary>
        public int StartTime { get; set; }
        /// <summary>
        /// 终止时间
        /// </summary>
        public int EndTime { get; set; }

        /// <summary>
        /// 服务费
        /// </summary>
        public decimal ServiceCost { get; set; }
    }


}
