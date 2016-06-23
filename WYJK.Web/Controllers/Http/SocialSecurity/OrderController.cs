﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Web.Controllers.Http
{
    /// <summary>
    /// 订单接口
    /// </summary>
    public class OrderController : ApiController
    {
        private readonly IOrderService _orderService = new OrderService();
        private readonly ISocialSecurityService _socialSecurityService = new SocialSecurityService();
        private readonly IEnterpriseService _enterpriseService = new EnterpriseService();
        /// <summary>
        /// 生成订单 Data里头是订单编号
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JsonResult<dynamic> GenerateOrder(GenerateOrderParameter parameter)
        {
            //var headers = HttpContext.Current.Request.Headers["Content-Type"];

            Dictionary<bool, string> dic = null;
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                { //首先判断是否有未支付订单，若有，则不能生成订单
                    if (_orderService.IsExistsWaitingPayOrderByMemberID(parameter.MemberID))
                    {
                        return new JsonResult<dynamic>
                        {
                            status = false,
                            Message = "有未支付的订单，请先进行支付"
                        };
                    }
                    string SocialSecurityPeopleIDStr = string.Join(",", parameter.SocialSecurityPeopleIDS);
                    //判断所选参保人中有没有超过13号的
                    string sqlstr = $"select * from SocialSecurity where SocialSecurityPeopleID in({SocialSecurityPeopleIDStr})";
                    List<SocialSecurity> socialSecurityList = DbHelper.Query<SocialSecurity>(sqlstr);
                    foreach (var socialSecurity in socialSecurityList)
                    {
                        if ((socialSecurity.PayTime.Value.Month < DateTime.Now.Month || (socialSecurity.PayTime.Value.Month == DateTime.Now.Month && DateTime.Now.Day > 13)) && socialSecurity.Status == "1" && socialSecurity.IsPay == false)
                        {
                            return new JsonResult<dynamic>
                            {
                                status = false,
                                Message = "参保人日期已失效，请修改"
                            };
                        }
                    }

                    string sqlstr1 = $"select * from AccumulationFund where SocialSecurityPeopleID in({SocialSecurityPeopleIDStr})";
                    List<AccumulationFund> accumulationFundList = DbHelper.Query<AccumulationFund>(sqlstr1);
                    foreach (var accumulationFund in accumulationFundList)
                    {
                        if ((accumulationFund.PayTime.Value.Month < DateTime.Now.Month || (accumulationFund.PayTime.Value.Month == DateTime.Now.Month && DateTime.Now.Day > 13)) && accumulationFund.Status == "1" && accumulationFund.IsPay == false)
                        {
                            return new JsonResult<dynamic>
                            {
                                status = false,
                                Message = "参保人日期已失效，请修改"
                            };
                        }
                    }

                    string orderCode = DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000).ToString().PadLeft(3, '0');

                    dic = _orderService.GenerateOrder(SocialSecurityPeopleIDStr, parameter.MemberID, orderCode);

                    //通过前面生成的订单，再加上冻结费
                    if (dic.First().Key)
                    {
                        //查询订单下的所有子订单
                        List<OrderDetails> orderDetailsList = DbHelper.Query<OrderDetails>($"select * from OrderDetails where OrderCode = '{dic.First().Value}'");
                        //每人每月补差费用
                        decimal FreezingAmount = DbHelper.QuerySingle<decimal>("select FreezingAmount from CostParameterSetting where Status = 0");
                        //更新订单补差费用
                        string BuchaSqlStr = string.Empty;
                        //遍历订单下的所有子订单
                        foreach (var orderDetails in orderDetailsList)
                        {
                            decimal BuchaAmount = 0;
                            if (orderDetails.IsPaySocialSecurity)
                            {
                                //参保月份、参保月数、签约单位ID
                                SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select InsuranceArea,PayTime,PayMonthCount,RelationEnterprise from SocialSecurity where SocialSecurityPeopleID ={orderDetails.SocialSecurityPeopleID}");
                                int payMonth = socialSecurity.PayTime.Value.Month;
                                int monthCount = socialSecurity.PayMonthCount;
                                //相对应的签约单位城市是否已调差（社平工资）
                                EnterpriseSocialSecurity enterpriseSocialSecurity = _enterpriseService.GetEnterpriseSocialSecurity(socialSecurity.RelationEnterprise);//签约公司
                                //已调,当年以后知道年末都不需交，直到一月份开始交
                                if (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year == DateTime.Now.Year)
                                {
                                    int freeBuchaMonthCount = 12 + 1 - payMonth;//免补差月数
                                    int BuchaMonthCount = monthCount - freeBuchaMonthCount;
                                    if (freeBuchaMonthCount < monthCount)
                                    {
                                        BuchaAmount = FreezingAmount * BuchaMonthCount;
                                    }
                                }
                                //未调，往后每个月都需要交，许吧当年1月份到现在的都要交上
                                if (enterpriseSocialSecurity.AdjustDt == null || (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year != DateTime.Now.Year))
                                {
                                    int BuchaMonthCount = payMonth - 1 + monthCount;
                                    BuchaAmount = FreezingAmount * BuchaMonthCount;
                                }
                            }

                            //如果产生补差费用，则需要更新订单
                            if (BuchaAmount != 0)
                            {
                                BuchaSqlStr += $"update OrderDetails set SocialSecurityBuCha={BuchaAmount} where OrderDetailID={orderDetails.OrderDetailID};";
                            }
                        }
                        if (BuchaSqlStr != string.Empty)
                            DbHelper.ExecuteSqlCommand(BuchaSqlStr, null);
                    }
                    else {
                        throw new Exception("订单生成失败");
                    }

                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return new JsonResult<dynamic>
                    {
                        status = false,
                        Message = "订单生成失败"
                    };
                }
                finally
                {
                    transaction.Dispose();
                }
            }
            return new JsonResult<dynamic>
            {
                status = true,
                Message = "订单生成成功！",
                Data = dic.First().Value
            };
        }

        /// <summary>
        /// 是否可以自动付款  
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public JsonResult<dynamic> IsCanAutoPayment(GenerateOrderParameter parameter)
        {
            //首先判断是否有未支付订单，若有，则不能生成订单
            if (_orderService.IsExistsWaitingPayOrderByMemberID(parameter.MemberID))
            {
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "有未支付的订单，请先进行支付"
                };
            }
            string SocialSecurityPeopleIDStr = string.Join(",", parameter.SocialSecurityPeopleIDS);
            //判断所选参保人中有没有超过13号的
            string sqlstr2 = $"select * from SocialSecurity where SocialSecurityPeopleID in({SocialSecurityPeopleIDStr})";
            List<SocialSecurity> socialSecurityList = DbHelper.Query<SocialSecurity>(sqlstr2);
            foreach (var socialSecurity in socialSecurityList)
            {
                if (socialSecurity.PayTime.Value.Month < DateTime.Now.Month || (socialSecurity.PayTime.Value.Month == DateTime.Now.Month && DateTime.Now.Day > 13))
                {
                    return new JsonResult<dynamic>
                    {
                        status = false,
                        Message = "参保人日期已失效，请修改"
                    };
                }
            }

            string sqlstr1 = $"select * from AccumulationFund where SocialSecurityPeopleID in({SocialSecurityPeopleIDStr})";
            List<AccumulationFund> accumulationFundList = DbHelper.Query<AccumulationFund>(sqlstr1);
            foreach (var accumulationFund in accumulationFundList)
            {
                if (accumulationFund.PayTime.Value.Month < DateTime.Now.Month || (accumulationFund.PayTime.Value.Month == DateTime.Now.Month && DateTime.Now.Day > 13))
                {
                    return new JsonResult<dynamic>
                    {
                        status = false,
                        Message = "参保人日期已失效，请修改"
                    };
                }
            }



            //判断待办与正常的参保人所有所要缴纳金额+未参保但已付款金额+本次金额之和与账户金额作比较

            //待办与正常的参保人所有所要缴纳金额
            decimal totalAmount = _socialSecurityService.GetMonthTotalAmountByMemberID(parameter.MemberID);

            string str = $@"select ISNULL(sum(SocialSecurityAmount*SocialSecuritypayMonth+SocialSecurityServiceCost+SocialSecurityFirstBacklogCost+SocialSecurityBuCha
  + AccumulationFundAmount * AccumulationFundpayMonth + AccumulationFundServiceCost + AccumulationFundFirstBacklogCost),0)
   from OrderDetails
   where OrderDetails.SocialSecurityPeopleID in 
   (select SocialSecurityPeople.SocialSecurityPeopleID from SocialSecurityPeople
    left join SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID  where MemberID = {parameter.MemberID} and SocialSecurityPeople.IsPay = 1 and SocialSecurity.Status = 1)
    or SocialSecurityPeopleID in 
    (select SocialSecurityPeople.SocialSecurityPeopleID from SocialSecurityPeople
    left join AccumulationFund on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID  where MemberID = {parameter.MemberID} and SocialSecurityPeople.IsPay = 1 and AccumulationFund.Status = 1)";

            decimal autoAmount = DbHelper.QuerySingle<decimal>(str);

            //本次订单金额
            string SocialSecurityPeopleIDsStr = string.Join(",", parameter.SocialSecurityPeopleIDS);
            string sqlstr = $@"select SUM(ISNULL(dbo.SocialSecurity.SocialSecurityBase*SocialSecurity.PayProportion/100,0)*ISNULL(dbo.SocialSecurity.PayMonthCount,0)+
ISNULL(dbo.AccumulationFund.AccumulationFundBase*AccumulationFund.PayProportion/100,0)*ISNULL(dbo.AccumulationFund.PayMonthCount,0))
from dbo.SocialSecurityPeople 
left join dbo.SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID 
left join dbo.AccumulationFund on SocialSecurityPeople.SocialSecurityPeopleID=AccumulationFund.SocialSecurityPeopleID 
where SocialSecurityPeople.SocialSecurityPeopleID in({SocialSecurityPeopleIDsStr})";
            decimal SSAF_Amount = DbHelper.QuerySingle<decimal>(sqlstr);

            int SSNum = DbHelper.QuerySingle<int>($@"select count(1) from SocialSecurityPeople 
                left join SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID
where SocialSecurityPeople.SocialSecurityPeopleID in({SocialSecurityPeopleIDsStr})");
            decimal SSBacklogCost = DbHelper.QuerySingle<decimal>("select BacklogCost from CostParameterSetting where Status =0 ") * SSNum;


            int AFNum = DbHelper.QuerySingle<int>($@"select count(1) from SocialSecurityPeople 
                left join AccumulationFund on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
where SocialSecurityPeople.SocialSecurityPeopleID in({SocialSecurityPeopleIDsStr})");
            decimal AFBacklogCost = DbHelper.QuerySingle<decimal>("select BacklogCost from CostParameterSetting where Status =1 ") * SSNum;

            #region 补差费
            decimal BuchaAmountTotal = 0;
            //每人每月补差费用
            decimal FreezingAmount = DbHelper.QuerySingle<decimal>("select FreezingAmount from CostParameterSetting where Status = 0");
            //更新订单补差费用
            string BuchaSqlStr = string.Empty;
            //遍历订单下的所有子订单
            foreach (var SocialSecurityPeopleID in parameter.SocialSecurityPeopleIDS)
            {
                //参保月份、参保月数、签约单位ID
                SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select InsuranceArea,PayTime,PayMonthCount,RelationEnterprise from SocialSecurity where SocialSecurityPeopleID ={SocialSecurityPeopleID}");
                decimal BuchaAmount = 0;
                if (socialSecurity != null)
                {
                    int payMonth = socialSecurity.PayTime.Value.Month;
                    int monthCount = socialSecurity.PayMonthCount;
                    //相对应的签约单位城市是否已调差（社平工资）
                    EnterpriseSocialSecurity enterpriseSocialSecurity = _enterpriseService.GetEnterpriseSocialSecurity(socialSecurity.RelationEnterprise);//签约公司
                    //已调,当年以后知道年末都不需交，直到一月份开始交
                    if (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year == DateTime.Now.Year)
                    {
                        int freeBuchaMonthCount = 12 + 1 - payMonth;//免补差月数
                        int BuchaMonthCount = monthCount - freeBuchaMonthCount;
                        if (freeBuchaMonthCount < monthCount)
                        {
                            BuchaAmount = FreezingAmount * BuchaMonthCount;
                        }
                    }
                    //未调，往后每个月都需要交，许吧当年1月份到现在的都要交上
                    if (enterpriseSocialSecurity.AdjustDt == null || (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year != DateTime.Now.Year))
                    {
                        int BuchaMonthCount = payMonth - 1 + monthCount;
                        BuchaAmount = FreezingAmount * BuchaMonthCount;
                    }
                }
                BuchaAmountTotal += BuchaAmount;
            }
            #endregion

            decimal ThisAmount = SSAF_Amount + SSBacklogCost + AFBacklogCost + BuchaAmountTotal;

            decimal AccountAmount = DbHelper.QuerySingle<decimal>($"select ISNULL(Account,0) from Members where MemberID = {parameter.MemberID}");

            if (totalAmount + autoAmount + ThisAmount < AccountAmount)
                return new JsonResult<dynamic>
                {
                    status = true,
                    Message = "可以自动扣款"
                };
            else
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "不可以自动扣款"
                };
        }


        /// <summary>
        /// 自动扣款
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JsonResult<dynamic> AutoPayment(GenerateOrderParameter parameter)
        {
            Dictionary<bool, string> dic = null;
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    #region 生成订单
                    string SocialSecurityPeopleIDStr = string.Join(",", parameter.SocialSecurityPeopleIDS);

                    string orderCode = DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000).ToString().PadLeft(3, '0');

                    dic = _orderService.GenerateOrder(SocialSecurityPeopleIDStr, parameter.MemberID, orderCode);
                    #endregion

                    if (dic.First().Key == true)
                    {
                        //查询订单下的所有子订单
                        List<OrderDetails> orderDetailsList = DbHelper.Query<OrderDetails>($"select * from OrderDetails where OrderCode = '{dic.First().Value}'");
                        //每人每月补差费用
                        decimal FreezingAmount = DbHelper.QuerySingle<decimal>("select FreezingAmount from CostParameterSetting where Status = 0");
                        //更新订单补差费用
                        string BuchaSqlStr = string.Empty;
                        //遍历订单下的所有子订单
                        foreach (var orderDetails in orderDetailsList)
                        {
                            //参保月份、参保月数、签约单位ID
                            SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select InsuranceArea,PayTime,PayMonthCount,RelationEnterprise from SocialSecurity where SocialSecurityPeopleID ={orderDetails.SocialSecurityPeopleID}");
                            decimal BuchaAmount = 0;
                            if (socialSecurity != null)
                            {
                                int payMonth = socialSecurity.PayTime.Value.Month;
                                int monthCount = socialSecurity.PayMonthCount;
                                //相对应的签约单位城市是否已调差（社平工资）
                                EnterpriseSocialSecurity enterpriseSocialSecurity = _enterpriseService.GetEnterpriseSocialSecurity(socialSecurity.RelationEnterprise);//签约公司
                                //已调,当年以后知道年末都不需交，直到一月份开始交
                                if (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year == DateTime.Now.Year)
                                {
                                    int freeBuchaMonthCount = 12 + 1 - payMonth;//免补差月数
                                    int BuchaMonthCount = monthCount - freeBuchaMonthCount;
                                    if (freeBuchaMonthCount < monthCount)
                                    {
                                        BuchaAmount = FreezingAmount * BuchaMonthCount;
                                    }
                                }
                                //未调，往后每个月都需要交，许吧当年1月份到现在的都要交上
                                if (enterpriseSocialSecurity.AdjustDt == null || (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year != DateTime.Now.Year))
                                {
                                    int BuchaMonthCount = payMonth - 1 + monthCount;
                                    BuchaAmount = FreezingAmount * BuchaMonthCount;
                                }
                            }

                            //如果产生补差费用，则需要更新订单
                            if (BuchaAmount != 0)
                            {
                                BuchaSqlStr += $"update OrderDetails set SocialSecurityBuCha={BuchaAmount} where OrderDetailID={orderDetails.OrderDetailID};";
                            }
                        }
                        if (BuchaSqlStr != string.Empty)
                            DbHelper.ExecuteSqlCommand(BuchaSqlStr, null);
                    }
                    else {
                        throw new Exception("自动扣款失败");
                    }

                    string sqlOrderDetail = $"select * from OrderDetails where OrderCode ={dic.First().Value}";
                    List<OrderDetails> orderDetailList = DbHelper.Query<OrderDetails>(sqlOrderDetail);

                    string sqlAccountRecord = "";
                    string sqlSocialSecurityPeople = "";
                    //收支记录
                    string ShouNote = "自动扣款：";
                    string ZhiNote = string.Empty;
                    decimal Bucha = 0;//补差
                    decimal ZhiAccount = 0;//支出总额
                    decimal accountNum = 0;//订单总额
                    decimal memberAccount = DbHelper.QuerySingle<decimal>($"select Account from Members where MemberID = {parameter.MemberID}");
                    //支出记录
                    foreach (var orderDetail in orderDetailList)
                    {
                        if (orderDetail.IsPaySocialSecurity)
                            sqlSocialSecurityPeople += $"update SocialSecurity set IsPay=1 where SocialSecurityPeopleID ={orderDetail.SocialSecurityPeopleID};";
                        if (orderDetail.IsPayAccumulationFund)
                            sqlSocialSecurityPeople += $"update AccumulationFund set IsPay=1 where SocialSecurityPeopleID ={orderDetail.SocialSecurityPeopleID};";


                        accountNum += orderDetail.SocialSecurityAmount * orderDetail.SocialSecuritypayMonth + orderDetail.SocialSecurityFirstBacklogCost + orderDetail.SocialSecurityBuCha
                            + orderDetail.AccumulationFundAmount * orderDetail.AccumulationFundpayMonth + orderDetail.AccumulationFundFirstBacklogCost;
                        Bucha += orderDetail.SocialSecurityBuCha;
                        ZhiAccount += orderDetail.SocialSecurityFirstBacklogCost + orderDetail.AccumulationFundFirstBacklogCost;
                        ShouNote += (orderDetail.SocialSecurityAmount != 0 ? string.Format("{0}:{1}个月社保,单月保费:{2},社保待办费:{3},补差费:{4};", orderDetail.SocialSecurityPeopleName, orderDetail.SocialSecuritypayMonth, orderDetail.SocialSecurityAmount, orderDetail.SocialSecurityFirstBacklogCost, orderDetail.SocialSecurityBuCha) : string.Empty) + (orderDetail.AccumulationFundAmount != 0 ? string.Format("{0}:{1}个月公积金,单月公积金费:{2},公积金代办费:{3};", orderDetail.SocialSecurityPeopleName, orderDetail.AccumulationFundpayMonth, orderDetail.AccumulationFundAmount, orderDetail.AccumulationFundFirstBacklogCost) : string.Empty);
                        ZhiNote += (orderDetail.SocialSecurityFirstBacklogCost != 0 ? string.Format("{0}:社保待办费:{1}", orderDetail.SocialSecurityPeopleName, orderDetail.SocialSecurityFirstBacklogCost) : string.Empty) + (orderDetail.AccumulationFundFirstBacklogCost != 0 ? string.Format("{0}:公积金代办费:{1};", orderDetail.SocialSecurityPeopleName, orderDetail.AccumulationFundFirstBacklogCost) : string.Empty) + (orderDetail.SocialSecurityBuCha != 0 ? string.Format("{0}:补差费:{1}", orderDetail.SocialSecurityPeopleName, orderDetail.SocialSecurityBuCha) : string.Empty);
                    }

                    sqlAccountRecord += $@"insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime)
values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{parameter.MemberID},'','','收入','余额扣款','{ShouNote}',{accountNum},{memberAccount + accountNum},getdate());
                                       insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime) 
values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{parameter.MemberID},'','','支出','余额','{ZhiNote}',{ZhiAccount},{memberAccount + accountNum - Bucha - ZhiAccount},getdate()); ";

                    //更新未参保人的支付状态
                    DbHelper.ExecuteSqlCommand(sqlSocialSecurityPeople, null);

                    //更新记录
                    DbHelper.ExecuteSqlCommand(sqlAccountRecord, null);

                    //更新个人账户
                    string sqlMember = $"update Members set Account=ISNULL(Account,0)-{ZhiAccount}-{Bucha},Bucha=ISNULL(Bucha,0)+{Bucha} where MemberID={parameter.MemberID}";
                    int updateResult = DbHelper.ExecuteSqlCommand(sqlMember, null);
                    if (!(updateResult > 0)) throw new Exception("更新个人账户失败");

                    //更新订单
                    string sqlUpdateOrder = $"update [Order] set Status = {(int)OrderEnum.completed},PaymentMethod='余额扣款',PayTime=getdate() where OrderCode={dic.First().Value}";
                    DbHelper.ExecuteSqlCommand(sqlUpdateOrder, null);
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return new JsonResult<dynamic>
                    {
                        status = false,
                        Message = "自动扣款失败"
                    };
                }
                finally
                {
                    transaction.Dispose();
                }
            }

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "自动扣款成功",
                Data = dic.First<KeyValuePair<bool, string>>().Value
            };
        }

        /// <summary>
        /// 获取订单列表 0：待付款、1：审核中、2：已完成
        /// </summary>
        /// <returns></returns>
        public JsonResult<dynamic> GetOrderList(int MemberID, int Status)
        {
            List<OrderListForMobile> list = _orderService.GetOrderList(MemberID, Status);
            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JsonResult<dynamic> CancelOrder(OrderCodeArrayParameter parameter)
        {
            string OrderCodeStrs = string.Join("','", parameter.OrderCode);

            if (DbHelper.QuerySingle<int>($"select count(0) from [Order] where OrderCode in('{OrderCodeStrs}') and IsNotCancel=1") > 0)
            {
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "订单不允许删除"
                };
            }

            bool flag = _orderService.CancelOrder(OrderCodeStrs);
            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "取消成功" : "取消失败",
            };
        }

        /// <summary>
        /// 订单支付详情
        /// </summary>
        /// <returns></returns>
        public JsonResult<OrderDetailForMobile> GetOrderDetail(int MemberID, string OrderCode)
        {
            OrderDetailForMobile model = _orderService.GetOrderDetail(MemberID, OrderCode);

            return new JsonResult<OrderDetailForMobile>
            {
                status = true,
                Message = "获取成功",
                Data = model
            };
        }

        /// <summary>
        /// 订单支付 需要传订单编号、支付方式
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JsonResult<dynamic> OrderPayment(Order model)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    string sqlOrder = $"select * from [Order] where OrderCode={model.OrderCode}";
                    Order order = DbHelper.QuerySingle<Order>(sqlOrder);

                    string sqlOrderDetail = $"select * from OrderDetails where OrderCode ={model.OrderCode}";
                    List<OrderDetails> orderDetailList = DbHelper.Query<OrderDetails>(sqlOrderDetail);

                    int[] SocialSecurityPeopleIDS = new int[orderDetailList.Count];
                    for (int i = 0; i < orderDetailList.Count; i++)
                    {
                        SocialSecurityPeopleIDS[i] = orderDetailList[i].SocialSecurityPeopleID;
                    }

                    string SocialSecurityPeopleIDStr = string.Join(",", SocialSecurityPeopleIDS);
                    //判断所选参保人中有没有超过13号的
                    string sqlstr = $"select * from SocialSecurity where SocialSecurityPeopleID in({SocialSecurityPeopleIDStr})";
                    List<SocialSecurity> socialSecurityList = DbHelper.Query<SocialSecurity>(sqlstr);
                    foreach (var socialSecurity in socialSecurityList)
                    {
                        if ((socialSecurity.PayTime.Value.Month < DateTime.Now.Month || (socialSecurity.PayTime.Value.Month == DateTime.Now.Month && DateTime.Now.Day > 13)) && socialSecurity.Status == "1" && socialSecurity.IsPay == false)
                        {
                            return new JsonResult<dynamic>
                            {
                                status = false,
                                Message = "参保人日期已失效，请修改"
                            }; 
                        }
                    }

                    string sqlstr1 = $"select * from AccumulationFund where SocialSecurityPeopleID in({SocialSecurityPeopleIDStr})";
                    List<AccumulationFund> accumulationFundList = DbHelper.Query<AccumulationFund>(sqlstr1);
                    foreach (var accumulationFund in accumulationFundList)
                    {
                        if ((accumulationFund.PayTime.Value.Month < DateTime.Now.Month || (accumulationFund.PayTime.Value.Month == DateTime.Now.Month && DateTime.Now.Day > 13)) && accumulationFund.Status == "1" && accumulationFund.IsPay == false)
                        {
                            return new JsonResult<dynamic>
                            {
                                status = false,
                                Message = "参保人日期已失效，请修改"
                            };
                        }
                    }

                    string sqlAccountRecord = "";
                    string sqlSocialSecurityPeople = "";

                    decimal memberAccount = DbHelper.QuerySingle<decimal>($"select Account from Members where MemberID = {model.MemberID}");
                    //收支记录
                    string ShouNote = "缴费：";
                    string ZhiNote = string.Empty;
                    decimal Bucha = 0;//补差
                    decimal ZhiAccount = 0;//支出总额
                    decimal accountNum = 0;//订单总额
                    foreach (var orderDetail in orderDetailList)
                    {
                        if (orderDetail.IsPaySocialSecurity)
                            sqlSocialSecurityPeople += $"update SocialSecurity set IsPay=1 where SocialSecurityPeopleID ={orderDetail.SocialSecurityPeopleID};";
                        if (orderDetail.IsPayAccumulationFund)
                            sqlSocialSecurityPeople += $"update AccumulationFund set IsPay=1 where SocialSecurityPeopleID ={orderDetail.SocialSecurityPeopleID};";

                        accountNum += orderDetail.SocialSecurityAmount * orderDetail.SocialSecuritypayMonth + orderDetail.SocialSecurityFirstBacklogCost + orderDetail.SocialSecurityBuCha
                            + orderDetail.AccumulationFundAmount * orderDetail.AccumulationFundpayMonth + orderDetail.AccumulationFundFirstBacklogCost;
                        Bucha += orderDetail.SocialSecurityBuCha;
                        ZhiAccount += orderDetail.SocialSecurityFirstBacklogCost + orderDetail.AccumulationFundFirstBacklogCost;
                        ShouNote += (orderDetail.SocialSecurityAmount != 0 ? string.Format("{0}:{1}个月社保,单月保费:{2},社保待办费:{3},补差费:{4};", orderDetail.SocialSecurityPeopleName, orderDetail.SocialSecuritypayMonth, orderDetail.SocialSecurityAmount, orderDetail.SocialSecurityFirstBacklogCost, orderDetail.SocialSecurityBuCha) : string.Empty) + (orderDetail.AccumulationFundAmount != 0 ? string.Format("{0}:{1}个月公积金,单月公积金费:{2},公积金代办费:{3};", orderDetail.SocialSecurityPeopleName, orderDetail.AccumulationFundpayMonth, orderDetail.AccumulationFundAmount, orderDetail.AccumulationFundFirstBacklogCost) : string.Empty);

                        ZhiNote += (orderDetail.SocialSecurityFirstBacklogCost != 0 ? string.Format("{0}:社保待办费:{1}", orderDetail.SocialSecurityPeopleName, orderDetail.SocialSecurityFirstBacklogCost) : string.Empty) + (orderDetail.AccumulationFundFirstBacklogCost != 0 ? string.Format("{0}:公积金代办费:{1};", orderDetail.SocialSecurityPeopleName, orderDetail.AccumulationFundFirstBacklogCost) : string.Empty) + (orderDetail.SocialSecurityBuCha != 0 ? string.Format("{0}:补差费:{1}", orderDetail.SocialSecurityPeopleName, orderDetail.SocialSecurityBuCha) : string.Empty);

                        #region 作废
                        //sqlAccountRecord += $"insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime) values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000).ToString().PadLeft(3, '0')},{order.MemberID},{orderDetail.SocialSecurityPeopleID},'{orderDetail.SocialSecurityPeopleName}','收入','{model.PaymentMethod}','缴费',{accountNum},{memberAccount},getdate());";
                        //memberAccount -= orderDetail.SocialSecurityFirstBacklogCost;
                        //sqlAccountRecord += orderDetail.SocialSecurityFirstBacklogCost != 0 ? $"insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime) values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000).ToString().PadLeft(3, '0')},{order.MemberID},{orderDetail.SocialSecurityPeopleID},'{orderDetail.SocialSecurityPeopleName}','支出','余额','社保代办',{orderDetail.SocialSecurityFirstBacklogCost},{memberAccount},getdate());" : string.Empty;
                        //memberAccount -= orderDetail.AccumulationFundFirstBacklogCost;
                        //sqlAccountRecord += orderDetail.AccumulationFundFirstBacklogCost != 0 ? $"insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime) values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000).ToString().PadLeft(3, '0')},{order.MemberID},{orderDetail.SocialSecurityPeopleID},'{orderDetail.SocialSecurityPeopleName}','支出','余额','公积金代办',{orderDetail.AccumulationFundFirstBacklogCost},{memberAccount},getdate());" : string.Empty;
                        #endregion
                    }

                    sqlAccountRecord += $@"insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime)
values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{order.MemberID},'','','收入','{model.PaymentMethod}','{ShouNote}',{accountNum},{memberAccount + accountNum},getdate());
                                       insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime) 
values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{order.MemberID},'','','支出','余额','{ZhiNote}',{ZhiAccount},{memberAccount + accountNum - Bucha - ZhiAccount},getdate()); ";

                    //更新未参保人的支付状态
                    if (!string.IsNullOrEmpty(sqlSocialSecurityPeople))
                        DbHelper.ExecuteSqlCommand(sqlSocialSecurityPeople, null);

                    //计算出要进入个人账户的总额
                    decimal Account = 0;
                    orderDetailList.ForEach(n =>
                    {
                        Account += n.SocialSecurityAmount * n.SocialSecuritypayMonth + n.AccumulationFundAmount * n.AccumulationFundpayMonth;
                    });
                    //更新个人账户
                    string sqlMember = $"update Members set Account=ISNULL(Account,0)+{Account},Bucha=ISNULL(Bucha,0)+{Bucha} where MemberID={order.MemberID}";
                    int updateResult = DbHelper.ExecuteSqlCommand(sqlMember, null);
                    if (!(updateResult > 0)) throw new Exception("更新个人账户失败");

                    //更新记录
                    DbHelper.ExecuteSqlCommand(sqlAccountRecord, null);

                    //更新订单
                    string sqlUpdateOrder = $"update [Order] set Status = {(int)OrderEnum.Auditing},PaymentMethod='{model.PaymentMethod}',PayTime=getdate() where OrderCode={model.OrderCode}";
                    DbHelper.ExecuteSqlCommand(sqlUpdateOrder, null);

                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    //return new JsonResult<dynamic>
                    //{
                    //    status = false,
                    //    Message = "支付失败"
                    //};
                    throw new Exception(ex.Message);
                }
                finally
                {
                    transaction.Dispose();
                }
            }


            return new JsonResult<dynamic>
            {
                status = true,
                Message = "支付成功"
            };
        }

    }
}

