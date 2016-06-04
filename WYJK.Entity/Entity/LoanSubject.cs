using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 题目
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class LoanSubject
    {
        /// <summary>
        /// 题目ID
        /// </summary>		

        public int SubjectID { get; set; }
        /// <summary>
        /// 题目
        /// </summary>		
        public string Subject { get; set; }
        /// <summary>
        /// 排序
        /// </summary>		
        [JsonIgnore]
        public int Sort { get; set; }

        /// <summary>
        /// 答案列表
        /// </summary>
        public List<LoanAnswer> LoanAnswerList { get; set; } = new List<LoanAnswer>();

        /// <summary>
        /// 下一题目ID
        /// </summary>
        public int NextSubjectID { get; set; }

        /// <summary>
        /// 题目总数
        /// </summary>
        public int SubjectCount { get; set; }

    }

    /// <summary>
    /// 身价计算参数
    /// </summary>
    public class ValueCalculationParameter
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int MemberID { get; set; }

        /// <summary>
        /// 课题答案对应组合
        /// </summary>
        public List<SubjectAnswerRelation> RelList { get; set; }
    }

    /// <summary>
    /// 课题答案
    /// </summary>
    public class SubjectAnswerRelation
    {
        /// <summary>
        /// 课题ID
        /// </summary>
        public int SubjectID { get; set; }

        /// <summary>
        /// 答案ID
        /// </summary>
        public int AnswerID { get; set; }
    }
}
