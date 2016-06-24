using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;

namespace WYJK.Web.Controllers.Mvc
{
    /// <summary>
    /// 日志记录
    /// </summary>
    public class LoggerController : BaseController
    {
        ILogService _logService = new LogService();
        
        /// <summary>
        /// 获取日志列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public ActionResult GetLogList(PagedParameter parameter)
        {
            PagedResult<Log> logList = _logService.GetLogList(parameter);
            return View(logList);
        }
    }
}