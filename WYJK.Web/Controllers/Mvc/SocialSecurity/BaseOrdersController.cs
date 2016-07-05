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
    public class BaseOrdersController : Controller
    {
        private readonly IBaseOrdersService _baseOrdersService = new BaseOrdersService();

        /// <summary>
        /// 获取调基订单列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> GetBaseOrderList(BaseOrdersParameter parameter)
        {
            PagedResult<BaseOrders> BaseOrdersList = await _baseOrdersService.GetBaseOrderList(parameter);
            List<SelectListItem> ApplyStatus = new List<SelectListItem>();
            ApplyStatus.Insert(0, new SelectListItem { Text = "全部", Value = "-1" });
            ApplyStatus.Insert(1, new SelectListItem { Text = "审核中", Value = "1" });
            ApplyStatus.Insert(2, new SelectListItem { Text = "已通过", Value = "2" });
            ApplyStatus.Insert(3, new SelectListItem { Text = "未通过", Value = "3" });
            ViewData["Status"] = ApplyStatus;

            return View(BaseOrdersList);
        }

        /// <summary>
        /// 通过
        /// </summary>
        /// <returns></returns>
        public ActionResult Agree(int[] OrderIds)
        {
            DbHelper.ExecuteSqlCommand($"update BaseOrders set Status=2,AuditTime=getdate() where OrderID in({string.Join(",", OrderIds)})", null);

            //调基
            BaseOrders baseOrders = DbHelper.QuerySingle<BaseOrders>($"select * from BaseOrders where OrderID ={OrderIds[0]}");
            if (baseOrders.IsPaySocialSecurity)
            {
                DbHelper.ExecuteSqlCommand($"update SocialSecurity set Status=2 ,IsAdjustingBase=1,SocialSecurityBase='{baseOrders.SSCurrentBase}',AdjustingBaseNote='{baseOrders.SSAdjustingBaseNote}' where SocialSecurityPeopleID={baseOrders.SocialSecurityPeopleID}", null);
            }
            if (baseOrders.IsPayAccumulationFund)
            {
                DbHelper.ExecuteSqlCommand($"update AccumulationFund set Status=2 ,IsAdjustingBase=1,AccumulationFundBase='{baseOrders.AFCurrentBase}',AdjustingBaseNote='{baseOrders.AFAdjustingBaseNote}' where SocialSecurityPeopleID={baseOrders.SocialSecurityPeopleID}", null);
            }

            string memberName = DbHelper.QuerySingle<string>($"select MemberName from Members where MemberID =(select MemberID from BaseOrders where OrderID={OrderIds[0]})");

            LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("调基审核通过，注册客户:{0}", memberName) });

            return Json(new { status = true, Message = "操作成功" });

        }


        /// <summary>
        /// 不通过
        /// </summary>
        /// <returns></returns>
        public ActionResult NoAgree(int[] OrderIds)
        {
            DbHelper.ExecuteSqlCommand($"update BaseOrders set Status=3,AuditTime=getdate() where OrderID in({string.Join(",", OrderIds)})", null);

            string memberName = DbHelper.QuerySingle<string>($"select MemberName from Members where MemberID =(select MemberID from BaseOrders where OrderID={OrderIds[0]})");

            LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("调基审核未通过，注册客户:{0}", memberName) });

            return Json(new { status = true, Message = "操作成功" });

        }
    }
}