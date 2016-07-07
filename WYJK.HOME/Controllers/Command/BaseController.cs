using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WYJK.Data;
using WYJK.Entity;

namespace WYJK.HOME.Controllers
{
    public class BaseController : Controller
    {
        public String WeekString = "";
        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {
            DateTime now = DateTime.Now;
            int n = (int)now.DayOfWeek;
            string[] weekDays = { "星期天", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
            requestContext.HttpContext.Session["WeekString"] = weekDays[n];


            //string sql = "select * from Members where MemberID=@MemberID";
            //Members member = DbHelper.QuerySingle<Members>(sql, new { MemberID = 1 });

            //requestContext.HttpContext.Session["UserInfo"] = member;

            return base.BeginExecute(requestContext, callback, state);
        }

        protected void assignMessage(String msg,Boolean success)
        {
            TempData["Message"] = msg;
            TempData["MessageType"] = success;
        }

        public Int32 MemberId()
        {
            Members m = (Members)Session["UserInfo"];
            if (m==null)
            {
                Redirect("~/User/Login");
                return 0;
            }
            return m.MemberID;
        }

    }


    public static class Common
    {
        
        public static void CopyTo<T>(this object source, T target)
            where T : class, new()
        {
            if (source == null)
                return;

            if (target == null)
            {
                target = new T();
            }

            foreach (var property in target.GetType().GetProperties())
            {
                var propertyValue = source.GetType().GetProperty(property.Name).GetValue(source, null);
                if (propertyValue != null)
                {
                    if (propertyValue.GetType().IsClass)
                    {

                    }
                    target.GetType().InvokeMember(property.Name, BindingFlags.SetProperty, null, target, new object[] { propertyValue });
                }

            }

            foreach (var field in target.GetType().GetFields())
            {
                var fieldValue = source.GetType().GetField(field.Name).GetValue(source);
                if (fieldValue != null)
                {
                    target.GetType().InvokeMember(field.Name, BindingFlags.SetField, null, target, new object[] { fieldValue });
                }
            }
        }
    }
}