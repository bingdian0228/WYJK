using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
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
        private HttpClient client = new HttpClient();
        private string url = ConfigurationManager.AppSettings["ServerUrl"] + "/api";
        private JsonMediaTypeFormatter formatter = System.Web.Http.GlobalConfiguration.Configuration.Formatters.Where(f =>
        {
            return f.SupportedMediaTypes.Any(v => v.MediaType.Equals("application/json", StringComparison.CurrentCultureIgnoreCase));
        }).FirstOrDefault() as JsonMediaTypeFormatter;

        private readonly IMemberService _memberService = new MemberService();

        // GET: UserAccount
        public async System.Threading.Tasks.Task<ActionResult> MyAccount(UserAccountPageModel pageModel)
        {
            Members members = memberSv.UserInfos(CommonHelper.CurrentUser.MemberID);
            ViewBag.Account = members.Account;
            ViewBag.Bucha = members.Bucha;

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
        public async Task<ActionResult> Charge(RechargeParameters parameter)
        {
            parameter.MemberID = CommonHelper.CurrentUser.MemberID;
            parameter.PayMethod = "银联";
            var createOrder = await client.PostAsync(url + "/Member/SubmitRechargeAmountOrder", new { MemberID = CommonHelper.CurrentUser.MemberID, PayMethid = "银联", Amount = parameter.Amount }, formatter);
            var order = await createOrder.Content.ReadAsAsync<JsonResult<ChargeOrder>>();
            ChargeOrder entity = order.Data;
           // string payUrl = $@"https://netpay.cmbchina.com/netpayment/BaseHttp.dll?PrePayC1?BranchID={PayHelper.BranchID}&CoNo={PayHelper.CoNo}&BillNo={order.Data}&Amount={parameter.Amount}&Date={DateTime.Now.ToString("yyyyMMdd")}&MerchantUrl={"http://localhost:65292/UserOrder/NoticeResult"}";
            var getPayment = await client.PostAsync(url + "/Member/SubmitRechargeAmountPayment", new { OrderID = entity.OrderId, platType = 2 }, formatter);
            ChargeUrl result = (await getPayment.Content.ReadAsAsync<JsonResult<ChargeUrl>>()).Data;

            //if (accountSv.SubmitRechargeAmount(parameter))
            //{
            //    return RedirectToAction("MyAccount");
            //}

            // string payUrl = $@"https://netpay.cmbchina.com/netpayment/BaseHttp.dll?PrePayC1?BranchID={PayHelper.BranchID}&CoNo={PayHelper.CoNo}&BillNo={entity.OrderId}&Amount={parameter.Amount}&Date={DateTime.Now.ToString("yyyyMMdd")}&MerchantUrl={ConfigurationManager.AppSettings["ServerUrl"] + "api/Member/SubmitRechargeAmountPayment_Return"}";
            return Redirect(result.URL);
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

        public async Task<ActionResult> Renew()
        {
            //  / api / Member / GetRenewalServiceList ? MemberID =
            var selectService = await client.GetAsync(url + $"/Member/GetRenewalServiceList?MemberID={CommonHelper.CurrentUser.MemberID}");
            List<KeyValuePair<int, decimal>> result1 = (await selectService.Content.ReadAsAsync<JsonResult<List<KeyValuePair<int, decimal>>>>()).Data;
            ViewBag.selectService = result1;
            var personStatus = await client.GetAsync(url + $"/Member/GetAccountStatus?MemberID={CommonHelper.CurrentUser.MemberID}");
            var resultStatus = await personStatus.Content.ReadAsAsync<JsonResult<string>>();
            ViewBag.PerStatus = resultStatus.Data;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Renew(RenewalServiceParameters param)
        {
            param.Amount = Convert.ToDecimal(param.PayMethod.Split('_')[1].ToString());
            param.MonthCount = Convert.ToInt32(param.PayMethod.Split('_')[0].ToString());
            var order = await client.PostAsync(url + "/Member/SubmitRenewalServiceOrder", new { MemberID = CommonHelper.CurrentUser.MemberID, PayMethod = 1,Amount=param.Amount,MonthCount=param.MonthCount }, formatter);
            ChargeOrder result = (await order.Content.ReadAsAsync<JsonResult<ChargeOrder>>()).Data;           
            var getPayment = await client.PostAsync(url + "/Member/SubmitRenewalServiceOrderPayment", new { OrderID = result.OrderId, platType = 2 }, formatter);
            var resultUrl = await getPayment.Content.ReadAsAsync<JsonResult<ChargeUrl>>();
            ChargeUrl ent = resultUrl.Data;
            return Redirect(ent.URL);
        }
    }

    public class ChargeOrder
    {
       public int OrderId { get; set; }
    }

    public class ChargeUrl
    {
        public string URL { get; set; }
    }
   

}