﻿using System;
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
        IOrderService orderService = new OrderService();
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

            ViewData["memberList"] = _memberService.GetMembersList();
            return View(orderList);
        }

        /// <summary>
        /// 订单批量审核 若审核通过，订单变成已完成，并且参保人状态变成待办
        /// </summary>
        /// <param name="OrderCodes"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult BatchAuditing(string[] OrderCodes, int Type)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    string OrderCodeStr = string.Join("','", OrderCodes);
                    //修改订单状态
                    bool flag1 = orderService.ModifyOrderStatus(OrderCodeStr);
                    //修改订单对应的参保人的社保和公积金状态  SocialSecurityStatusEnum.WaitingHandle
                    //首先需要判断参保人是否已经通过客服审核
                    foreach (var orderCode in OrderCodes)
                    {
                        string sqlstr = $"select SocialSecurityPeopleID from OrderDetails where OrderCode ='{orderCode}'";
                        List<OrderDetails> orderDetailList = DbHelper.Query<OrderDetails>(sqlstr);
                        foreach (var orderDetail in orderDetailList)
                        {
                            string sqlStr1 = $"select * from SocialSecurityPeople where SocialSecurityPeopleID ={orderDetail.SocialSecurityPeopleID}";
                            SocialSecurityPeople socialSecurityPeople = DbHelper.QuerySingle<SocialSecurityPeople>(sqlStr1);
                            //是否通过客服审核，若通过，则修改社保和公积金状态
                            if (Convert.ToInt32(socialSecurityPeople.Status) == (int)CustomerServiceAuditEnum.Pass)
                            {
                                string sqlStr2 = $"update SocialSecurity set Status = {(int)SocialSecurityStatusEnum.WaitingHandle} where SocialSecurityPeopleID ={socialSecurityPeople.SocialSecurityPeopleID};update AccumulationFund set Status = {(int)SocialSecurityStatusEnum.WaitingHandle}  where SocialSecurityPeopleID ={socialSecurityPeople.SocialSecurityPeopleID};";
                                DbHelper.ExecuteSqlCommand(sqlStr2, null);
                            }
                        }

                        LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("审核通过了订单：{0}", orderCode) });

                    }

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