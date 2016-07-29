using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Web.Controllers.Http
{
    /// <summary>
    /// 借款接口
    /// </summary>
    public class LoanController : BaseApiController
    {
        private ILoanSubjectService _loanSubjectService = new LoanSubjectService();
        private ILoanMemberService _loanMemberService = new LoanMemberService();
        private ILoanRepayment _loanRepayment = new LoanRepaymentService();
        /// <summary>
        /// 获取身价计算选择题 注：1、如果获取第一道题目，则无需传参 2、如果“下一题目ID”为0，则没有下一道题
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<LoanSubject> GetChoiceSubject(int? SubjectID = null)
        {
            LoanSubject loanSubject = _loanSubjectService.GetChoiceSubject(SubjectID);
            return new JsonResult<LoanSubject>
            {
                status = true,
                Message = "获取成功",
                Data = loanSubject
            };
        }

        /// <summary>
        /// 身价计算
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JsonResult<LoanSubject> ValueCalculation(ValueCalculationParameter parameter)
        {
            string AnswerIDStr = string.Empty;
            foreach (var item in parameter.RelList)
            {
                AnswerIDStr += item.AnswerID + ",";
            }
            AnswerIDStr = AnswerIDStr.Trim(new char[] { ',' });
            //身价计算
            bool flag = _loanSubjectService.ValueCalculation(parameter.MemberID, AnswerIDStr);

            return new JsonResult<LoanSubject>
            {
                status = flag,
                Message = flag == true ? "计算成功" : "计算失败"
            };
        }

        /// <summary>
        /// 获取用户身价
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public JsonResult<Decimal> GetMemberValue(int MemberID)
        {
            decimal value = _loanSubjectService.GetMemberValue(MemberID);
            return new JsonResult<Decimal>
            {
                status = true,
                Message = "获取成功",
                Data = value
            };
        }

        /// <summary>
        /// 获取还款期限类型
        /// </summary>
        /// <returns></returns>
        public JsonResult<List<Property>> GetLoanTermList()
        {
            List<Property> selectList = SelectListClass.GetSelectList(typeof(LoanTermEnum));
            return new JsonResult<List<Property>>
            {
                status = true,
                Message = "获取成功",
                Data = selectList
            };
        }

        /// <summary>
        /// 获取还款方式
        /// </summary>
        /// <returns></returns>
        public JsonResult<List<Property>> GetLoanMethodList()
        {
            List<Property> selectList = SelectListClass.GetSelectList(typeof(LoanMethodEnum));
            return new JsonResult<List<Property>>
            {
                status = true,
                Message = "获取成功",
                Data = selectList
            };
        }

        /// <summary>
        /// 是否可以借款
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<AppayLoan> IsCanLoan(int MemberID)
        {
            //判断用户下的社保否在平台缴纳三个月
            if (DbHelper.QuerySingle<int>($@"select count(1) from SocialSecurity 
    left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID
    where SocialSecurityPeople.MemberID = {MemberID} and SocialSecurity.AlreadyPayMonthCount >= 3 and SocialSecurity.Status = 3", null) == 0)
            {
                return new JsonResult<AppayLoan>
                {
                    status = false,
                    Message = "用户至少在平台缴纳三个月社保才可以借款"
                };
            }
            else {
                return new JsonResult<AppayLoan>
                {
                    status = true,
                    Message = "进入借款"
                };
            }
        }

        /// <summary>
        /// 申请借款
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public JsonResult<AppayLoan> GetApplyloan(int MemberID)
        {
            //判断用户下的社保否在平台缴纳三个月
            if (DbHelper.QuerySingle<int>($@"select count(1) from SocialSecurity 
    left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID
    where SocialSecurityPeople.MemberID = {MemberID} and SocialSecurity.AlreadyPayMonthCount >= 3 and SocialSecurity.Status = 3", null) == 0)
            {
                return new JsonResult<AppayLoan>
                {
                    status = false,
                    Message = "用户至少在平台缴纳三个月社保才可以借款"
                };
            }

            AppayLoan appayLoan = _loanMemberService.GetMemberLoanDetail(MemberID);
            return new JsonResult<AppayLoan>
            {
                status = true,
                Message = "获取成功",
                Data = appayLoan
            };
        }

        /// <summary>
        /// 提交借款申请
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JsonResult<dynamic> SubmitLoanApply(MemberLoanAuditParameter model)
        {
            decimal AvailableAmount = _loanMemberService.GetMemberLoanDetail(model.MemberID).AvailableAmount;

            if (model.ApplyAmount > AvailableAmount)
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "借款额度超过可借额度"
                };

            bool flag = _loanMemberService.SubmitLoanApply(model);
            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag == true ? "提交申请成功" : "提交申请失败"
            };
        }

        /// <summary>
        /// 获取借款申请进度  {借款金额：ApplyAmount,申请日期：ApplyDate,状态：Status}
        /// </summary>
        /// <param name="memberID"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>

        public async Task<JsonResult<PagedResult<MemberLoanAudit>>> GetLoanApplayProgress(int memberID, int PageIndex = 1, int PageSize = 1)
        {
            PagedResult<MemberLoanAudit> list = await _loanMemberService.GetMemberLoanAuditList(memberID, new PagedParameter { PageIndex = PageIndex, PageSize = PageSize });

            list.Items.ToList().ForEach(n =>
            {
                n.Status = EnumExt.GetEnumCustomDescription((LoanAuditEnum)Convert.ToInt32(n.Status));
            });

            return new JsonResult<PagedResult<MemberLoanAudit>>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };
        }

        /// <summary>
        /// 获取借款列表 {借款金额：ApplyAmount,借款余额：LoanBalance,时间：AlreadyLoanDate,状态：RepaymentStatus}
        /// </summary>
        /// <param name="memberID"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>

        public async Task<JsonResult<PagedResult<MemberLoanAudit>>> GetMemberLoanAuditList(int memberID, int PageIndex = 1, int PageSize = 1)
        {
            PagedResult<MemberLoanAudit> list = await _loanMemberService.GetMemberLoanAuditList(memberID, new PagedParameter { PageIndex = PageIndex, PageSize = PageSize }, "4");

            list.Items.ToList().ForEach(n =>
            {
                n.RepaymentStatus = EnumExt.GetEnumCustomDescription((RepaymentStatusEnum)Convert.ToInt32(n.RepaymentStatus));
            });

            return new JsonResult<PagedResult<MemberLoanAudit>>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };
        }

        /// <summary>
        /// 获取借款详情 {借款金额：ApplyAmount,借款余额：LoanBalance,时间：AlreadyLoanDate,状态：RepaymentStatus,还款方式：LoanTerm+LoanMethod}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult<MemberLoanAudit> GetMemberLoanAuditDetail(int id)
        {

            MemberLoanAudit memberLoanAudit = _loanMemberService.GetMemberLoanAuditDetail(id);

            memberLoanAudit.RepaymentStatus = EnumExt.GetEnumCustomDescription((RepaymentStatusEnum)Convert.ToInt32(memberLoanAudit.RepaymentStatus));
            if (!string.IsNullOrWhiteSpace(memberLoanAudit.LoanMethod))
                memberLoanAudit.LoanMethod = EnumExt.GetEnumCustomDescription((LoanMethodEnum)Convert.ToInt32(memberLoanAudit.LoanMethod));

            memberLoanAudit.LoanTerm = EnumExt.GetEnumCustomDescription((LoanTermEnum)Convert.ToInt32(memberLoanAudit.LoanTerm));


            return new JsonResult<MemberLoanAudit>
            {
                status = true,
                Message = "获取成功",
                Data = memberLoanAudit
            };
        }



        /// <summary>
        /// 新建我要还款
        /// </summary>
        /// <param name="id">借款id</param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<Repayment> MemberLoanRepayment(int id)
        {
            Repayment repayment = null;//我要还款展示实体

            decimal InThreeMonthsDayLiLv = (decimal)0.018 / 30;//随借随还日利率
            const int InThreeMonthsCount = 3;
            decimal HalfYearMonthLiLv = 0.015M;//半年等额本息月利率
            const int HalfYearMonthCount = 6;
            decimal OneYearPeriodMonthLiLv = 0.012M;//一年等额本息月利率
            const int OneYearPeriodMonthCount = 12;
            decimal ZhiNaJinPercent = 0;//滞纳金百分比

            //decimal totalAmount = 0;//还款总金额
            decimal BenJin = 0;//本金
            decimal LiXi = 0;//利息

            //decimal WeiYueJin = 0;//违约金
            string LoanMethod = string.Empty;//还款方式


            //借款详情
            MemberLoanAudit memberLoanAudit = _loanMemberService.GetMemberLoanAuditDetail(id);
            //根据还款状态进行还款,分为正常和逾期

            if (memberLoanAudit.RepaymentStatus == Convert.ToString((int)RepaymentStatusEnum.NoSettled))
            {
                repayment.RepaymentType = "正常还";
                if (memberLoanAudit.LoanMethod == Convert.ToString((int)LoanTermEnum.InThreeMonths))
                {
                    #region 随借随还
                    repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.InThreeMonths));

                    BenJin = memberLoanAudit.ApplyAmount;
                    LiXi = memberLoanAudit.ApplyAmount * (decimal)((DateTime.Now - memberLoanAudit.AlreadyLoanDate).Days * InThreeMonthsDayLiLv);
                    repayment.TotalAmount = BenJin + LiXi;
                    repayment.DetailList.Add(new RepaymentDetail { BenJin = BenJin, LiXi = LiXi });

                    #endregion
                }
                else if (memberLoanAudit.LoanMethod == Convert.ToString((int)LoanTermEnum.HalfYear))
                {
                    #region 半年期
                    repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.HalfYear))
                        + EnumExt.GetEnumCustomDescription((LoanMethodEnum)((int)LoanMethodEnum.DengEBenXi));
                    //等额本息计算方式：{每月偿还本息：[贷款本金×月利率×（1+月利率）^还款月数]÷[（1+月利率）^还款月数－1]}
                    decimal MonthBenXi = GetMonthBenXi(memberLoanAudit.ApplyAmount, HalfYearMonthLiLv, HalfYearMonthCount);
                    LiXi = memberLoanAudit.LoanBalance * HalfYearMonthLiLv;
                    BenJin = MonthBenXi - LiXi;
                    repayment.TotalAmount = MonthBenXi;
                    repayment.DetailList.Add(new RepaymentDetail { BenJin = BenJin, LiXi = LiXi, MonthDt = DateTime.Now.ToString("yyyy-MM") });


                    #endregion

                }
                else if (memberLoanAudit.LoanMethod == Convert.ToString((int)LoanTermEnum.OneYearPeriod))
                {
                    #region 一年期
                    repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.OneYearPeriod))
    + EnumExt.GetEnumCustomDescription((LoanMethodEnum)((int)LoanMethodEnum.DengEBenXi));
                    decimal MonthBenXi = GetMonthBenXi(memberLoanAudit.ApplyAmount, OneYearPeriodMonthLiLv, OneYearPeriodMonthCount);
                    LiXi = memberLoanAudit.LoanBalance * OneYearPeriodMonthLiLv;
                    BenJin = MonthBenXi - LiXi;
                    repayment.TotalAmount = MonthBenXi;
                    repayment.DetailList.Add(new RepaymentDetail { BenJin = BenJin, LiXi = LiXi, MonthDt = DateTime.Now.ToString("yyyy-MM") });
                    #endregion
                }
            }
            else if (memberLoanAudit.RepaymentStatus == Convert.ToString((int)RepaymentStatusEnum.Overdue))
            {
                repayment.RepaymentType = "逾期还";
                if (memberLoanAudit.LoanMethod == Convert.ToString((int)LoanTermEnum.InThreeMonths))
                {
                    #region 随借随还
                    repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.InThreeMonths));

                    BenJin = memberLoanAudit.ApplyAmount;
                    LiXi = memberLoanAudit.ApplyAmount * (decimal)((memberLoanAudit.AlreadyLoanDate.AddMonths(InThreeMonthsCount) - memberLoanAudit.AlreadyLoanDate).Days * InThreeMonthsDayLiLv);

                    decimal ZhiNaJin = (DateTime.Now - memberLoanAudit.AlreadyLoanDate.AddMonths(InThreeMonthsCount)).Days * (BenJin + LiXi) * ZhiNaJinPercent;
                    repayment.TotalAmount = BenJin + LiXi + ZhiNaJin;
                    repayment.DetailList.Add(new RepaymentDetail { BenJin = BenJin, LiXi = LiXi, ZhiNaJin = ZhiNaJin, MonthDt = DateTime.Now.ToString("yyyy-MM") });

                    #endregion
                }
                else if (memberLoanAudit.LoanMethod == Convert.ToString((int)LoanTermEnum.HalfYear))
                {
                    #region 半年期
                    repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.HalfYear))
                        + EnumExt.GetEnumCustomDescription((LoanMethodEnum)((int)LoanMethodEnum.DengEBenXi));

                    decimal MonthBenXi = GetMonthBenXi(memberLoanAudit.ApplyAmount, HalfYearMonthLiLv, HalfYearMonthCount);
                    LiXi = memberLoanAudit.LoanBalance * HalfYearMonthLiLv;
                    BenJin = MonthBenXi - LiXi;

                    List<MemberLoanRepayment> memberLoanRepaymentList = _loanRepayment.GetMemberLoanRepaymentList(id);//借款对应的还款列表

                    for (int i = memberLoanRepaymentList.Sum(n => n.MonthCount); i <= HalfYearMonthCount; i++)
                    {
                        DateTime TempDt = memberLoanAudit.AlreadyLoanDate.AddMonths(i);

                        DateTime t1 = new DateTime(TempDt.Year, TempDt.Month, 1);
                        DateTime t2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        if (DateTime.Compare(t1, t2) == 0)
                        {
                            break;
                        }

                        DateTime t3 = t1.AddMonths(1).AddDays(-1);
                        int ZhiNaDayCount = (DateTime.Now - t3).Days;

                        decimal ZhiNaJin = MonthBenXi * ZhiNaDayCount * ZhiNaJinPercent;//滞纳金

                        repayment.TotalAmount += MonthBenXi + ZhiNaJin;
                        repayment.DetailList.Add(new RepaymentDetail { BenJin = BenJin, LiXi = LiXi, ZhiNaJin = ZhiNaJin, MonthDt = TempDt.ToString("yyyy-MM") });

                    }

                    #endregion
                }
                else if (memberLoanAudit.LoanMethod == Convert.ToString((int)LoanTermEnum.OneYearPeriod))
                {
                    #region 一年期
                    LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.OneYearPeriod))
    + EnumExt.GetEnumCustomDescription((LoanMethodEnum)((int)LoanMethodEnum.DengEBenXi));
                    decimal MonthBenXi = GetMonthBenXi(memberLoanAudit.ApplyAmount, OneYearPeriodMonthLiLv, OneYearPeriodMonthCount);
                    LiXi = memberLoanAudit.LoanBalance * OneYearPeriodMonthLiLv;
                    BenJin = MonthBenXi - LiXi;

                    List<MemberLoanRepayment> memberLoanRepaymentList = _loanRepayment.GetMemberLoanRepaymentList(id);//借款对应的还款列表

                    for (int i = memberLoanRepaymentList.Sum(n => n.MonthCount); i <= OneYearPeriodMonthCount; i++)
                    {
                        DateTime TempDt = memberLoanAudit.AlreadyLoanDate.AddMonths(i);

                        DateTime t1 = new DateTime(TempDt.Year, TempDt.Month, 1);
                        DateTime t2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        if (DateTime.Compare(t1, t2) == 0)
                        {
                            break;
                        }

                        DateTime t3 = t1.AddMonths(1).AddDays(-1);
                        int ZhiNaDayCount = (DateTime.Now - t3).Days;

                        decimal ZhiNaJin = MonthBenXi * ZhiNaDayCount * ZhiNaJinPercent;//滞纳金

                        repayment.TotalAmount += MonthBenXi + ZhiNaJin;
                        repayment.DetailList.Add(new RepaymentDetail { BenJin = BenJin, LiXi = LiXi, ZhiNaJin = ZhiNaJin, MonthDt = TempDt.ToString("yyyy-MM") });

                    }
                    #endregion
                }
            }

            return new JsonResult<Repayment>
            {
                status = true,
                Message = "获取成功",
                Data = repayment
            };
        }

        /// <summary>
        /// 选择还款类型
        /// </summary>
        /// <param name="id"></param>
        /// <param name="RepaymentType"></param>
        /// <returns></returns>
        public JsonResult<Repayment> SelectRepaymentType(int id, int RepaymentType)
        {
            Repayment repayment = null;//我要还款展示实体

            decimal InThreeMonthsDayLiLv = (decimal)0.018 / 30;//随借随还日利率
            const int InThreeMonthsCount = 3;
            decimal HalfYearMonthLiLv = 0.015M;//半年等额本息月利率
            const int HalfYearMonthCount = 6;
            decimal OneYearPeriodMonthLiLv = 0.012M;//一年等额本息月利率
            const int OneYearPeriodMonthCount = 12;
            decimal ZhiNaJinPercent = 0;//滞纳金百分比

            decimal BenJin = 0;//本金
            decimal LiXi = 0;//利息

            string LoanMethod = string.Empty;//还款方式


            //借款详情
            MemberLoanAudit memberLoanAudit = _loanMemberService.GetMemberLoanAuditDetail(id);
            //根据还款状态进行还款,分为正常和逾期

            if (RepaymentType == (int)RepaymentTypeEnum.Normal)
            {
                if (memberLoanAudit.LoanMethod == Convert.ToString((int)LoanTermEnum.InThreeMonths))
                {
                    #region 随借随还
                    repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.InThreeMonths));

                    BenJin = memberLoanAudit.ApplyAmount;
                    LiXi = memberLoanAudit.ApplyAmount * (decimal)((DateTime.Now - memberLoanAudit.AlreadyLoanDate).Days * InThreeMonthsDayLiLv);
                    repayment.TotalAmount = BenJin + LiXi;
                    repayment.DetailList.Add(new RepaymentDetail { BenJin = BenJin, LiXi = LiXi });

                    #endregion
                }
                else if (memberLoanAudit.LoanMethod == Convert.ToString((int)LoanTermEnum.HalfYear))
                {
                    #region 半年期
                    repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.HalfYear))
                        + EnumExt.GetEnumCustomDescription((LoanMethodEnum)((int)LoanMethodEnum.DengEBenXi));
                    //等额本息计算方式：{每月偿还本息：[贷款本金×月利率×（1+月利率）^还款月数]÷[（1+月利率）^还款月数－1]}
                    decimal MonthBenXi = GetMonthBenXi(memberLoanAudit.ApplyAmount, HalfYearMonthLiLv, HalfYearMonthCount);
                    LiXi = memberLoanAudit.LoanBalance * HalfYearMonthLiLv;
                    BenJin = MonthBenXi - LiXi;
                    repayment.TotalAmount = MonthBenXi;
                    repayment.DetailList.Add(new RepaymentDetail { BenJin = BenJin, LiXi = LiXi, MonthDt = DateTime.Now.ToString("yyyy-MM") });

                    #endregion

                }
                else if (memberLoanAudit.LoanMethod == Convert.ToString((int)LoanTermEnum.OneYearPeriod))
                {
                    #region 一年期
                    repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.OneYearPeriod))
    + EnumExt.GetEnumCustomDescription((LoanMethodEnum)((int)LoanMethodEnum.DengEBenXi));
                    decimal MonthBenXi = GetMonthBenXi(memberLoanAudit.ApplyAmount, OneYearPeriodMonthLiLv, OneYearPeriodMonthCount);
                    LiXi = memberLoanAudit.LoanBalance * OneYearPeriodMonthLiLv;
                    BenJin = MonthBenXi - LiXi;
                    repayment.TotalAmount = MonthBenXi;
                    repayment.DetailList.Add(new RepaymentDetail { BenJin = BenJin, LiXi = LiXi, MonthDt = DateTime.Now.ToString("yyyy-MM") });
                    #endregion
                }
            }
            else if (RepaymentType == (int)RepaymentTypeEnum.TiQianHuan)
            {
                //提前还

            }

            return new JsonResult<Repayment>
            {
                status = true,
                Message = "获取成功",
                Data = repayment
            };
        }

        /// <summary>
        /// 等额本息获取还款本息
        /// </summary>
        /// <param name="applyAmount"></param>
        /// <param name="monthLiLv"></param>
        /// <param name="monthCount"></param>
        /// <returns></returns>
        private decimal GetMonthBenXi(decimal applyAmount, decimal monthLiLv, int monthCount)
        {
            //等额本息计算方式：{每月偿还本息：[贷款本金×月利率×（1+月利率）^还款月数]÷[（1+月利率）^还款月数－1]}
            return (applyAmount * monthLiLv * Convert.ToDecimal(Math.Pow(Convert.ToDouble((1 + monthLiLv)), monthCount))) / Convert.ToDecimal((Math.Pow(Convert.ToDouble(1 + monthLiLv), monthCount) - 1));
        }
    }
}