using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Entity;
using WYJK.Web.Models;

namespace WYJK.Web.Controllers.Mvc
{
    /// <summary>
    /// 基础信息控制器
    /// </summary>
    [Authorize]
    public class OptionViewController : Controller
    {
        #region 省市区联动
        /// <summary>
        /// 省市区联动
        /// </summary>
        /// <param name="pname"></param>
        /// <param name="cname"></param>
        /// <param name="coname"></param>
        /// <returns></returns>
        public PartialViewResult RegionView(string pname, string cname, string coname, bool? IsHideCounty,string id, string callback)
        {
            ViewBag.IsHideCounty = IsHideCounty ?? false;
            ViewBag.Id = id;
            ViewBag.CallBack = callback;

            //根据省市区名称获取对应编号
            string pcode = string.Empty;
            string ccode = string.Empty;
            string cocode = string.Empty;

            Region region = DbHelper.QuerySingle<Region>($"select * from Region where RegionName='{pname}' ");
            if (region != null) pcode = region.RegionCode;

            Region region1 = DbHelper.QuerySingle<Region>($"select * from Region where RegionName='{cname}' ");
            if (region1 != null) ccode = region1.RegionCode;

            Region region2 = DbHelper.QuerySingle<Region>($"select * from Region where RegionName='{coname}' ");
            if (region2 != null) cocode = region2.RegionCode;

            RegionViewModel viewModel = new RegionViewModel();
            const string sql = "SELECT * FROM dbo.Region WHERE ParentCode = @ParentCode";

            List<Region> province = DbHelper.Query<Region>(sql, new { ParentCode = "000000" });
            if (province != null && province.Count > 0)
            {
                viewModel.ProvinceList.AddRange(province.Select(item => new SelectListItem
                {
                    Text = item.RegionName,
                    Value = item.RegionCode,
                    Selected = item.RegionCode == pcode
                }));
            }

            if (string.IsNullOrWhiteSpace(pcode) == false)
            {
                List<Region> cityList = DbHelper.Query<Region>(sql, new { ParentCode = pcode });
                if (cityList != null && cityList.Count > 0 && cityList.All(item => item.ParentCode == pcode))
                {
                    viewModel.CityList.AddRange(cityList.Select(item => new SelectListItem
                    {
                        Text = item.RegionName,
                        Value = item.RegionCode,
                        Selected = item.RegionCode == ccode
                    }));

                    if (string.IsNullOrWhiteSpace(ccode) == false)
                    {
                        List<Region> countyList = DbHelper.Query<Region>(sql, new { ParentCode = ccode });
                        if (countyList != null && countyList.Count > 0 &&
                            countyList.All(item => item.ParentCode == ccode))
                        {
                            viewModel.CountyList.AddRange(countyList.Select(item => new SelectListItem
                            {
                                Text = item.RegionName,
                                Value = item.RegionCode,
                                Selected = item.RegionCode == cocode
                            }));
                        }

                    }
                }
            }
            return PartialView(viewModel);
        }
        #endregion



    }
}