using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 客户服务实体
    /// </summary>
    public class CustomerServiceViewModel
    {
        /// <summary>
        /// 用户ID
        /// </summary>

        public int MemberID { get; set; }
        /// <summary>
        /// 用户类型
        /// </summary>
        public string UserType { get; set; }

        /// <summary>
        /// 代理机构
        /// </summary>
        public string MemberName { get; set; }

        public string EnterpriseName { get; set; }

        public string BusinessName { get; set; }

        /// <summary>
        /// 用户电话
        /// </summary>
        public string MemberPhone { get; set; }

        /// <summary>
        /// 个人账户
        /// </summary>
        public decimal Account { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string OrderStatus { get; set; }

        /// <summary>
        /// 参保人ID
        /// </summary>
        public string SocialSecurityPeopleID { get; set; }

        /// <summary>
        /// 客服状态  0:未审核，1：已通过
        /// </summary>
        public string CustomerServiceStatus { get; set; }

        /// <summary>
        /// 客户姓名
        /// </summary>
        public string SocialSecurityPeopleName { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        public string IdentityCard { get; set; }

        /// <summary>
        /// 社保状态
        /// </summary>
        public string SSstatus { get; set; }

        /// <summary>
        /// 社保异常备注
        /// </summary>
        public string SocialSecurityException { get; set; }

        /// <summary>
        /// 公积金状态
        /// </summary>
        public string AFStatus { get; set; }

        /// <summary>
        /// 公积金异常备注
        /// </summary>
        public string AccumulationFundException { get; set; }

        /// <summary>
        /// 欠费金额
        /// </summary>
        public decimal ArrearAmount { get; set; }

    }

    /// <summary>
    /// 客户服务参数
    /// </summary>
    public class CustomerServiceParameter : PagedParameter
    {
        /// <summary>
        /// 用户类型
        /// </summary>
        public string UserType { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string MemberID { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string SocialSecurityPeopleName { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        public string IdentityCard { get; set; }

        /// <summary>
        /// 客服审核状态
        /// </summary>
        public string CustomerServiceStatus { get; set; }
    }
}
