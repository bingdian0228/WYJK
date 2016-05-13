﻿using WYJK.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IServices
{
    public interface ISocialSecurityService
    {
        /// <summary>
        /// 获取社保列表(Admin)
        /// </summary>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        Task<PagedResult<SocialSecurityShowModel>> GetSocialSecurityList(SocialSecurityParameter Parameter);

        /// <summary>
        /// 获取未参保人(Mobile)
        /// </summary>
        /// <returns></returns>
        Task<List<UnInsuredPeople>> GetUnInsuredPeopleList(int memberID, int status);

        /// <summary>
        /// 删除为参保人(Mobile)
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        Task<bool> DeleteUninsuredPeople(int SocialSecurityPeopleID);

        /// <summary>
        /// 添加参保人(Mobile)
        /// </summary>
        /// <param name="socialSecurityPeople"></param>
        /// <returns></returns>
        Task<bool> AddSocialSecurityPeople(SocialSecurityPeople socialSecurityPeople);

        /// <summary>
        /// 修改参保人(Mobile)
        /// </summary>
        /// <param name="socialSecurityPeople"></param>
        /// <returns></returns>
        Task<bool> ModifySocialSecurityPeople(SocialSecurityPeople socialSecurityPeople);

        /// <summary>
        /// 根据区域获取默认社保企业(Mobile)
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        EnterpriseSocialSecurity GetDefaultEnterpriseSocialSecurityByArea(string area,string HouseholdProperty);

        /// <summary>
        /// 添加社保(Mobile)
        /// </summary>
        /// <param name="socialSecurityPeople"></param>
        /// <returns></returns>
        int AddSocialSecurity(SocialSecurity socialSecurity);

        /// <summary>
        /// 修改社保(Modify)
        /// </summary>
        /// <param name="socialSecurity"></param>
        /// <returns></returns>
        //bool ModifySocialSecurity(SocialSecurity socialSecurity);

        /// <summary>
        /// 添加公积金(Mobile)
        /// </summary>
        /// <param name="socialSecurityPeople"></param>
        /// <returns></returns>
        int AddAccumulationFund(AccumulationFund socialSecurity);

        /// <summary>
        /// 修改公积金(Mobile)
        /// </summary>
        /// <param name="socialSecurity"></param>
        /// <returns></returns>
        //bool ModifyAccumulationFund(AccumulationFund accumulationFund);
        /// <summary>
        /// 根据ID查询社保单月金额(Mobile)
        /// </summary>
        /// <param name="SocialSecurityID"></param>
        /// <returns></returns>
        decimal GetSocialSecurityAmount(int SocialSecurityID);

        /// <summary>
        /// 根据ID查询公积金单月金额(Mobile)
        /// </summary>
        /// <param name="AccumulationFundID"></param>
        /// <returns></returns>
        decimal GetAccumulationFundAmount(int AccumulationFundID);

        /// <summary>
        /// 获取参保人详情(Mobile)
        /// </summary>
        /// <param name="SocialSecurityPeopleID">参保人ID</param>
        /// <returns></returns>
        SocialSecurityPeopleDetail GetSocialSecurityPeopleDetail(int SocialSecurityPeopleID);

        /// <summary>
        /// 修改社保状态(Admin)
        /// </summary>
        /// <param name="SocialSecurityPeopleIDs"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        bool ModifySocialStatus(int[] SocialSecurityPeopleIDs, int status);

        string GetSocialPeopleNames(int[] SocialSecurityPeopleIDs);

        /// <summary>
        /// 修改社保状态(Admin)
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        bool ModifySocialStatus(int SocialSecurityPeopleID, int status);

        /// <summary>
        /// 获取参保人列表(Mobile)
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        List<SocialSecurityPeoples> GetSocialSecurityPeopleList(int Status, int MemberID);

        /// <summary>
        /// 根据ID查询社保月数(Mobile)
        /// </summary>
        /// <param name="SocialSecurityID"></param>
        /// <returns></returns>
        int GetSocialSecurityMonthCount(int SocialSecurityID);

        /// <summary>
        /// 根据ID查询公积金月数(Mobile)
        /// </summary>
        /// <param name="AccumulationFundID"></param>
        /// <returns></returns>
        int GetAccumulationFundMonthCount(int AccumulationFundID);

        /// <summary>
        /// 申请停社保(Mobile)
        /// </summary>
        /// <param name="SocialSecurityID"></param>
        /// <returns></returns>
        bool ApplyTopSocialSecurity(StopSocialSecurityParameter parameter);
        /// <summary>
        /// 申请停公积金(Mobile)
        /// </summary>
        /// <param name="AccumulationFundID"></param>
        /// <returns></returns>
        bool ApplyTopAccumulationFund(StopAccumulationFundParameter parameter);

        /// <summary>
        /// 获取社保详情(Admin)
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        SocialSecurity GetSocialSecurityDetail(int SocialSecurityPeopleID);

        /// <summary>
        /// 查询社保公积金详情(Mobile)
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        SocialSecurityDetail GetSocialSecurityAndAccumulationFundDetail(int SocialSecurityPeopleID);

        /// <summary>
        /// 保存社保号(Admin)
        /// </summary>
        /// <param name="SocialSecurityNo"></param>
        /// <returns></returns>
        bool SaveSocialSecurityNo(int SocialSecurityPeopleID, string SocialSecurityNo);

        /// <summary>
        /// 获取投缴剩余月数(Mobile)
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        int GetRemainingMonth(int MemberID);
        /// <summary>
        /// 参保人是否已存在该身份证号(Mobile)
        /// </summary>
        /// <param name="IdentityCard"></param>
        /// <returns></returns>
        bool IsExistsSocialSecurityPeopleIdentityCard(string IdentityCard, int SocialSecurityPeopleID = 0);

        /// <summary>
        /// 获取参保人(Admin)
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        SocialSecurityPeople GetSocialSecurityPeopleForAdmin(int SocialSecurityPeopleID);

        /// <summary>
        /// 获取社保计算结果
        /// </summary>
        /// <param name="InsuranceArea"></param>
        /// <param name="HouseholdProperty"></param>
        /// <param name="SocialSecurityBase"></param>
        /// <param name="AccountRecordBase"></param>
        /// <returns></returns>
        SocialSecurityCalculation GetSocialSecurityCalculationResult(string InsuranceArea, string HouseholdProperty, decimal SocialSecurityBase, decimal AccountRecordBase);

        /// <summary>
        /// 根据用户ID获取月社保公积金总金额（待办与正常）
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        decimal GetMonthTotalAmountByMemberID(int MemberID);
    }
}
