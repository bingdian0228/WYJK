using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IService
{
    /// <summary>
    /// 用户认证
    /// </summary>
    public interface ICertificationAuditService
    {
        /// <summary>
        /// 获取认证审核列表
        /// </summary>
        /// <returns></returns>
        PagedResult<CertificationAudit> GetCertificationAuditList(CertificationAuditParameter parameter);

        /// <summary>
        /// 获取认证详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        CertificationAudit GetCertificationAuditDetail(int id);

    }
}
