using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WYJK.Web.Controllers.Mvc
{
    public class BaseOrdersController : Controller
    {
        /// <summary>
        /// 获取调基订单列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetBaseOrderList()
        {

            return View();
        }
    }
}