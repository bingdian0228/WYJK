using System;
using System.Collections.Generic;

namespace WYJK.Framework.EnumHelper
{
    /// <summary>
    /// 户籍性质
    /// </summary>
    public enum HouseholdPropertyEnum
    {
        //[EnumDisplayName("本市农村")]
        //InRural = 1,

        //[EnumDisplayName("本市城镇")]
        //InTown = 2,

        //[EnumDisplayName("外市农村")]
        //OutRural = 3,

        //[EnumDisplayName("外市城镇")]
        //OutTown = 4

        [EnumDisplayName("本市农村")]
        ThisCityRural = 1,

        [EnumDisplayName("本市城镇")]
        ThisCityTown = 2,

        [EnumDisplayName("本省农村")]
        ThisProvinceRural = 3,

        [EnumDisplayName("本省城镇")]
        ThisProvinceTown = 4,

        [EnumDisplayName("外省农村")]
        OtherProvinceRural = 5,

        [EnumDisplayName("外省城镇")]
        OtherProvinceTown = 6
    }

    /// <summary>
    /// 参保状态
    /// </summary>
    public enum SocialSecurityStatusEnum
    {
        [EnumDisplayName("未参保")]
        UnInsured = 1,
        [EnumDisplayName("待办")]
        WaitingHandle = 2,
        [EnumDisplayName("正常")]
        Normal = 3,
        [EnumDisplayName("续费")]
        Renew = 4,
        [EnumDisplayName("待停")]
        WaitingStop = 5,
        [EnumDisplayName("已停")]
        AlreadyStop = 6
    }

    ///// <summary>
    ///// 参公积金状态
    ///// </summary>
    //public enum AccumulationFundEnum
    //{
    //    [EnumDisplayName("未参公积金")]
    //    UnAccumulationFund = 1,
    //    [EnumDisplayName("正常")]
    //    NormalAccumulationFund = 2

    //}

    /// <summary>
    /// 订单状态
    /// </summary>
    public enum OrderEnum
    {
        [EnumDisplayName("待付款")]
        WaitingPay = 0,
        [EnumDisplayName("审核中")]
        Auditing = 1,
        [EnumDisplayName("已完成")]
        completed = 2,
        [EnumDisplayName("不通过")]
        NoPass = 3
    }

    /// <summary>
    /// 收支类型
    /// </summary>
    public enum ShouZhiTypeEnum
    {
        [EnumDisplayName("收入")]
        ShouRu = 0,
        [EnumDisplayName("支出")]
        ZhiChu = 1
    }

    /// <summary>
    /// 来源
    /// </summary>
    public enum LaiYuanTypeEnum
    {
        [EnumDisplayName("微信")]
        WeiXin = 0,
        [EnumDisplayName("支付宝")]
        ZhiFuBao = 1,
        [EnumDisplayName("银联")]
        YinLian = 2
    }

    /// <summary>
    /// 待停
    /// </summary>
    public enum waitingTopEnum
    {
        [EnumDisplayName("申请")]
        Apply = 0,
        [EnumDisplayName("未续费")]
        UnRenew = 1
    }

    /// <summary>
    /// 用户类型
    /// </summary>
    public enum UserTypeEnum
    {
        [EnumDisplayName("个人")]
        GeRen = 0,
        [EnumDisplayName("企业")]
        QiYe = 1,
        [EnumDisplayName("个体经营")]
        GeTiJingYing = 2
    }

    /// <summary>
    /// 客服审核
    /// </summary>
    public enum CustomerServiceAuditEnum
    {
        [EnumDisplayName("未审核")]
        NoAudited = 0,
        [EnumDisplayName("已通过")]
        Pass = 1,
    }

    /// <summary>
    /// 缴费类型
    /// </summary>
    public enum PayTypeEnum
    {
        [EnumDisplayName("社保")]
        SocialSecurity = 0,
        [EnumDisplayName("公积金")]
        AccumulationFund = 1,
    }

    /// <summary>
    /// 还款期限
    /// </summary>
    public enum LoanTermEnum
    {
        [EnumDisplayName("随借随还")]
        InThreeMonths = 1,
        [EnumDisplayName("半年期")]
        HalfYear = 2,
        [EnumDisplayName("一年期")]
        OneYearPeriod = 3
    }

    /// <summary>
    /// 还款方式
    /// </summary>
    public enum LoanMethodEnum
    {
        [EnumDisplayName("等额本息")]
        DengEBenXi = 1
    }

    /// <summary>
    /// 借款审核
    /// </summary>
    public enum LoanAuditEnum
    {
        [EnumDisplayName("未审核")]
        NoAudited = 1,
        [EnumDisplayName("已通过")]
        Pass = 2,
        [EnumDisplayName("未通过")]
        NoPass = 3,
        [EnumDisplayName("已放款")]
        AlreadyLoan = 4
    }

    /// <summary>
    /// 还款状态
    /// </summary>
    public enum RepaymentStatusEnum
    {
        [EnumDisplayName("未结清")]
        NoSettled = 1,
        [EnumDisplayName("已逾期")]
        Overdue = 2,
        [EnumDisplayName("已结清")]
        Settled = 3
    }

    /// <summary>
    /// 还款类型
    /// </summary>
    public enum RepaymentTypeEnum
    {
        [EnumDisplayName("正常还")]
        Normal = 1,
        [EnumDisplayName("提前还")]
        TiQianHuan = 2

    }

    /// <summary>
    /// 基数审核
    /// </summary>
    public enum BaseAuditEnum
    {
        [EnumDisplayName("未审核")]
        NoAudited = 1,
        [EnumDisplayName("已通过")]
        Pass = 2,
        [EnumDisplayName("未通过")]
        NoPass = 3
    }

    /// <summary>
    /// 认证审核
    /// </summary>
    public enum CertificationAuditEnum
    {
        [EnumDisplayName("审核中")]
        NoAudited = 1,
        [EnumDisplayName("已通过")]
        Pass = 2,
        [EnumDisplayName("未通过")]
        NoPass = 3
    }

    /// <summary>
    /// 公积金办理类型
    /// </summary>
    public enum AccumulationFundTypeEnum
    {
        [EnumDisplayName("新开户")]
        NewAccount = 1,
        [EnumDisplayName("转移")]
        Transfer = 2
    }

    /// <summary>
    /// 公积金办停类型
    /// </summary>
    public enum AccumulationFundTopTypeEnum
    {
        [EnumDisplayName("封存")]
        Sealed = 1,
        [EnumDisplayName("转移")]
        Transfer = 2
    }

    /// <summary>
    /// 社保材料收取类型
    /// </summary>
    public enum SocialSecurityCollectTypeEnum
    {
        [EnumDisplayName("自取")]
        PickUp = 1,
        [EnumDisplayName("邮寄")]
        Mail = 2
    }

    /// <summary>
    /// 信息类型
    /// </summary>
    public enum InfoTypeEnum
    {
        [EnumDisplayName("参保介绍")]
        InsuredIntroduce = 1
    }

    public class SelectListClass
    {
        public static List<Property> GetSelectList(Type enumType)
        {
            List<Property> list = new List<Property>();

            foreach (object e in Enum.GetValues(enumType))
            {

                list.Add(new Property { Text = EnumExt.GetEnumCustomDescription(e), Value = ((int)e) });
            }

            return list;
        }
    }


    /// <summary>
    /// 户籍属性类
    /// </summary>
    public class Property
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }
}