﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;
using WYJK.Framework.Helpers;
using WYJK.Web.Models;

namespace WYJK.Web.Controllers.Mvc
{
    [Authorize]
    public class DrawCashController : BaseController
    {
        private readonly IDrawCashService _drawCashService = new DrawCashService();

        /// <summary>
        /// 提现申请列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> DrawCashList(DrawCashParameter parameter)
        {

            PagedResult<DrawCashViewModel> list = await _drawCashService.GetDrawCashListAsync(parameter);

            List<SelectListItem> ApplyStatus = new List<SelectListItem>();
            ApplyStatus.Insert(0, new SelectListItem { Text = "全部", Value = "-1" });
            ApplyStatus.Insert(1, new SelectListItem { Text = "审核中", Value = "1" });
            ApplyStatus.Insert(2, new SelectListItem { Text = "已通过", Value = "2" });
            ApplyStatus.Insert(3, new SelectListItem { Text = "未通过", Value = "3" });
            ViewData["ApplyStatus"] = new SelectList(ApplyStatus, "Value", "Text");

            return View(list);
        }

        /// <summary>
        /// 不通过提现审核
        /// </summary>
        /// <param name="DrawCashIds"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> NotAgree(int[] DrawCashIds)
        {
            string dcIds = string.Join(",", DrawCashIds);
            int restult = await DbHelper.ExecuteSqlCommandAsync($"UPDATE dbo.DrawCash SET ApplyStatus=3 WHERE DrawCashId IN ({ dcIds}) ");
            bool flag = false;
            if (restult > 0)
            {
                flag = true;
                List<AccountInfo> list = await DbHelper.QueryAsync<AccountInfo>($"select MemberID, MemberName, Account,Bucha,HeadPortrait from Members where MemberID in(select memberid from DrawCash  WHERE DrawCashId IN ({dcIds})) ");
                //string names = string.Empty;
                //list.ForEach(l =>
                //{
                //    names = names + l.MemberName + ',';
                //});
                //if (!string.IsNullOrEmpty(names))
                //{
                //    names = names.TrimEnd(',');
                //}
                list.ForEach(l =>
                {
                    LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, MemberID = l.MemberID, Contents = string.Format("拒绝提现申请业务，注册客户:{0}", l.MemberName) });
                });

            }
            return Json(new { status = flag, Message = flag ? "操作成功" : "操作失败" });
        }

        /// <summary>
        /// 同意提现
        /// </summary>
        /// <param name="DrawCashIds"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AgreeDrawCash(int[] DrawCashIds)
        {

            DrawCashModel model = new DrawCashModel()
            {
                DrawCashIds = string.Join(",", DrawCashIds)
            };

            string sql = $"select * from DrawCash where DrawCashId in ({model.DrawCashIds});";

            DrawCash entity = DbHelper.QuerySingle<DrawCash>(sql);
            ViewData["money"] = entity.Money.ToString("N2");



            return View(model);
        }
        private static string apikey = ConfigurationManager.AppSettings["apikey"].ToString();
        private static string contentFormat = ConfigurationManager.AppSettings["SMS15Warn"].ToString();
        /// <summary>
        /// 同意提现填写打款信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public  ActionResult AgreeDrawCash(DrawCashModel model)
        {
            if (ModelState.IsValid)
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    try
                    {
                        string sql = $"update DrawCash set ApplyStatus=2,agreetime=getdate(),paysn='{model.OrderNo}' where DrawCashId in ({model.DrawCashIds});";
                        foreach (string item in model.DrawCashIds.Split(','))
                        {
                            sql += $"update Members set Account=Account-(select money from DrawCash where DrawCashid={item}) where memberid = (select memberid from DrawCash where DrawCashid={item});";
                        }

                        DbHelper.ExecuteSqlCommand(sql, null);

                        #region 检测余额是否够用,不够用则状态变为待续费
                        string sqlStr2 = string.Empty;
                        #region 查询每个用户余额
                        int memberid = DbHelper.QuerySingle<int>($"select MemberId from DrawCash where DrawCashId={model.DrawCashIds}");
                        Members member = DbHelper.QuerySingle<Members>($"select * from Members where MemberID={memberid}");
                        decimal totalAccount = 0;
                        //查询该用户下的所有参保人
                        string sqlSocialSecurityPeople = $"select * from SocialSecurityPeople where MemberID={member.MemberID}";
                        List<SocialSecurityPeople> SocialSecurityPeopleList = DbHelper.Query<SocialSecurityPeople>(sqlSocialSecurityPeople);
                        string SocialSecurityPeopleIDStr = string.Join("','", SocialSecurityPeopleList.Select(n => n.SocialSecurityPeopleID));

                        //查询该用户下的所有正常参保方案
                        string sqlSocialSecurity = $"select * from SocialSecurity where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.Normal}";
                        List<SocialSecurity> SocialSecurityList = DbHelper.Query<SocialSecurity>(sqlSocialSecurity);
                        foreach (SocialSecurity socialSecurity in SocialSecurityList)
                        {
                            //社保单月金额
                            decimal account = socialSecurity.SocialSecurityBase * socialSecurity.PayProportion / 100;
                            totalAccount += account;
                        }

                        //查询该用户下的所有正常参公积金方案
                        string sqlAccumulationFund = $"select * from AccumulationFund where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.Normal}";
                        List<AccumulationFund> AccumulationFundList = DbHelper.Query<AccumulationFund>(sqlAccumulationFund);
                        foreach (AccumulationFund accumulationFund in AccumulationFundList)
                        {
                            //公积金单月金额
                            decimal account = accumulationFund.AccumulationFundBase * accumulationFund.PayProportion / 100;
                            totalAccount += account;
                        }
                        #endregion

                        #region 查询余额是否够用,若不够用，则变为待续费
                        if (member.Account < totalAccount)
                        {
                            //该用户下的正常社保变为待续费
                            foreach (SocialSecurity socialSecurity in SocialSecurityList)
                            {
                                sqlStr2 += $"update SocialSecurity set Status ={(int)SocialSecurityStatusEnum.Renew} where SocialSecurityPeopleID={socialSecurity.SocialSecurityPeopleID};";
                            }
                            //该用户下的正常公积金变为待续费
                            foreach (AccumulationFund accumulationFund in AccumulationFundList)
                            {
                                sqlStr2 += $"update AccumulationFund set Status={(int)SocialSecurityStatusEnum.Renew} where SocialSecurityPeopleID ={accumulationFund.SocialSecurityPeopleID};";
                            }


                            Message message = new Message();
                            message.MemberID = member.MemberID;
                            message.ContentStr = "您的账户余额已不足抵扣下个月社保、公积金.请及时充值.";

                            //发送消息提醒
                            DbHelper.ExecuteSqlCommand($"insert into Message(MemberID,ContentStr) values({message.MemberID},'{message.ContentStr}')", null);

                            #region 发送短信
                            string content = string.Format(contentFormat, member.MemberPhone.Substring(member.MemberPhone.Length - 4), member.Account, DateTime.Now.AddMonths(1).Month, totalAccount, totalAccount - member.Account);

                            Sms.sendSms(apikey, content, member.MemberPhone);
                            #endregion

                        }
                        #endregion
                        if (sqlStr2.Trim() != string.Empty)
                            DbHelper.ExecuteSqlCommand(sqlStr2, null);

                        #endregion

                        #region 记录流水
                        decimal money = DbHelper.QuerySingle<decimal>($"select Money from DrawCash where DrawCashId={model.DrawCashIds}");
                        string sqlAccountRecord = $@"insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime)
values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{member.MemberID},'','','支出','余额','提现',{money},{member.Account },getdate())";
                        DbHelper.ExecuteSqlCommand(sqlAccountRecord, null);
                        #endregion

                        List<AccountInfo> list =  DbHelper.Query<AccountInfo>($"select MemberID, MemberName, Account, Bucha, HeadPortrait from Members where MemberID in(select memberid from DrawCash WHERE DrawCashId IN ({ model.DrawCashIds})) ");
               
                        list.ForEach(l =>
                        {
                            LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, MemberID = l.MemberID, Contents = string.Format("提现申请成功客户:{0}", l.MemberName) });
                        });

                        transaction.Complete();

                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorMessage = ex.Message;
                    }
                }
            }
            return View(model);
        }

    }
}
