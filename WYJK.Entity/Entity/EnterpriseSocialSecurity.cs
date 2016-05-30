using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 企业社保
    /// </summary>
    public class EnterpriseSocialSecurity
    {

        /// <summary>
        /// 企业ID
        /// </summary>		
        public int EnterpriseID { get; set; }
        /// <summary>
        /// 企业名称
        /// </summary>		
        [Required(ErrorMessage = "企业名称必填")]
        public string EnterpriseName { get; set; }
        /// <summary>
        /// 区域
        /// </summary>		
        [Required]
        public string EnterpriseArea { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        public string ProvinceCode { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string CityCode { get; set; }
        /// <summary>
        /// 区
        /// </summary>
        public string CountyCode { get; set; }

        /// <summary>
        /// 联系人
        /// </summary>
        [Required(ErrorMessage = "联系人必填")]
        public string ContactUser { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [Required(ErrorMessage = "联系电话必填")]
        public string ContactTel { get; set; }

        /// <summary>
        /// 传真
        /// </summary>
        [Required(ErrorMessage = "传真必填")]
        public string Fax { get; set; }

        /// <summary>
        /// 电子邮件
        /// </summary>
        [Required(ErrorMessage = "电子邮件必填")]
        public string Email { get; set; }

        /// <summary>
        /// 办公电话
        /// </summary>
        [Required(ErrorMessage = "办公电话必填")]
        public string OfficeTel { get; set; }

        ///// <summary>
        ///// 户口类型
        ///// </summary>
        //[Required(ErrorMessage ="户口类型必填")]
        //public string HouseholdProperty { get; set; }

        /// <summary>
        /// 机构地址
        /// </summary>
        [Required(ErrorMessage = "机构地址必填")]
        public string OrgAddress { get; set; }
        /// <summary>
        /// 社平工资
        /// </summary>		
        [Required(ErrorMessage = "社平工资必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal SocialAvgSalary { get; set; }
        /// <summary>
        /// 最低社保百分比
        /// </summary>		
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        [Required(ErrorMessage = "最低社保必填")]
        public decimal MinSocial { get; set; }
        /// <summary>
        /// 最高社保百分比
        /// </summary>		
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        [Required(ErrorMessage = "最高社保必填")]
        public decimal MaxSocial { get; set; }
        /// <summary>
        /// 单位养老百分比
        /// </summary>		
        [Required(ErrorMessage = "单位养老百分比必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal CompYangLao { get; set; }
        /// <summary>
        /// 单位医疗百分比
        /// </summary>		
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        [Required(ErrorMessage = "单位医疗百分比必填")]
        public decimal CompYiLiao { get; set; }
        /// <summary>
        /// 单位失业百分比
        /// </summary>		
        [Required(ErrorMessage = "单位失业百分比必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal CompShiYe { get; set; }
        /// <summary>
        /// 单位工伤百分比
        /// </summary>		
        [Required(ErrorMessage = "单位工伤百分比必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal CompGongShang { get; set; }
        /// <summary>
        /// 单位生育百分比
        /// </summary>		
        [Required(ErrorMessage = "单位生育百分比必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal CompShengYu { get; set; }
        /// <summary>
        /// 个人养老百分比
        /// </summary>		
        [Required(ErrorMessage = "个人养老百分比必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal PersonalYangLao { get; set; }
        /// <summary>
        /// 个人医疗百分比
        /// </summary>		
        [Required(ErrorMessage = "个人医疗百分比必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal PersonalYiLiao { get; set; }
        /// <summary>
        /// 个人失业（城镇）百分比
        /// </summary>		
        [Required(ErrorMessage = "个人失业（城镇）百分比必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal PersonalShiYeTown { get; set; }
        /// <summary>
        /// 个人失业（农村）百分比
        /// </summary>		
        [Required(ErrorMessage = "个人失业（农村）百分比必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal PersonalShiYeRural { get; set; }
        /// <summary>
        /// 个人工伤百分比
        /// </summary>		
        [Required(ErrorMessage = "个人工伤百分比必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal PersonalGongShang { get; set; }
        /// <summary>
        /// 个人生育百分比
        /// </summary>		
        [Required(ErrorMessage = "个人生育百分比必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal PersonalShengYu { get; set; }
        /// <summary>
        /// 公积金基数范围最小值
        /// </summary>		
        [Required(ErrorMessage = "公积金基数范围最小值必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal MinAccumulationFund { get; set; }
        /// <summary>
        /// 公积金基数范围最大值
        /// </summary>		
        [Required(ErrorMessage = "公积金基数范围最大值必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal MaxAccumulationFund { get; set; }
        /// <summary>
        /// 公积金单位比例
        /// </summary>		
        [Required(ErrorMessage = "公积金单位比例必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal CompProportion { get; set; }
        /// <summary>
        /// 公积金个人比例
        /// </summary>		
        [Required(ErrorMessage = "公积金个人比例必填")]
        [RegularExpression(@"^[0-9]+(.[0-9]+)?$", ErrorMessage = "数字不符合规则")]
        public decimal PersonalProportion { get; set; }
        /// <summary>
        /// 是否默认
        /// </summary>		
        [Required]
        public bool IsDefault { get; set; }

        /// <summary>
        /// 调整时间
        /// </summary>
        public DateTime? AdjustDt { get; set; }

    }

    /// <summary>
    /// 签约企业参数
    /// </summary>
    public class EnterpriseSocialSecurityParameter : PagedParameter
    {
        /// <summary>
        /// 签约企业名称
        /// </summary>
        public string EnterpriseName { get; set; }
    }

    /// <summary>
    /// 城市社平
    /// </summary>
    public class EnterpriseCity
    {
        /// <summary>
        /// 参保城市
        /// </summary>
        public string City { get; set; }
    }
}
