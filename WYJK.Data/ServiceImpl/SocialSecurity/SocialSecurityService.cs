using WYJK.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IServices;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Data.ServiceImpl
{
    public class SocialSecurityService : ISocialSecurityService
    {
        /// <summary>
        /// 删除未参保人
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DeleteUninsuredPeople(int SocialSecurityPeopleID)
        {
            string sql = "delete from AccumulationFund where SocialSecurityPeopleID = @SocialSecurityPeopleID "
                            + " delete from SocialSecurity where SocialSecurityPeopleID = @SocialSecurityPeopleID "
                            + " delete from SocialSecurityPeople where SocialSecurityPeopleID = @SocialSecurityPeopleID ";
            //string sql = " select * from SocialSecurityPeople where SocialSecurityPeopleID = @SocialSecurityPeopleID ";
            DbParameter[] parameters = {
                new SqlParameter("@SocialSecurityPeopleID",SqlDbType.Int) { Value=SocialSecurityPeopleID}
            };
            int result = await DbHelper.ExecuteSqlCommandAsync(sql, parameters);

            return result > 0;

        }

        /// <summary>
        /// 获取社保列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<PagedResult<SocialSecurityShowModel>> GetSocialSecurityList(SocialSecurityParameter parameter)
        {

            string userTypeSql = string.IsNullOrEmpty(parameter.UserType) ? "1=1" : "UserType=" + parameter.UserType;



            string innersqlstr = $@"SELECT dbo.SocialSecurityPeople.SocialSecurityPeopleName, dbo.SocialSecurityPeople.IdentityCard, dbo.SocialSecurityPeople.HouseholdProperty, 
                      dbo.SocialSecurity.Status, dbo.SocialSecurity.InsuranceArea, dbo.SocialSecurity.SocialSecurityBase, dbo.SocialSecurityPeople.SocialSecurityPeopleID, 
                      dbo.SocialSecurity.SocialSecurityID, dbo.SocialSecurity.PayProportion, dbo.SocialSecurity.PayTime, dbo.SocialSecurity.PayMonthCount, SocialSecurity.AlreadyPayMonthCount,
                      dbo.SocialSecurity.PayBeforeMonthCount, dbo.SocialSecurity.BankPayMonth, dbo.SocialSecurity.EnterprisePayMonth, dbo.Members.UserType, SocialSecurity.UpdateDt,
                      dbo.Members.MemberName,Members.EnterpriseName,Members.BusinessName, ISNULL(dbo.Members.Account,0) Account, dbo.SocialSecurity.StopReason, dbo.SocialSecurity.ApplyStopDate, dbo.SocialSecurity.StopDate,SocialSecurity.CollectType,SocialSecurity.MailAddress,SocialSecurity.ContactsPhone,SocialSecurity.ContactsUser,SocialSecurity.MailOrder,SocialSecurity.ExpressCompany, dbo.Members.MemberID,SocialSecurity.IsAdjustingBase,SocialSecurity.AdjustingBaseNote,
                        case when exists(
                             select * from SocialSecurityPeople
                             left join SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID
                              left join AccumulationFund on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
                              where MemberID = members.MemberID and(SocialSecurity.Status = {(int)SocialSecurityStatusEnum.Renew} or AccumulationFund.Status = {(int)SocialSecurityStatusEnum.Renew})
                             ) 
                             then 1 else 0 end IsArrears
                      FROM dbo.SocialSecurityPeople 
                      INNER JOIN dbo.SocialSecurity ON dbo.SocialSecurityPeople.SocialSecurityPeopleID = dbo.SocialSecurity.SocialSecurityPeopleID 
                      INNER JOIN dbo.Members ON dbo.SocialSecurityPeople.MemberID = dbo.Members.MemberID";

            string sqlMemberID = string.IsNullOrEmpty(parameter.MemberID) ? "1=1" : $" MemberID = {parameter.MemberID}";
            string sqlstr = string.IsNullOrEmpty(parameter.Status) ? "1=1" : $" Status = {parameter.Status}";
            string sql = $"select * from (select ROW_NUMBER() OVER(ORDER BY S.UpdateDt desc )AS Row,s.* from ({innersqlstr}) s where " + userTypeSql + $" and {sqlMemberID} and {sqlstr} and SocialSecurityPeopleName like '%{parameter.SocialSecurityPeopleName}%' and IdentityCard like '%{parameter.IdentityCard}%' ) ss WHERE ss.Row BETWEEN @StartIndex AND @EndIndex";

            List<SocialSecurityShowModel> modelList = await DbHelper.QueryAsync<SocialSecurityShowModel>(sql, new
            {
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });

            int totalCount = await DbHelper.QuerySingleAsync<int>($"SELECT COUNT(0) AS TotalCount FROM ({innersqlstr}) t  where  " + userTypeSql + $" and {sqlMemberID}  and {sqlstr} and  SocialSecurityPeopleName like '%{parameter.SocialSecurityPeopleName}%' and IdentityCard like '%{parameter.IdentityCard}%'");

            return new PagedResult<SocialSecurityShowModel>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = modelList
            };
        }

        /// <summary>
        /// 获取未参保人列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<UnInsuredPeople>> GetUnInsuredPeopleList(int memberID, int status, int peopleid = 0)
        {
            //            string sql = $@"select SocialSecurityPeople.SocialSecurityPeopleID,SocialSecurityPeople.SocialSecurityPeopleName,IsPaySocialSecurity, IsPayAccumulationFund, 
            //SocialSecurity.PayTime SSPayTime, SocialSecurity.PayMonthCount SSPayMonthCount, SocialSecurity.SocialSecurityBase,SocialSecurity.PayProportion SSPayProportion,SocialSecurity.Status SSStatus, 
            //AccumulationFund.PayTime AFPayTime, AccumulationFund.PayMonthCount AFPayMonthCount ,AccumulationFund.AccumulationFundBase,AccumulationFund.PayProportion AFPayProportion,AccumulationFund.Status AFStatus 
            //from SocialSecurityPeople 
            //left join SocialSecurity 
            //on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID 
            //left join dbo.AccumulationFund 
            //on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID 
            //where SocialSecurityPeople.MemberID = @MemberID and (SocialSecurity.status = @status or AccumulationFund.status=@status) and SocialSecurityPeople.IsPay=0";

            string sql = $@"select * from (
select SocialSecurityPeopleID,SocialSecurityPeopleName,
case when exists (select * from dbo.SocialSecurity where SocialSecurityPeopleID=t.SocialSecurityPeopleID and Status= 1 and IsPay=0) then 1 else 0 end IsPaySocialSecurity,
(select PayTime from dbo.SocialSecurity where SocialSecurityPeopleID=t.SocialSecurityPeopleID and Status= 1 and IsPay=0) SSPayTime,
(select PayMonthCount from dbo.SocialSecurity where SocialSecurityPeopleID=t.SocialSecurityPeopleID and Status= 1 and IsPay=0) SSPayMonthCount,
(select SocialSecurityBase from dbo.SocialSecurity where SocialSecurityPeopleID=t.SocialSecurityPeopleID and Status= 1 and IsPay=0) SocialSecurityBase,
(select PayProportion from dbo.SocialSecurity where SocialSecurityPeopleID=t.SocialSecurityPeopleID and Status= 1 and IsPay=0) SSPayProportion,
(select Status from dbo.SocialSecurity where SocialSecurityPeopleID=t.SocialSecurityPeopleID and Status= 1 and IsPay=0) SSStatus,

case when exists (select * from dbo.AccumulationFund where SocialSecurityPeopleID=t.SocialSecurityPeopleID and Status= 1 and IsPay=0) then 1 else 0 end IsPayAccumulationFund,
(select PayTime from dbo.AccumulationFund where SocialSecurityPeopleID=t.SocialSecurityPeopleID and Status= 1 and IsPay=0) AFPayTime,
(select PayMonthCount from dbo.AccumulationFund where SocialSecurityPeopleID=t.SocialSecurityPeopleID and Status= 1 and IsPay=0) AFPayMonthCount,
(select AccumulationFundBase from dbo.AccumulationFund where SocialSecurityPeopleID=t.SocialSecurityPeopleID and Status= 1 and IsPay=0) AccumulationFundBase,
(select PayProportion from dbo.AccumulationFund where SocialSecurityPeopleID=t.SocialSecurityPeopleID and Status= 1 and IsPay=0) AFPayProportion,
(select Status from dbo.AccumulationFund where SocialSecurityPeopleID=t.SocialSecurityPeopleID and Status= 1 and IsPay=0) AFStatus

from dbo.SocialSecurityPeople t
where MemberID={memberID} ) tt
where (tt.IsPaySocialSecurity =1 or tt.IsPayAccumulationFund=1)";
            if (peopleid != 0)
            {
                sql += $" and tt.SocialSecurityPeopleID = {peopleid}";
            }
            List<UnInsuredPeople> unIsuredPeopleList = await DbHelper.QueryAsync<UnInsuredPeople>(sql, new
            {
                MemberID = memberID,
                status = status
            });
            return unIsuredPeopleList;
        }

        /// <summary>
        /// 添加参保人
        /// </summary>
        /// <param name="socialSecurityPeople"></param>
        /// <returns></returns>
        public async Task<bool> AddSocialSecurityPeople(SocialSecurityPeople socialSecurityPeople)
        {

            DbParameter[] parameters = new DbParameter[]{
                new SqlParameter("@Flag", SqlDbType.Bit) { Direction = ParameterDirection.Output },
                //参保人
                new SqlParameter("@MemberID", SqlDbType.Int) { Value = socialSecurityPeople.MemberID },
                new SqlParameter("@SocialSecurityPeopleName", SqlDbType.NVarChar, 50) { Value = socialSecurityPeople.SocialSecurityPeopleName },
                new SqlParameter("@IdentityCard", SqlDbType.NVarChar, 50) { Value = socialSecurityPeople.IdentityCard },
                new SqlParameter("@IdentityCardPhoto", SqlDbType.NVarChar, 512) { Value = socialSecurityPeople.IdentityCardPhoto },
                new SqlParameter("@HouseholdProperty", SqlDbType.NVarChar,512) { Value=socialSecurityPeople.HouseholdProperty },

                //社保信息
                new SqlParameter("@SocialSecurityID", SqlDbType.Int) { Value=socialSecurityPeople.socialSecurity.SocialSecurityID },
                //new SqlParameter("@SocialSecurityPeopleID", SqlDbType.Int) { Value=socialSecurityPeople.socialSecurity.SocialSecurityPeopleID },
                //new SqlParameter("@InsuranceArea", SqlDbType.NVarChar,50) { Value=socialSecurityPeople.socialSecurity.InsuranceArea },
                //new SqlParameter("@SocialSecurityBase", SqlDbType.NVarChar,50) { Value=socialSecurityPeople.socialSecurity.SocialSecurityBase },
                //new SqlParameter("@PayProportion", SqlDbType.NVarChar,50) { Value=socialSecurityPeople.socialSecurity.PayProportion },
                //new SqlParameter("@PayTime", SqlDbType.DateTime) { Value=socialSecurityPeople.socialSecurity.PayTime },
                //new SqlParameter("@PayMonthCount", SqlDbType.Int) { Value=socialSecurityPeople.socialSecurity.PayMonthCount },
                //new SqlParameter("@PayBeforeMonthCount", SqlDbType.Int) { Value=socialSecurityPeople.socialSecurity.PayBeforeMonthCount },
                //new SqlParameter("@BankPayMonth", SqlDbType.Int) { Value=socialSecurityPeople.socialSecurity.BankPayMonth },
                //new SqlParameter("@EnterprisePayMonth", SqlDbType.Int) { Value=socialSecurityPeople.socialSecurity.EnterprisePayMonth },
                //new SqlParameter("@Note", SqlDbType.NVarChar,512) { Value=socialSecurityPeople.socialSecurity.Note },
                //new SqlParameter("@Status", SqlDbType.NVarChar,50) { Value=socialSecurityPeople.socialSecurity.Status?? ((int)SocialSecurityStatusEnum.UnInsured).ToString() },

                //公积金信息
                new SqlParameter("@AccumulationFundID", SqlDbType.Int) { Value=socialSecurityPeople.accumulationFund.AccumulationFundID },
               // new SqlParameter("@SocialSecurityPeopleID", SqlDbType.Int) { Value=socialSecurityPeople.socialSecurity.SocialSecurityPeopleID },
               //new SqlParameter("@AccumulationFundArea", SqlDbType.NVarChar,50) { Value=socialSecurityPeople.accumulationFund.AccumulationFundArea },
               // new SqlParameter("@AccumulationFundBase", SqlDbType.NVarChar,50) { Value=socialSecurityPeople.accumulationFund.AccumulationFundBase },
               // new SqlParameter("@AFPayProportion", SqlDbType.NVarChar,50) { Value=socialSecurityPeople.accumulationFund.PayProportion },
               // new SqlParameter("@AFPayTime", SqlDbType.DateTime) { Value=socialSecurityPeople.accumulationFund.PayTime },
               // new SqlParameter("@AFPayMonthCount", SqlDbType.Int) { Value=socialSecurityPeople.accumulationFund.PayMonthCount },
               // new SqlParameter("@AFPayBeforeMonthCount", SqlDbType.Int) { Value=socialSecurityPeople.accumulationFund.PayBeforeMonthCount },
               // new SqlParameter("@AFNote", SqlDbType.NVarChar,512) { Value=socialSecurityPeople.accumulationFund.Note },
               // new SqlParameter("@AFStatus", SqlDbType.NVarChar,50) { Value=socialSecurityPeople.accumulationFund.Status?? ((int)AccumulationFundEnum.UnAccumulationFund).ToString() }

                //new SqlParameter("@Message", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output }
            };
            await DbHelper.ExecuteSqlCommandAsync("SocialSecurityPeople_Add", parameters, CommandType.StoredProcedure);

            bool flag = (bool)parameters[0].Value;

            return flag;
        }

        /// <summary>
        /// 修改参保人
        /// </summary>
        /// <param name="socialSecurityPeople"></param>
        /// <returns></returns>
        public async Task<bool> ModifySocialSecurityPeople(SocialSecurityPeople socialSecurityPeople)
        {

            DbParameter[] parameters = new DbParameter[] {
                new SqlParameter("@Flag", SqlDbType.Bit) { Direction = ParameterDirection.Output },
                //参保人
                new SqlParameter("@SocialSecurityPeopleID", SqlDbType.Int) { Value = socialSecurityPeople.SocialSecurityPeopleID },
                new SqlParameter("@SocialSecurityPeopleName", SqlDbType.NVarChar, 50) { Value = socialSecurityPeople.SocialSecurityPeopleName },
                new SqlParameter("@IdentityCard", SqlDbType.NVarChar, 50) { Value = socialSecurityPeople.IdentityCard },
                new SqlParameter("@IdentityCardPhoto", SqlDbType.NVarChar, 512) { Value = socialSecurityPeople.IdentityCardPhoto },
                new SqlParameter("@HouseholdProperty", SqlDbType.NVarChar,512) { Value=socialSecurityPeople.HouseholdProperty },

                //社保信息
                new SqlParameter("@SocialSecurityID", SqlDbType.Int) { Value=socialSecurityPeople.socialSecurity.SocialSecurityID },
               
                //公积金信息
                new SqlParameter("@AccumulationFundID", SqlDbType.Int) { Value=socialSecurityPeople.accumulationFund.AccumulationFundID },

            };
            await DbHelper.ExecuteSqlCommandAsync("SocialSecurityPeople_Modify", parameters, CommandType.StoredProcedure);

            bool flag = (bool)parameters[0].Value;

            return flag;
        }
        /// <summary>
        /// 根据区域和户籍性质获取默认社保企业
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public EnterpriseSocialSecurity GetDefaultEnterpriseSocialSecurityByArea(string area, string HouseholdProperty)
        {
            //string sqlHouseholdProperty = string.Empty;
            //if (HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.InRural)) ||
            //    HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.OutRural)))
            //{
            //    sqlHouseholdProperty = Convert.ToString((int)HouseholdPropertyEnum.InRural) + "," + Convert.ToString((int)HouseholdPropertyEnum.OutRural);
            //}
            //else if (HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.InTown)) ||
            //  HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.OutTown)))
            //{
            //    sqlHouseholdProperty = Convert.ToString((int)HouseholdPropertyEnum.InTown) + "," + Convert.ToString((int)HouseholdPropertyEnum.OutTown);
            //}


            string sql = $"select * from EnterpriseSocialSecurity where enterpriseArea  like '%{area}%' and IsDefault = 1";
            EnterpriseSocialSecurity model = DbHelper.QuerySingle<EnterpriseSocialSecurity>(sql);
            return model;
        }

        /// <summary>
        /// 根据区域和户籍性质获取社保企业列表
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public List<EnterpriseSocialSecurity> GetEnterpriseSocialSecurityByAreaList(string area, string HouseholdProperty)
        {
            //string sqlHouseholdProperty = string.Empty;
            //if (HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.InRural)) ||
            //    HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.OutRural)))
            //{
            //    sqlHouseholdProperty = Convert.ToString((int)HouseholdPropertyEnum.InRural) + "," + Convert.ToString((int)HouseholdPropertyEnum.OutRural);
            //}
            //else if (HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.InTown)) ||
            //  HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.OutTown)))
            //{
            //    sqlHouseholdProperty = Convert.ToString((int)HouseholdPropertyEnum.InTown) + "," + Convert.ToString((int)HouseholdPropertyEnum.OutTown);
            //}


            string sql = $"select * from EnterpriseSocialSecurity where enterpriseArea  like '%{area}%'";
            List<EnterpriseSocialSecurity> list = DbHelper.Query<EnterpriseSocialSecurity>(sql);
            return list;
        }

        /// <summary>
        /// 添加社保
        /// </summary>
        /// <param name="socialSecurityPeople"></param>
        /// <returns></returns>
        public int AddSocialSecurity(SocialSecurity socialSecurity)
        {
            EnterpriseSocialSecurity model = GetDefaultEnterpriseSocialSecurityByArea(socialSecurity.InsuranceArea, socialSecurity.HouseholdProperty);
            decimal value = 0;
            //model.PersonalShiYeTown
            if (socialSecurity.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.ThisCityRural)) ||
    socialSecurity.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.ThisProvinceRural))||
    socialSecurity.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.OtherProvinceRural)))
            {
                value = model.PersonalShiYeRural;
            }
            else if (socialSecurity.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.ThisCityTown)) ||
              socialSecurity.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.ThisProvinceTown))||
              socialSecurity.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.OtherProvinceTown)))
            {
                value = model.PersonalShiYeTown;
            }

            decimal PayProportion = model.CompYangLao + model.CompYiLiao + model.CompShiYe + model.CompGongShang + model.CompShengYu
                + model.PersonalYangLao + model.PersonalYiLiao + value + model.PersonalGongShang + model.PersonalShengYu;

            string sql = "insert into SocialSecurity(SocialSecurityPeopleID,InsuranceArea,SocialSecurityBase,PayProportion,PayTime,PayMonthCount,PayBeforeMonthCount,BankPayMonth,EnterprisePayMonth,Note,Status,RelationEnterprise)"
                + $" values(@SocialSecurityPeopleID,@InsuranceArea,@SocialSecurityBase,@PayProportion,@PayTime,@PayMonthCount,@PayBeforeMonthCount,@BankPayMonth,@EnterprisePayMonth,@Note,{(int)SocialSecurityStatusEnum.UnInsured},@RelationEnterprise)";
            DbParameter[] parameters = new DbParameter[] {
                new SqlParameter("@SocialSecurityPeopleID",socialSecurity.SocialSecurityPeopleID),
                new SqlParameter("@InsuranceArea",socialSecurity.InsuranceArea),
                new SqlParameter("@SocialSecurityBase",socialSecurity.SocialSecurityBase),
                new SqlParameter("@PayProportion",PayProportion),
                new SqlParameter("@PayTime",socialSecurity.PayTime),
                new SqlParameter("@PayMonthCount",socialSecurity.PayMonthCount),
                new SqlParameter("@PayBeforeMonthCount",socialSecurity.PayBeforeMonthCount),
                new SqlParameter("@BankPayMonth",socialSecurity.BankPayMonth),
                new SqlParameter("@EnterprisePayMonth",socialSecurity.EnterprisePayMonth),
                new SqlParameter("@Note",socialSecurity.Note ?? ""),
                new SqlParameter("@RelationEnterprise",model.EnterpriseID)
            };

            int result = DbHelper.ExecuteSqlCommandScalar(sql, parameters);
            return result;

        }



        /// <summary>
        /// 添加公积金
        /// </summary>
        /// <param name="socialSecurityPeople"></param>
        /// <returns></returns>
        public int AddAccumulationFund(AccumulationFund accumulationFund)
        {
            EnterpriseSocialSecurity model = GetDefaultEnterpriseSocialSecurityByArea(accumulationFund.AccumulationFundArea, accumulationFund.HouseholdProperty);
            decimal PayProportion = model.CompProportion + model.PersonalProportion;
            string sql = "insert into AccumulationFund(SocialSecurityPeopleID,AccumulationFundArea,AccumulationFundBase,PayProportion,PayTime,PayMonthCount,PayBeforeMonthCount,Note,Status,RelationEnterprise,AccumulationFundType)"
                + $" values(@SocialSecurityPeopleID,@AccumulationFundArea,@AccumulationFundBase,@PayProportion,@PayTime,@PayMonthCount,@PayBeforeMonthCount,@Note,{(int)SocialSecurityStatusEnum.UnInsured},@RelationEnterprise,@AccumulationFundType)";
            DbParameter[] parameters = new DbParameter[] {
                new SqlParameter("@SocialSecurityPeopleID",accumulationFund.SocialSecurityPeopleID),
                new SqlParameter("@AccumulationFundArea",accumulationFund.AccumulationFundArea),
                new SqlParameter("@AccumulationFundBase",accumulationFund.AccumulationFundBase),
                new SqlParameter("@PayProportion",PayProportion),
                new SqlParameter("@PayTime",accumulationFund.PayTime),
                new SqlParameter("@PayMonthCount",accumulationFund.PayMonthCount),
                new SqlParameter("@PayBeforeMonthCount",accumulationFund.PayBeforeMonthCount),
                new SqlParameter("@Note",accumulationFund.Note ?? ""),
                new SqlParameter("@RelationEnterprise",model.EnterpriseID),
                new SqlParameter("@AccumulationFundType",accumulationFund.AccumulationFundType)
            };

            int result = DbHelper.ExecuteSqlCommandScalar(sql, parameters);
            return result;
        }

        /// <summary>
        /// 根据ID查询社保单月金额
        /// </summary>
        /// <param name="SocialSecurityID"></param>
        /// <returns></returns>
        public decimal GetSocialSecurityAmount(int SocialSecurityID)
        {
            string sql = "select SocialSecurityBase * PayProportion/100 from SocialSecurity where SocialSecurityID = @SocialSecurityID";
            decimal result = DbHelper.QuerySingle<decimal>(sql, new { SocialSecurityID = SocialSecurityID });
            return result;
        }

        /// <summary>
        /// 根据ID查询社保月数
        /// </summary>
        /// <param name="SocialSecurityID"></param>
        /// <returns></returns>
        public int GetSocialSecurityMonthCount(int SocialSecurityID)
        {
            string sql = "select PayMonthCount from SocialSecurity where SocialSecurityID = @SocialSecurityID";
            int result = DbHelper.QuerySingle<int>(sql, new { SocialSecurityID = SocialSecurityID });
            return result;
        }

        /// <summary>
        /// 根据ID查询公积金单月金额
        /// </summary>
        /// <param name="AccumulationFundID"></param>
        /// <returns></returns>
        public decimal GetAccumulationFundAmount(int AccumulationFundID)
        {
            string sql = "select AccumulationFundBase * PayProportion/100 from AccumulationFund where AccumulationFundID = @AccumulationFundID";
            decimal result = DbHelper.QuerySingle<decimal>(sql, new { AccumulationFundID = AccumulationFundID });
            return result;
        }

        /// <summary>
        /// 根据ID查询公积金月数
        /// </summary>
        /// <param name="AccumulationFundID"></param>
        /// <returns></returns>
        public int GetAccumulationFundMonthCount(int AccumulationFundID)
        {
            string sql = "select PayMonthCount from AccumulationFund where AccumulationFundID = @AccumulationFundID";
            int result = DbHelper.QuerySingle<int>(sql, new { AccumulationFundID = AccumulationFundID });
            return result;
        }

        /// <summary>
        /// 获取参保人详情
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        public SocialSecurityPeopleDetail GetSocialSecurityPeopleDetail(int SocialSecurityPeopleID)
        {
            string sql = $"select IsPaySocialSecurity,IsPayAccumulationFund, SocialSecurityPeopleID,SocialSecurityPeopleName,IdentityCard,ISNULL(IdentityCardPhoto,'') IdentityCardPhoto,HouseholdProperty  from SocialSecurityPeople where SocialSecurityPeopleID={SocialSecurityPeopleID} ";

            SocialSecurityPeopleDetail model = DbHelper.QuerySingle<SocialSecurityPeopleDetail>(sql);

            return model;
        }

        /// <summary>
        /// 修改社保状态
        /// </summary>
        /// <returns></returns>
        public bool ModifySocialStatus(int[] SocialSecurityPeopleIDs, int status)
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

            string sql = $"update SocialSecurity set Status={status}{sqlstr} where SocialSecurityPeopleID in({string.Join(",", SocialSecurityPeopleIDs)})";

            int result = DbHelper.ExecuteSqlCommand(sql, null);

            return result > 0;
        }

        /// <summary>
        /// 手机获取参保人列表
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<SocialSecurityPeoples> GetSocialSecurityPeopleList(int Status, int MemberID)
        {
            string sql = "select ssp.SocialSecurityPeopleID,ssp.SocialSecurityPeopleName,ss.PayTime SSPayTime,ISNULL(ss.AlreadyPayMonthCount,0) SSAlreadyPayMonthCount,ss.Status SSStatus,ss.PayMonthCount SSRemainingMonthCount, af.PayTime AFPayTime,ISNULL(af.AlreadyPayMonthCount,0) AFAlreadyPayMonthCount,af.Status AFStatus,af.PayMonthCount AFRemainingMonthCount"
                        + " from SocialSecurityPeople ssp"
                        + " left join SocialSecurity ss on ssp.SocialSecurityPeopleID = ss.SocialSecurityPeopleID"
                        + " left join AccumulationFund af on ssp.SocialSecurityPeopleID = af.SocialSecurityPeopleID"
                        + $" where (ss.Status = {Status} or af.Status = {Status}) and ssp.MemberID = {MemberID}";
            List<SocialSecurityPeoples> socialSecurityPeopleList = DbHelper.Query<SocialSecurityPeoples>(sql);
            return socialSecurityPeopleList;
        }

        /// <summary>
        /// 申请停社保
        /// </summary>
        /// <param name="SocialSecurityID"></param>
        /// <returns></returns>
        public bool ApplyTopSocialSecurity(StopSocialSecurityParameter parameter)
        {
            string sql = $"update SocialSecurity set Status={(int)SocialSecurityStatusEnum.WaitingStop}, StopMethod = {(int)waitingTopEnum.Apply},ApplyStopDate=getdate(),StopReason='{parameter.StopReason}',CollectType='{parameter.CollectType}',MailAddress='{parameter.MailAddress}',ContactsPhone='{parameter.ContactsPhone}',ContactsUser='{parameter.ContactsUser}' where SocialSecurityPeopleID ={parameter.SocialSecurityPeopleID}";

            int result = DbHelper.ExecuteSqlCommand(sql, null);

            return result > 0;
        }
        /// <summary>
        /// 申请停公积金
        /// </summary>
        /// <param name="AccumulationFundID"></param>
        /// <returns></returns>
        public bool ApplyTopAccumulationFund(StopAccumulationFundParameter parameter)
        {
            string sql = $"update AccumulationFund set Status={(int)SocialSecurityStatusEnum.WaitingStop}, StopMethod = {(int)waitingTopEnum.Apply},ApplyStopDate=getdate(),AccumulationFundTopType={parameter.AccumulationFundTopType},CompanyName='{parameter.CompanyName}',CompanyAccumulationFundCode='{parameter.CompanyAccumulationFundCode}' where SocialSecurityPeopleID ={parameter.SocialSecurityPeopleID}";

            int result = DbHelper.ExecuteSqlCommand(sql, null);

            return result > 0;
        }

        /// <summary>
        /// 获取社保详情
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        public SocialSecurity GetSocialSecurityDetail(int SocialSecurityPeopleID)
        {
            string sql = $"select SocialSecurity.*,SocialSecurityPeople.SocialSecurityPeopleName from SocialSecurity left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID   where SocialSecurity.SocialSecurityPeopleID ={SocialSecurityPeopleID}";


            SocialSecurity model = DbHelper.QuerySingle<SocialSecurity>(sql);
            return model;
        }

        /// <summary>
        /// 获取社保和公积金详情
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        public SocialSecurityDetail GetSocialSecurityAndAccumulationFundDetail(int SocialSecurityPeopleID)
        {
            string sql = $"select SocialSecurityPeople.SocialSecurityPeopleName,SocialSecurity.Status SSStatus, SocialSecurity.SocialSecurityBase,SocialSecurity.InsuranceArea,SocialSecurity.PayTime SSPayTime, ISNULL(SocialSecurity.AlreadyPayMonthCount, 0) SSAlreadyPayMonthCount,SocialSecurity.PayMonthCount SSRemainingMonths,"
                            + " AccumulationFund.Status AFStatus,AccumulationFund.AccumulationFundBase,AccumulationFund.AccumulationFundArea,AccumulationFund.PayTime AFPayTime, ISNULL(AccumulationFund.AlreadyPayMonthCount, 0) AFAlreadyPayMonthCount,AccumulationFund.PayMonthCount AFRemainingMonths"
                            + " from SocialSecurityPeople"
                            + " left join SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID"
                            + " left join AccumulationFund on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID"
                            + $" where SocialSecurityPeople.SocialSecurityPeopleID = {SocialSecurityPeopleID}";


            SocialSecurityDetail model = DbHelper.QuerySingle<SocialSecurityDetail>(sql);
            return model;
        }

        /// <summary>
        /// 保存社保号
        /// </summary>
        /// <param name="SocialSecurityNo"></param>
        /// <returns></returns>
        public bool SaveSocialSecurityNo(int SocialSecurityPeopleID, string SocialSecurityNo)
        {
            string sql = $"update SocialSecurity set SocialSecurityNo ='{SocialSecurityNo}' where SocialSecurityPeopleID={SocialSecurityPeopleID}";
            int result = DbHelper.ExecuteSqlCommand(sql, null);
            return result > 0;
        }

        /// <summary>
        /// 获取投缴剩余月数
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public int GetRemainingMonth(int MemberID)
        {
            string sql = " --社保月金额"
                         + " declare @SocialSecurityAmount decimal = 0"
                         + " select @SocialSecurityAmount+= SocialSecurity.SocialSecurityBase * SocialSecurity.PayProportion / 100 from SocialSecurityPeople"
                         + " left join SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID"
                         + $" where SocialSecurity.Status in ({SocialSecurityStatusEnum.WaitingHandle},{SocialSecurityStatusEnum.Normal})"
                         + " print @SocialSecurityAmount"
                         + " --公积金月金额"
                         + " declare @AccumulationFundAmount decimal = 0"
                         + " select @AccumulationFundAmount+= AccumulationFund.AccumulationFundBase * accumulationfund.PayProportion / 100  from SocialSecurityPeople"
                         + " left join AccumulationFund on socialsecuritypeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID"
                         + $" where AccumulationFund.Status in ({SocialSecurityStatusEnum.WaitingHandle},{SocialSecurityStatusEnum.Normal})"
                         + " print @AccumulationFundAmount"
                         + $" select(select Account from Members where MemberID = {MemberID}) / (@SocialSecurityAmount + @AccumulationFundAmount)";
            int result = DbHelper.QuerySingle<int>(sql);
            return result;
        }

        /// <summary>
        /// 参保人是否已存在该身份证号
        /// </summary>
        /// <param name="IdentityCard"></param>
        /// <returns></returns>
        public bool IsExistsSocialSecurityPeopleIdentityCard(string IdentityCard, int SocialSecurityPeopleID)
        {
            string sql = $"select COUNT(0) from SocialSecurityPeople where IdentityCard='{IdentityCard}' and SocialSecurityPeopleID <> {SocialSecurityPeopleID}";
            int result = DbHelper.QuerySingle<int>(sql);
            return result > 0;
        }

        ///// <summary>
        ///// 修改社保
        ///// </summary>
        ///// <param name="socialSecurity"></param>
        ///// <returns></returns>
        //public bool ModifySocialSecurity(SocialSecurity socialSecurity)
        //{
        //    EnterpriseSocialSecurity model = GetDefaultEnterpriseSocialSecurityByArea(socialSecurity.InsuranceArea);
        //    decimal PayProportion = model.CompYangLao + model.CompYiLiao + model.CompShiYe + model.CompGongShang + model.CompShengYu
        //        + model.PersonalYangLao + model.PersonalYiLiao + model.PersonalShiYeTown + model.PersonalGongShang + model.PersonalShengYu;

        //    //string sql = "insert into SocialSecurity(InsuranceArea,SocialSecurityBase,PayProportion,PayTime,PayMonthCount,PayBeforeMonthCount,BankPayMonth,EnterprisePayMonth,Note)"
        //    //    + " values(@InsuranceArea,@SocialSecurityBase,@PayProportion,@PayTime,@PayMonthCount,@PayBeforeMonthCount,@BankPayMonth,@EnterprisePayMonth,@Note)";
        //    //DbParameter[] parameters = new DbParameter[] {
        //    //    new SqlParameter("@InsuranceArea",socialSecurity.InsuranceArea),
        //    //    new SqlParameter("@SocialSecurityBase",socialSecurity.SocialSecurityBase),
        //    //    new SqlParameter("@PayProportion",PayProportion),
        //    //    new SqlParameter("@PayTime",socialSecurity.PayTime),
        //    //    new SqlParameter("@PayMonthCount",socialSecurity.PayMonthCount),
        //    //    new SqlParameter("@PayBeforeMonthCount",socialSecurity.PayBeforeMonthCount),
        //    //    new SqlParameter("@BankPayMonth",socialSecurity.BankPayMonth),
        //    //    new SqlParameter("@EnterprisePayMonth",socialSecurity.EnterprisePayMonth),
        //    //    new SqlParameter("@Note",socialSecurity.Note ?? ""),
        //    //    new SqlParameter("@RelationEnterprise",model.EnterpriseID)
        //    //};

        //    string sql = "update SocialSecurity set InsuranceArea=@InsuranceArea,SocialSecurityBase=@SocialSecurityBase,PayProportion=@PayProportion,PayTime=@PayTime,PayMonthCount=@PayMonthCount,PayBeforeMonthCount=@PayBeforeMonthCount,BankPayMonth=@BankPayMonth,EnterprisePayMonth=@EnterprisePayMonth,Note=@Note,RelationEnterprise=@RelationEnterprise";
        //    DbParameter[] parameters = new DbParameter[] {
        //        new SqlParameter("@InsuranceArea",socialSecurity.InsuranceArea),
        //        new SqlParameter("@SocialSecurityBase",socialSecurity.SocialSecurityBase),
        //        new SqlParameter("@PayProportion",PayProportion),
        //        new SqlParameter("@PayTime",socialSecurity.PayTime),
        //        new SqlParameter("@PayMonthCount",socialSecurity.PayMonthCount),
        //        new SqlParameter("@PayBeforeMonthCount",socialSecurity.PayBeforeMonthCount),
        //        new SqlParameter("@BankPayMonth",socialSecurity.BankPayMonth),
        //        new SqlParameter("@EnterprisePayMonth",socialSecurity.EnterprisePayMonth),
        //        new SqlParameter("@Note",socialSecurity.Note ?? ""),
        //        new SqlParameter("@RelationEnterprise",model.EnterpriseID)
        //    };


        //    int result = DbHelper.ExecuteSqlCommand(sql, parameters);
        //    return result > 0;
        //}

        ///// <summary>
        ///// 修改公积金
        ///// </summary>
        ///// <param name="socialSecurity"></param>
        ///// <returns></returns>
        //public bool ModifyAccumulationFund(AccumulationFund accumulationFund)
        //{
        //    EnterpriseSocialSecurity model = GetDefaultEnterpriseSocialSecurityByArea(accumulationFund.AccumulationFundArea);
        //    decimal PayProportion = model.CompProportion + model.PersonalProportion;
        //    //string sql = "insert into AccumulationFund(AccumulationFundArea,AccumulationFundBase,PayProportion,PayTime,PayMonthCount,PayBeforeMonthCount,Note)"
        //    //    + " values(@AccumulationFundArea,@AccumulationFundBase,@PayProportion,@PayTime,@PayMonthCount,@PayBeforeMonthCount,@Note)";
        //    //DbParameter[] parameters = new DbParameter[] {
        //    //    new SqlParameter("@AccumulationFundArea",accumulationFund.AccumulationFundArea),
        //    //    new SqlParameter("@AccumulationFundBase",accumulationFund.AccumulationFundBase),
        //    //    new SqlParameter("@PayProportion",PayProportion),
        //    //    new SqlParameter("@PayTime",accumulationFund.PayTime),
        //    //    new SqlParameter("@PayMonthCount",accumulationFund.PayMonthCount),
        //    //    new SqlParameter("@PayBeforeMonthCount",accumulationFund.PayBeforeMonthCount),
        //    //    new SqlParameter("@Note",accumulationFund.Note ?? ""),
        //    //    new SqlParameter("@RelationEnterprise",model.EnterpriseID)
        //    //};

        //    string sql = "update AccumulationFund set AccumulationFundArea=@AccumulationFundArea,AccumulationFundBase=@AccumulationFundBase,PayProportion=@PayProportion,PayTime=@PayTime,PayMonthCount=@PayMonthCount,PayBeforeMonthCount=@PayBeforeMonthCount,Note=@Note,RelationEnterprise=@RelationEnterprise";
        //    DbParameter[] parameters = new DbParameter[] {
        //        new SqlParameter("@AccumulationFundArea",accumulationFund.AccumulationFundArea),
        //        new SqlParameter("@AccumulationFundBase",accumulationFund.AccumulationFundBase),
        //        new SqlParameter("@PayProportion",PayProportion),
        //        new SqlParameter("@PayTime",accumulationFund.PayTime),
        //        new SqlParameter("@PayMonthCount",accumulationFund.PayMonthCount),
        //        new SqlParameter("@PayBeforeMonthCount",accumulationFund.PayBeforeMonthCount),
        //        new SqlParameter("@Note",accumulationFund.Note ?? ""),
        //        new SqlParameter("@RelationEnterprise",model.EnterpriseID)
        //    };

        //    int result = DbHelper.ExecuteSqlCommand(sql, parameters);
        //    return result > 0;
        //}

        /// <summary>
        /// 修改社保状态
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool ModifySocialStatus(int SocialSecurityPeopleID, int status)
        {
            string sqlstr = $"update SocialSecurity set Status = {status} where SocialSecurityPeopleID={SocialSecurityPeopleID}";
            int result = DbHelper.ExecuteSqlCommand(sqlstr, null);
            return result >= 0;
        }

        /// <summary>
        /// 获取参保人(Admin)
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        public SocialSecurityPeople GetSocialSecurityPeopleForAdmin(int SocialSecurityPeopleID)
        {
            string sqlstr = $"select * from SocialSecurityPeople where SocialSecurityPeopleID={SocialSecurityPeopleID}";
            SocialSecurityPeople model = DbHelper.QuerySingle<SocialSecurityPeople>(sqlstr);
            return model;
        }

        /// <summary>
        /// 获取参保人列表
        /// </summary>
        /// <param name="SocialSecurityPeopleIDs"></param>
        /// <returns></returns>
        public string GetSocialPeopleNames(int[] SocialSecurityPeopleIDs)
        {
            string SocialSecurityPeopleIDStr = string.Join("','", SocialSecurityPeopleIDs);
            string strsql = $@"declare @Name nvarchar(50)
                            select @Name = ISNULL(@Name + '，', '') + SocialSecurityPeopleName from SocialSecurityPeople where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}');
                            select @Name";

            return DbHelper.QuerySingle<string>(strsql);
        }


        /// <summary>
        /// 保费计算器
        /// </summary>
        /// <returns></returns>
        public SocialSecurityCalculation GetSocialSecurityCalculationResult(string InsuranceArea, string HouseholdProperty, decimal SocialSecurityBase, decimal AccountRecordBase)
        {
            EnterpriseSocialSecurity model = GetDefaultEnterpriseSocialSecurityByArea(InsuranceArea, HouseholdProperty);
            decimal value = 0;
            //model.PersonalShiYeTown
            if (HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.ThisCityRural)) ||
    HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.ThisProvinceRural))||
    HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.OtherProvinceRural)))
            {
                value = model.PersonalShiYeRural;
            }
            else if (HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.ThisCityTown)) ||
              HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.ThisProvinceTown))||
              HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.OtherProvinceTown)))
            {
                value = model.PersonalShiYeTown;
            }

            decimal SSPayProportion = model.CompYangLao + model.CompYiLiao + model.CompShiYe + model.CompGongShang + model.CompShengYu
                + model.PersonalYangLao + model.PersonalYiLiao + value + model.PersonalGongShang + model.PersonalShengYu;

            decimal AFPayProportion = model.CompProportion + model.PersonalProportion;

            return new SocialSecurityCalculation { SocialSecuritAmount = SocialSecurityBase * SSPayProportion / 100, AccumulationFundAmount = AccountRecordBase * AFPayProportion / 100 };
        }

        /// <summary>
        /// 根据用户ID获取月社保公积金总金额（待办与正常）
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public decimal GetMonthTotalAmountByMemberID(int MemberID)
        {
            string sqlstr = $@"declare @SocialSecurityAmount decimal = 0, @AccumulationFundAmount decimal = 0,@totalAmount decimal =0
select @SocialSecurityAmount += SocialSecurity.SocialSecurityBase * SocialSecurity.PayProportion / 100 from SocialSecurityPeople
      left join SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID
      where SocialSecurityPeople.MemberID = {MemberID} and SocialSecurity.Status in(2,3);
            select @AccumulationFundAmount += AccumulationFund.AccumulationFundBase * AccumulationFund.PayProportion / 100 from SocialSecurityPeople
                  left join AccumulationFund on socialsecuritypeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
                  where SocialSecurityPeople.MemberID = {MemberID} and AccumulationFund.Status in(2,3);
            select @totalAmount = @SocialSecurityAmount + @AccumulationFundAmount;
select @totalAmount";

            decimal result = DbHelper.QuerySingle<decimal>(sqlstr);
            return result;
        }

        /// <summary>
        /// 是否存在续费
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public bool IsExistsRenew(int MemberID)
        {
            string sqlstr = $@"select count(1) from SocialSecurityPeople 
  left join SocialSecurity on socialsecuritypeople.SocialSecurityPeopleID = socialsecurity.SocialSecurityPeopleID
  left join AccumulationFund on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
  where SocialSecurityPeople.MemberID = {MemberID} and(SocialSecurity.Status = {(int)SocialSecurityStatusEnum.Renew} or AccumulationFund.Status = {(int)SocialSecurityStatusEnum.Renew})";
            int result = DbHelper.QuerySingle<int>(sqlstr);
            return result > 0;
        }

        /// <summary>
        /// 根据MemberID获取社保待续费列表
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public List<SocialSecurityPeople> GetSocialSecurityRenewListByMemberID(int MemberID)
        {
            string sqlstr = $@"select SocialSecurityPeople.* from SocialSecurity
  left join SocialSecurityPeople on SocialSecurity.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID
  where SocialSecurityPeople.MemberID ={MemberID}
            and(SocialSecurity.Status in({(int)SocialSecurityStatusEnum.Renew}))";

            return DbHelper.Query<SocialSecurityPeople>(sqlstr);
        }

        /// <summary>
        /// 根据MemberID获取公积金待续费列表
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public List<SocialSecurityPeople> GetAccumulationFundRenewListByMemberID(int MemberID)
        {
            string sqlstr = $@"select 1 from AccumulationFund
  left join SocialSecurityPeople on AccumulationFund.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID
  where SocialSecurityPeople.MemberID ={MemberID}
            and(AccumulationFund.Status in({(int)SocialSecurityStatusEnum.Renew}))";

            return DbHelper.Query<SocialSecurityPeople>(sqlstr); ;
        }

        /// <summary>
        /// 更新用户ID下的所有待续费的社保和公积金变成正常
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public void UpdateRenewToNormalByMemberID(int MemberID, int MonthCount)
        {
            string sqlstr = $@"update SocialSecurity set SocialSecurity.Status = {(int)SocialSecurityStatusEnum.Normal},SocialSecurity.PayMonthCount= case when SocialSecurity.PayMonthCount>{MonthCount} then SocialSecurity.PayMonthCount else {MonthCount} end where socialsecurity.SocialSecurityID in
  (select SocialSecurity.SocialSecurityID from SocialSecurity
left join SocialSecurityPeople on SocialSecurity.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID
  where SocialSecurityPeople.MemberID = {MemberID} and SocialSecurity.Status = {(int)SocialSecurityStatusEnum.Renew});
update AccumulationFund set AccumulationFund.Status = {(int)SocialSecurityStatusEnum.Normal},AccumulationFund.PayMonthCount= case when AccumulationFund.PayMonthCount> {MonthCount} then AccumulationFund.PayMonthCount else {MonthCount} end where AccumulationFund.AccumulationFundID in
  (select AccumulationFund.AccumulationFundID from AccumulationFund
left join SocialSecurityPeople on AccumulationFund.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID
  where SocialSecurityPeople.MemberID = {MemberID} and AccumulationFund.Status = {(int)SocialSecurityStatusEnum.Renew});
  ";
            DbHelper.ExecuteSqlCommand(sqlstr, null);
        }

        /// <summary>
        /// 获取当前基数
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        public AdjustingBase GetCurrentBase(int SocialSecurityPeopleID)
        {
            string sqlstr = $@"select SocialSecurityPeople.SocialSecurityPeopleID,case when SocialSecurity.Status={(int)SocialSecurityStatusEnum.Normal} then 1 else 0 end IsPaySocialSecurity,
case when  AccumulationFund.Status={(int)SocialSecurityStatusEnum.Normal} then 1 else 0 end IsPayAccumulationFund,SocialSecurity.SocialSecurityBase,AccumulationFund.AccumulationFundBase,
SocialSecurityPeople.HouseholdProperty,SocialSecurity.InsuranceArea,AccumulationFund.AccumulationFundArea,
ess1.SocialAvgSalary*ess1.MinSocial/100 SocialSecurityMinBase,ess1.SocialAvgSalary*ess1.MaxSocial/100 SocialSecurityMaxBase,
ess2.MinAccumulationFund AccumulationFundMinBase,ess2.MaxAccumulationFund AccumulationFundMaxBase
 from SocialSecurityPeople
left join SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID
left join EnterpriseSocialSecurity ESS1 on SocialSecurity.RelationEnterprise = ESS1.EnterpriseID
left join AccumulationFund on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
left join EnterpriseSocialSecurity ESS2 on AccumulationFund.RelationEnterprise = ESS2.EnterpriseID
where SocialSecurityPeople.SocialSecurityPeopleID={SocialSecurityPeopleID}";

            AdjustingBase model = DbHelper.QuerySingle<AdjustingBase>(sqlstr);
            return model;
        }

        /// <summary>
        /// 调整基数
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public int AddAdjustingBase(AdjustingBaseParameter parameter)
        {
            string sqlstr = string.Empty;
            string orderCode = DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000).ToString().PadLeft(3, '0');
            decimal SSBaseServiceCharge = 0, SSCurrentBase = 0, AFBaseServiceCharge = 0, AFCurrentBase = 0;

            if (parameter.IsPaySocialSecurity)
            {
                SSBaseServiceCharge = DbHelper.QuerySingle<decimal>("select BaseServiceCharge from CostParameterSetting where Status=0");
                SSCurrentBase = DbHelper.QuerySingle<decimal>($"select SocialSecurityBase from SocialSecurity where SocialSecurityPeopleID={parameter.SocialSecurityPeopleID}");

            }
            if (parameter.IsPayAccumulationFund)
            {
                AFBaseServiceCharge = DbHelper.QuerySingle<decimal>("select BaseServiceCharge from CostParameterSetting where Status=1");
                AFCurrentBase = DbHelper.QuerySingle<decimal>($"select AccumulationFundBase from AccumulationFund where SocialSecurityPeopleID={parameter.SocialSecurityPeopleID}");

            }

            int memberID = DbHelper.QuerySingle<int>($"select MemberID from SocialSecurityPeople where SocialSecurityPeopleID={parameter.SocialSecurityPeopleID}");

            sqlstr = $@"insert into BaseOrders(OrderCode,MemberID,SocialSecurityPeopleID,IsPaySocialSecurity,SSCurrentBase,SSBaseAdjusted,SSAdjustingBaseNote,SSBaseServiceCharge,IsPayAccumulationFund,AFCurrentBase,AFBaseAdjusted,AFAdjustingBaseNote,AFBaseServiceCharge)
values('{orderCode}',{memberID},{parameter.SocialSecurityPeopleID},{Convert.ToInt32(parameter.IsPaySocialSecurity) },{SSCurrentBase},{parameter.SocialSecurityBaseAdjusted},'{parameter.SSAdjustingBaseNote}',{SSBaseServiceCharge},{Convert.ToInt32(parameter.IsPayAccumulationFund)},{AFCurrentBase},{parameter.AccumulationFundBaseAdjusted},'{parameter.AFAdjustingBaseNote}',{AFBaseServiceCharge})";

            int result = DbHelper.ExecuteSqlCommandScalar(sqlstr, new DbParameter[] { });

            return result;

        }

        /// <summary>
        /// 获取某用户下所有的社保
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public List<SocialSecurityShowModel> GetSocialSecurityList(int MemberID)
        {
            string sqlstr = $"select * from View_SocialSecurity where MemberID={MemberID}";

            List<SocialSecurityShowModel> list = DbHelper.Query<SocialSecurityShowModel>(sqlstr);

            return list;
        }

        /// <summary>
        /// 获取某签约企业的社保列表
        /// </summary>
        /// <param name="RelationEnterprise"></param>
        /// <returns></returns>
        public List<SocialSecurityShowModel> GetSocialSecurityListByEnterpriseID(int RelationEnterprise)
        {
            string sqlstr = $"select * from View_SocialSecurity where RelationEnterprise = {RelationEnterprise}";
            List<SocialSecurityShowModel> list = DbHelper.Query<SocialSecurityShowModel>(sqlstr);
            return list;
        }

        /// <summary>
        /// 获取待续费用户所产生的金额和
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public decimal GetRenewAmountByMemberID(int MemberID)
        {
            string sqlstr = $@"declare @SocialSecurityAmount decimal = 0, @AccumulationFundAmount decimal = 0,@totalAmount decimal =0
select @SocialSecurityAmount += SocialSecurity.SocialSecurityBase * SocialSecurity.PayProportion / 100 from SocialSecurityPeople
      left join SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID
      where SocialSecurityPeople.MemberID = {MemberID} and SocialSecurity.Status = 4;
            select @AccumulationFundAmount += AccumulationFund.AccumulationFundBase * AccumulationFund.PayProportion / 100 from SocialSecurityPeople
                  left join AccumulationFund on socialsecuritypeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
                  where SocialSecurityPeople.MemberID = {MemberID} and AccumulationFund.Status = 4;
            select @totalAmount = @SocialSecurityAmount + @AccumulationFundAmount;
select @totalAmount";

            decimal result = DbHelper.QuerySingle<decimal>(sqlstr);
            return result;
        }

        /// <summary>
        ///  获取该用户下所有参保人的所有待办金额之和
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public decimal GetWaitingHandleTotalByMemberID(int MemberID)
        {
            string sqlstr = $@"declare @SocialSecurityAmount decimal = 0, @AccumulationFundAmount decimal = 0,@totalAmount decimal =0
select @SocialSecurityAmount += SocialSecurity.SocialSecurityBase*SocialSecurity.PayMonthCount * SocialSecurity.PayProportion / 100 from SocialSecurityPeople
      left join SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID
      where SocialSecurityPeople.MemberID = {MemberID} and SocialSecurity.Status = 2;
            select @AccumulationFundAmount += AccumulationFund.AccumulationFundBase *AccumulationFund.PayMonthCount* AccumulationFund.PayProportion / 100 from SocialSecurityPeople
                  left join AccumulationFund on socialsecuritypeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
                  where SocialSecurityPeople.MemberID = {MemberID} and AccumulationFund.Status = 2;
            select @totalAmount = @SocialSecurityAmount + @AccumulationFundAmount;
select @totalAmount";

            decimal result = DbHelper.QuerySingle<decimal>(sqlstr);
            return result;
        }
    }
}
