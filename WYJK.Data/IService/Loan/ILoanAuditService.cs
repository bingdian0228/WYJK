﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IService
{
    /// <summary>
    /// 借款审核接口
    /// </summary>
    public interface ILoanAuditService
    {
        /// <summary>
        /// 获取借款审核列表 
        /// </summary>
        /// <returns></returns>
        PagedResult<MemberLoanAuditList> GetLoanAuditList(MemberLoanAuditListParameter parameter);

        /// <summary>
        /// 借款审核
        /// </summary>
        /// <param name="MembersStr"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        bool MemberLoanAudit(string MembersIDsStr,string Status);
    }
}
