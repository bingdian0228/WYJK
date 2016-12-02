using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Data.IServices;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;
using WYJK.HOME.Models;
using WYJK.HOME.Service;

namespace WYJK.HOME.Controllers
{
    public class CalculatorController : BaseFilterController
    {
        RegionService regionSv = new RegionService();

        ISocialSecurityService _socialSecurity = new WYJK.Data.ServiceImpl.SocialSecurityService();
        private string url = ConfigurationManager.AppSettings["ServerUrl"] + "/api";
        // GET: Calculator
        public async Task<ActionResult> Calculator()
        {
            HttpClient httpClient = new HttpClient();
            var req = await httpClient.GetAsync(url + "/SocialSecurity/GetProvinceList");
            List<string> list = (await req.Content.ReadAsAsync<JsonResult<List<string>>>()).Data;
            List<SelectListItem> selList = new List<SelectListItem>();

            selList.Insert(0, new SelectListItem { Text = "请选择省份", Value = "" });

            foreach (var item in list)
            {
                selList.Add(new SelectListItem { Text = item, Value = item });
            }

            //获取省份
            ViewBag.Provinces = selList;
            ViewBag.Url = url;
            //获取户籍
            ViewBag.UserTypes = CommonHelper.SelectListType(typeof(HouseholdPropertyEnum), "请选择户籍性质");
            return View();
        }

        /// <summary>
        /// 根据省份获取城市
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetCityListByProvince(string provinceName)
        {
            HttpClient httpClient = new HttpClient();
            var req = await httpClient.GetAsync(url + "/SocialSecurity/GetCityListByProvince?provinceName=" + provinceName);
            List<string> list = (await req.Content.ReadAsAsync<JsonResult<List<string>>>()).Data;

            return Json(new { result = list }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult CalculatorResult(string InsuranceArea, string HouseholdProperty, decimal SocialSecurityBase, decimal AccountRecordBase)
        {
            SocialSecurityCalculation cal = _socialSecurity.GetSocialSecurityCalculationResult(InsuranceArea, HouseholdProperty, SocialSecurityBase, AccountRecordBase);

            Dictionary<string, decimal> dict = new Dictionary<string, decimal>();
            dict["SocialSecuritAmount"] = cal.SocialSecuritAmount;
            dict["AccumulationFundAmount"] = cal.AccumulationFundAmount;
            dict["TotalAmount"] = cal.TotalAmount;

            return Json(dict, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据省份获取城市
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CitysByProvince(int id)
        {
            return Json(regionSv.GetCitys(id), JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取社保公积金基数
        /// </summary>
        /// <param name="area"></param>
        /// <param name="householdProperty"></param>
        /// <returns></returns>
        public ActionResult GetSocialSecurityBase(string area, string householdProperty)
        {
            EnterpriseSocialSecurity model = _socialSecurity.GetDefaultEnterpriseSocialSecurityByArea(area, householdProperty);

            decimal minBase = 0;
            decimal maxBase = 0;
            decimal aFMinBase = 0;
            decimal aFMaxBase = 0;


            if (model != null)
            {
                minBase = Math.Round(model.SocialAvgSalary * (model.MinSocial / 100));
                maxBase = Math.Round(model.SocialAvgSalary * (model.MaxSocial / 100));
                aFMinBase = model.MinAccumulationFund;
                aFMaxBase = model.MaxAccumulationFund;
            }



            Dictionary<string, decimal> dict = new Dictionary<string, decimal>();
            dict["MinBase"] = minBase;
            dict["MaxBase"] = maxBase;
            dict["AFMinBase"] = aFMinBase;
            dict["AFMaxBase"] = aFMaxBase;


            return Json(dict, JsonRequestBehavior.AllowGet);
        }


    }
}