using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 消息实体
    /// </summary>
    public class Message
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
        /// 消息内容
        /// </summary>		
        public string ContentStr { get; set; }
        /// <summary>
        /// 生成时间
        /// </summary>		
        public DateTime Dt { get; set; }
    }
}
