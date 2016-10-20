using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.Captcha;
using WYJK.Framework.EnumHelper;
using WYJK.HOME.Models;
using WYJK.HOME.Service;

namespace WYJK.HOME.Controllers
{

    public class UserLoanController : BaseFilterController
    {
        private readonly IMemberService _memberService = new MemberService();

        UserLoanService loanSv = new UserLoanService();

        private string url = ConfigurationManager.AppSettings["ServerUrl"] + "/api";

        public async Task<ActionResult> Index(LoanQueryParamsModel parameter)
        {
            HttpClient client = new HttpClient();
            var req = await client.GetAsync(url + "/Loan/GetMemberLoanAuditList?memberID=" + CommonHelper.CurrentUser.MemberID + "&PageIndex=" + parameter.PageIndex + "&PageSize=" + parameter.PageSize);

            var reqResult = (await req.Content.ReadAsAsync<JsonResult<PagedResult<MemberLoanAudit>>>()).Data;

            return View(reqResult);
        }


        /// <summary>
        /// 获取申请进度
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ApplayProgress(PagedParameter parameter)
        {
            HttpClient client = new HttpClient();

            var req = await client.GetAsync(url + "/Loan/GetLoanApplayProgress?memberID=" + CommonHelper.CurrentUser.MemberID + "&PageIndex=" + parameter.PageIndex + "&PageSize=" + parameter.PageSize);

            var reqResult = (await req.Content.ReadAsAsync<JsonResult<PagedResult<MemberLoanAudit>>>()).Data;

            return View(reqResult);
        }

        /// <summary>
        /// 根据借款ID获取还款列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetRepaymentList(int id)
        {
            HttpClient client = new HttpClient();

            var req = await client.GetAsync(url + "/Loan/GetRepaymentList?ID=" + id);

            var reqResult = (await req.Content.ReadAsAsync<JsonResult<List<MemberLoanRepayment>>>()).Data;

            return View(reqResult);
        }


        /// <summary>
        /// 还款详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetRepaymentDetail(int id)
        {
            HttpClient client = new HttpClient();

            var req = await client.GetAsync(url + "/Loan/GetRepaymentDetail?ID=" + id);

            var reqResult = (await req.Content.ReadAsAsync<JsonResult<MemberLoanRepayment>>()).Data;

            return View(reqResult);
        }

        /// <summary>
        /// 贷款详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Detail(int? id)
        {
            if (id != null)
            {
                return View(loanSv.GetLoadAuditDetail(id.Value));
            }

            return Redirect("/Index/Index");
        }

        /// <summary>
        /// 新建我要还款
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> MemberLoanRepayment(int id)
        {
            HttpClient client = new HttpClient();
            var req = await client.GetAsync(url + "/Loan/MemberLoanRepayment?id=" + id);
            var reqResult = (await req.Content.ReadAsAsync<JsonResult<MemberLoanRepayment>>()).Data;


            return View(reqResult);
        }


        /// <summary>
        /// 选择还款类型
        /// </summary>
        /// <param name="id"></param>
        /// <param name="RepaymentType"></param>
        /// <returns></returns>
        public async Task<ActionResult> SelectRepaymentType(int id, string RepaymentType)
        {
            HttpClient client = new HttpClient();
            var req = await client.GetAsync(url + "/Loan/SelectRepaymentType?id=" + id + "&RepaymentType=" + RepaymentType);
            var reqResult = (await req.Content.ReadAsAsync<JsonResult<MemberLoanRepayment>>()).Data;

            return View("MemberLoanRepayment", reqResult);
        }

        /// <summary>
        /// 提交还款
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> MemberLoanRepayment(MemberLoanRepaymentOrderParameter parameter)
        {
            //HttpClient client = new HttpClient();
            //var req = await client.PostAsJsonAsync(url + "/Loan/MemberLoanRepayment", parameter);
            //JsonResult<dynamic> reqResult = await req.Content.ReadAsAsync<JsonResult<dynamic>>();
            //return RedirectToAction("GetRepaymentDetail", new { id = reqResult.Data });
            //调起招行支付接口
            parameter.PlatType = "2";

            HttpClient client = new HttpClient();
            var req = await client.PostAsJsonAsync(url + "/Loan/OrderPay", parameter);
            JsonResult<dynamic> reqResult = await req.Content.ReadAsAsync<JsonResult<dynamic>>();


            return Content(reqResult.Data);
        }


        /// <summary>
        /// 还款1
        /// </summary>
        /// <returns></returns>
        public ActionResult Payback1(int? id)
        {
            if (id != null)
            {
                return View(loanSv.GetLoadAuditDetail(id.Value));
            }
            return Redirect("/Index/Index");
        }

        /// <summary>
        /// 还款1
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Payback1(MemberLoanAudit audit)
        {
            return RedirectToAction("Payback2");
        }

        /// <summary>
        /// 还款二
        /// </summary>
        /// <returns></returns>
        public ActionResult Payback2()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Payback2(MemberLoanAudit audit)
        {
            return RedirectToAction("Payback3"); ;
        }

        /// <summary>
        /// 还款三
        /// </summary>
        /// <returns></returns>
        public ActionResult Payback3()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Payback3(MemberLoanAudit audit)
        {
            return RedirectToAction("Index");
        }

    }
}