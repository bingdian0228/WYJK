using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 认证审核
    /// </summary>
    public class CertificationAudit
    {
        /// <summary>
        /// ID
        /// </summary>		
        public int ID { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>		
        public int MemberID { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string MemberName { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string MemberPhone { get; set; }
        /// <summary>
        /// 审核状态
        /// </summary>		
        public string Status { get; set; }
        /// <summary>
        /// 认证类型
        /// </summary>		
        public string UserType { get; set; }
        /// <summary>
        /// 申请时间
        /// </summary>		
        public DateTime ApplyDate { get; set; }
        /// <summary>
        /// 审核时间
        /// </summary>		
        public DateTime? AuditDate { get; set; }
        /// <summary>
        /// 企业名称
        /// </summary>		
        public string EnterpriseName { get; set; }
        /// <summary>
        /// 行业类型
        /// </summary>
        public string EnterpriseType { get; set; }
        /// <summary>
        /// 企业税号
        /// </summary>		
        public string EnterpriseTax { get; set; }
        /// <summary>
        /// 所在城市
        /// </summary>		
        public string EnterpriseArea { get; set; }
        /// <summary>
        /// 法人
        /// </summary>		
        public string EnterpriseLegal { get; set; }
        /// <summary>
        /// 法人身份证
        /// </summary>		
        public string EnterpriseLegalIdentityCardNo { get; set; }
        /// <summary>
        /// 企业人数
        /// </summary>		
        public string EnterprisePeopleNum { get; set; }
        /// <summary>
        /// 社会信用代码
        /// </summary>		
        public string SocialSecurityCreditCode { get; set; }
        /// <summary>
        /// 营业执照
        /// </summary>		
        public string EnterpriseBusinessLicense { get; set; }
        /// <summary>
        /// 营业执照名称
        /// </summary>		
        public string BusinessName { get; set; }
        /// <summary>
        /// 经营者姓名
        /// </summary>		
        public string BusinessUser { get; set; }
        /// <summary>
        /// 经营者身份证号
        /// </summary>		
        public string BusinessIdentityCardNo { get; set; }
        /// <summary>
        /// 经营者身份证
        /// </summary>		
        public string BusinessIdentityPhoto { get; set; }
        /// <summary>
        /// 经营者营业执照
        /// </summary>		
        public string BusinessLicensePhoto { get; set; }

    }

    /// <summary>
    /// 认证参数
    /// </summary>
    public class CertificationAuditParameter : PagedParameter
    {
        public string MemberName { get; set; }
        public string MemberPhone { get; set; }
        public string UserType { get; set; }
        public string Status { get; set; }
    }
}
