using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WYJK.Data.IService;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Web.Controllers.Mvc
{
    /// <summary>
    /// 基数审核
    /// </summary>
    public class BaseAuditController : BaseController
    {
        IBaseAuditService _baseAuditService = new BaseAuditService();
        private readonly IMemberService _memberService = new MemberService();
        /// <summary>
        /// 基数审核列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetBaseAuditList(BaseAuditParameter parameter)
        {
            PagedResult<BaseAuditList> list = _baseAuditService.GetBaseAuditList(parameter);

            List<Members> memberList = _memberService.GetMembersList();
            memberList.ForEach(item =>
            {
                item.MemberName = item.UserType == "0" ? item.MemberName : (item.UserType == "1" ? item.EnterpriseName : item.BusinessName);
            });
            ViewBag.memberList = memberList;

            List<SelectListItem> selectList = new List<SelectListItem> { new SelectListItem() { Value = "", Text = "全部" } };
            selectList.AddRange(EnumExt.GetSelectList(typeof(BaseAuditEnum)));
            ViewData["StatusType"] = selectList;

            return View(list);
        }

        /// <summary>
        /// 批量审核
        /// </summary>
        /// <param name="IDs"></param>
        /// <param name="Status"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public ActionResult BatchAudit(int[] IDs, string Status, int Type)
        {
            bool flag = _baseAuditService.BatchAudit(IDs, Status, Type);
            return Json(new { status = flag, message = flag == true ? "审核成功" : "审核失败" });
        }
    }
}