using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IServices
{
    /// <summary>
    /// 用户
    /// </summary>
    public interface IMemberService
    {
        /// <summary>
        /// 注册(Mobile)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Dictionary<bool, string>> RegisterMember(MemberRegisterModel model);

        /// <summary>
        /// 登录(Mobile)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Dictionary<bool, string>> LoginMember(MemberLoginModel model);

        /// <summary>
        /// 忘记密码(Mobile)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Dictionary<bool, string>> ForgetPassword(MemberForgetPasswordModel model);

        /// <summary>
        /// 获取账号（用户名或手机号）获取用户信息(Mobile)
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        Task<Members> GetMemberInfo(int MemberID);

        /// <summary>
        /// 获得用户Id
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<int> GetMemberId(string MemberName);
        /// <summary>
        /// 获取会员信息
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        Members GetMemberInfoForAdmin(int MemberID);

        /// <summary>
        /// 判断原密码是否正确(Mobile)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> IsTrueOldPassword(MemberMidifyPassword model);
        /// <summary>
        /// 修改密码(Mobile)
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        Task<bool> ModifyPassword(MemberMidifyPassword model);

        /// <summary>
        /// 企业资质认证(Mobile)
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task<bool> CommitEnterpriseCertification(EnterpriseCertification parameter);
        /// <summary>
        /// 个体资质认证(Mobile)
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task<bool> CommitPersonCertification(IndividualCertification parameter);

        /// <summary>
        /// 获取账户信息(Mobile)
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        AccountInfo GetAccountInfo(int MemberID);

        /// <summary>
        /// 获取流水记录(Mobile)
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        Task<PagedResult<AccountRecord>> GetAccountRecordList(int MemberID, string ShouZhiType, DateTime? StartTime, DateTime? EndTime, PagedParameter parameter);

        /// <summary>
        /// 获取流水记录(Admin)
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        List<AccountRecord> GetAccountRecordList(int MemberID);
        /// <summary>
        /// 获取账单记录（Mobile)
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        Task<AccountRecord> GetAccountRecord(int ID);
        /// <summary>
        /// 补充用户信息(Mobile)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> ModifyMemberExtensionInformation(ExtensionInformationParameter model);

        /// <summary>
        /// 获取补充信息
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        Task<ExtensionInformation> GetExtensionInformation(int MemberID);

        /// <summary>
        /// 获取补充信息
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        Task<ExtensionInformationParameter> GetMemberExtensionInformation(int MemberID);

        /// <summary>
        /// 获取交费记录列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        PagedResult<MembersPayList> GetMembersPayList(MembersPayParameters parameter);

        /// <summary>
        /// 获取交费金额
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        dynamic GetMembersPayAmount(MembersPayParameters parameter);
        /// <summary>
        /// 会员统计列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        PagedResult<MembersStatistics> GetMembersStatisticsList(MembersParameters parameter);
        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        List<Members> GetMembersList();

        /// <summary>
        /// 获取账户状态
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        bool GetAccountStatus(int MemberID);

        /// <summary>
        /// 获取冻结金额介绍
        /// </summary>
        /// <returns></returns>
        string GetFreezingAmountInstruction();

        /// <summary>
        /// 保存头像
        /// </summary>
        /// <param name="MemberID"></param>
        /// <param name="HeadPortrait"></param>
        /// <returns></returns>
        bool SaveHeadPortrait(int MemberID, string HeadPortrait);
    }
}
