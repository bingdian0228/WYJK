using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;
using WYJK.HOME.Models;
using WYJK.HOME.Service;

namespace WYJK.HOME.Controllers
{
    public class UserOrderController : BaseFilterController
    {
        UserOrderService userOderSv = new UserOrderService();

        Service.SocialSecurityService sss = new Service.SocialSecurityService();

        private readonly IOrderService _orderService = new OrderService();
        private readonly ISocialSecurityService _socialSecurityService = new Data.ServiceImpl.SocialSecurityService();


        // GET: UserOrder
        public ActionResult Index(PagedParameter parameter,int? id=0)
        {

            List<UserOrderViewModel> list = userOderSv.GetOrderList(CommonHelper.CurrentUser.MemberID,id);

            var c = list.Skip(parameter.SkipCount - 1).Take(parameter.PageSize);


            UserOrderPageResult<UserOrderViewModel> page = new UserOrderPageResult<UserOrderViewModel>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = list.Count,
                Items = c,
                Status = id
            };

            return View(page);
        }

        private HttpClient client = new HttpClient();
        private string url = ConfigurationManager.AppSettings["ServerUrl"] + "/api";
        private JsonMediaTypeFormatter formatter = System.Web.Http.GlobalConfiguration.Configuration.Formatters.Where(f =>
        {
            return f.SupportedMediaTypes.Any(v => v.MediaType.Equals("application/json", StringComparison.CurrentCultureIgnoreCase));
        }).FirstOrDefault() as JsonMediaTypeFormatter;

        /// <summary>
        /// 订单详情
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public async Task<ActionResult> OrderDetail(string id)
        {
            //订单详情总计
            OrderDetailForMobile detail = _orderService.GetOrderDetail(CommonHelper.CurrentUser.MemberID, id);
       
            
            List<OrderDetaisViewModel> list = userOderSv.GetOrderDetails(id);

            foreach (var item in list)
            {
                var req1 = await client.GetAsync(url + "/SocialSecurity/GetUnInsuredPeopleList?memberid=" + CommonHelper.CurrentUser.MemberID);
                List<UnInsuredPeople> list2 = (await req1.Content.ReadAsAsync<JsonResult<List<UnInsuredPeople>>>()).Data;

                UnInsuredPeople entity = list2.Where(a => a.SocialSecurityPeopleID == item.SocialSecurityPeopleID).FirstOrDefault();

                SocialSecurityPeopleViewModel ssp = sss.SocialSecurityDetail(item.SocialSecurityPeopleID);               
                item.Bucha = entity.Bucha;
                detail.Bucha = entity.Bucha;
                item.ServiceCost = entity.socialSecurityFirstBacklogCost + entity.AccumulationFundFirstBacklogCost;
                item.AllTotal = entity.SocialSecurityAmount + entity.AccumulationFundAmount + entity.Bucha + entity.socialSecurityFirstBacklogCost + entity.AccumulationFundFirstBacklogCost;
                detail.AllTotal = item.AllTotal;
                detail.ServiceCost = item.ServiceCost;
            }
           
            ViewBag.Detail = detail;


            return View(list);
        }

     
        /// <summary>
        /// 订单确认
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Create(int? id)
        {
            
            var req = await client.GetAsync(url + "/SocialSecurity/GetUnInsuredPeopleList?memberid="+ CommonHelper.CurrentUser.MemberID);
            List<UnInsuredPeople> list = (await req.Content.ReadAsAsync<JsonResult<List<UnInsuredPeople>>>()).Data;

            UnInsuredPeople entity = list.Where(a => a.SocialSecurityPeopleID == id).FirstOrDefault();


            SocialSecurityPeopleViewModel ssp = sss.SocialSecurityDetail(id.Value);
            //保费计算
            ssp.Calculation = _socialSecurityService.GetSocialSecurityCalculationResult(ssp.InsuranceArea, ssp.HouseholdProperty, ssp.SocialSecurityBase, ssp.AccumulationFundBase);
            ssp.Bucha = entity.Bucha;
            ssp.ServiceCost = entity.socialSecurityFirstBacklogCost+entity.AccumulationFundFirstBacklogCost;
            ssp.Total = entity.SocialSecurityAmount + entity.AccumulationFundAmount + entity.Bucha + entity.socialSecurityFirstBacklogCost + entity.AccumulationFundFirstBacklogCost;
            return View(ssp);
        }

        /// <summary>
        /// 生成订单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Create(SocialSecurityPeopleViewModel model)
        {
            int[] SocialPeople = new int[1] { model.SocialSecurityPeopleID };
            
            var req = await client.PostAsync(url + "/Order/GenerateOrder",new { SocialSecurityPeopleIDS = SocialPeople, MemberID = CommonHelper.CurrentUser.MemberID,PlatType=1, IsUseBalance = false },formatter);
            var result = await req.Content.ReadAsAsync<JsonResult<object>>();

            var req1 = await client.GetAsync(url + "/SocialSecurity/GetUnInsuredPeopleList?memberid=" + CommonHelper.CurrentUser.MemberID);
            List<UnInsuredPeople> list = (await req1.Content.ReadAsAsync<JsonResult<List<UnInsuredPeople>>>()).Data;

            UnInsuredPeople entity = list.Where(a => a.SocialSecurityPeopleID == model.SocialSecurityPeopleID).FirstOrDefault();

            SocialSecurityPeopleViewModel ssp = sss.SocialSecurityDetail(model.SocialSecurityPeopleID);
            model.Calculation = _socialSecurityService.GetSocialSecurityCalculationResult(ssp.InsuranceArea, ssp.HouseholdProperty, ssp.SocialSecurityBase, ssp.AccumulationFundBase);
            model.Bucha = entity.Bucha;
            model.ServiceCost = entity.socialSecurityFirstBacklogCost + entity.AccumulationFundFirstBacklogCost;
            model.Total = entity.SocialSecurityAmount + entity.AccumulationFundAmount + entity.Bucha + entity.socialSecurityFirstBacklogCost + entity.AccumulationFundFirstBacklogCost;


            if (result.status)
                return Redirect("/UserOrder/Pay/" + result.Data.ToString());
            else
            {
                ViewBag.ErrorMessage = result.Message;
                return View(model);
            }
            

            //string errorMsg = "";
            //string orderCode = "";           

            //Dictionary<bool,string> result = userOderSv.CreateOrder(model, out errorMsg, out orderCode);

            //if (!string.IsNullOrEmpty(errorMsg))//是否允许创建订单
            //{
            //    return Redirect("/UserOrder/Create/"+model.SocialSecurityPeopleID);
            //}
            //if (!result.First().Key)//生成订单失败
            //{
            //    return Redirect("/UserOrder/Create/" + model.SocialSecurityPeopleID);
            //}

            //return Redirect("/UserOrder/Pay/"+orderCode);
        }


        public ActionResult Pay(string id)
        {
            OrderDetaisViewModel detail = userOderSv.GetOrderDetails(id)[0];
            return View(detail);
        }

        /// <summary>
        /// 支付
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Pay(OrderDetaisViewModel detail)
        {
            //主订单
            OrderViewModel order = userOderSv.GetOrderByOrderCode(detail.OrderCode);
            //order.OrderID = order.OrderID.PadLeft(10, '0');


            var getPayment = await client.PostAsync(url + "/Order/OrderPayment1", new { OrderID = order.OrderID, platType = 2 }, formatter);
            var resultPay = await getPayment.Content.ReadAsAsync<JsonResult<ChargeUrl>>();
            ChargeUrl result = resultPay.Data;

            ////订单详情
            //detail = userOderSv.GetOrderDetails(detail.OrderCode)[0];
            //decimal totalMoney = detail.SocialSecurityAmount + detail.SocialSecurityServiceCost + detail.SocialSecurityFirstBacklogCost + detail.SocialSecurityFirstBacklogCost + detail.AccumulationFundServiceCost + detail.AccumulationFundFirstBacklogCost;

            //string url = $@"https://netpay.cmbchina.com/netpayment/BaseHttp.dll?PrePayC1?BranchID={PayHelper.BranchID}&CoNo={PayHelper.CoNo}&BillNo={order.OrderID}&Amount={totalMoney}&Date={DateTime.Now.ToString("yyyyMMdd")}&MerchantUrl={ConfigurationManager.AppSettings["ServerUrl"] + "/api/Order/OrderPayment1_Return"}";
            return Redirect(result.URL);
        }

        /// <summary>
        /// 支付回调
        /// </summary>
        /// <param name="Succeed"></param>
        /// <param name="CoNo"></param>
        /// <param name="BillNo"></param>
        /// <param name="Amount"></param>
        /// <param name="Date"></param>
        /// <param name="Msg"></param>
        /// <returns></returns>
        public ActionResult NoticeResult(string Succeed,string CoNo,string BillNo,decimal Amount,string Date,string Msg)
        {

            CMBCHINALib.FirmClient client = new CMBCHINALib.FirmClient();
            //获取key路径
            string keyPath = Server.MapPath("/Libs/public.key");
            //获取返回的参数
            string query = Request.Url.Query.Substring(1);
            //检验收到通知内容的真实性（检验数字签名）
            short fromBank = client.exCheckInfoFromBank(keyPath, query);

            string branchID = Msg.Substring(0, 4);
            string cono = Msg.Substring(4, 6);

            if (branchID == PayHelper.BranchID && cono == PayHelper.CoNo)//返回的商户号与分支号与原先的匹配
            {
                //主订单
                OrderViewModel order = userOderSv.GetOrderByOrdeID(BillNo.TrimStart(new char[] { '0' }));

                userOderSv.Payed(new Order { MemberID=CommonHelper.CurrentUser.MemberID, OrderCode = order.OrderCode });
            }

            //if (fromBank == 0)//来自银行
            //{
            //    //修改数据
            //    //userOderSv.Payed(new Order { MemberID=CommonHelper.CurrentUser.MemberID, OrderCode });
            //}
            //else
            //{
            //    string errorMsg = client.exGetLastErr(fromBank);
            //}

            return Redirect("/UserOrder/Index");
        }

    }
}