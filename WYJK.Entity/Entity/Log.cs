using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 日志记录类
    /// </summary>
    public class Log
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public int? MemberID { get; set; }
        public string UserType { get; set; }
        public string MemberName { get; set; }
        public string EnterpriseName { get; set; }
        public string BusinessName { get; set; }
        public int? SocialSecurityPeopleID { get; set; }

        public string SocialSecurityPeopleName { get; set; }
        public string Contents { get; set; }
        public DateTime Dt { get; set; }
    }
}
