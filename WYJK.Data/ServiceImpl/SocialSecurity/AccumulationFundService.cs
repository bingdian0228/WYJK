using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Data.ServiceImpl
{
    /// <summary>
    /// 公积金
    /// </summary>
    public class AccumulationFundService : IAccumulationFundService
    {

        /// <summary>
        /// 获取公积金列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<PagedResult<AccumulationFundShowModel>> GetAccumulationFundList(AccumulationFundParameter parameter)
        {
            string userTypeSql = string.IsNullOrEmpty(parameter.UserType) ? "1=1" : "UserType=" + parameter.UserType;

            string innersqlstr = $@"SELECT     dbo.Members.UserType, dbo.SocialSecurityPeople.SocialSecurityPeopleID, dbo.SocialSecurityPeople.SocialSecurityPeopleName, 
                      dbo.SocialSecurityPeople.IdentityCard, dbo.SocialSecurityPeople.HouseholdProperty, dbo.AccumulationFund.AccumulationFundID, 
                      dbo.AccumulationFund.AccumulationFundArea, dbo.AccumulationFund.AccumulationFundBase, dbo.AccumulationFund.PayProportion, dbo.AccumulationFund.PayTime, 
                      dbo.AccumulationFund.PayMonthCount,AccumulationFund.AlreadyPayMonthCount, dbo.AccumulationFund.PayBeforeMonthCount, dbo.AccumulationFund.Status, dbo.Members.MemberName,  AccumulationFund.UpdateDt,
                      dbo.Members.MemberID,Members.EnterpriseName,Members.BusinessName, ISNULL(dbo.Members.Account,0) Account,  dbo.AccumulationFund.StopDate, dbo.AccumulationFund.ApplyStopDate,AccumulationFund.IsAdjustingBase,AccumulationFund.AdjustingBaseNote,
case when exists(
                             select * from SocialSecurityPeople
                             left join SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID
                              left join AccumulationFund on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
                              where MemberID = members.MemberID and(SocialSecurity.Status = {(int)SocialSecurityStatusEnum.Renew} or AccumulationFund.Status = {(int)SocialSecurityStatusEnum.Renew})
                             ) 
                             then 1 else 0 end IsArrears
                      FROM  dbo.AccumulationFund 
                      INNER JOIN dbo.SocialSecurityPeople ON dbo.AccumulationFund.SocialSecurityPeopleID = dbo.SocialSecurityPeople.SocialSecurityPeopleID 
                      INNER JOIN dbo.Members ON dbo.SocialSecurityPeople.MemberID = dbo.Members.MemberID";
            string sqlstr = string.IsNullOrEmpty(parameter.Status) ? "1=1" : $" Status = {parameter.Status}";
            string sql = $"select * from (select ROW_NUMBER() OVER(ORDER BY S.UpdateDt desc  )AS Row,s.* from ({innersqlstr}) s where " + userTypeSql + $" and {sqlstr} and SocialSecurityPeopleName like '%{parameter.SocialSecurityPeopleName}%' and IdentityCard like '%{parameter.IdentityCard}%' ) ss WHERE ss.Row BETWEEN @StartIndex AND @EndIndex";

            List<AccumulationFundShowModel> modelList = await DbHelper.QueryAsync<AccumulationFundShowModel>(sql, new
            {
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });

            int totalCount = await DbHelper.QuerySingleAsync<int>($"SELECT COUNT(0) AS TotalCount FROM ({innersqlstr}) s where  " + userTypeSql + $" and {sqlstr} and  SocialSecurityPeopleName like '%{parameter.SocialSecurityPeopleName}%' and IdentityCard like '%{parameter.IdentityCard}%'");

            return new PagedResult<AccumulationFundShowModel>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = modelList
            };
        }

        /// <summary>
        /// 修改公积金状态
        /// </summary>
        /// <param name="SocialSecurityPeopleIDs"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool ModifyAccumulationFundStatus(int[] SocialSecurityPeopleIDs, int status)
        {
            string sqlstr = "";
            if (status == (int)SocialSecurityStatusEnum.Normal)
            {
                sqlstr = ",HandleDate=getdate()";
            }
            else if (status == (int)SocialSecurityStatusEnum.AlreadyStop)
            {
                sqlstr = ",StopDate=getdate()";
            }

            string sql = $"update AccumulationFund set Status={status}{sqlstr} where SocialSecurityPeopleID in({string.Join(",", SocialSecurityPeopleIDs)})";

            int result = DbHelper.ExecuteSqlCommand(sql, null);

            return result > 0;
        }

        /// <summary>
        /// 获取公积金详情
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>

        public AccumulationFund GetAccumulationFundDetail(int SocialSecurityPeopleID)
        {
            string sql = $"select AccumulationFund.*,SocialSecurityPeople.SocialSecurityPeopleName from AccumulationFund left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID   where AccumulationFund.SocialSecurityPeopleID ={SocialSecurityPeopleID}";
            AccumulationFund model = DbHelper.QuerySingle<AccumulationFund>(sql);
            return model;
        }

        /// <summary>
        /// 保存公积金号
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <param name="AccumulationFundNo"></param>
        /// <returns></returns>
        public bool SaveAccumulationFundNo(int SocialSecurityPeopleID, string AccumulationFundNo)
        {
            string sql = $"update AccumulationFund set AccumulationFundNo ='{AccumulationFundNo}' where SocialSecurityPeopleID={SocialSecurityPeopleID}";
            int result = DbHelper.ExecuteSqlCommand(sql, null);
            return result > 0;
        }

        /// <summary>
        /// 修改公积金状态
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool ModifyAccumulationFundStatus(int SocialSecurityPeopleID, int status)
        {
            string sqlstr = $"update AccumulationFund set Status={status} where SocialSecurityPeopleID={SocialSecurityPeopleID}";
            int result = DbHelper.ExecuteSqlCommand(sqlstr, null);
            return result >= 0;

        }

        /// <summary>
        /// 获取某用户公积金列表
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public List<AccumulationFundShowModel> GetAccumulationFundList(int MemberID)
        {
            string sqlstr = $"select * from View_AccumulationFund where MemberID={MemberID}";
            List<AccumulationFundShowModel> list = DbHelper.Query<AccumulationFundShowModel>(sqlstr);
            return list;
        }

        /// <summary>
        /// 获取某签约企业下的公积金列表
        /// </summary>
        /// <param name="RelationEnterprise"></param>
        /// <returns></returns>
        public List<AccumulationFundShowModel> GetAccumulationFundListByEnterpriseID(int RelationEnterprise)
        {
            string sqlstr = $"select * from View_AccumulationFund where RelationEnterprise = {RelationEnterprise}";
            List<AccumulationFundShowModel> list = DbHelper.Query<AccumulationFundShowModel>(sqlstr);
            return list;
        }
    }
}
