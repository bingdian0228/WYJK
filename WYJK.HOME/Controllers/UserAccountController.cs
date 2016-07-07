using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.HOME.Models;
using WYJK.HOME.Service;

namespace WYJK.HOME.Controllers
{
    public class UserAccountController : BaseFilterController
    {
        UserAccountService accountSv = new UserAccountService();
        UserMemberService memberSv = new UserMemberService();

        private readonly IMemberService _memberService = new MemberService();

        // GET: UserAccount
        public async System.Threading.Tasks.Task<ActionResult> MyAccount(UserAccountPageModel pageModel)
        {
            Members members = memberSv.UserInfos(CommonHelper.CurrentUser.MemberID);
            ViewBag.Account = members.Account;

            PagedResult<AccountRecord> list = await _memberService.GetAccountRecordList(CommonHelper.CurrentUser.MemberID, "", pageModel.BeginTime, pageModel.EndTime, pageModel);
            list.Items.ToList().ForEach(n =>
            {
                if (n.ShouZhiType.Trim() == "收入")
                    n.CostDisplay = "+" + Convert.ToString(n.Cost);
                else if (n.ShouZhiType.Trim() == "支出")
                    n.CostDisplay = "-" + Convert.ToString(n.Cost);
            });

            return View(list);
        }

        public ActionResult Charge()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Charge(RechargeParameters parameter)
        {
            parameter.MemberID = CommonHelper.CurrentUser.MemberID;
            parameter.PayMethod = "银联";

            if (accountSv.SubmitRechargeAmount(parameter))
            {
                return RedirectToAction("MyAccount");
            }


            return View();
        }

        /// <summary>
        /// 提现申请
        /// </summary>
        /// <returns></returns>
        public ActionResult WithDraw()
        {
            Models.DrawCash drawCash = accountSv.GetLastestDrawCash(CommonHelper.CurrentUser.MemberID);
            if (drawCash == null)
            {
                drawCash = new Models.DrawCash();
                drawCash.FrozenMoney = 0;
                drawCash.LeftAccount = CommonHelper.CurrentUser.Account;
            }
            else
            {
                drawCash.FrozenMoney = CommonHelper.CurrentUser.Account - drawCash.LeftAccount;
                drawCash.Money = 0;
            }

            return View(drawCash);
        }

        [HttpPost]
        public ActionResult WithDraw(Models.DrawCash drawCash)
        {
            drawCash.MemberId = CommonHelper.CurrentUser.MemberID;
            
            drawCash.ApplyStatus = 0;//审核中

            if (drawCash.Money > drawCash.LeftAccount)
            {
                assignMessage("提现金额不能大于可提现余额", false);
                return RedirectToAction("WithDraw");
            }

            drawCash.LeftAccount = drawCash.LeftAccount - drawCash.Money;
            int num = accountSv.DrawCash(drawCash);

            if (num > 0)//提现申请成功
            {
                assignMessage("提现申请成功,请等待审核", true);
            }
            else
            {
                assignMessage("提现失败", false);
            }

            return RedirectToAction("WithDraw");
        }
    }
}