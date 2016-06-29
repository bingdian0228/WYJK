using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;
using WYJK.Web.Models;

namespace WYJK.Web.Controllers.Mvc
{
    [Authorize]
    public class DrawCashController : BaseController
    {
        private readonly IDrawCashService _drawCashService = new DrawCashService();

        /// <summary>
        /// 提现申请列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> DrawCashList(DrawCashParameter parameter)
        {

            PagedResult<DrawCashViewModel> list = await _drawCashService.GetDrawCashListAsync(parameter);

            List<SelectListItem> ApplyStatus = new List<SelectListItem>();
            ApplyStatus.Insert(0, new SelectListItem { Text = "全部", Value = "-1" });
            ApplyStatus.Insert(1, new SelectListItem { Text = "未审核", Value = "0" });
            ApplyStatus.Insert(2, new SelectListItem { Text = "已审核", Value = "1" });
            ApplyStatus.Insert(2, new SelectListItem { Text = "审核未通过", Value = "2" });
            ViewData["ApplyStatus"] = new SelectList(ApplyStatus, "Value", "Text");

            return View(list);
        }

        /// <summary>
        /// 不通过提现审核
        /// </summary>
        /// <param name="DrawCashIds"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> NotAgree(int[] DrawCashIds)
        {
            string dcIds = string.Join(",", DrawCashIds);
            int restult = await DbHelper.ExecuteSqlCommandAsync($"UPDATE dbo.DrawCash SET ApplyStatus=2 WHERE DrawCashId IN ({ dcIds}) ");
            bool flag = false;
            if (restult > 0)
            {
                flag = true;
                List<AccountInfo> list = await DbHelper.QueryAsync<AccountInfo>($"select MemberName, Account,Bucha,HeadPortrait from Members where MemberID in(select memberid from DrawCash  WHERE DrawCashId IN ({dcIds})) ");
                string names = string.Empty;
                list.ForEach(l => {
                    names = names + l.MemberName + ',';
                });
                if (!string.IsNullOrEmpty(names))
                {
                    names = names.TrimEnd(',');
                }
                LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("拒绝提现申请业务，注册客户:{0}", names) });
            }
            return Json(new { status = flag, Message = flag ? "操作成功" : "操作失败" });
        }

       /// <summary>
       /// 同意提现
       /// </summary>
       /// <param name="DrawCashIds"></param>
       /// <returns></returns>
        [HttpGet]
        public ActionResult AgreeDrawCash(int[] DrawCashIds)
        {

            DrawCashModel model = new DrawCashModel()
            {
                DrawCashIds = string.Join(",", DrawCashIds)
            };

            string sql = $"select * from DrawCash where DrawCashId in ({model.DrawCashIds});";

            DrawCash entity = DbHelper.QuerySingle<DrawCash>(sql);
            ViewData["money"] = entity.Money.ToString("N2");
            return View(model);
        }

        /// <summary>
        /// 同意提现填写打款信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> AgreeDrawCash(DrawCashModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    string sql = $"update DrawCash set ApplyStatus=1,agreetime=getdate(),paysn='{model.OrderNo}' where DrawCashId in ({model.DrawCashIds});";
                    foreach (string item in model.DrawCashIds.Split(','))
                    {
                        sql += $"update Members set Account=Account-(select money from DrawCash where DrawCashid={item}) where memberid = (select memberid from DrawCash where DrawCashid={item});";
                    }

                    await Data.DbHelper.ExecuteSqlCommandAsync(sql);
                    List<AccountInfo> list = await DbHelper.QueryAsync<AccountInfo>($"select MemberName, Account,Bucha,HeadPortrait from Members where MemberID in(select memberid from DrawCash  WHERE DrawCashId IN ({model.DrawCashIds})) ");
                    string names = string.Empty;
                    list.ForEach(l => {
                        names = names + l.MemberName + ',';
                    });
                    if (!string.IsNullOrEmpty(names))
                    {
                        names = names.TrimEnd(',');
                    }
                    LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("提现申请成功客户:{0}", names) });
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = ex.Message;
                }
            }
            return View(model);
        }

    }
}
