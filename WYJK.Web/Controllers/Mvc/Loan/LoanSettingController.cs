using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Web.Models;

namespace WYJK.Web.Controllers.Mvc.Loan
{
    /// <summary>
    /// 借款设置
    /// </summary>
    [Authorize]
    public class LoanSettingController : Controller
    {
        private ILoanSubjectService _loanSubjectService = new LoanSubjectService();

        /// <summary>
        /// 获取身价计算列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSubjectList(string Subject)
        {
            if (!string.IsNullOrEmpty(Subject))
                Subject = Subject.Replace("'", "''");          
            Dictionary<int, LoanSubject>.ValueCollection list = _loanSubjectService.GetSubjectList(Subject);
            return View("~/Views/Loan/LoanSetting/GetSubjectList.cshtml", list);
        }

        /// <summary>
        /// 创建题目
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddSubject()
        {
            return View("~/Views/Loan/LoanSetting/AddSubject.cshtml");
        }

        /// <summary>
        /// 创建题目
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddSubject(SubjectViewModel model)
        {
            if (!ModelState.IsValid) return View("~/Views/Loan/LoanSetting/AddSubject.cshtml");

            LoanSubject loanSubject = new LoanSubject();
            loanSubject.Subject = model.Subject;
            loanSubject.Sort = model.Sort;

            if (model.Answer != null && model.Answer.Count() > 0)
            {
                for (int i = 0; i < model.Answer.Count(); i++)
                {
                    if (model.Answer[i] != "" && model.LoanAmount[i] != 0)
                        loanSubject.LoanAnswerList.Add(new LoanAnswer { Answer = model.Answer[i], LoanAmount = model.LoanAmount[i] });
                }
            }

            if (_loanSubjectService.AddSubject(loanSubject) == true)
            {
                TempData["Message"] = "保存成功";
                return RedirectToAction("GetSubjectList");
            }
            else {
                TempData["Message"] = "保存失败";
                return RedirectToAction("GetSubjectList");
            }
        }

        /// <summary>
        /// 批量删除题目
        /// </summary>
        /// <param name="SubjectIDs"></param>
        /// <returns></returns>
        public ActionResult BatchDelete(int[] SubjectIDs)
        {
            string SubjectIDstr = string.Join("','", SubjectIDs);
            bool flag = _loanSubjectService.BatchDelete(SubjectIDstr);
            if (flag == true)
                return Json(new { status = true, message = "删除成功" });
            else
                return Json(new { status = false, message = "删除失败" });
        }

        /// <summary>
        /// 编辑题目
        /// </summary>
        /// <param name="SubjectID"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditSubject(int SubjectID)
        {
            LoanSubject subjectModel = _loanSubjectService.GetSubjectDetail(SubjectID);
            return View("~/Views/Loan/LoanSetting/EditSubject.cshtml", subjectModel);
        }

        /// <summary>
        /// 提交编辑题目
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult EditSubject(SubjectViewModel model)
        {
            if (!ModelState.IsValid) return View("~/Views/Loan/LoanSetting/EditSubject.cshtml");

            LoanSubject loanSubject = new LoanSubject();
            loanSubject.SubjectID = model.SubjectID;
            loanSubject.Subject = model.Subject;
            loanSubject.Sort = model.Sort;

            if (model.Answer != null && model.Answer.Count() > 0)
            {
                for (int i = 0; i < model.Answer.Count(); i++)
                {
                    if (model.Answer[i] != "" && model.LoanAmount[i] != 0)
                        loanSubject.LoanAnswerList.Add(new LoanAnswer { Answer = model.Answer[i], LoanAmount = model.LoanAmount[i] });
                }
            }

            if (_loanSubjectService.UpdateSubject(loanSubject) == true)
            {
                TempData["Message"] = "保存成功";
                return RedirectToAction("GetSubjectList");
            }
            else {
                TempData["Message"] = "保存失败";
                return RedirectToAction("GetSubjectList");
            }
        }

        /// <summary>
        /// 借款收费标准
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult MemberLoanSetting()
        {
            MemberLoanSetting memberLoanSetting = DbHelper.QuerySingle<MemberLoanSetting>("select * from MemberLoanSetting");

            return View("~/Views/Loan/LoanSetting/MemberLoanSetting.cshtml", memberLoanSetting);
        }

        /// <summary>
        /// 借款收费标准
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MemberLoanSetting(MemberLoanSetting memberLoanSetting)
        {
            DbHelper.ExecuteSqlCommand($"delete from MemberLoanSetting;insert into MemberLoanSetting values({memberLoanSetting.InThreeMonthsLiLv},{memberLoanSetting.HalfYearLiLv},{memberLoanSetting.OneYearPeriodLiLv},{memberLoanSetting.ZhiNaJinPercent},{memberLoanSetting.WeiYueJinPercent})",null);

            return View("~/Views/Loan/LoanSetting/MemberLoanSetting.cshtml", memberLoanSetting);
        }
    }
}