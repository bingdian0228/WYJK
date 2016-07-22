using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IService
{
    /// <summary>
    /// 订单接口
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// 生成订单
        /// </summary>
        /// <param name="SocialSecurityPeopleIDStr"></param>
        /// <param name="MemberID"></param>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        Dictionary<bool, string> GenerateOrder(string SocialSecurityPeopleIDStr, int MemberID, string orderCode, decimal Balance = 0);

        /// <summary>
        /// 获取订单列表(Mobile)
        /// </summary>
        /// <param name="MemberID"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        List<OrderListForMobile> GetOrderList(int MemberID, int Status);
        /// <summary>
        /// 获取订单详情(Mobile)
        /// </summary>
        /// <param name="MemberID"></param>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        OrderDetailForMobile GetOrderDetail(int MemberID, string OrderCode);

        /// <summary>
        /// 获取待财务审核订单列表（PC）
        /// </summary>
        /// <returns></returns>
        PagedResult<FinanceOrder> GetFinanceOrderList(FinanceOrderParameter parameter);

        /// <summary>
        /// 获取订单总金额
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        decimal GetFinanceOrderAmount(FinanceOrderParameter parameter);

        /// <summary>
        /// 修改订单状态(Admin)
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        bool ModifyOrderStatus(string OrderCode);

        /// <summary>
        /// 修改订单对应的参保人的社保和公积金状态(Admin)
        /// </summary>
        /// <returns></returns>
        bool ModifySocialSecurityPeopleForOrder(int Status, string OrderCodeStr);

        /// <summary>
        /// 获取子订单列表(Admin)
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        List<FinanceSubOrder> GetSubOrderList(string OrderCode);

        /// <summary>
        /// 取消订单(Mobile)
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        bool CancelOrder(string OrderCode);

        /// <summary>
        /// 根据订单号查询主订单实体(Admin)
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        Order GetOrder(string OrderCode);

        /// <summary>
        /// 根据参保人ID获取订单列表
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        List<Order> GetOrderList(int SocialSecurityPeopleID);

        /// <summary>
        /// 某用户下是否存在未支付订单
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        bool IsExistsWaitingPayOrderByMemberID(int MemberID);
    }
}
