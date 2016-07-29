using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;

namespace WYJK.Data.ServiceImpl
{
    public class LoanRepaymentService : ILoanRepayment
    {
        /// <summary>
        ///  获取还款详情
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public List<MemberLoanRepayment> GetMemberLoanRepaymentList(int ID)
        {
            List<MemberLoanRepayment> list
                 = DbHelper.Query<MemberLoanRepayment>($"select * from MemberLoanRepayment where JieID={ID}");
            return list;
        }
    }
}
