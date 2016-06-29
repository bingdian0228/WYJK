using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
namespace WYJK.Entity
{
    /// <summary>
    /// 提现申请表
    /// </summary>
    public class DrawCash
    {

        /// <summary>
        /// 主键ID
        /// </summary>		
        public int DrawCashID { get; set; }
        /// <summary>
        /// 注册人ID
        /// </summary>		
        public int MemberID { get; set; }

        /// <summary>
        /// 申请金额
        /// </summary>
        public decimal Money { get; set; }
        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime ApplyTime { get; set; }
        /// <summary>
        /// 申请状态
        /// </summary>		
        public int ApplyStatus { get; set; }
        /// <summary>
        /// 同意时间
        /// </summary>		
        public DateTime AgreeTime { get; set; }
        /// <summary>
        /// 打款订单
        /// </summary>		
        public string PaySN { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        public decimal LeftAccount { get; set; }

    }

    /// <summary>
    /// 提现显示模型类
    /// </summary>
    public class DrawCashViewModel
    {
        /// <summary>
        /// 主键ID
        /// </summary>		
        public int DrawCashID { get; set; }
        /// <summary>
        /// 注册人ID
        /// </summary>		
        public int MemberID { get; set; }

        /// <summary>
        /// 申请金额
        /// </summary>
        public decimal Money { get; set; }
        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime ApplyTime { get; set; }
        /// <summary>
        /// 申请状态
        /// </summary>		
        public int ApplyStatus { get; set; }
        /// <summary>
        /// 同意时间
        /// </summary>		
        public DateTime AgreeTime { get; set; }
        /// <summary>
        /// 打款订单
        /// </summary>		
        public string PaySN { get; set; }

        /// <summary>
        /// 注册人名称
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        /// 欠款金额
        /// </summary>
        public decimal ArrearAmount { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        public decimal Account { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        public string BankCardNo { get; set; }

        /// <summary>
        /// 开户行
        /// </summary>
        public string BankAccount { get; set; }
    }

    /// <summary>
    /// 提现分页查询类
    /// </summary>
    public class DrawCashParameter : PagedParameter
    {
        /// <summary>
        /// 客户姓名
        /// </summary>
        public string SocialSecurityPeopleName { get; set; }


        /// <summary>
        /// 审核状态
        /// </summary>
        public int Status { get; set; }

    }




}

