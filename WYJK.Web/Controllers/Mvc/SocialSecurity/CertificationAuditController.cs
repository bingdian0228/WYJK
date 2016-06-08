using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Web.Controllers.Mvc
{
    /// <summary>
    /// 认证审核
    /// </summary>
    public class CertificationAuditController : Controller
    {
        private readonly ICertificationAuditService _certificationAuditService = new CertificationAuditService();

        /// <summary>
        /// 获取认证审核列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCertificationAuditList(CertificationAuditParameter parameter)
        {
            PagedResult<CertificationAudit> certificationAuditList = _certificationAuditService.GetCertificationAuditList(parameter);

            List<SelectListItem> selectList1 = new List<SelectListItem> { new SelectListItem() { Value = "", Text = "全部" } };
            selectList1.AddRange(EnumExt.GetSelectList(typeof(UserTypeEnum)));
            ViewData["UserTypeList"] = selectList1;


            List<SelectListItem> selectList = new List<SelectListItem> { new SelectListItem() { Value = "", Text = "全部" } };
            selectList.AddRange(EnumExt.GetSelectList(typeof(CertificationAuditEnum)));
            ViewData["StatusType"] = selectList;

            return View(certificationAuditList);
        }

        /// <summary>
        /// 获取认证详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetCertificationAuditDetail(int id)
        {
            CertificationAudit certificationAudit = _certificationAuditService.GetCertificationAuditDetail(id);
            return View(certificationAudit);
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ActionResult CertificationAudit(int id, int status)
        {
            if (status == (int)CertificationAuditEnum.NoPass)
            {
                DbHelper.ExecuteSqlCommand($"update CertificationAudit set Status={(int)CertificationAuditEnum.NoPass},AuditDate=getdate() where ID={id}", null);
            }
            else {
                CertificationAudit certificationAudit = DbHelper.QuerySingle<CertificationAudit>($"select * from CertificationAudit where ID={id}");
                if (certificationAudit.UserType == Convert.ToString((int)UserTypeEnum.QiYe))
                {
                    DbHelper.ExecuteSqlCommand($@"update CertificationAudit set Status={(int)CertificationAuditEnum.Pass},AuditDate=getdate() where ID={id};
                                      update Members set IsAuthentication=1,UserType={(int)UserTypeEnum.QiYe},EnterpriseName='{certificationAudit.EnterpriseName}',EnterpriseType='{certificationAudit.EnterpriseType}',EnterpriseTax='{certificationAudit.EnterpriseTax}',EnterpriseArea='{certificationAudit.EnterpriseArea}',EnterpriseLegal='{certificationAudit.EnterpriseLegal}',EnterpriseLegalIdentityCardNo='{certificationAudit.EnterpriseLegalIdentityCardNo}',EnterprisePeopleNum='{certificationAudit.EnterprisePeopleNum}',SocialSecurityCreditCode='{certificationAudit.SocialSecurityCreditCode}',EnterpriseBusinessLicense='{certificationAudit.EnterpriseBusinessLicense}' where MemberID={certificationAudit.MemberID};", null);
                }
                else {
                    DbHelper.ExecuteSqlCommand($@"update CertificationAudit set Status={(int)CertificationAuditEnum.Pass},AuditDate=getdate() where ID={id};
                                      update Members set IsAuthentication=1,UserType={(int)UserTypeEnum.GeTiJingYing},BusinessName='{certificationAudit.BusinessName}',BusinessUser='{certificationAudit.BusinessUser}',BusinessIdentityCardNo='{certificationAudit.BusinessIdentityCardNo}',BusinessIdentityPhoto='{certificationAudit.BusinessIdentityPhoto}',BusinessLicensePhoto='{certificationAudit.BusinessLicensePhoto}'  where MemberID={certificationAudit.MemberID};", null);
                }
            }

            return Json(new { status = true, message = "审核成功" });
        }
    }
}