using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Web.Controllers.Mvc
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService orderService = new OrderService();
        private readonly IMemberService _memberService = new MemberService();

        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetOrderList(FinanceOrderParameter parameter)
        {
            PagedResult<FinanceOrder> orderList = orderService.GetFinanceOrderList(parameter);

            List<Members> memberList = _memberService.GetMembersList();
            memberList.ForEach(item =>
            {
                item.MemberName = item.UserType == "0" ? item.MemberName : (item.UserType == "1" ? item.EnterpriseName : item.BusinessName);
            });
            //费用来源
            
            ViewData["PaymentMethod"] = new SelectList(DbHelper.Query<Order>("select * from [Order]").Select(n => new { PaymentMethod = n.PaymentMethod }).Distinct().ToList(), "PaymentMethod", "PaymentMethod");


            ViewBag.TotalAmount = orderList.Sum(n => n.Amounts);
            ViewBag.memberList = memberList;
            return View(orderList);
        }

        /// <summary>
        /// 订单批量审核 若审核通过，订单变成已完成，并且参保人状态变成待办
        /// </summary>
        /// <param name="OrderCodes"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult BatchAuditing(string OrderCodeStr,string Amount, int Type)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    string orderCode = OrderCodeStr;
                    //修改订单状态
                    bool flag1 = orderService.ModifyOrderStatus(orderCode);
                    //修改订单对应的参保人的社保和公积金状态  SocialSecurityStatusEnum.WaitingHandle
                    //首先需要判断参保人是否已经通过客服审核

                    string sqlstr = $"select SocialSecurityPeopleID from OrderDetails where OrderCode ='{orderCode}'";
                    List<OrderDetails> orderDetailList = DbHelper.Query<OrderDetails>(sqlstr);
                    foreach (var orderDetail in orderDetailList)
                    {
                        string sqlStr1 = $"select * from SocialSecurityPeople where SocialSecurityPeopleID ={orderDetail.SocialSecurityPeopleID}";
                        SocialSecurityPeople socialSecurityPeople = DbHelper.QuerySingle<SocialSecurityPeople>(sqlStr1);
                        //是否通过客服审核，若通过，则修改社保和公积金状态
                        if (Convert.ToInt32(socialSecurityPeople.Status) == (int)CustomerServiceAuditEnum.Pass)
                        {
                            if (orderDetail.IsPaySocialSecurity)
                                DbHelper.ExecuteSqlCommand($@"update SocialSecurity set Status = {(int)SocialSecurityStatusEnum.WaitingHandle} where SocialSecurityPeopleID ={socialSecurityPeople.SocialSecurityPeopleID};", null);
                            if (orderDetail.IsPayAccumulationFund)
                                DbHelper.ExecuteSqlCommand($@"update AccumulationFund set Status = {(int)SocialSecurityStatusEnum.WaitingHandle}  where SocialSecurityPeopleID ={socialSecurityPeople.SocialSecurityPeopleID};", null);

                        }
                    }

                    LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = "财务审核通过了订单：" + orderCode + "，金额：" + Amount + "，客户：{0}" ,MemberID=DbHelper.QuerySingle<int>($"select MemberID from [Order] where OrderCode='{orderCode}';") }) ;


                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    if (Type == 0)
                    {
                        return Json(new { status = false, message = "审核不通过" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        TempData["Message"] = "审核不通过";
                        return RedirectToAction("GetOrderList");
                    }
                }
                finally
                {
                    transaction.Dispose();
                }
            }

            if (Type == 0)
            {
                return Json(new { status = true, message = "审核通过" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                TempData["Message"] = "审核通过";
                return RedirectToAction("GetOrderList");
            }
        }

        /// <summary>
        /// 获取订单详情列表
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public ActionResult GetSubOrderList(string OrderCode)
        {
            List<FinanceSubOrder> subOrderList = orderService.GetSubOrderList(OrderCode);
            return View(subOrderList);
        }
    }
}