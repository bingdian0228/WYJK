using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Entity;
using WYJK.HOME.Service;

namespace WYJK.HOME.Controllers
{
    public class IndexController : BaseController
    {
        InfomationService infoSv = new InfomationService();


        // GET: Index
        public ActionResult Index()
        {
            //首页社保资讯
            ViewBag.insuranceInfos = infoSv.InsuranceOfPageSize(4);

            return View(); 
        }

        /// <summary>
        /// 社保公积金
        /// </summary>
        /// <returns></returns>
        public ActionResult Insurance()
        {
            return View();
        }

        /// <summary>
        /// 贷款
        /// </summary>
        /// <returns></returns>
        public ActionResult Loan()
        {
            return View();
        }

        /// <summary>
        /// 关于
        /// </summary>
        /// <returns></returns>
        public ActionResult About()
        {
            return View();
        }

    }
}