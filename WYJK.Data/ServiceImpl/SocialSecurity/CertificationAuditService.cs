using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;

namespace WYJK.Data.ServiceImpl
{
    public class CertificationAuditService : ICertificationAuditService
    {
        /// <summary>
        /// 获取认证详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CertificationAudit GetCertificationAuditDetail(int id)
        {
            string sqlstr = $@"select Members.MemberID,members.MemberName,members.MemberPhone,CertificationAudit.* from CertificationAudit
  left join Members on CertificationAudit.MemberID = Members.MemberID where ID = {id}";
            CertificationAudit certificationAudit = DbHelper.QuerySingle<CertificationAudit>(sqlstr);
            return certificationAudit;
        }

        /// <summary>
        /// 获取企业审核列表
        /// </summary>
        /// <returns></returns>
        public PagedResult<CertificationAudit> GetCertificationAuditList(CertificationAuditParameter parameter)
        {
            StringBuilder strBuilder = new StringBuilder(" where 1 =1 ");
            if (!string.IsNullOrEmpty(parameter.UserType))
            {
                strBuilder.AppendFormat(" and CertificationAudit.UserType = {0}", parameter.UserType);
            }
            if (!string.IsNullOrEmpty(parameter.Status))
            {
                strBuilder.AppendFormat(" and CertificationAudit.Status = {0}", parameter.Status);
            }

            string innersqlstr = @"select Members.MemberID,members.MemberName,members.MemberPhone,CertificationAudit.ID,CertificationAudit.UserType,CertificationAudit.Status,CertificationAudit.ApplyDate,CertificationAudit.AuditDate from CertificationAudit
  left join Members on CertificationAudit.MemberID = Members.MemberID" + strBuilder.ToString();

            string sqlstr = $"select * from (select ROW_NUMBER() OVER(ORDER BY t.ID desc )AS Row,t.* from ({innersqlstr}) t ) tt WHERE tt.Row BETWEEN @StartIndex AND @EndIndex";
            List<CertificationAudit> certificationAuditList = DbHelper.Query<CertificationAudit>(sqlstr, new
            {
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });
            int totalCount = DbHelper.QuerySingle<int>($"select count(0) from  ({innersqlstr}) t ");

            return new PagedResult<CertificationAudit>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = certificationAuditList
            };
        }
    }
}
