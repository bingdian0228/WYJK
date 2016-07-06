using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WYJK.Entity;

namespace WYJK.HOME.Models
{
    public class InsuranceQueryParamModel : PagedParameter
    {
        public string SocialSecurityPeopleName { get; set; }
        public string HouseholdProperty { get; set; }
        public string InsuranceArea { get; set; }
    }

    public class InsuranceListViewModel
    {

        public string SocialSecurityPeopleID { get; set; }

        public string SocialSecurityPeopleName { get; set; }
        public string IdentityCard { get; set; }
        public string HouseholdProperty { get; set; }
        public string PayTime { get; set; }
        public string StopDate { get; set; }
        public string SocialSecurityBase { get; set; }
        public string SocialSecurityStatus { get; set; }
        public string SocialSecurityAmount { get; set; }
        public string AccumulationFundBase { get; set; }
        public string AccumulationFundStatus { get; set; }

    }

    public class InsuranceAdd1ViewModel
    {
        /// <summary>
        /// 姓名
        /// </summary>		
        [Required]
        [Display(Name = "真实姓名")]
        public string SocialSecurityPeopleName { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>		
        [Required]
        [MinLength(18)]
        [Display(Name = "身份证号")]
        public string IdentityCard { get; set; }
        /// <summary>
        /// 身份证照片
        /// </summary>
        [Display(Name = "身份证照片")]
        public string IdentityCardPhoto { get; set; }
        /// <summary>
        /// 身份证照片数组
        /// </summary>
       
        [Display(Name = "身份证照片数组")]
        public string[] ImgUrls { get; set; }
        /// <summary>
        /// 户口性质
        /// </summary>		
        [Required]
        [Display(Name = "户口性质")]
        public string HouseholdProperty { get; set; }
    }


    public class InsuranceAdd2ViewModel
    {
        [Display(Name = "参保地")]
        public string InsuranceArea { get; set; }

        [Required]
        [Display(Name = "社保基数")]
        public decimal SocialSecurityBase { get; set; }

        [Required]
        [Display(Name = "起缴时间")]
        public DateTime PayTime { get; set; }

        [Required]
        [Display(Name = "参保月份")]
        public int PayMonthCount { get; set; }

        [Required]
        [Display(Name = "补交月份")]
        public int PayBeforeMonthCount { get; set; }

        [Required]
        [Display(Name = " 在企业缴纳")]
        public int BankPayMonth { get; set; }

        [Required]
        [Display(Name = "在银行缴纳")]
        public int EnterprisePayMonth { get; set; }
        
        [Display(Name = "备注")]
        public string Note { get; set; }
         
    }


    public class InsuranceAdd3ViewModel
    {
        [Display(Name = "参保地")]
        public string AccumulationFundArea { get; set; }

        [Required]
        [Display(Name = "公积金基数")]
        public decimal AccumulationFundBase { get; set; }

        [Required]
        [Display(Name = "起缴时间")]
        public DateTime PayTime { get; set; }

        [Required]
        [Display(Name = "参保月份")]
        public int PayMonthCount { get; set; }

        [Required]
        [Display(Name = "补交月份")]
        public int PayBeforeMonthCount { get; set; }

        [Required]
        [Display(Name = " 在企业缴纳")]
        public int BankPayMonth { get; set; }

        [Required]
        [Display(Name = "在银行缴纳")]
        public int EnterprisePayMonth { get; set; }

        [Display(Name = "备注")]
        public string Note { get; set; }

    }

}