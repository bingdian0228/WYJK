using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 缴费明细
    /// </summary>
    public class PaymentDetail
    {
        /// <summary>
		/// ID
        /// </summary>		
        public int ID { get; set; }
        /// <summary>
        /// 人员编号
        /// </summary>		
        public string PersonnelNumber { get; set; }
        /// <summary>
        /// 身份证
        /// </summary>		
        public string IdentityCard { get; set; }
        /// <summary>
        /// TrueName
        /// </summary>		
        public string TrueName { get; set; }
        /// <summary>
        /// 缴费年月
        /// </summary>		
        public string PayTime { get; set; }
        /// <summary>
        /// 业务年月
        /// </summary>		
        public string BusinessTime { get; set; }
        /// <summary>
        /// 缴费类型
        /// </summary>		
        public string PaymentType { get; set; }
        /// <summary>
        /// 缴费基数
        /// </summary>		
        public int SocialInsuranceBase { get; set; }
        /// <summary>
        /// 个人缴费
        /// </summary>		
        public decimal PersonalExpenses { get; set; }
        /// <summary>
        /// 单位缴费
        /// </summary>		
        public decimal CompanyExpenses { get; set; }
        /// <summary>
        /// 缴费标志
        /// </summary>		
        public string PaymentMark { get; set; }
        /// <summary>
        /// 单位编号
        /// </summary>		
        public string CompanyNumber { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>		
        public string CompanyName { get; set; }
        /// <summary>
        /// 结算方式
        /// </summary>		
        public string SettlementMethod { get; set; }

        /// <summary>
        /// 社保类型
        /// </summary>
        public string SocialSecurityType { get; set; }

        /// <summary>
        /// 当年只能导入一份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 明细
        /// </summary>
        public string PaymentDetails { get; set; }

        /// <summary>
        /// 合计
        /// </summary>
        public decimal TotalCount { get; set; }
    }


    /// <summary>
    /// 企业缴费明细
    /// </summary>
    public class PaymentDetailsParameter : PagedParameter
    {
        public override int PageSize { set; get; } = 1000;
        public string IdentityCard { get; set; }
        public string CompanyName { get; set; }
        public string Year { get; set; }
    }
}
