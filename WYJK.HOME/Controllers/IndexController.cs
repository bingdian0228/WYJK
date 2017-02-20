using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
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
            //首页最新新闻
            ViewBag.newsInfos = infoSv.InsuranceOfPageSize(4, "1");
            //首页最新咨询
            ViewBag.zixunInfos = infoSv.InsuranceOfPageSize(4, "2");

            return View();
        }

        [HttpGet]
        public ActionResult IntroBuy()
        {
            return View();
        }
        [HttpGet]
        public ActionResult IntroShebao()
        {
            return View();
        }
        [HttpGet]
        public ActionResult IntroWuyou()
        {
            return View();
        }

        /// <summary>
        /// 新闻资讯详情
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult InfoDetail(int ID)
        {
            Information info = DbHelper.QuerySingle<Information>("select * from Information where ID=" + ID);
            return View(info);
        }

        /// <summary>
        /// 新闻资讯列表
        /// </summary>
        /// <returns></returns>
        public ActionResult InfoList(string Type)
        {
            List<Information> infoList = DbHelper.Query<Information>("select * from Information where Type='" + Type + "' order by CreateTime desc");
            return View(infoList);
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

        public ActionResult InfoDetailDescrip1()
        {
            return View();
        }

        public ActionResult InfoDetailDescrip2()
        {
            return View();
        }
        public ActionResult InfoDetailDescrip3()
        {
            return View();
        }
        public ActionResult InfoDetailDescrip4()
        {
            return View();
        }
        public ActionResult InfoDetailDescrip5()
        {
            return View();
        }
        public ActionResult InfoDetailDescrip6()
        {
            return View();
        }
        public ActionResult InfoDetailDescrip7()
        {
            return View();
        }
        public ActionResult InfoDetailDescrip8()
        {
            return View();
        }
    }
}