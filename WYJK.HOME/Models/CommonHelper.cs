using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WYJK.Entity;
using System.Web.SessionState;
using System.Web.Mvc;
using System.IO;
using WYJK.Framework.EnumHelper;

namespace WYJK.HOME.Models
{
    public class CommonHelper
    {
        public static readonly string BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UploadFiles");
        public static readonly string[] ImageExt = { ".jpg", ".jpeg", ".png", ".gif" };


        /// <summary>
        /// 获取当前用户
        /// </summary>
        public static Members CurrentUser
        {
            get
            {
                if (HttpContext.Current.Session["UserInfo"] != null)
                {
                    return HttpContext.Current.Session["UserInfo"] as Members;
                }
                return null;


            }
        }

        public static List<SelectListItem> EntityListToSelctList(List<Region> list, string promt = "请选择")
        {
            List<SelectListItem> selList = new List<SelectListItem>();

            selList.Insert(0, new SelectListItem { Text = promt, Value = "0" });

            foreach (Region item in list)
            {
                selList.Add(new SelectListItem { Text = item.RegionName, Value = item.RegionCode });
            }

            return selList;

        }

        /// <summary>
        /// 根据枚举获取下拉列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="promt"></param>
        /// <returns></returns>
        public static List<SelectListItem> SelectListType(Type type, string promt = "")
        {

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(type);
            if (!string.IsNullOrEmpty(promt))
            {
                UserTypeList.Insert(0, new SelectListItem { Text = promt, Value = "" });
            }

            return UserTypeList;


        }

    }
}