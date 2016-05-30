﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Web.Controllers.Mvc
{
    /// <summary>
    /// 签约企业
    /// </summary>
    [Authorize]
    public class EnterpriseController : Controller
    {
        private readonly ISocialSecurityService _socialSecurityService = new SocialSecurityService();
        private readonly IEnterpriseService _enterpriseService = new EnterpriseService();
        private readonly IAccumulationFundService _accumulationFundService = new AccumulationFundService();
        private readonly IPaymentDetailService paymentDetailService = new PaymentDetailService();
        private readonly IMemberService _memberService = new MemberService();
        /// <summary>
        /// 获取参保企业列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public ActionResult GetEnterpriseList(EnterpriseSocialSecurityParameter parameter)
        {
            PagedResult<EnterpriseSocialSecurity> EnterpriseSocialSecurityList = _enterpriseService.GetEnterpriseList(parameter);
            return View(EnterpriseSocialSecurityList);
        }

        /// <summary>
        /// 导入缴费明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ImportPaymentDetails()
        {

            HttpPostedFileBase file = Request.Files["files"];
            string FileName;
            string savePath;
            string NoFileName;
            if (file == null || file.ContentLength <= 0)
            {
                ViewBag.error = "文件不能为空";
                return View();
            }
            else
            {
                string filename = Path.GetFileName(file.FileName);
                int filesize = file.ContentLength;//获取上传文件的大小单位为字节byte
                string fileEx = System.IO.Path.GetExtension(filename);//获取上传文件的扩展名
                NoFileName = System.IO.Path.GetFileNameWithoutExtension(filename);//获取无扩展名的文件名
                int Maxsize = 4000 * 1024;//定义上传文件的最大空间大小为4M
                string FileType = ".xls,.xlsx";//定义上传文件的类型字符串

                FileName = NoFileName + DateTime.Now.ToString("yyyyMMddhhmmss") + fileEx;
                if (!FileType.Contains(fileEx))
                {
                    ViewBag.error = "文件类型不对，只能导入xls和xlsx格式的文件";
                    return View();
                }
                if (filesize >= Maxsize)
                {
                    ViewBag.error = "上传文件超过4M，不能上传";
                    return View();
                }

                string path = AppDomain.CurrentDomain.BaseDirectory + "uploads/excel/";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                savePath = Path.Combine(path, FileName);
                file.SaveAs(savePath);
            }

            //string result = string.Empty;
            string strConn;
            strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + savePath + ";" + "Extended Properties=Excel 8.0";
            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            OleDbDataAdapter myCommand = new OleDbDataAdapter("select * from [Sheet0$]", strConn);
            DataSet myDataSet = new DataSet();
            try
            {
                myCommand.Fill(myDataSet, "ExcelInfo");
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return View();
            }
            DataTable table = myDataSet.Tables["ExcelInfo"].DefaultView.ToTable();

            //引用事务机制，出错时，事物回滚
            using (TransactionScope transaction = new TransactionScope())
            {
                List<PaymentDetail> list = new List<PaymentDetail>();
                for (int i = 1; i < table.Rows.Count; i++)
                {
                    PaymentDetail paymentDetail = new PaymentDetail();
                    paymentDetail.PersonnelNumber = table.Rows[i][0].ToString();//个人编号
                    paymentDetail.IdentityCard = table.Rows[i][1].ToString();//身份证
                    paymentDetail.TrueName = table.Rows[i][2].ToString();//姓名
                    paymentDetail.PayTime = table.Rows[i][3].ToString();//缴费年月
                    paymentDetail.BusinessTime = table.Rows[i][4].ToString();//业务年月
                    paymentDetail.PaymentType = table.Rows[i][5].ToString();//缴费类型
                    paymentDetail.SocialInsuranceBase = Convert.ToInt32(table.Rows[i][6]);//缴费基数
                    paymentDetail.PersonalExpenses = Convert.ToDecimal(table.Rows[i][7]);//个人缴费
                    paymentDetail.CompanyExpenses = Convert.ToDecimal(table.Rows[i][8]);//单位缴费
                    paymentDetail.PaymentMark = table.Rows[i][9].ToString();//缴费标志
                    paymentDetail.CompanyNumber = table.Rows[i][10].ToString();//单位编号
                    paymentDetail.CompanyName = table.Rows[i][11].ToString();//单位名称
                    paymentDetail.SettlementMethod = table.Rows[i][12].ToString();//结算方式
                    paymentDetail.SocialSecurityType = NoFileName.Substring(0, 4);//社保类型
                    list.Add(paymentDetail);
                }

                paymentDetailService.AddPaymentDetail(list);

                transaction.Complete();
            }
            return RedirectToAction("GetEnterpriseList");
        }

        /// <summary>
        /// 城市社平管理列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public ActionResult GetEnterpriseCityList(PagedParameter parameter)
        {
            PagedResult<EnterpriseCity> EnterpriseSocialSecurityList = _enterpriseService.GetEnterpriseCityList(parameter);
            return View(EnterpriseSocialSecurityList);
        }

        /// <summary>
        /// 调整社平工资
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AdjustSocialAvgSalary(string City, string SocialAvgSalary)
        {
            int result = DbHelper.ExecuteSqlCommand($"update EnterpriseSocialSecurity set SocialAvgSalary='{SocialAvgSalary}' where EnterpriseArea like '{City}%'", null);

            //根据城市名称获取城市下的所有签约企业
            //DbHelper.Query<EnterpriseSocialSecurity>()

            if (result > 0)
                return Json(new { status = true, message = "调整成功" });
            else
                return Json(new { status = false, message = "调整失败" });
        }

        ///// <summary>
        ///// 差额调整
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult DifferenceAdjustment()
        //{
        //    //获取所有用户
        //    List<Members> memberList = _memberService.GetMembersList();
        //    StringBuilder builderBucha = new StringBuilder();//用户扣除冻结金额
        //    StringBuilder builderRecord = new StringBuilder();//补差记录
        //    //遍历所有用户
        //    foreach (var member in memberList)
        //    {
        //        decimal BuchaAmount = 0;
        //        //获取用户下的所有参保人
        //        List<SocialSecurityPeople> socialSecurityPeopleList = DbHelper.Query<SocialSecurityPeople>($"select * from SocialSecurityPeople where MemberID = {member.MemberID}");
        //        foreach (var socialSecurityPeople in socialSecurityPeopleList)
        //        {
        //            List<PaymentDetail> PaymentDetailList = DbHelper.Query<PaymentDetail>($"select * from PaymentDetail where IdentityCard='{socialSecurityPeople.IdentityCard}' and PaymentType='社平调整补差' and PaymentMark='已实缴'");
        //            foreach (var paymentDetail in PaymentDetailList)
        //            {
        //                BuchaAmount += paymentDetail.PersonalExpenses + paymentDetail.CompanyExpenses;

        //            }
        //        }
        //        builderBucha.Append("update Members ");
        //    }
        //}

        /// <summary>
        /// 新增签约企业
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddEnterprise()
        {
            //List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(HouseholdPropertyEnum));
            //UserTypeList.Insert(0, new SelectListItem { Text = "请选择", Value = "" });

            //ViewData["HouseholdProperty"] = new SelectList(UserTypeList, "Value", "Text");

            EnterpriseSocialSecurity model = new EnterpriseSocialSecurity();

            return View(model);
        }

        /// <summary>
        /// 保存企业
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddEnterprise(EnterpriseSocialSecurity model)
        {
            //已存在判断
            bool isExists = _enterpriseService.IsExistsEnterprise(model.EnterpriseName);
            if (isExists)
            {
                ViewBag.Message = "企业名称已存在";
                return AddEnterprise();
            }

            //ProvinceCode CityCode CountyCode
            string ProvinceName = string.Empty;
            string CityName = string.Empty;
            string CountyName = string.Empty;

            #region 将编码变成名称
            string sqlstr = "select * from Region where RegionCode = '{0}'";
            ProvinceName = DbHelper.QuerySingle<Region>(string.Format(sqlstr, model.ProvinceCode)).RegionName;
            CityName = DbHelper.QuerySingle<Region>(string.Format(sqlstr, model.CityCode)).RegionName;
            CountyName = DbHelper.QuerySingle<Region>(string.Format(sqlstr, model.CountyCode)).RegionName;
            #endregion

            model.EnterpriseArea = ProvinceName + "|" + CityName + "|" + CountyName;

            //更新其他签约企业  注：满足省份|城市和户口类型  默认的只有一个
            if (model.IsDefault)
            {
                _enterpriseService.UpdateEnterpriseDefault(ProvinceName + "|" + CityName, 0);
            }

            //添加
            bool flag = _enterpriseService.AddEnterprise(model);

            #region 记录日志
            LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("新增签约企业:{0}", model.EnterpriseName) });
            #endregion

            TempData["Message"] = flag ? "保存成功" : "保存失败";
            return RedirectToAction("GetEnterpriseList");
        }


        /// <summary>
        /// 批量删除企业
        /// </summary>
        /// <param name="EnterpriseIDs"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BatchDelete(int[] EnterpriseIDs)
        {
            bool flag = _enterpriseService.BatchDeleteEnterprise(EnterpriseIDs);

            #region 记录日志
            string names = _enterpriseService.GetEnterpriseNames(EnterpriseIDs);
            LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("删除签约企业:{0}", names) });
            #endregion

            return Json(new { status = flag, message = flag ? "删除成功" : "删除失败" });
        }

        /// <summary>
        /// 获取企业详情
        /// </summary>
        /// <param name="EnterpriseID"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetEnterpriseDetail(int EnterpriseID)
        {
            EnterpriseSocialSecurity model = _enterpriseService.GetEnterpriseSocialSecurity(EnterpriseID);
            return View(model);
        }


        /// <summary>
        /// 编辑企业
        /// </summary>
        /// <param name="EnterpriseID"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditEnterprise(int EnterpriseID)
        {

            EnterpriseSocialSecurity model = _enterpriseService.GetEnterpriseSocialSecurity(EnterpriseID);

            //List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(HouseholdPropertyEnum));
            //UserTypeList.Insert(0, new SelectListItem { Text = "请选择", Value = "" });

            //ViewData["HouseholdProperty1"] = UserTypeList; 

            return View(model);
        }

        /// <summary>
        /// 保存编辑企业
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditEnterprise(EnterpriseSocialSecurity model)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    //已存在判断
                    bool isExists = _enterpriseService.IsExistsEnterprise(model.EnterpriseName, model.EnterpriseID);
                    if (isExists)
                    {
                        ViewBag.Message = "企业名称已存在";
                        return EditEnterprise(model.EnterpriseID);
                    }

                    //ProvinceCode CityCode CountyCode
                    string ProvinceName = string.Empty;
                    string CityName = string.Empty;
                    string CountyName = string.Empty;

                    #region 将编码变成名称
                    string sqlstr = "select * from Region where RegionCode = '{0}'";
                    ProvinceName = DbHelper.QuerySingle<Region>(string.Format(sqlstr, model.ProvinceCode)).RegionName;
                    CityName = DbHelper.QuerySingle<Region>(string.Format(sqlstr, model.CityCode)).RegionName;
                    CountyName = DbHelper.QuerySingle<Region>(string.Format(sqlstr, model.CountyCode)).RegionName;
                    #endregion

                    model.EnterpriseArea = ProvinceName + "|" + CityName + "|" + CountyName;

                    //更新其他签约企业  注：满足省份|城市和户口类型  默认的只有一个
                    if (model.IsDefault)
                    {
                        _enterpriseService.UpdateEnterpriseDefault(ProvinceName + "|" + CityName, model.EnterpriseID);
                    }

                    bool flag = _enterpriseService.UpdateEnterprise(model);

                    #region 企业调整
                    //所有关联的参保人，社保基数低于更改后的则更新为更改后的基数，社保比例变动；公积金基数低于更改后的则更新为更改后的基数，公积金比例变动；
                    //如果某用户下的账户状态为正常，则需要重新计算检测是否需要变成待续费状态，并发送消息
                    decimal CommonPayProportion = model.CompYangLao + model.CompYiLiao + model.CompShiYe + model.CompGongShang + model.CompShengYu
                          + model.PersonalYangLao + model.PersonalYiLiao + model.PersonalGongShang + model.PersonalShengYu;
                    decimal RuralPayProportion = CommonPayProportion + model.PersonalShiYeRural;
                    decimal TownPayProportion = CommonPayProportion + model.PersonalShiYeTown;

                    //List<int> SocialSecurityIDList = _socialSecurityService.GetSocialSecurityListByEnterpriseID(model.EnterpriseID).Select(m => m.SocialSecurityPeopleID).ToList();
                    //foreach (var SocialSecurityPeopleID in SocialSecurityIDList)
                    //{
                    string socialSecuritySqlStr = $@"update SocialSecurity set SocialSecurityBase = case when SocialSecurityBase > {Math.Round(model.SocialAvgSalary * model.MinSocial / 100)} then SocialSecurityBase else {Math.Round(model.SocialAvgSalary * model.MinSocial / 100)} end ,
    PayProportion = case (select case HouseholdProperty when '本市农村' then '1' when '本市城镇' then '2' when '外市农村' then '3' when '外市城镇' then '4' end from SocialSecurityPeople where SocialSecurity.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID) when '1' then {RuralPayProportion} when '2' then {TownPayProportion} when '3' then {RuralPayProportion} when '4' then {TownPayProportion} end
  where RelationEnterprise = {model.EnterpriseID}";
                    DbHelper.ExecuteSqlCommand(socialSecuritySqlStr, null);
                    //}

                    //List<int> accumulationFundIDList = _accumulationFundService.GetAccumulationFundListByEnterpriseID(model.EnterpriseID).Select(m => m.SocialSecurityPeopleID).ToList();
                    //foreach (var item in accumulationFundIDList)
                    //{
                    string accumulationFundSqlStr = $@"update AccumulationFund set AccumulationFundBase = case when AccumulationFundBase >{model.MinAccumulationFund} then AccumulationFundBase else {model.MinAccumulationFund} end ,
  PayProportion = {model.CompProportion + model.PersonalProportion}
  where RelationEnterprise = {model.EnterpriseID}";
                    DbHelper.ExecuteSqlCommand(accumulationFundSqlStr, null);
                    //}

                    //获取相关联的所有用户ID
                    string MemberIDStr = $@" select * from
 (select SocialSecurityPeople.MemberID from SocialSecurity
 left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID=socialsecurity.SocialSecurityPeopleID
  where RelationEnterprise = {model.EnterpriseID}
  union 
 select SocialSecurityPeople.MemberID from AccumulationFund
 left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID=AccumulationFund.SocialSecurityPeopleID
  where RelationEnterprise = {model.EnterpriseID}) t
  where t.MemberID is not null";

                    List<int> MemberIDList = DbHelper.Query<int>(MemberIDStr);

                    #region 检测这个月余额是否够用，不够用则状态变为待续费
                    string sqlStr = string.Empty;
                    foreach (int MemberID in MemberIDList)
                    {
                        //如果是正常，则检测是否能变成待续费
                        if (!_socialSecurityService.IsExistsRenew(MemberID))
                        {
                            Members member = DbHelper.QuerySingle<Members>($"select * from Members where MemberID={MemberID}");

                            #region 查询每个用户余额
                            decimal totalAccount = 0;
                            //查询该用户下的所有参保人
                            string sqlSocialSecurityPeople = $"select * from SocialSecurityPeople where MemberID={MemberID}";
                            List<SocialSecurityPeople> SocialSecurityPeopleList = DbHelper.Query<SocialSecurityPeople>(sqlSocialSecurityPeople);
                            string SocialSecurityPeopleIDStr = string.Join("','", SocialSecurityPeopleList.Select(n => n.SocialSecurityPeopleID));

                            //查询该用户下的所有正常参保方案
                            string sqlSocialSecurity = $"select * from SocialSecurity where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status in({(int)SocialSecurityStatusEnum.Normal},{(int)SocialSecurityStatusEnum.WaitingHandle})";
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

                            #region 查询下月余额是否够用,若不够用，则变为待续费
                            if (member.Account < totalAccount)
                            {
                                //该用户下的正常社保变为待续费
                                foreach (SocialSecurity socialSecurity in SocialSecurityList)
                                {
                                    sqlStr += $"update SocialSecurity set Status ={(int)SocialSecurityStatusEnum.Renew} where SocialSecurityPeopleID={socialSecurity.SocialSecurityPeopleID};";
                                }
                                //该用户下的正常公积金变为待续费
                                foreach (AccumulationFund accumulationFund in AccumulationFundList)
                                {
                                    sqlStr += $"update AccumulationFund set Status={(int)SocialSecurityStatusEnum.Renew} where SocialSecurityPeopleID ={accumulationFund.SocialSecurityPeopleID};";
                                }

                                Message message = new Message();
                                message.MemberID = member.MemberID;
                                message.ContentStr = "您的账户余额已不足抵扣下个月社保、公积金.请及时充值.";

                                //发送消息提醒
                                DbHelper.ExecuteSqlCommand($"insert into Message(MemberID,ContentStr) values({message.MemberID},'{message.ContentStr}')", null);
                            }
                        }

                        #endregion
                    }
                    if (sqlStr.Trim() != string.Empty)
                        DbHelper.ExecuteSqlCommand(sqlStr, null);
                    #endregion

                    #endregion

                    #region 记录日志
                    LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("修改签约企业:{0}", model.EnterpriseName) });
                    #endregion

                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "保存失败";
                    return RedirectToAction("GetEnterpriseList");
                }
                finally
                {
                    transaction.Dispose();
                }
            }

            TempData["Message"] = "保存成功";
            return RedirectToAction("GetEnterpriseList");
        }

        /// <summary>
        /// 根据地区和户籍性质获取默认企业
        /// </summary>
        /// <param name="area"></param>
        /// <param name="HouseholdProperty"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetDefaultEnterpriseSocialSecurityByArea(string area, string HouseholdProperty)
        {
            EnterpriseSocialSecurity model = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(area, HouseholdProperty);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取社保列表
        /// </summary>
        /// <param name="RelationEnterprise"></param>
        /// <returns></returns>
        public ActionResult GetSocialSecurityList(int RelationEnterprise)
        {

            ViewData["SocialSecurityList"] = _socialSecurityService.GetSocialSecurityListByEnterpriseID(RelationEnterprise);
            ViewData["AccumulationFundList"] = _accumulationFundService.GetAccumulationFundListByEnterpriseID(RelationEnterprise);
            return View();
        }
    }
}