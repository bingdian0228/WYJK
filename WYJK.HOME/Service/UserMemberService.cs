using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WYJK.Data;
using WYJK.Entity;
using WYJK.HOME.Models;

namespace WYJK.HOME.Service
{
    public class UserMemberService
    {
        /// <summary>
        /// 根据用户名和手机号获取用户
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public Members Password(PasswordViewModel pwd)
        {
            string sql = $@"select 
	                            *
                            from Members
	                            where MemberName = '{pwd.MemberName}' and MemberPhone = '{pwd.MemberPhone}'";
            return DbHelper.QuerySingle<Members>(sql);
        }

        /// <summary>
        /// 根据id获取用户信息
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public Members UserInfos(int memberId)
        {
            string sql = $@"select 
	                            *
                            from Members
	                            where MemberID = {memberId}";

            return DbHelper.QuerySingle<Members>(sql);
        }

    }
}