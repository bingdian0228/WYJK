using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 缴费记录表
    /// </summary>
    public class JiaoFei_ZS
    {
        public int Id { get; set; }
        public string BusinessCode { get; set; }

        public string BankCode { get; set; }
        public string MemberPhone { get; set; }
        public string Money { get; set; }
        public string MonthCount { get; set; }
        public string BusinessDate { get; set; }
        public string BusinessTime { get; set; }
        public string SerialNumber { get; set; }
        public string Status { get; set; }
    }


    #region 用户查询

    /// <summary>
    /// 用户查询
    /// </summary>
    public class MemberQuery
    {
        /// <summary>
        /// 交易代码
        /// </summary>
        public string BusinessCode { get; set; }
        /// <summary>
        /// 银行代码
        /// </summary>
        public string BankCode { get; set; }

        /// <summary>
        /// 用户缴费号
        /// </summary>
        public string MemberPhone { get; set; }
    }

    /// <summary>
    /// 查询返回
    /// </summary>
    public class MemberQueryReturn : MemberQuery
    {
        /// <summary>
        /// 返回码
        /// </summary>
        public string ReturnCode { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string TrueName { get; set; }
        /// <summary>
        /// 一月金额
        /// </summary>
        public List<decimal> MoneyArray { get; set; } = new List<decimal>();
    }

    #endregion

    #region 缴费
    /// <summary>
    /// 缴费
    /// </summary>
    public class JiaoFeiSubmit : MemberQuery
    {
        /// <summary>
        /// 缴费金额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 月份数
        /// </summary>
        public int MonthCount { get; set; }

        /// <summary>
        /// 交易日期
        /// </summary>
        public string BusinessDate { get; set; }
        /// <summary>
        /// 交易时间
        /// </summary>
        public string BusinessTime { get; set; }

        /// <summary>
        /// 流水号
        /// </summary>
        public string SerialNumber { get; set; }
    }

    /// <summary>
    /// 缴费提交返回
    /// </summary>
    public class JiaoFeiSubmitReturn : MemberQuery
    {
        /// <summary>
        /// 流水号
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// 返回码
        /// </summary>
        public string ReturnCode { get; set; }
    }

    #endregion

    #region 冲正
    /// <summary>
    /// 冲正请求
    /// </summary>
    public class ChongZheng : MemberQuery
    {
        /// <summary>
        /// 流水号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 交易日期
        /// </summary>
        public string BusinessDate { get; set; }
        /// <summary>
        /// 缴费金额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 月份数
        /// </summary>
        public int MonthCount { get; set; }
    }

    /// <summary>
    /// 冲正返回
    /// </summary>
    public class ChongZhengReturn : MemberQuery
    {
        /// <summary>
        /// 流水号
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// 返回码
        /// </summary>
        public string ReturnCode { get; set; }
    }

    #endregion

    #region 日终对账
    /// <summary>
    /// 日终对账请求
    /// </summary>
    public class RiZhongDuiZhang
    {
        /// <summary>
        /// 交易代码
        /// </summary>
        public string BusinessCode { get; set; }
        /// <summary>
        /// 银行代码
        /// </summary>
        public string BankCode { get; set; }
        /// <summary>
        /// 交易日期
        /// </summary>
        public string BusinessDate { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 全路径
        /// </summary>
        public string FullFilename { get; set; }
    }

    /// <summary>
    /// 文件内容
    /// </summary>
    public class FileContent
    {
        /// <summary>
        /// 缴费号
        /// </summary>
        public string MemberPhone { get; set; }
        /// <summary>
        /// 缴费金额
        /// </summary>
        public decimal Money { get; set; }
        /// <summary>
        /// 缴费月份数
        /// </summary>
        public int MonthCount { get; set; }

        /// <summary>
        /// 交易日期
        /// </summary>
        public string BusinessDate { get; set; }
        /// <summary>
        /// 交易时间
        /// </summary>
        public string BusinessTime { get; set; }

        /// <summary>
        /// 流水号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 银行代码
        /// </summary>
        public string BankCode { get; set; }
    }

    /// <summary>
    /// 日终对账返回
    /// </summary>
    public class RiZhongDuiZhangReturn
    {
        /// <summary>
        /// 交易代码
        /// </summary>
        public string BusinessCode { get; set; }
        /// <summary>
        /// 银行代码
        /// </summary>
        public string BankCode { get; set; }
        /// <summary>
        /// 返回码
        /// </summary>
        public string ReturnCode { get; set; }
    }

    #endregion
}
