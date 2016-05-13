﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace WYJK.Entity
{
    /// <summary>
    /// 参保人
    /// </summary>
    public class SocialSecurityPeople
    {

        /// <summary>
        /// 参保人ID
        /// </summary>		
        public int SocialSecurityPeopleID { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public int MemberID { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>		
        public string SocialSecurityPeopleName { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>		
        public string IdentityCard { get; set; }
        /// <summary>
        /// 身份证照片
        /// </summary>		
        public string IdentityCardPhoto { get; set; }
        /// <summary>
        /// 户口性质
        /// </summary>		
        public string HouseholdProperty { get; set; }
        /// <summary>
        /// 是否缴纳社保
        /// </summary>		
        public bool IsPaySocialSecurity { get; set; }
        /// <summary>
        /// 是否缴纳公积金
        /// </summary>		
        public bool IsPayAccumulationFund { get; set; }

        /// <summary>
        /// 社保信息
        /// </summary>
        public SocialSecurity socialSecurity { get; set; }

        /// <summary>
        /// 公积金信息
        /// </summary>
        public AccumulationFund accumulationFund { get; set; }
        /// <summary>
        /// 客服审核状态
        /// </summary>
        public string Status { get; set; }

    }

    /// <summary>
    /// 未参保人列表信息显示内容(Mobile)
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class UnInsuredPeople
    {
        #region 参保人
        /// <summary>
        /// 参保人ID
        /// </summary>		
        public int SocialSecurityPeopleID { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>		
        public string SocialSecurityPeopleName { get; set; }
        /// <summary>
        /// 是否缴纳社保
        /// </summary>
        public bool IsPaySocialSecurity { get; set; }
        /// <summary>
        /// 是否缴纳公积金
        /// </summary>
        public bool IsPayAccumulationFund { get; set; }
        #endregion

        #region 社保信息
        /// <summary>
        /// 起缴时间
        /// </summary>		
        public DateTime SSPayTime { get; set; }
        /// <summary>
        /// 缴费月数
        /// </summary>		
        public int SSPayMonthCount { get; set; }
        /// <summary>
        /// 社保基数
        /// </summary>
        [JsonIgnore]
        public decimal SocialSecurityBase { get; set; }
        /// <summary>
        /// 社保缴费比例
        /// </summary>
        [JsonIgnore]
        public decimal SSPayProportion { get; set; }
        /// <summary>
        /// 社保状态
        /// </summary>
        public int? SSStatus { get; set; }
        #endregion

        #region 公积金信息
        /// <summary>
        /// 起缴时间
        /// </summary>		
        public DateTime AFPayTime { get; set; }
        /// <summary>
        /// 缴费月数
        /// </summary>		
        public int AFPayMonthCount { get; set; }
        /// <summary>
        /// 公积金基数
        /// </summary>
        [JsonIgnore]
        public decimal AccumulationFundBase { get; set; }
        /// <summary>
        /// 公积金缴费比例
        /// </summary>
        [JsonIgnore]
        public decimal AFPayProportion { get; set; }
        /// <summary>
        /// 公积金状态
        /// </summary>
        public int? AFStatus { get; set; }
        #endregion
        /// <summary>
        /// 社保费用
        /// </summary>
        public decimal SocialSecurityAmount { get; set; }

        /// <summary>
        /// 公积金费用
        /// </summary>
        public decimal AccumulationFundAmount { get; set; }
        /// <summary>
        /// 社保第一次代办费用
        /// </summary>
        public decimal socialSecurityFirstBacklogCost { get; set; }
        /// <summary>
        /// 公积金第一次代办费用
        /// </summary>
        public decimal AccumulationFundFirstBacklogCost { get; set; }


    }

    /// <summary>
    /// 参保人详情(Mobile)
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class SocialSecurityPeopleDetail
    {
        /// <summary>
        /// 是否缴纳社保
        /// </summary>		
        [JsonIgnore]
        public bool IsPaySocialSecurity { get; set; }
        /// <summary>
        /// 是否缴纳公积金
        /// </summary>		
        [JsonIgnore]
        public bool IsPayAccumulationFund { get; set; }
        /// <summary>
        /// 参保人ID
        /// </summary>		
        public int SocialSecurityPeopleID { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>		
        public string SocialSecurityPeopleName { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>		
        public string IdentityCard { get; set; }
        /// <summary>
        /// 身份证照片
        /// </summary>		
        public string IdentityCardPhoto { get; set; }
        /// <summary>
        /// 户口性质
        /// </summary>		
        public string HouseholdProperty { get; set; }
        /// <summary>
        /// 总额
        /// </summary>
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// 参保人列表（Mobile）
    /// </summary>
    public class SocialSecurityPeoples
    {
        #region 参保人
        /// <summary>
        /// 参保人ID
        /// </summary>		
        public int SocialSecurityPeopleID { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>		
        public string SocialSecurityPeopleName { get; set; }

        #endregion

        #region 社保信息
        /// <summary>
        /// 起缴时间
        /// </summary>		
        public DateTime? SSPayTime { get; set; }

        /// <summary>
        /// 已投月数
        /// </summary>
        public int? SSAlreadyPayMonthCount { get; set; }

        /// <summary>
        /// 剩余月数
        /// </summary>
        public int? SSRemainingMonthCount { get; set; }
        /// <summary>
        /// 社保状态
        /// </summary>
        public int? SSStatus { get; set; }
        #endregion

        #region 公积金信息
        /// <summary>
        /// 起缴时间
        /// </summary>		
        public DateTime? AFPayTime { get; set; }
        /// <summary>
        /// 已投月数
        /// </summary>
        public int? AFAlreadyPayMonthCount { get; set; }

        /// <summary>
        /// 剩余月数
        /// </summary>
        public int? AFRemainingMonthCount { get; set; }
        /// <summary>
        /// 社保状态
        /// </summary>
        public int? AFStatus { get; set; }
        #endregion

    }

    /// <summary>
    /// 申请停 (Mobile)
    /// </summary>
    public class ApplyTop
    {
        /// <summary>
        /// 参保人ID
        /// </summary>		
        public int SocialSecurityPeopleID { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>		
        public string SocialSecurityPeopleName { get; set; }
        /// <summary>
        /// 起缴时间
        /// </summary>		
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// 已投月数
        /// </summary>
        public int? AlreadyPayMonthCount { get; set; }

        /// <summary>
        /// 剩余月数
        /// </summary>
        public int? RemainingMonthCount { get; set; }
        /// <summary>
        /// 社保状态
        /// </summary>
        public int? Status { get; set; }
    }



    /// <summary>
    /// 停参保人列表（Mobile）
    /// </summary>
    public class TopSocialSecurityPeoples : SocialSecurityPeoples
    {
        /// <summary>
        /// 社保停方式
        /// </summary>
        public int SSStopMethod { get; set; }
        /// <summary>
        /// 社保停原因
        /// </summary>
        public string SSStopReason { get; set; }
        /// <summary>
        /// 社保申请停时间
        /// </summary>
        public DateTime? SSApplyStopDate { get; set; }
        /// <summary>
        /// 停保时间
        /// </summary>
        public DateTime? SSStopDate { get; set; }

        /// <summary>
        /// 公积金停方式
        /// </summary>
        public int AFStopMethod { get; set; }
        /// <summary>
        /// 公积金停原因
        /// </summary>
        public string AFStopReason { get; set; }
        /// <summary>
        /// 公积金申请停时间
        /// </summary>
        public DateTime? AFApplyStopDate { get; set; }
        /// <summary>
        /// 停公积金时间
        /// </summary>
        public DateTime? AFStopDate { get; set; }
    }

    /// <summary>
    /// 停社保参数 (Mobile)
    /// </summary>
    public class StopSocialSecurityParameter
    {
        /// <summary>
        /// 社保ID
        /// </summary>
        public int SocialSecurityPeopleID { get; set; }
        /// <summary>
        /// 停保原因
        /// </summary>
        public string StopReason { get; set; }
    }
    /// <summary>
    /// 停公积金参数 (Mobile)
    /// </summary>
    public class StopAccumulationFundParameter
    {
        /// <summary>
        /// 公积金ID
        /// </summary>
        public int SocialSecurityPeopleID { get; set; }

    }

    /// <summary>
    /// 社保计算器
    /// </summary>
    public class SocialSecurityCalculation
    {
        /// <summary>
        /// 社保金额
        /// </summary>
        public decimal SocialSecuritAmount { get; set; }
        /// <summary>
        /// 公积金金额
        /// </summary>
        public decimal AccumulationFundAmount { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal TotalAmount { get { return SocialSecuritAmount + AccumulationFundAmount; } set { } }
    }

}

