using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Data.ServiceImpl
{
    public class OrderService : IOrderService
    {
        /// <summary>
        /// 生成订单
        /// </summary>
        /// <param name="SocialSecurityPeopleIDStr"></param>
        /// <param name="MemberID"></param>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public Dictionary<bool, string> GenerateOrder(string SocialSecurityPeopleIDStr, int MemberID, string orderCode, decimal Balance)
        {
            Dictionary<bool, string> dic = new Dictionary<bool, string>();
            DbParameter[] parameters = new DbParameter[] {
                new SqlParameter("@Flag",SqlDbType.Bit) {Direction = ParameterDirection.Output },
                new SqlParameter("@OrderCode",orderCode) {Direction = ParameterDirection.InputOutput },
                new SqlParameter("@MemberID",MemberID),
                new SqlParameter("@SocialSecurityPeopleIDS",SocialSecurityPeopleIDStr),
                new SqlParameter("@Balance",Balance)
            };

            DbHelper.ExecuteSqlCommand("Order_Generate", parameters, CommandType.StoredProcedure);
            dic.Add((bool)parameters[0].Value, (string)parameters[1].Value);
            return dic;
        }


        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <param name="MemberID"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        public List<OrderListForMobile> GetOrderList(int MemberID, int Status)
        {
            string sql = "select [Order].OrderCode,dbo.OrderPeopleName_add([order].OrderCode) Names , SUM(OrderDetails.SocialSecurityAmount * OrderDetails.SocialSecuritypayMonth + OrderDetails.SocialSecurityServiceCost + OrderDetails.SocialSecurityFirstBacklogCost + OrderDetails.SocialSecurityBuCha + OrderDetails.AccumulationFundAmount * OrderDetails.AccumulationFundpayMonth + OrderDetails.AccumulationFundServiceCost + OrderDetails.AccumulationFundFirstBacklogCost) Amounts "
                         + " from[Order] right join OrderDetails on[Order].OrderCode = OrderDetails.OrderCode"
                         + " where[Order].MemberID = @MemberID and[Order].Status = @Status"
                         + " group by[order].OrderCode";

            List<OrderListForMobile> list = DbHelper.Query<OrderListForMobile>(sql, new
            {
                MemberID = MemberID,
                Status = Status
            });

            return list;
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="MemberID"></param>
        /// <param name="Status"></param>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public OrderDetailForMobile GetOrderDetail(int MemberID, string OrderCode)
        {
            string sql = "declare @Balance decimal(18,2), @OrderID int, @OrderCode nvarchar(50),@Names nvarchar(50), @SocialSecurityTotalAmount decimal(18, 2) = 0, @AccumulationFundTotalAmount decimal(18, 2) = 0, @ServiceCost decimal(18, 2) = 0, @FirstBacklogCost decimal(18, 2) = 0, @PaymentMethod nvarchar(50),@SocialSecurityBuCha decimal(18,2)=0"
                        + " select @Balance=ISNULL([Order].Balance,0), @OrderID=[Order].OrderID, @OrderCode =[Order].OrderCode, @Names = ISNULL(@Names + '，', '') + OrderDetails.SocialSecurityPeopleName,"
                        + " @SocialSecurityTotalAmount += OrderDetails.SocialSecurityAmount * OrderDetails.SocialSecuritypayMonth,"
                        + " @AccumulationFundTotalAmount += OrderDetails.AccumulationFundAmount * OrderDetails.AccumulationFundpayMonth,"
                        + " @ServiceCost += OrderDetails.SocialSecurityServiceCost + OrderDetails.AccumulationFundServiceCost,"
                        + " @FirstBacklogCost += OrderDetails.SocialSecurityFirstBacklogCost + OrderDetails.AccumulationFundFirstBacklogCost,"
                        + " @SocialSecurityBuCha += OrderDetails.SocialSecurityBuCha"
                        + " from[Order] right join OrderDetails on[Order].OrderCode = OrderDetails.OrderCode where [Order].MemberID = @MemberID and [order].OrderCode = @OrderCode1"
                        + " select @Balance Balance,  @SocialSecurityBuCha SocialSecurityBuCha,@OrderID OrderID,  @OrderCode OrderCode, @Names Names, @SocialSecurityTotalAmount SocialSecurityTotalAmount, @AccumulationFundTotalAmount AccumulationFundTotalAmount, @ServiceCost ServiceCost, @FirstBacklogCost FirstBacklogCost, ISNULL(@PaymentMethod, '')  PaymentMethod";

            OrderDetailForMobile model = DbHelper.QuerySingle<OrderDetailForMobile>(sql, new
            {
                MemberID = MemberID,
                OrderCode1 = OrderCode
            });

            return model;
        }

        /// <summary>
        /// 获取财务审核订单列表(Admin)
        /// </summary>
        /// <returns></returns>
        public PagedResult<FinanceOrder> GetFinanceOrderList(FinanceOrderParameter parameter)
        {
            StringBuilder builder = new StringBuilder(" where 1 = 1");
            if (!string.IsNullOrEmpty(parameter.MemberID))
            {
                builder.AppendFormat(" and Members.MemberID = {0}", parameter.MemberID);
            }

            if (!string.IsNullOrEmpty(parameter.PaymentMethod))
            {
                builder.AppendFormat($" and orders.PaymentMethod='{parameter.PaymentMethod}'");
            }
            if (!string.IsNullOrEmpty(parameter.OrderCode))
            {
                parameter.OrderCode = parameter.OrderCode.Replace("'", "''");
                builder.Append($" and OrderCode like '%{parameter.OrderCode}%'");
            }


            if (!string.IsNullOrEmpty(parameter.Status))
            {
                builder.Append($" and orders.Status = {parameter.Status}");
            }
            else {
                builder.Append($" and orders.Status in(1,2)");
            }

            if (string.IsNullOrEmpty(parameter.StartTime.ToString()) && string.IsNullOrEmpty(parameter.EndTime.ToString()))
            {
                builder.Append("and 1 = 1 ");
            }
            else if (string.IsNullOrEmpty(parameter.StartTime.ToString()) && !string.IsNullOrEmpty(parameter.EndTime.ToString()))
            {
                builder.Append($"and PayTime < '{ parameter.EndTime.Value.AddDays(1)}' ");
            }
            else if (!string.IsNullOrEmpty(parameter.StartTime.ToString()) && string.IsNullOrEmpty(parameter.EndTime.ToString()))
            {
                builder.Append($"and PayTime > '{parameter.StartTime}'");
            }
            else {
                builder.Append($"and PayTime between '{parameter.StartTime}' and '{parameter.EndTime.Value.AddDays(1)}'");
            }

            string innerSql = "select Orders.PayTime,orders.OrderCode,members.UserType,members.MemberID,members.MemberName,members.EnterpriseName,members.BusinessName,"
                            + " (select COUNT(*) from OrderDetails where OrderDetails.OrderCode = Orders.OrderCode) payUserCount,"
                            + " (select left((select SocialSecurityPeopleName+','  from OrderDetails  where OrderCode=Orders.OrderCode for xml path('')),LEN((select SocialSecurityPeopleName+','  from OrderDetails  where OrderCode=Orders.OrderCode for xml path('')))-1))  payUserName,"
                            + " orders.PaymentMethod,"
                            + " (select  SUM(OrderDetails.SocialSecurityAmount * OrderDetails.SocialSecuritypayMonth + OrderDetails.SocialSecurityServiceCost + OrderDetails.SocialSecurityFirstBacklogCost + OrderDetails.SocialSecurityBuCha + OrderDetails.AccumulationFundAmount * OrderDetails.AccumulationFundpayMonth + OrderDetails.AccumulationFundServiceCost + OrderDetails.AccumulationFundFirstBacklogCost)  from OrderDetails where OrderDetails.OrderCode = orders.OrderCode) Amounts,"
                            + " orders.Status"
                            + " from[Order] orders"
                            + " left join Members on orders.MemberID = Members.MemberID"
                            + $"  {builder.ToString()}";

            string sql = $"select * from (select ROW_NUMBER() OVER(ORDER BY t.PayTime desc )AS Row,t.* from"
                            + $" ({innerSql}) t) tt"
                            + " WHERE tt.Row BETWEEN @StartIndex AND @EndIndex";

            List<FinanceOrder> orderList = DbHelper.Query<FinanceOrder>(sql, new
            {
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });

            int totalCount = DbHelper.QuerySingle<int>($"select count(0) as TotalCount from ({innerSql}) t");

            return new PagedResult<FinanceOrder>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = orderList
            };
        }

        /// <summary>
        /// 获取订单查询总金额
        /// </summary>
        /// <returns></returns>
        public decimal GetFinanceOrderAmount(FinanceOrderParameter parameter)
        {
            StringBuilder builder = new StringBuilder(" where 1 = 1");
            if (!string.IsNullOrEmpty(parameter.MemberID))
            {
                builder.AppendFormat(" and Members.MemberID = {0}", parameter.MemberID);
            }

            if (!string.IsNullOrEmpty(parameter.PaymentMethod))
            {
                builder.AppendFormat($" and orders.PaymentMethod='{parameter.PaymentMethod}'");
            }

            builder.Append($" and OrderCode like '%{parameter.OrderCode}%'");

            if (!string.IsNullOrEmpty(parameter.Status))
            {
                builder.Append($" and orders.Status = {parameter.Status}");
            }
            else {
                builder.Append($" and orders.Status in(1,2)");
            }

            if (string.IsNullOrEmpty(parameter.StartTime.ToString()) && string.IsNullOrEmpty(parameter.EndTime.ToString()))
            {
                builder.Append("and 1 = 1 ");
            }
            else if (string.IsNullOrEmpty(parameter.StartTime.ToString()) && !string.IsNullOrEmpty(parameter.EndTime.ToString()))
            {
                builder.Append($"and PayTime < '{ parameter.EndTime.Value.AddDays(1)}' ");
            }
            else if (!string.IsNullOrEmpty(parameter.StartTime.ToString()) && string.IsNullOrEmpty(parameter.EndTime.ToString()))
            {
                builder.Append($"and PayTime > '{parameter.StartTime}'");
            }
            else {
                builder.Append($"and PayTime between '{parameter.StartTime}' and '{parameter.EndTime.Value.AddDays(1)}'");
            }

            string innerSql = "select Orders.PayTime,orders.OrderCode,members.UserType,members.MemberID,members.MemberName,members.EnterpriseName,members.BusinessName,"
                            + " (select COUNT(*) from OrderDetails where OrderDetails.OrderCode = Orders.OrderCode) payUserCount,"
                            + " (select left((select SocialSecurityPeopleName+','  from OrderDetails  where OrderCode=Orders.OrderCode for xml path('')),LEN((select SocialSecurityPeopleName+','  from OrderDetails  where OrderCode=Orders.OrderCode for xml path('')))-1))  payUserName,"
                            + " orders.PaymentMethod,"
                            + " (select  SUM(OrderDetails.SocialSecurityAmount * OrderDetails.SocialSecuritypayMonth + OrderDetails.SocialSecurityServiceCost + OrderDetails.SocialSecurityFirstBacklogCost + OrderDetails.SocialSecurityBuCha + OrderDetails.AccumulationFundAmount * OrderDetails.AccumulationFundpayMonth + OrderDetails.AccumulationFundServiceCost + OrderDetails.AccumulationFundFirstBacklogCost)  from OrderDetails where OrderDetails.OrderCode = orders.OrderCode) Amounts,"
                            + " orders.Status"
                            + " from[Order] orders"
                            + " left join Members on orders.MemberID = Members.MemberID"
                            + $"  {builder.ToString()}";
            
            decimal totalAmount = DbHelper.QuerySingle<decimal>($"select ISNULL(sum(Amounts),0) as TotalCount from ({innerSql}) t");

            return totalAmount;
        }

        /// <summary>
        /// 修改订单状态
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public bool ModifyOrderStatus(string OrderCode)
        {
            string sql = $"update [Order] set Status={(int)OrderEnum.completed} where OrderCode in('{OrderCode}')";
            int result = DbHelper.ExecuteSqlCommand(sql, null);
            return result > 0;
        }

        /// <summary>
        /// 修改订单对应的参保人的社保和公积金状态
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="OrderStr"></param>
        /// <returns></returns>
        public bool ModifySocialSecurityPeopleForOrder(int Status, string OrderCodeStr)
        {
            string sql = $"update SocialSecurity set Status={Status} where SocialSecurityPeopleID in(select distinct SocialSecurityPeopleID from dbo.OrderDetails where OrderCode in('{OrderCodeStr}'));"
                + $" update AccumulationFund set Status={Status} where SocialSecurityPeopleID in(select distinct SocialSecurityPeopleID from dbo.OrderDetails where OrderCode in('{OrderCodeStr}'))";
            int result = DbHelper.ExecuteSqlCommand(sql, null);
            return result > 0;
        }

        /// <summary>
        /// 获取子订单列表
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public List<FinanceSubOrder> GetSubOrderList(string OrderCode)
        {
            string sql = $"select OrderDetails.OrderCode,[Order].Status, OrderDetails.SocialSecurityPeopleName ,SocialSecurityPeople.HouseholdProperty,"
                        + " CONVERT(varchar(7), OrderDetails.SSPayTime, 120) ssStartTime,"
                        + " CONVERT(varchar(7), DATEADD(MONTH, OrderDetails.SocialSecurityAmount, OrderDetails.SSPayTime), 120)  ssEndTime,"
                        //+ " SocialSecurity.SocialSecurityBase,"
                        + " OrderDetails.SocialSecurityAmount* orderdetails.SocialSecuritypayMonth ssAmount,"
                        + " OrderDetails.SocialSecurityServiceCost,"
                        + " OrderDetails.SocialSecurityFirstBacklogCost,"
                        + " orderdetails.SocialSecurityBuCha,"
                        + " CONVERT(varchar(7), OrderDetails.AFPayTime, 120) afStartTime,"
                        + " CONVERT(varchar(7), DATEADD(MONTH, OrderDetails.AccumulationFundAmount, OrderDetails.AFPayTime), 120)  afEndTime,"
                        + " orderdetails.AccumulationFundAmount* OrderDetails.AccumulationFundpayMonth afAmount,"
                        + " OrderDetails.AccumulationFundServiceCost,"
                        + " OrderDetails.AccumulationFundFirstBacklogCost,"
                        + " OrderDetails.SocialSecurityAmount* orderdetails.SocialSecuritypayMonth +"
                        + " OrderDetails.SocialSecurityServiceCost +"
                        + " OrderDetails.SocialSecurityFirstBacklogCost +"
                        + " orderdetails.SocialSecurityBuCha +"
                        + " orderdetails.AccumulationFundAmount * OrderDetails.AccumulationFundpayMonth +"
                        + " OrderDetails.AccumulationFundServiceCost +"
                        + " OrderDetails.AccumulationFundFirstBacklogCost totalAmount"
                        + " from OrderDetails"
                        + " left join [Order] on [Order].OrderCode = OrderDetails.OrderCode "
                        + " left join SocialSecurityPeople on OrderDetails.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID"
                        + " left join SocialSecurity on OrderDetails.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID"
                        + " left join AccumulationFund on OrderDetails.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID"
                        + $" where OrderDetails.OrderCode = '{OrderCode}'";

            List<FinanceSubOrder> subOrderList = DbHelper.Query<FinanceSubOrder>(sql);
            return subOrderList;
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public bool CancelOrder(string OrderCode)
        {
            DbHelper.ExecuteSqlCommand($@"update SocialSecurity set IsGenerateOrder=0 
  where SocialSecurityPeopleID in(select SocialSecurityPeopleID from OrderDetails where OrderCode in('{OrderCode}') and IsPaySocialSecurity = 1);
 update AccumulationFund set IsGenerateOrder=0 
  where SocialSecurityPeopleID in(select SocialSecurityPeopleID from OrderDetails where OrderCode in('{OrderCode}') and IsPayAccumulationFund = 1)", null);

            string sql = $"delete from [Order] where OrderCode in('{OrderCode}')";
            int result = DbHelper.ExecuteSqlCommand(sql, null);
            return result > 0;
        }

        /// <summary>
        /// 根据订单号查询主订单实体(Admin)
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public Order GetOrder(string OrderCode)
        {
            string sqlstr = $"select * from [Order] where OrderCode = '{OrderCode}'";
            Order model = DbHelper.QuerySingle<Order>(sqlstr);
            return model;
        }

        /// <summary>
        /// 根据参保人ID获取订单列表
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        public List<Order> GetOrderList(int SocialSecurityPeopleID)
        {
            string sqlstr = "select [order].ordercode,[order].status from OrderDetails"
                            + " left join[order] on OrderDetails.OrderCode = [Order].OrderCode"
                            + $" where SocialSecurityPeopleID = {SocialSecurityPeopleID}";
            List<Order> orderList = DbHelper.Query<Order>(sqlstr);
            return orderList;
        }

        /// <summary>
        /// 用户下是否存在未支付订单
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public bool IsExistsWaitingPayOrderByMemberID(int MemberID)
        {
            string sqlstr = $"select count(*) from [Order] where MemberID = {MemberID} and Status ={(int)OrderEnum.WaitingPay}";
            int result = DbHelper.QuerySingle<int>(sqlstr);
            return result > 0;
        }
    }
}
