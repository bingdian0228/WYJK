using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WYJK.Entity;
using WYJK.HOME.Models;
using WYJK.HOME.Service;

namespace WYJK.HOME.Controllers
{
    public class UserApplyWithDrawController : BaseFilterController
    {
        UserAccountService accountSv = new UserAccountService();

        public ActionResult Index(UserWithDrawPageModel parameter)
        {
            #region 体现
            Models.DrawCash drawCash = accountSv.GetLastestDrawCash(CommonHelper.CurrentUser.MemberID);
            //可提金额
            ViewBag.LeftAccount = drawCash == null ? CommonHelper.CurrentUser.Account : drawCash.LeftAccount;
            #endregion

            #region 列表

            Models.DrawCash drawCashSearch = new Models.DrawCash
            {
                MemberId = CommonHelper.CurrentUser.MemberID,
                StartTime = parameter.StartTime,
                EndTime = parameter.EndTime
            };

            List<Models.DrawCash> list = accountSv.GetDrawCashRecord(drawCashSearch,null);

            var c = list.Skip(parameter.SkipCount - 1).Take(parameter.PageSize);

            UserWithDrawPageResult<Models.DrawCash> page = new UserWithDrawPageResult<Models.DrawCash>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = list.Count,
                Items = c,
                Parameter = parameter
            };
            #endregion

            return View(page);
        }

        [HttpPost]
        public ActionResult WithDraw(decimal leftAccount,decimal money)
        {
            Models.DrawCash drawCash = new Models.DrawCash();

            drawCash.MemberId = CommonHelper.CurrentUser.MemberID;
            drawCash.ApplyStatus = 0;//审核中
            drawCash.Money = money;//取现金额


            if (money > leftAccount)
            {
                assignMessage("提现金额不能大于可提现余额", false);
                return RedirectToAction("Index");
            }

            drawCash.LeftAccount = leftAccount - drawCash.Money;

            int num = accountSv.DrawCash(drawCash);

            if (num > 0)//提现申请成功
            {
                assignMessage("提现申请成功,请等待审核", true);
            }
            else
            {
                assignMessage("提现失败", false);
            }

            return RedirectToAction("Index");
        }
    }
}