using System;
using System.Collections.Generic;
using System.Configuration;
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
using WYJK.Framework.Helpers;

namespace WYJK.Web.Controllers.Mvc
{
    [Authorize]
    public class OrderController : BaseController
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

            ViewData["PaymentMethod"] = new SelectList(DbHelper.Query<Order>("select * from [Order] where PaymentMethod<>'' and PaymentMethod is not null").Select(n => new { PaymentMethod = n.PaymentMethod ?? "" }).Distinct().ToList(), "PaymentMethod", "PaymentMethod");


            ViewBag.TotalAmount = orderService.GetFinanceOrderAmount(parameter);
            ViewBag.memberList = memberList;
            return View(orderList);
        }

        private static string apikey = ConfigurationManager.AppSettings["apikey"].ToString();
        private static string contentFormat = ConfigurationManager.AppSettings["SMS15Warn"].ToString();

        /// <summary>
        /// 15号办结扣费
        /// </summary>
        /// <returns></returns>
        public ActionResult BanJieKouFei()
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    #region 每个用户下的所有正常的参保人进行扣款,并将已投月数+1,剩余月数-1
                    //查询所有用户
                    string sqlMember = "select * from Members where ISNULL(IsFrozen,0) = 0";//此用户必须是非冻结的账户
                    List<Members> memberList = DbHelper.Query<Members>(sqlMember);
                    string sqlStr = string.Empty;

                    foreach (Members member in memberList)
                    {
                        decimal yue = DbHelper.QuerySingle<decimal>($"select ISNULL(Account,0) from Members where MemberID={member.MemberID}");
                        //查询该用户下的所有参保人
                        string sqlSocialSecurityPeople = $"select * from SocialSecurityPeople where MemberID={member.MemberID}";
                        List<SocialSecurityPeople> SocialSecurityPeopleList = DbHelper.Query<SocialSecurityPeople>(sqlSocialSecurityPeople);
                        string SocialSecurityPeopleIDStr = string.Join("','", SocialSecurityPeopleList.Select(n => n.SocialSecurityPeopleID));

                        //查询该用户下的所有正常参保方案
                        string sqlSocialSecurity = $"select * from SocialSecurity where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.Normal}";
                        List<SocialSecurity> SocialSecurityList = DbHelper.Query<SocialSecurity>(sqlSocialSecurity);

                        int day = DateTime.Now.Day;
                        DateTime dt = new DateTime();
                        if (day >= 15)
                            dt = DateTime.Now;
                        else
                            dt = DateTime.Now.AddMonths(-1);

                        foreach (SocialSecurity socialSecurity in SocialSecurityList)
                        {
                            //流水记录
                            List<AccountRecord> accountRecordList = DbHelper.Query<AccountRecord>($"select * from AccountRecord where Type=0 and convert(varchar(7),CreateTime,120)='{dt.ToString("yyyy-MM")}'");

                            bool flag = false;
                            //如果流水记录中有，则跳出
                            foreach (var accountRecord in accountRecordList)
                            {
                                if (accountRecord.SocialSecurityPeopleID == socialSecurity.SocialSecurityPeopleID)
                                {
                                    flag = true;
                                    break;
                                }
                            }

                            if (flag)
                                continue;

                            //社保单月金额
                            decimal account = socialSecurity.SocialSecurityBase * socialSecurity.PayProportion / 100;
                            //余额减
                            sqlStr += $"update Members set Account=Account-{account} where MemberID={member.MemberID};";

                            yue -= account;
                            //社保流水账
                            sqlStr += $"insert into AccountRecord(Type,SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime)"
                                       + $" values(0,{DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{member.MemberID},{socialSecurity.SocialSecurityPeopleID},'{socialSecurity.SocialSecurityPeopleName}','支出','余额','社保费',{account},{yue},getdate());";
                            //已投月数+1,剩余月数-1
                            sqlStr += $"update SocialSecurity set AlreadyPayMonthCount=ISNULL(AlreadyPayMonthCount,0)+1,PayMonthCount=ISNULL(PayMonthCount,0)-1  where SocialSecurityPeopleID={socialSecurity.SocialSecurityPeopleID};";
                        }

                        //查询该用户下的所有正常参公积金方案
                        string sqlAccumulationFund = $"select * from AccumulationFund where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.Normal}";
                        List<AccumulationFund> AccumulationFundList = DbHelper.Query<AccumulationFund>(sqlAccumulationFund);
                        foreach (AccumulationFund accumulationFund in AccumulationFundList)
                        {
                            //流水记录
                            List<AccountRecord> accountRecordList = DbHelper.Query<AccountRecord>($"select * from AccountRecord where Type=1 and convert(varchar(7),CreateTime,120)='{dt.ToString("yyyy-MM")}'");

                            bool flag = false;
                            //如果流水记录中有，则跳出
                            foreach (var accountRecord in accountRecordList)
                            {
                                if (accountRecord.SocialSecurityPeopleID == accumulationFund.SocialSecurityPeopleID)
                                {
                                    flag = true;
                                    break;
                                }
                            }

                            if (flag)
                                continue;

                            //公积金单月金额
                            decimal account = accumulationFund.AccumulationFundBase * accumulationFund.PayProportion / 100;
                            //余额减
                            sqlStr += $"update Members set Account=Account-{account} where MemberID={member.MemberID};";

                            yue -= account;
                            //公积金流水账
                            sqlStr += $"insert into AccountRecord(Type,SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime)"
                                       + $" values(1,{DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{member.MemberID},{accumulationFund.SocialSecurityPeopleID},'{accumulationFund.SocialSecurityPeopleName}','支出','余额','公积金费',{account},{yue},getdate());";
                            //已投月数+1,剩余月数-1
                            sqlStr += $"update AccumulationFund set AlreadyPayMonthCount=ISNULL(AlreadyPayMonthCount,0)+1,PayMonthCount=ISNULL(PayMonthCount,0)-1 where SocialSecurityPeopleID={accumulationFund.SocialSecurityPeopleID};";
                        }
                    }
                    if (sqlStr.Trim() != string.Empty)
                    {
                        DbHelper.ExecuteSqlCommand(sqlStr, null);
                    }

                    #endregion

                    #region 检测下月余额是否够用，不够用则状态变为待续费
                    string sqlStr2 = string.Empty;
                    foreach (Members member in memberList)
                    {
                        #region 查询每个用户余额
                        decimal totalAccount = 0;
                        //查询该用户下的所有参保人
                        string sqlSocialSecurityPeople = $"select * from SocialSecurityPeople where MemberID={member.MemberID}";
                        List<SocialSecurityPeople> SocialSecurityPeopleList = DbHelper.Query<SocialSecurityPeople>(sqlSocialSecurityPeople);
                        string SocialSecurityPeopleIDStr = string.Join("','", SocialSecurityPeopleList.Select(n => n.SocialSecurityPeopleID));

                        //查询该用户下的所有正常参保方案
                        string sqlSocialSecurity = $"select * from SocialSecurity where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.Normal}";
                        List<SocialSecurity> SocialSecurityList = DbHelper.Query<SocialSecurity>(sqlSocialSecurity);
                        foreach (SocialSecurity socialSecurity in SocialSecurityList)
                        {
                            //社保单月金额
                            decimal account = socialSecurity.SocialSecurityBase * socialSecurity.PayProportion / 100;
                            totalAccount += account;
                        }

                        //查询该用户下的所有正常参公积金方案
                        string sqlAccumulationFund = $"select * from AccumulationFund where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.Normal}";
                        List<AccumulationFund> AccumulationFundList = DbHelper.Query<AccumulationFund>(sqlAccumulationFund);
                        foreach (AccumulationFund accumulationFund in AccumulationFundList)
                        {
                            //公积金单月金额
                            decimal account = accumulationFund.AccumulationFundBase * accumulationFund.PayProportion / 100;
                            totalAccount += account;
                        }
                        #endregion

                        #region 查询下月余额是否够用,若不够用，则变为待续费
                        if (member.Account < totalAccount)
                        {
                            //该用户下的正常社保变为待续费
                            foreach (SocialSecurity socialSecurity in SocialSecurityList)
                            {
                                sqlStr2 += $"update SocialSecurity set Status ={(int)SocialSecurityStatusEnum.Renew} where SocialSecurityPeopleID={socialSecurity.SocialSecurityPeopleID};";
                            }
                            //该用户下的正常公积金变为待续费
                            foreach (AccumulationFund accumulationFund in AccumulationFundList)
                            {
                                sqlStr2 += $"update AccumulationFund set Status={(int)SocialSecurityStatusEnum.Renew} where SocialSecurityPeopleID ={accumulationFund.SocialSecurityPeopleID};";
                            }

                            Message message = new Message();
                            message.MemberID = member.MemberID;
                            message.ContentStr = "您的账户余额已不足抵扣下个月社保、公积金.请及时充值.";

                            //发送消息提醒
                            DbHelper.ExecuteSqlCommand($"insert into Message(MemberID,ContentStr) values({message.MemberID},'{message.ContentStr}')", null);

                            #region 发送短信
                            string content = string.Format(contentFormat, member.MemberPhone.Substring(member.MemberPhone.Length - 4), member.Account, DateTime.Now.AddMonths(1).Month, totalAccount, totalAccount - member.Account);

                            Sms.sendSms(apikey, content, member.MemberPhone);
                            #endregion
                        }

                        #endregion
                    }
                    if (sqlStr2.Trim() != string.Empty)
                        DbHelper.ExecuteSqlCommand(sqlStr2, null);
                    #endregion

                    #region 待停变停保
                    string sqlStr3 = string.Empty;
                    foreach (Members member in memberList)
                    {
                        //查询该用户下的所有参保人
                        string sqlSocialSecurityPeople = $"select * from SocialSecurityPeople where MemberID={member.MemberID}";
                        List<SocialSecurityPeople> SocialSecurityPeopleList = DbHelper.Query<SocialSecurityPeople>(sqlSocialSecurityPeople);
                        string SocialSecurityPeopleIDStr = string.Join("','", SocialSecurityPeopleList.Select(n => n.SocialSecurityPeopleID));

                        //查询该用户下的所有待停保参保方案
                        string sqlSocialSecurity = $"select * from SocialSecurity where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.WaitingStop}";
                        List<SocialSecurity> SocialSecurityList = DbHelper.Query<SocialSecurity>(sqlSocialSecurity);
                        foreach (SocialSecurity socialSecurity in SocialSecurityList)
                        {
                            sqlStr3 += $"update SocialSecurity set Status ={(int)SocialSecurityStatusEnum.AlreadyStop},StopDate=getdate() where SocialSecurityPeopleID={socialSecurity.SocialSecurityPeopleID};";
                        }

                        //查询该用户下的所有待停保参公积金方案
                        string sqlAccumulationFund = $"select * from AccumulationFund where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.WaitingStop}";
                        List<AccumulationFund> AccumulationFundList = DbHelper.Query<AccumulationFund>(sqlAccumulationFund);
                        foreach (AccumulationFund accumulationFund in AccumulationFundList)
                        {
                            sqlStr3 += $"update AccumulationFund set Status ={(int)SocialSecurityStatusEnum.AlreadyStop},StopDate=getdate() where SocialSecurityPeopleID={accumulationFund.SocialSecurityPeopleID};";
                        }
                    }
                    if (sqlStr3.Trim() != string.Empty)
                        DbHelper.ExecuteSqlCommand(sqlStr3, null);
                    #endregion

                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return Json(new { status = false, message = "扣费失败" }, JsonRequestBehavior.AllowGet);
                }
                finally
                {
                    transaction.Dispose();
                }
            }

            return Json(new { status = true, message = "扣费成功" }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetPayList(MembersPayParameters parameter)
        {
            //PagedResult<MembersStatistics> membersStatisticsList = _memberService.GetMembersStatisticsList(parameter);

            PagedResult<MembersPayList> membersPayList = _memberService.GetMembersPayList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text");

            List<Members> memberList = _memberService.GetMembersList();
            memberList.ForEach(item =>
            {
                item.MemberName = item.UserType == "0" ? item.MemberName : (item.UserType == "1" ? item.EnterpriseName : item.BusinessName);
            });
            ViewBag.memberList = memberList;

            //费用来源
            ViewData["PaymentMethod"] = new SelectList(DbHelper.Query<AccountRecord>("select * from AccountRecord where LaiYuan<>'' and LaiYuan is not null").Select(n => new { PaymentMethod = n.LaiYuan ?? "" }).Distinct().ToList(), "PaymentMethod", "PaymentMethod");

            List<Users> userList = DbHelper.Query<Users>("select * from Users");
            userList.Insert(0, new Users { UserName = "全部" });
            ViewData["Payee"] = new SelectList(userList, "UserName", "UserName");
            dynamic dynAmount = _memberService.GetMembersPayAmount(parameter);
            ViewBag.TotalAmount = dynAmount.TotalAmount;//总计交费
            ViewBag.DeduceAmount = dynAmount.DeduceAmount;//扣费

            return View(membersPayList);
        }

        /// <summary>
        /// 根据用户ID获取账户列表
        /// </summary>
        /// <param name="memberID"></param>
        /// <returns></returns>
        public ActionResult GetAccountRecordList(int memberID)
        {
            //获取会员信息
            ViewData["member"] = _memberService.GetMemberInfoForAdmin(memberID);

            List<AccountRecord> accountRecordList = _memberService.GetAccountRecordList(memberID).OrderByDescending(n => n.ID).ToList();
            return View(accountRecordList);
        }




        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetMemberList1(string UserType)
        {
            List<Members> memberList = _memberService.GetMembersList();
            var list = memberList.Where(n => UserType == string.Empty ? true : n.UserType == UserType)
                .Select(item => new { MemberID = item.MemberID, MemberName = item.UserType == "0" ? item.MemberName : (item.UserType == "1" ? item.EnterpriseName : item.BusinessName) });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 订单批量审核 若审核通过，订单变成已完成，并且参保人状态变成待办
        /// </summary>
        /// <param name="OrderCodes"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult BatchAuditing(string OrderCodeStr, string Amount, int Type)
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

                    string sqlstr = $"select * from OrderDetails where OrderCode ='{orderCode}'";
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

                    LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = "财务审核通过了订单：" + orderCode + "，金额：" + Amount + "，客户：{0}", MemberID = DbHelper.QuerySingle<int>($"select MemberID from [Order] where OrderCode='{orderCode}';") });


                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    if (Type == 0)
                    {
                        return Json(new { status = false, message = "审核未生效" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        TempData["Message"] = "审核未生效";
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
        /// 财务审核不通过
        /// </summary>
        /// <param name="OrderCodeStr"></param>
        /// <param name="Amount"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult BatchNoPass(string OrderCodeStr, string Amount, int Type)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    //修改订单状态
                    string sql = $"update [Order] set Status={(int)OrderEnum.NoPass} where OrderCode in('{OrderCodeStr}')";
                    DbHelper.ExecuteSqlCommand(sql, null);

                    //冻结账户
                    DbHelper.ExecuteSqlCommand($"update Members set IsFrozen=1 where MemberID=(select MemberID from [Order] where OrderCode='{OrderCodeStr}')", null);

                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    if (Type == 0)
                    {
                        return Json(new { status = false, message = "审核未生效" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        TempData["Message"] = "审核未生效";
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
                return Json(new { status = true, message = "审核不通过" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                TempData["Message"] = "审核不通过";
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

            ViewData["Balance"] = DbHelper.QuerySingle<decimal>($"select ISNULL(Balance,0) from [Order] where OrderCode='{OrderCode}'");

            return View(subOrderList);
        }
    }
}