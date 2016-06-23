using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Entity;

namespace WYJK.Web.Controllers.Mvc
{
    /// <summary>
    /// 服务协议
    /// </summary>
    public class ServiceProtocolController : Controller
    {
        // GET: ServiceProtocol
        public ActionResult GetServiceProtocol()
        {
            ServiceProtocol serviceProtocol = DbHelper.QuerySingle<ServiceProtocol>("select ServiceProtocolContent from ServiceProtocol");
            return View(serviceProtocol);
        }

        [ValidateInput(false)]
        public ActionResult PostServiceProtocol(ServiceProtocol erviceProtocol)
        {
            DbHelper.ExecuteSqlCommand($"delete from ServiceProtocol;insert into ServiceProtocol values('{erviceProtocol.ServiceProtocolContent}')", null);

            return RedirectToAction("GetServiceProtocol");
        }
    }
}