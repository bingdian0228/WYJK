using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;

namespace WYJK.Web.Controllers.Mvc
{
    public class RechargeOrdersController : Controller
    {
        private readonly IRechargeOrdersService _rechargeOrdersService = new RechargeOrdersService();
        /// <summary>
        /// 获取充值订单列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> GetRechargeOrderList(RechargeOrdersParameter parameter)
        {
            PagedResult<RechargeOrders> list = await _rechargeOrdersService.GetRechargeOrderList(parameter);
            List<SelectListItem> ApplyStatus = new List<SelectListItem>();
            ApplyStatus.Insert(0, new SelectListItem { Text = "全部", Value = "-1" });
            ApplyStatus.Insert(1, new SelectListItem { Text = "未审核", Value = "0" });
            ApplyStatus.Insert(2, new SelectListItem { Text = "已审核", Value = "1" });
            ApplyStatus.Insert(2, new SelectListItem { Text = "审核未通过", Value = "2" });
            ViewData["Status"] = ApplyStatus;

            return View(list);
        }

        /// <summary>
        /// 通过
        /// </summary>
        /// <returns></returns>
        public ActionResult Agree(int[] OrderIds)
        {
            DbHelper.ExecuteSqlCommand($"update RechargeOrders set Status=1,AuditTime=getdate() where OrderID in({string.Join(",", OrderIds)})", null);

            string memberName = DbHelper.QuerySingle<string>($"select MemberName from Members where MemberID =(select MemberID from RechargeOrders where OrderID={OrderIds[0]})");

            LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("充值审核通过，注册客户:{0}", memberName) });

            return Json(new { status = true, Message = "操作成功" });

        }


        /// <summary>
        /// 通过
        /// </summary>
        /// <returns></returns>
        public ActionResult NoAgree(int[] OrderIds)
        {
            DbHelper.ExecuteSqlCommand($"update RechargeOrders set Status=2,AuditTime=getdate() where OrderID in({string.Join(",", OrderIds)})", null);

            string memberName = DbHelper.QuerySingle<string>($"select MemberName from Members where MemberID =(select MemberID from RechargeOrders where OrderID={OrderIds[0]})");

            LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("充值审核未通过，注册客户:{0}", memberName) });

            return Json(new { status = true, Message = "操作成功" });

        }
    }
}