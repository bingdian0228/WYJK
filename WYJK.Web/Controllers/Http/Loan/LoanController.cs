using System;
using System.Collections.Generic;
using System.Data.Common;
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
        private ILoanRepaymentService _loanRepaymentService = new LoanRepaymentService();
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

        ///// <summary>
        ///// 获取还款类型
        ///// </summary>
        ///// <returns></returns>
        //public List<Property> GetRepaymentTypeList()
        //{
        //    List<Property> selectList = SelectListClass.GetSelectList(typeof(RepaymentTypeEnum));
        //    return selectList;
        //}

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
    //        if (DbHelper.QuerySingle<int>($@"select count(1) from SocialSecurity 
    //left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID
    //where SocialSecurityPeople.MemberID = {MemberID} and SocialSecurity.AlreadyPayMonthCount >= 3 and SocialSecurity.Status = 3", null) == 0)
    //        {
    //            return new JsonResult<AppayLoan>
    //            {
    //                status = false,
    //                Message = "用户至少在平台缴纳三个月社保才可以借款"
    //            };
    //        }

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
        /// 如果RepaymentStatus为已结清，则《我要还款》按钮隐藏
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
        /// 借款参数值设置
        /// </summary>
        private static MemberLoanSetting memberLoanSetting
        {
            get
            {
                return DbHelper.Query<MemberLoanSetting>("select * from MemberLoanSetting").FirstOrDefault();
            }
        }

        decimal InThreeMonthsDayLiLv = (decimal)memberLoanSetting.InThreeMonthsLiLv / 100 / 30;//随借随还日利率
        const int InThreeMonthsCount = 3;
        decimal HalfYearMonthLiLv = memberLoanSetting.HalfYearLiLv / 100M;//半年等额本息月利率
        const int HalfYearMonthCount = 6;
        decimal OneYearPeriodMonthLiLv = memberLoanSetting.OneYearPeriodLiLv / 100M;//一年等额本息月利率
        const int OneYearPeriodMonthCount = 12;

        decimal ZhiNaJinPercent = memberLoanSetting.ZhiNaJinPercent / 100M;//滞纳金百分比

        decimal WeiYueJinPercent = memberLoanSetting.WeiYueJinPercent / 100M;//违约金百分比

        //decimal totalAmount = 0;//还款总金额
        decimal BenJin = 0;//本金
        decimal LiXi = 0;//利息
        decimal ZhiNaJin = 0;//滞纳金
        decimal WeiYueJin = 0;//违约金

        string LoanMethod = string.Empty;//还款方式

        /// <summary>
        /// 是否可以还款   在借款页面点击还款时判断一下,data=0代表不可以还，data=1代表可以还
        /// </summary>
        /// <param name="id">借款详情ID</param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<dynamic> IsCanRepayment(int id)
        {
            //查看是否存在审核中，如果存在则不能进行付款
            int val = DbHelper.QuerySingle<int>($"select count(1) from MemberLoanRepayment where JieID={id} and Status=1");
            if (val > 0)
                return new JsonResult<dynamic>
                {
                    status = true,
                    Message = "存在未审核的，不能进行还款",
                    Data = 0
                };
            else
            {
                return new JsonResult<dynamic>
                {
                    status = true,
                    Message = "可以还款",
                    Data = 1
                };
            }
        }

        /// <summary>
        /// 新建我要还款
        /// </summary>
        /// <param name="id">借款id</param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<MemberLoanRepayment> MemberLoanRepayment(int id)
        {

            MemberLoanRepayment repayment = GetMemberLoanRepayment(id);
            return new JsonResult<MemberLoanRepayment>
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
        [System.Web.Http.HttpGet]
        public JsonResult<MemberLoanRepayment> SelectRepaymentType(int id, string RepaymentType)
        {
            MemberLoanRepayment repayment = GetMemberLoanRepayment(id, RepaymentType);
            return new JsonResult<MemberLoanRepayment>
            {
                status = true,
                Message = "获取成功",
                Data = repayment
            };
        }

        /// <summary>
        /// 提交还款
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public JsonResult<dynamic> MemberLoanRepayment(MemberLoanRepaymentParameter parameter)
        {
            int id = MemberLoanRepayment(parameter.ID, parameter.RepaymentType);

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "还款成功",
                Data = id

            };
        }
        /// <summary>
        /// 提交还款内部类
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="RepaymentType"></param>
        internal int MemberLoanRepayment(int ID, string RepaymentType)
        {
            MemberLoanRepayment repayment = GetMemberLoanRepayment(ID, RepaymentType);

            //保存还款记录
            int id = DbHelper.ExecuteSqlCommandScalar($"insert into MemberLoanRepayment(TotalAmount,WeiYueJin,RepaymentDt,Status,RepaymentType,JieID) values({repayment.TotalAmount},{repayment.WeiYueJin},getdate(),1,{repayment.RepaymentType},{ID})", new DbParameter[] { });
            foreach (MemberLoanRepaymentDetail detail in repayment.DetailList)
            {
                DbHelper.ExecuteSqlCommand($"insert into MemberLoanRepaymentDetail(BenJin,LiXi,ZhiNaJin,MonthStr,HuanID) values({detail.BenJin},{detail.LiXi},{detail.ZhiNaJin},'{detail.MonthStr}',{id})", null);
            }

            //借款审核修改未还余额
            DbHelper.ExecuteSqlCommand($"update MemberLoanAudit set LoanBalance-={repayment.DetailList.Select(n => n.BenJin).Sum()} where ID={ID}", null);
            //借款表已用额度和可用额度更新
            DbHelper.ExecuteSqlCommand($"update MemberLoan set AlreadyUsedAmount-={repayment.DetailList.Select(n => n.BenJin).Sum()},AvailableAmount+={repayment.DetailList.Select(n => n.BenJin).Sum()} where MemberID=(select MemberID from MemberLoanAudit where ID={ID})", null);

            //修改还款状态
            MemberLoanAudit memberLoanAudit = DbHelper.QuerySingle<MemberLoanAudit>($"select * from MemberLoanAudit where ID={ID}");
            //如果是随借随还，则还款状态变为已结清
            if (memberLoanAudit.LoanTerm == Convert.ToString((int)LoanTermEnum.InThreeMonths))
            {
                DbHelper.ExecuteSqlCommand($"update MemberLoanAudit set RepaymentStatus={(int)RepaymentStatusEnum.Settled} where ID={ID}", null);
            }
            else
            {
                if (RepaymentType == Convert.ToString((int)RepaymentTypeEnum.TiQianHuan))
                {
                    DbHelper.ExecuteSqlCommand($"update MemberLoanAudit set RepaymentStatus={(int)RepaymentStatusEnum.Settled} where ID={ID}", null);
                }
                else {
                    int period = 0;
                    if (memberLoanAudit.LoanTerm == Convert.ToString((int)LoanTermEnum.HalfYear))
                        period = 6;
                    else if (memberLoanAudit.LoanTerm == Convert.ToString((int)LoanTermEnum.OneYearPeriod))
                        period = 12;

                    List<MemberLoanRepaymentDetail> DetailList = DbHelper.Query<MemberLoanRepaymentDetail>($"select * from MemberLoanRepaymentDetail left join MemberLoanRepayment on MemberLoanRepayment.ID=MemberLoanRepaymentDetail.HuanID where JieID={ID}");
                    if (DetailList.Count < period)
                        DbHelper.ExecuteSqlCommand($"update MemberLoanAudit set RepaymentStatus={(int)RepaymentStatusEnum.NoSettled} where ID={ID}", null);
                    else
                        DbHelper.ExecuteSqlCommand($"update MemberLoanAudit set RepaymentStatus={(int)RepaymentStatusEnum.Settled} where ID={ID}", null);
                }
            }

            return id;
        }

        /// <summary>
        /// 得到还款实体
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal MemberLoanRepayment GetMemberLoanRepayment(int ID, string RepaymentType = null)
        {

            MemberLoanRepayment repayment = new MemberLoanRepayment();//我要还款展示实体

            //借款详情
            MemberLoanAudit memberLoanAudit = _loanMemberService.GetMemberLoanAuditDetail(ID);

            //借款对应的还款列表
            List<MemberLoanRepayment> memberLoanRepaymentList = _loanRepaymentService.GetMemberLoanRepaymentList(ID);

            //根据还款状态进行还款,分为正常和逾期
            if (memberLoanAudit.RepaymentStatus == Convert.ToString((int)RepaymentStatusEnum.NoSettled))
            {


                //查询当前月是否已经还款
                List<MemberLoanRepaymentDetail> DetailList = DbHelper.Query<MemberLoanRepaymentDetail>($"select * from MemberLoanRepaymentDetail left join MemberLoanRepayment on MemberLoanRepayment.ID=MemberLoanRepaymentDetail.HuanID where JieID={ID}");
                bool flag = false;//当前月是否已交
                if (DetailList != null && DetailList.Count > 0)
                {
                    foreach (var item in DetailList)
                    {
                        if (item.MonthStr == DateTime.Now.ToString("yyyy-MM"))
                        {
                            flag = true;
                        }
                    }
                }
                DateTime t1 = new DateTime();
                if (flag == false)
                {
                    if (memberLoanAudit.LoanTerm == Convert.ToString((int)LoanTermEnum.InThreeMonths))
                        repayment.RepaymentTypeList = new List<Property>() { new Property() { Value = 2, Text = "正常还" } };
                    else
                        repayment.RepaymentTypeList = new List<Property>() { new Property() { Value = 2, Text = "正常还" }, new Property { Value = 3, Text = "提前还" } };

                    if (RepaymentType == null)
                        repayment.RepaymentType = "2";
                    else
                        repayment.RepaymentType = RepaymentType;

                }
                else {
                    repayment.RepaymentTypeList = new List<Property>() { new Property() { Value = 3, Text = "提前还" } };
                    if (RepaymentType == null)
                        repayment.RepaymentType = "3";
                    else
                        repayment.RepaymentType = RepaymentType;
                }


                if (repayment.RepaymentType == "2")
                {
                    //正常还
                    if (memberLoanAudit.LoanTerm == Convert.ToString((int)LoanTermEnum.InThreeMonths))
                    {
                        #region 随借随还
                        repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.InThreeMonths));

                        BenJin = memberLoanAudit.ApplyAmount;
                        LiXi = memberLoanAudit.ApplyAmount * (decimal)(((DateTime.Now - memberLoanAudit.AlreadyLoanDate).Days + 1) * InThreeMonthsDayLiLv);
                        repayment.TotalAmount = BenJin + LiXi;
                        repayment.DetailList.Add(new MemberLoanRepaymentDetail { BenJin = BenJin, LiXi = LiXi });

                        #endregion
                    }
                    else if (memberLoanAudit.LoanTerm == Convert.ToString((int)LoanTermEnum.HalfYear))
                    {
                        #region 半年期
                        repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.HalfYear))
                            + EnumExt.GetEnumCustomDescription((LoanMethodEnum)((int)LoanMethodEnum.DengEBenXi));
                        //等额本息计算方式：{每月偿还本息：[贷款本金×月利率×（1+月利率）^还款月数]÷[（1+月利率）^还款月数－1]}
                        decimal MonthBenXi = GetMonthBenXi(memberLoanAudit.ApplyAmount, HalfYearMonthLiLv, HalfYearMonthCount);
                        LiXi = memberLoanAudit.LoanBalance * HalfYearMonthLiLv;
                        BenJin = MonthBenXi - LiXi;
                        repayment.TotalAmount = MonthBenXi;
                        repayment.DetailList.Add(new MemberLoanRepaymentDetail { BenJin = BenJin, LiXi = LiXi, MonthStr = DateTime.Now.ToString("yyyy-MM") });


                        #endregion

                    }
                    else if (memberLoanAudit.LoanTerm == Convert.ToString((int)LoanTermEnum.OneYearPeriod))
                    {
                        #region 一年期
                        repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.OneYearPeriod))
        + EnumExt.GetEnumCustomDescription((LoanMethodEnum)((int)LoanMethodEnum.DengEBenXi));
                        decimal MonthBenXi = GetMonthBenXi(memberLoanAudit.ApplyAmount, OneYearPeriodMonthLiLv, OneYearPeriodMonthCount);
                        LiXi = memberLoanAudit.LoanBalance * OneYearPeriodMonthLiLv;
                        BenJin = MonthBenXi - LiXi;
                        repayment.TotalAmount = MonthBenXi;
                        repayment.DetailList.Add(new MemberLoanRepaymentDetail { BenJin = BenJin, LiXi = LiXi, MonthStr = DateTime.Now.ToString("yyyy-MM") });
                        #endregion
                    }
                }
                else if (repayment.RepaymentType == "3")
                {
                    //提前还
                    if (memberLoanAudit.LoanTerm == Convert.ToString((int)LoanTermEnum.HalfYear))
                    {
                        #region 半年期

                        #region 提前还
                        repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.HalfYear))
    + EnumExt.GetEnumCustomDescription((LoanMethodEnum)((int)LoanMethodEnum.DengEBenXi));
                        BenJin = memberLoanAudit.LoanBalance; //获取剩余还款金额
                        repayment.WeiYueJin = BenJin * WeiYueJinPercent;
                        repayment.TotalAmount += BenJin + repayment.WeiYueJin;
                        repayment.DetailList.Add(new MemberLoanRepaymentDetail { BenJin = BenJin });
                        #endregion

                        #endregion

                    }
                    else if (memberLoanAudit.LoanTerm == Convert.ToString((int)LoanTermEnum.OneYearPeriod))
                    {
                        #region 一年期

                        #region 提前还
                        repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.OneYearPeriod))
+ EnumExt.GetEnumCustomDescription((LoanMethodEnum)((int)LoanMethodEnum.DengEBenXi));
                        BenJin = memberLoanAudit.LoanBalance; //获取剩余还款金额
                        repayment.WeiYueJin = BenJin * WeiYueJinPercent;
                        repayment.TotalAmount += BenJin + repayment.WeiYueJin;
                        repayment.DetailList.Add(new MemberLoanRepaymentDetail { BenJin = BenJin });
                        #endregion

                        #endregion
                    }
                }
            }
            else if (memberLoanAudit.RepaymentStatus == Convert.ToString((int)RepaymentStatusEnum.Overdue))
            {
                repayment.RepaymentType = "1";//逾期还
                repayment.RepaymentTypeList = new List<Property>() { new Property() { Value = 1, Text = "逾期还" } };

                if (memberLoanAudit.LoanTerm == Convert.ToString((int)LoanTermEnum.InThreeMonths))
                {
                    #region 随借随还
                    repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.InThreeMonths));

                    BenJin = memberLoanAudit.ApplyAmount;
                    LiXi = memberLoanAudit.ApplyAmount * (decimal)((memberLoanAudit.AlreadyLoanDate.AddMonths(InThreeMonthsCount) - memberLoanAudit.AlreadyLoanDate).Days * InThreeMonthsDayLiLv);

                    ZhiNaJin = ((DateTime.Now - memberLoanAudit.AlreadyLoanDate.AddMonths(InThreeMonthsCount)).Days + 1) * (BenJin + LiXi) * ZhiNaJinPercent;
                    repayment.TotalAmount = BenJin + LiXi + ZhiNaJin;
                    repayment.DetailList.Add(new MemberLoanRepaymentDetail { BenJin = BenJin, LiXi = LiXi, ZhiNaJin = ZhiNaJin, MonthStr = DateTime.Now.ToString("yyyy-MM") });

                    #endregion
                }
                else if (memberLoanAudit.LoanTerm == Convert.ToString((int)LoanTermEnum.HalfYear))
                {
                    #region 半年期
                    repayment.LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.HalfYear))
                        + EnumExt.GetEnumCustomDescription((LoanMethodEnum)((int)LoanMethodEnum.DengEBenXi));

                    decimal MonthBenXi = GetMonthBenXi(memberLoanAudit.ApplyAmount, HalfYearMonthLiLv, HalfYearMonthCount);


                    decimal LoanBalance = memberLoanAudit.LoanBalance;

                    for (int i = memberLoanRepaymentList.Count(); i < HalfYearMonthCount; i++)
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

                        LiXi = LoanBalance * HalfYearMonthLiLv;
                        BenJin = MonthBenXi - LiXi;
                        LoanBalance -= BenJin;

                        ZhiNaJin = MonthBenXi * ZhiNaDayCount * ZhiNaJinPercent;//滞纳金

                        repayment.TotalAmount += MonthBenXi + ZhiNaJin;
                        repayment.DetailList.Add(new MemberLoanRepaymentDetail { BenJin = BenJin, LiXi = LiXi, ZhiNaJin = ZhiNaJin, MonthStr = TempDt.ToString("yyyy-MM") });

                    }

                    #endregion
                }
                else if (memberLoanAudit.LoanTerm == Convert.ToString((int)LoanTermEnum.OneYearPeriod))
                {
                    #region 一年期
                    LoanMethod = EnumExt.GetEnumCustomDescription((LoanTermEnum)((int)LoanTermEnum.OneYearPeriod))
    + EnumExt.GetEnumCustomDescription((LoanMethodEnum)((int)LoanMethodEnum.DengEBenXi));
                    decimal MonthBenXi = GetMonthBenXi(memberLoanAudit.ApplyAmount, OneYearPeriodMonthLiLv, OneYearPeriodMonthCount);
                    LiXi = memberLoanAudit.LoanBalance * OneYearPeriodMonthLiLv;
                    BenJin = MonthBenXi - LiXi;

                    decimal LoanBalance = memberLoanAudit.LoanBalance;
                    for (int i = memberLoanRepaymentList.Count(); i < OneYearPeriodMonthCount; i++)
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

                        LiXi = LoanBalance * HalfYearMonthLiLv;
                        BenJin = MonthBenXi - LiXi;
                        LoanBalance -= BenJin;
                        ZhiNaJin = MonthBenXi * ZhiNaDayCount * ZhiNaJinPercent;//滞纳金

                        repayment.TotalAmount += MonthBenXi + ZhiNaJin;
                        repayment.DetailList.Add(new MemberLoanRepaymentDetail { BenJin = BenJin, LiXi = LiXi, ZhiNaJin = ZhiNaJin, MonthStr = TempDt.ToString("yyyy-MM") });

                    }
                    #endregion
                }
            }

            return repayment;
        }


        //    /// <summary>
        //    /// 选择还款类型
        //    /// </summary>
        //    /// <param name="id"></param>
        //    /// <param name="RepaymentType"></param>
        //    /// <returns></returns>
        //    public JsonResult<Repayment> SelectRepaymentType(int id, int RepaymentType)
        //    {
        //        return new JsonResult<Repayment>
        //        {
        //            status = true,
        //            Message = "获取成功",
        //            Data = repayment
        //        };
        //    }

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

        /// <summary>
        /// 根据借款获取还款列表  显示：{还款类型：RepaymentType,还款金额：TotalAmount,还款时间：RepaymentDt}
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public JsonResult<List<MemberLoanRepayment>> GetRepaymentList(int ID)
        {
            List<MemberLoanRepayment> memberLoanRepaymentList = DbHelper.Query<MemberLoanRepayment>($"select * from MemberLoanRepayment where JieID={ID}");

            memberLoanRepaymentList.ForEach(n =>
            {
                n.RepaymentType = EnumExt.GetEnumCustomDescription((RepaymentTypeEnum)(Convert.ToInt32(n.RepaymentType)));
            });

            return new JsonResult<List<MemberLoanRepayment>>
            {
                status = true,
                Message = "获取成功",
                Data = memberLoanRepaymentList
            };
        }

        /// <summary>
        /// 获取还款详情
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public JsonResult<MemberLoanRepayment> GetRepaymentDetail(int ID)
        {
            MemberLoanRepayment memberLoanRepayment = DbHelper.QuerySingle<MemberLoanRepayment>($"select * from MemberLoanRepayment where ID={ID}");
            memberLoanRepayment.RepaymentType = EnumExt.GetEnumCustomDescription((RepaymentTypeEnum)(Convert.ToInt32(memberLoanRepayment.RepaymentType)));


            memberLoanRepayment.DetailList = DbHelper.Query<MemberLoanRepaymentDetail>($"select * from MemberLoanRepaymentDetail where HuanID={ID}");

            return new JsonResult<MemberLoanRepayment>
            {
                status = true,
                Message = "获取成功",
                Data = memberLoanRepayment
            };
        }

        /// <summary>
        /// 获取当前用户下的还款记录
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult<PagedResult<MemberLoanRepayment>>> GetRepaymentList(int MemberID, int PageIndex = 1, int PageSize = 1)
        {
            PagedResult<MemberLoanRepayment> list = await _loanRepaymentService.GetRepaymentList(MemberID, new PagedParameter { PageIndex = PageIndex, PageSize = PageSize });

            list.Items.ToList().ForEach(n =>
            {
                n.RepaymentType = EnumExt.GetEnumCustomDescription((RepaymentTypeEnum)(Convert.ToInt32(n.RepaymentType)));
            });

            return new JsonResult<PagedResult<MemberLoanRepayment>>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };

        }

    }
}