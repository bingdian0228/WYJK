using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.JiaoFei
{
    public class ReturnCode
    {
        public static string Success = "0000";//处理成功
        public static string Error = "0001";//处理失败
        public static string NoExist = "0002";//不存在用户缴费号
        public static string NoNeed = "0003";//不需要缴费
    }
}
