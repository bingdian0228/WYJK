using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.HOME.Models;

namespace WYJK.HOME.Controllers
{
    public class LoanCalculatorController : BaseFilterController
    {
        private ILoanSubjectService _loanSubjectService = new LoanSubjectService();

        // GET: LoanCalculator
        public ActionResult CalculatorFirst()
        {
            return View();
        }

        public ActionResult Calculator(int? id)
        {
            LoanSubject subject = _loanSubjectService.GetChoiceSubject(id);

            if (Session["relations"] == null)
            {
                Session["relations"] = new List<SubjectAnswerRelation>();
            }

            return View(subject);
        }

        /// <summary>
        /// 计算身价
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Calculator(LoanSubject subject)
        {
            AddRelation(subject);
            //计算身价
            string answerIDStr = string.Empty;
            foreach (var item in Relations)
            {
                answerIDStr += item.AnswerID + ",";
            }
            answerIDStr = answerIDStr.Trim(new char[] { ',' });
            //身价计算
            bool flag = _loanSubjectService.ValueCalculation(CommonHelper.CurrentUser.MemberID, answerIDStr);

            if (flag)
            {
                //清空保存的答题
                Session["relations"] = null;

                return Redirect("/LoanCalculator/CalculatorResult");
            }

            return Redirect("/LoanCalculator/Calculator/"+subject.SubjectID);
        }

        /// <summary>
        /// 下一步
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CalculatorNext(LoanSubject subject)
        {
            AddRelation(subject);

            return Redirect("/LoanCalculator/Calculator/" + subject.NextSubjectID);
        }

        public void AddRelation(LoanSubject subject)
        {
            SubjectAnswerRelation relation = new SubjectAnswerRelation();
            relation.SubjectID = subject.SubjectID;
            relation.AnswerID = int.Parse(Request["optionRadio"]);

            List<SubjectAnswerRelation > relations = (List<SubjectAnswerRelation>)Session["relations"];

            SubjectAnswerRelation rl = relations.FirstOrDefault(m => m.SubjectID == subject.SubjectID);

            if (rl == null)
            {
                relations.Add(relation);
            }

            Session["relations"] = relations;
        }

        /// <summary>
        /// 计算结果
        /// </summary>
        /// <returns></returns>
        public ActionResult CalculatorResult()
        {   
            return View(_loanSubjectService.GetMemberValue(CommonHelper.CurrentUser.MemberID));
        }

        public List<SubjectAnswerRelation> Relations
        {
            get
            {
                List<SubjectAnswerRelation > list  = Session["relations"] as List<SubjectAnswerRelation>;
                return list;
            }
        }
    }
}