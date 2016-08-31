using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WYJK.HOME.Models
{
    /// <summary>
    /// 省市区联动模型
    /// </summary>
    public class RegionViewModel
    {
        public RegionViewModel()
        {
            ProvinceList = new List<SelectListItem> { new SelectListItem {Text = "请选择省份",Value = ""} };
            CityList = new List<SelectListItem> { new SelectListItem { Text = "请选择城市", Value = "" } };
            CountyList = new List<SelectListItem> { new SelectListItem { Text = "请选择区县", Value = "" } };
        }

        /// <summary>
        /// 省
        /// </summary>
        [Required(ErrorMessage ="省份必填")]
        public string ProvinceCode { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        [Required(ErrorMessage = "城市必填")]
        public string CityCode { get; set; }
        /// <summary>
        /// 区
        /// </summary>
        [Required(ErrorMessage = "区县必填")]
        public string CountyCode { get; set; }

        /// <summary>
        /// 省份列表
        /// </summary>
        public List<SelectListItem> ProvinceList { set; get; }
        /// <summary>
        /// 城市列表
        /// </summary>
        public List<SelectListItem> CityList { set; get; }
        /// <summary>
        /// 区县列表
        /// </summary>
        public List<SelectListItem> CountyList { set; get; }
    }
}