using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IServices;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Data.ServiceImpl
{
    public class MemberService : IMemberService
    {
        /// <summary>
        /// 忘记密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Dictionary<bool, string>> ForgetPassword(MemberForgetPasswordModel model)
        {

            Dictionary<bool, string> dic = new Dictionary<bool, string>();
            DbParameter[] parameters = new DbParameter[]{
                new SqlParameter("@MemberName", SqlDbType.NVarChar, 50) { Value = model.MemberName },
                new SqlParameter("@MemberPhone", SqlDbType.NVarChar, 50) { Value = model.MemberPhone },
                new SqlParameter("@Password", SqlDbType.NVarChar, 100) { Value = model.HashPassword },
                new SqlParameter("@Flag", SqlDbType.Bit) { Direction = ParameterDirection.Output },
                new SqlParameter("@Message", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output }
            };
            await DbHelper.ExecuteSqlCommandAsync("Member_ForgetPassword", parameters, CommandType.StoredProcedure);

            dic.Add((bool)parameters[3].Value, parameters[4].Value.ToString());

            return dic;
        }


        /// <summary>
        /// 登录用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Dictionary<bool, string>> LoginMember(MemberLoginModel model)
        {
            Dictionary<bool, string> dic = new Dictionary<bool, string>();
            DbParameter[] parameters = new DbParameter[]{
                new SqlParameter("@Account", SqlDbType.NVarChar, 50) { Value = model.Account },
                new SqlParameter("@Password", SqlDbType.NVarChar, 100) { Value = model.HashPassword },
                new SqlParameter("@Flag", SqlDbType.Bit) { Direction = ParameterDirection.Output },
                new SqlParameter("@Message", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output }
            };
            await DbHelper.ExecuteSqlCommandAsync("Member_Login", parameters, CommandType.StoredProcedure);

            dic.Add((bool)parameters[2].Value, parameters[3].Value.ToString());

            return dic;
        }

        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Dictionary<bool, string>> RegisterMember(MemberRegisterModel model)
        {
            Dictionary<bool, string> dic = new Dictionary<bool, string>();
            DbParameter[] parameters = new DbParameter[]{
                new SqlParameter("@MemberName", SqlDbType.NVarChar, 50) { Value = model.MemberName },
                new SqlParameter("@MemberPhone", SqlDbType.NVarChar, 50) { Value = model.MemberPhone },
                new SqlParameter("@Password", SqlDbType.NVarChar, 100) { Value = model.HashPassword },
                new SqlParameter("@InviteCode", SqlDbType.NVarChar, 50) { Value = model.InviteCode ?? (object)DBNull.Value },
                new SqlParameter("@Flag", SqlDbType.Bit) { Direction = ParameterDirection.Output },
                new SqlParameter("@Message", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output }
            };
            await DbHelper.ExecuteSqlCommandAsync("Member_Register", parameters, CommandType.StoredProcedure);

            dic.Add((bool)parameters[4].Value, parameters[5].Value.ToString());

            return dic;
        }

        /// <summary>
        /// 获取用户名获取用户信息
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task<Members> GetMemberInfo(int MemberID)
        {
            string sql = "select TrueName,MemberName,MemberPhone,IsAuthentication,IsComplete,UserType from Members where MemberID=@MemberID";

            Members member = await DbHelper.QuerySingleAsync<Members>(sql, new { MemberID = MemberID });
            return member;
        }

        /// <summary>
        /// 判断原密码是否正确
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task<bool> IsTrueOldPassword(MemberMidifyPassword model)
        {
            string sql = "select count(*) from Members where MemberID = @MemberID and Password=@Password";
            int result = await DbHelper.QuerySingleAsync<int>(sql, new
            {
                MemberID = model.MemberID,
                Password = model.HashOldPassword
            });
            return result > 0;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task<bool> ModifyPassword(MemberMidifyPassword model)
        {
            string sql = "update Members set Password = @Password where MemberID = @MemberID";
            DbParameter[] parameters = new DbParameter[] {
                new SqlParameter("@MemberID",model.MemberID),
                new SqlParameter("@Password",model.HashNewPassword)
            };
            int result = await DbHelper.ExecuteSqlCommandAsync(sql, parameters);
            return result > 0;
        }

        /// <summary>
        /// 企业资质认证
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<bool> CommitEnterpriseCertification(EnterpriseCertification parameter)
        {
            //string sql = "update Members set EnterpriseName=@EnterpriseName ,EnterpriseTax=@EnterpriseTax,EnterpriseType=@EnterpriseType ,EnterpriseArea=@EnterpriseArea,EnterpriseLegal=@EnterpriseLegal,EnterpriseLegalIdentityCardNo=@EnterpriseLegalIdentityCardNo,"
            //    + $"EnterprisePeopleNum=@EnterprisePeopleNum,SocialSecurityCreditCode=@SocialSecurityCreditCode,EnterpriseBusinessLicense=@EnterpriseBusinessLicense,IsAuthentication=1,UserType={(int)UserTypeEnum.QiYe} where MemberID=@MemberID";
            //SqlParameter[] parameters = new SqlParameter[] {
            //    new SqlParameter("@EnterpriseName",parameter.EnterpriseName),
            //    new SqlParameter("@EnterpriseTax",parameter.EnterpriseTax),
            //    new SqlParameter("@EnterpriseType",parameter.EnterpriseType),
            //    new SqlParameter("@EnterpriseArea",parameter.EnterpriseArea),
            //    new SqlParameter("@EnterpriseLegal",parameter.EnterpriseLegal),
            //    new SqlParameter("@EnterpriseLegalIdentityCardNo",parameter.EnterpriseLegalIdentityCardNo),
            //    new SqlParameter("@EnterprisePeopleNum",parameter.EnterprisePeopleNum),
            //    new SqlParameter("@SocialSecurityCreditCode",parameter.SocialSecurityCreditCode ?? (object)DBNull.Value),
            //    new SqlParameter("@EnterpriseBusinessLicense",parameter.EnterpriseBusinessLicense),
            //    new SqlParameter("@MemberID",parameter.MemberID)
            //};
            //int result = await DbHelper.ExecuteSqlCommandAsync(sql, parameters);
            //return result > 0;

            string sql = $@"insert into CertificationAudit(MemberID,EnterpriseName,EnterpriseType,EnterpriseTax,EnterpriseArea,EnterpriseLegal,EnterpriseLegalIdentityCardNo,EnterprisePeopleNum,SocialSecurityCreditCode,EnterpriseBusinessLicense,UserType,EnterprisePositionName) 
                                                    values(@MemberID,@EnterpriseName ,@EnterpriseType ,@EnterpriseTax,@EnterpriseArea,@EnterpriseLegal,@EnterpriseLegalIdentityCardNo,@EnterprisePeopleNum,@SocialSecurityCreditCode,@EnterpriseBusinessLicense,{(int)UserTypeEnum.QiYe},@EnterprisePositionName)";
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@EnterpriseName",parameter.EnterpriseName),
                new SqlParameter("@EnterpriseTax",parameter.EnterpriseTax),
                new SqlParameter("@EnterpriseType",parameter.EnterpriseType),
                new SqlParameter("@EnterpriseArea",parameter.EnterpriseArea),
                new SqlParameter("@EnterpriseLegal",parameter.EnterpriseLegal),
                new SqlParameter("@EnterpriseLegalIdentityCardNo",parameter.EnterpriseLegalIdentityCardNo),
                new SqlParameter("@EnterprisePeopleNum",parameter.EnterprisePeopleNum),
                new SqlParameter("@SocialSecurityCreditCode",parameter.SocialSecurityCreditCode ?? (object)DBNull.Value),
                new SqlParameter("@EnterpriseBusinessLicense",parameter.EnterpriseBusinessLicense),
                new SqlParameter("@EnterprisePositionName",parameter.EnterprisePositionName),
                new SqlParameter("@MemberID",parameter.MemberID)
            };
            int result = await DbHelper.ExecuteSqlCommandAsync(sql, parameters);
            return result > 0;
        }

        /// <summary>
        /// 个体资质认证
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<bool> CommitPersonCertification(IndividualCertification parameter)
        {
            string sql = $@"insert into CertificationAudit(MemberID,BusinessName,BusinessUser,BusinessIdentityCardNo,BusinessIdentityPhoto,BusinessLicensePhoto,UserType,BusinessPositionName) 
                  values(@MemberID,@BusinessName,@BusinessUser,@BusinessIdentityCardNo,@BusinessIdentityPhoto,@BusinessLicensePhoto,{(int)UserTypeEnum.GeTiJingYing},@BusinessPositionName)";
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@MemberID",parameter.MemberID),
                new SqlParameter("@BusinessIdentityCardNo",parameter.BusinessIdentityCardNo),
                new SqlParameter("@BusinessName",parameter.BusinessName),
                new SqlParameter("@BusinessUser",parameter.BusinessUser),
                new SqlParameter("@BusinessIdentityPhoto",parameter.BusinessIdentityPhoto),
                new SqlParameter("@BusinessLicensePhoto",parameter.BusinessLicensePhoto),
                new SqlParameter("@BusinessPositionName",parameter.BusinessPositionName)

            };
            int result = await DbHelper.ExecuteSqlCommandAsync(sql, parameters);
            return result > 0;
        }

        /// <summary>
        /// 获取账户信息
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public AccountInfo GetAccountInfo(int MemberID)
        {
            string sql = "select MemberName, Account,Bucha,HeadPortrait from Members where MemberID=@MemberID";
            return DbHelper.QuerySingle<AccountInfo>(sql, new { MemberID = MemberID });
        }

        /// <summary>
        /// 获取账单流水记录
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public async Task<PagedResult<AccountRecord>> GetAccountRecordList(int MemberID, string ShouZhiType, DateTime? StartTime, DateTime? EndTime, PagedParameter parameter)
        {
            string sqlShouZhiType = string.Empty;
            if (string.IsNullOrEmpty(ShouZhiType))
            {
                sqlShouZhiType = " 1 = 1 ";
            }
            else {
                sqlShouZhiType = $" ShouZhiType = '{EnumExt.GetEnumCustomDescription((ShouZhiTypeEnum)(Convert.ToInt32(ShouZhiType)))}'";
            }
            string sqlTime = string.Empty;

            if (string.IsNullOrEmpty(StartTime.ToString()) && string.IsNullOrEmpty(EndTime.ToString()))
            {
                sqlTime = " 1 = 1 ";
            }
            else if (string.IsNullOrEmpty(StartTime.ToString()) && !string.IsNullOrEmpty(EndTime.ToString()))
            {
                sqlTime = $" CreateTime < '{ EndTime.Value.AddDays(1)}' ";
            }
            else if (!string.IsNullOrEmpty(StartTime.ToString()) && string.IsNullOrEmpty(EndTime.ToString()))
            {
                sqlTime = $" CreateTime > '{StartTime}'";
            }
            else {
                sqlTime = $" CreateTime between '{StartTime}' and '{EndTime.Value.AddDays(1)}'";
            }


            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat("{0};{1}", $"select * from (select ROW_NUMBER() OVER(ORDER BY AccountRecord.CreateTime desc )AS Row,AccountRecord.* from AccountRecord  where MemberID ={MemberID} and {sqlShouZhiType} and {sqlTime} ) ss WHERE ss.Row BETWEEN @StartIndex AND @EndIndex", $"select count(0) from AccountRecord  where MemberID ={ MemberID}  and {sqlShouZhiType} and {sqlTime} ");

            DbParameter[] parameters = new DbParameter[] {
                    new SqlParameter("@StartIndex", parameter.SkipCount),
                    new SqlParameter("@EndIndex", parameter.TakeCount)
            };
            var tuple = await DbHelper.QueryMultipleAsync<AccountRecord, int>(sbSql.ToString(), parameters);
            return new PagedResult<AccountRecord>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = tuple.Item2?.FirstOrDefault() ?? 0,
                Items = tuple.Item1
            };
        }

        /// <summary>
        /// 更新会员补充信息
        /// </summary>
        /// <param name="MemberID"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> ModifyMemberExtensionInformation(ExtensionInformationParameter model)
        {
            string sql = "update Members set TrueName=@TrueName,"
                + " CertificateType=@CertificateType,CertificateNo=@CertificateNo,"
                + " PoliticalStatus=@PoliticalStatus,Education=@Education,Birthday=@Birthday,"
                + " Sex=@Sex,Address=@Address,Phone=@Phone,Email=@Email,QQ=@QQ,Alipay=@Alipay,"
                + " BankCardNo=@BankCardNo,BankAccount=@BankAccount,UserAccount=@UserAccount,"
                + " SecondContact=@SecondContact,SecondContactPhone=@SecondContactPhone,InsuranceArea=@InsuranceArea,"
                + " HouseholdType=@HouseholdType,IsComplete= 1 "
                + " where MemberID=@MemberID";

            DbParameter[] parameters = new DbParameter[] {
                new SqlParameter("@TrueName",model.TrueName ?? ""),
                new SqlParameter("@CertificateType",model.CertificateType ?? ""),
                new SqlParameter("@CertificateNo",model.CertificateNo ?? ""),
                new SqlParameter("@PoliticalStatus",model.PoliticalStatus ?? ""),
                new SqlParameter("@Education",model.Education ?? ""),
                new SqlParameter("@Birthday",model.Birthday ?? (object)DBNull.Value),
                new SqlParameter("@Sex",model.Sex ?? ""),
                new SqlParameter("@Address",model.Address ?? ""),
                new SqlParameter("@Phone",model.Phone ?? ""),
                new SqlParameter("@Email",model.Email ?? ""),
                new SqlParameter("@QQ",model.QQ ?? ""),
                new SqlParameter("@Alipay",model.Alipay ?? ""),
                new SqlParameter("@BankCardNo",model.BankCardNo ?? ""),
                new SqlParameter("@BankAccount",model.BankAccount  ?? ""),
                new SqlParameter("@UserAccount",model.UserAccount ?? ""),
                new SqlParameter("@SecondContact",model.SecondContact ?? ""),
                new SqlParameter("@SecondContactPhone",model.SecondContactPhone ?? ""),
                new SqlParameter("@InsuranceArea",model.InsuranceArea ?? ""),
                new SqlParameter("@HouseholdType",model.HouseholdType ?? ""),
                new SqlParameter("@MemberID",model.MemberID)
            };

            int result = await DbHelper.ExecuteSqlCommandAsync(sql, parameters);
            return result > 0;

        }

        /// <summary>
        /// 获取补充信息
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public async Task<ExtensionInformation> GetExtensionInformation(int MemberID)
        {
            string sql = "select TrueName,"
                + " CertificateType,CertificateNo,"
                + " PoliticalStatus,Education,Birthday,"
                + " Sex,Address,Phone,Email,QQ,Alipay,"
                + " BankCardNo,BankAccount,UserAccount,"
                + " SecondContact,SecondContactPhone,InsuranceArea,"
                + " HouseholdType from Members "
                + $" where MemberID={MemberID}";
            ExtensionInformation model = await DbHelper.QuerySingleAsync<ExtensionInformation>(sql);
            return model;
        }


        public async Task<int> GetMemberId(string MemberName)
        {
            string sql = $"select MemberId from Members where MemberName='{MemberName}'";
            ExtensionInformationParameter model = await DbHelper.QuerySingleAsync<ExtensionInformationParameter>(sql);
            return model.MemberID;
        }

        /// <summary>
        /// 获取补充信息
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public async Task<ExtensionInformationParameter> GetMemberExtensionInformation(int MemberID)
        {
            string sql = "select MemberID, TrueName,"
                + " CertificateType,CertificateNo,"
                + " PoliticalStatus,Education,Birthday,"
                + " Sex,Address,Phone,Email,QQ,Alipay,"
                + " BankCardNo,BankAccount,UserAccount,"
                + " SecondContact,SecondContactPhone,InsuranceArea,"
                + " HouseholdType from Members "
                + $" where MemberID={MemberID}";
            ExtensionInformationParameter model = await DbHelper.QuerySingleAsync<ExtensionInformationParameter>(sql);
            return model;
        }

        /// <summary>
        /// 获取账单记录
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public async Task<AccountRecord> GetAccountRecord(int ID)
        {
            string sql = $"select * from AccountRecord where ID={ID}";
            AccountRecord model = await DbHelper.QuerySingleAsync<AccountRecord>(sql);
            return model;
        }

        /// <summary>
        /// 获取流水记录
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public List<AccountRecord> GetAccountRecordList(int MemberID)
        {
            string sqlstr = $"select * from AccountRecord where MemberID={MemberID}";
            List<AccountRecord> accountRecordList = DbHelper.Query<AccountRecord>(sqlstr);
            return accountRecordList;
        }
        /// <summary>
        /// 获取会员信息
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public Members GetMemberInfoForAdmin(int MemberID)
        {
            string sqlstr = $"select * from Members where MemberID = {MemberID}";
            Members member = DbHelper.QuerySingle<Members>(sqlstr);
            return member;
        }

        /// <summary>
        /// 获取会员统计列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public PagedResult<MembersStatistics> GetMembersStatisticsList(MembersParameters parameter)
        {
            StringBuilder strb = new StringBuilder("where 1 = 1");
            if (!string.IsNullOrEmpty(parameter.UserType))
            {
                strb.Append($" and UserType = {parameter.UserType} ");
            }

            if (!string.IsNullOrEmpty(parameter.MemberID))
            {
                strb.Append($" and Members.MemberID = {parameter.MemberID} ");
            }
            if (String.IsNullOrEmpty(parameter.SocialSecurityPeopleName))
                strb.Append($" and (SocialSecurityPeople.SocialSecurityPeopleName is null or SocialSecurityPeople.SocialSecurityPeopleName like '%{parameter.SocialSecurityPeopleName}%')");
            else
                strb.Append($" and SocialSecurityPeople.SocialSecurityPeopleName like '%{parameter.SocialSecurityPeopleName}%' ");

            if (String.IsNullOrEmpty(parameter.IdentityCard))
                strb.Append($" and (SocialSecurityPeople.IdentityCard is null or SocialSecurityPeople.IdentityCard like '%{parameter.IdentityCard}%')");
            else
                strb.Append($" and SocialSecurityPeople.IdentityCard like '%{parameter.IdentityCard}%' ");



            string innersql = "select Members.MemberID,Max(Members.UserType) UserType, MAX(members.MemberName) MemberName,MAX(members.EnterpriseName) EnterpriseName,MAX(members.BusinessName) BusinessName,  MAX(members.MemberPhone) MemberPhone, COUNT(SocialSecurityPeople.SocialSecurityPeopleID) SocialSecurityPeopleCount,MAX(ISNULL(members.Account,0)) Account,Max(ISNULL(members.Bucha,0)) Bucha,"
                            + " case when exists("
                            + " select * from SocialSecurityPeople"
                            + " left join SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID"
                            + " left join AccumulationFund on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID"
                            + $" where MemberID = members.MemberID and(SocialSecurity.Status = {(int)SocialSecurityStatusEnum.Renew} or AccumulationFund.Status = {(int)SocialSecurityStatusEnum.Renew})"
                            + " ) "
                            + " then '待续费' else '正常' end AccountStatus"
                            + " from Members"
                            + " left join SocialSecurityPeople on Members.MemberID = SocialSecurityPeople.MemberID"
                            + $"  {strb.ToString()}"
                            + " group by Members.MemberID";
            string sqlstr = "select * from (select ROW_NUMBER() OVER(ORDER BY t.MemberID )AS Row,t.* from"
                            + $" ({innersql}) t) tt"
                            + " where tt.Row BETWEEN @StartIndex AND @EndIndex";
            List<MembersStatistics> memberList = DbHelper.Query<MembersStatistics>(sqlstr, new
            {
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });

            int totalCount = DbHelper.QuerySingle<int>($"select count(0) from ({innersql}) t");

            return new PagedResult<MembersStatistics>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = memberList
            };
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        public List<Members> GetMembersList()
        {
            string sql = "select * from Members";
            List<Members> memberList = DbHelper.Query<Members>(sql);
            return memberList;
        }

        /// <summary>
        /// 获取账户状态
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public bool GetAccountStatus(int MemberID)
        {
            string sqlstr = $@"select count(1) from Members
left join SocialSecurityPeople on SocialSecurityPeople.MemberID = members.MemberID
left join SocialSecurity on SocialSecurity.SocialSecurityPeopleID = socialsecuritypeople.SocialSecurityPeopleID
left join AccumulationFund on AccumulationFund.SocialSecurityPeopleID = socialsecuritypeople.SocialSecurityPeopleID
where (SocialSecurity.Status = {(int)SocialSecurityStatusEnum.Renew} or AccumulationFund.Status = {(int)SocialSecurityStatusEnum.Renew}) and members.MemberID={MemberID}";
            int result = DbHelper.QuerySingle<int>(sqlstr);
            return result > 0;
        }

        /// <summary>
        /// 获取冻结金额介绍
        /// </summary>
        /// <returns></returns>
        public string GetFreezingAmountInstruction()
        {
            string sqlstr = "select FreezingAmountNote from FreezingAmountInstruction";
            string note = DbHelper.QuerySingle<string>(sqlstr);
            return note;
        }

        /// <summary>
        /// 保存头像
        /// </summary>
        /// <param name="MemberID"></param>
        /// <param name="HeadPortrait"></param>
        /// <returns></returns>
        public bool SaveHeadPortrait(int MemberID, string HeadPortrait)
        {
            string sqlstr = $"update Members set HeadPortrait='{HeadPortrait}' where MemberID={MemberID}";
            int result = DbHelper.ExecuteSqlCommand(sqlstr, null);
            return result > 0;
        }
    }
}
