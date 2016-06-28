using System;
using System.Collections.Generic;
using System.Linq;
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
    public class LoanController : ApiController
    {
        private ILoanSubjectService _loanSubjectService = new LoanSubjectService();
        private ILoanMemberService _loanMemberService = new LoanMemberService();
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
            decimal AuditAmount = _loanMemberService.GetTotalAuditAmountByMemberID(model.MemberID);

            if (model.ApplyAmount + AuditAmount > AvailableAmount)
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


    }
}