using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IService
{
    public interface ILoanRepaymentService
    {
        /// <summary>
        /// 获取还款列表
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        List<MemberLoanRepayment> GetMemberLoanRepaymentList(int ID);

        /// <summary>
        /// 根据memberID获取还款列表
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
       Task< PagedResult<MemberLoanRepayment>> GetRepaymentList(int memberID, PagedParameter parameter);

    }
}
