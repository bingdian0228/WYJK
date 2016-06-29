using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IService
{
    /// <summary>
    /// 日志记录接口
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// 获取日志记录列表
        /// </summary>
        /// <returns></returns>
        PagedResult<Log> GetLogList(LogParameter parameter);

        ///// <summary>
        ///// 写入日志
        ///// </summary>
        ///// <param name="log"></param>
        //void WriteLogInfo(Log log);

    }
}
