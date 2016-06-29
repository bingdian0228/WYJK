using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 账户记录
    /// </summary>
    public class AccountRecord
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
        /// 参保人ID
        /// </summary>		
        public int SocialSecurityPeopleID { get; set; }
        /// <summary>
        /// 参保人姓名
        /// </summary>		
        public string SocialSecurityPeopleName { get; set; }

        /// <summary>
        /// 收支类型
        /// </summary>		
        public string ShouZhiType { get; set; }
        /// <summary>
        /// 来源
        /// </summary>		
        public string LaiYuan { get; set; }
        /// <summary>
        /// 操作类型
        /// </summary>		
        public string OperationType { get; set; }
        /// <summary>
        /// 费用
        /// </summary>		
        public decimal Cost { get; set; }

        /// <summary>
        /// 显示费用
        /// </summary>
        public string CostDisplay { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>		
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 流水号
        /// </summary>
        public string SerialNum { get; set; }

        /// <summary>
        /// 社保公积金类型
        /// </summary>
        public string Type { get; set; }

    }
}
