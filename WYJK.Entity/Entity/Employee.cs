﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
namespace WYJK.Entity
{
    //员工
    public class Employee
    {

        /// <summary>
        /// ID
        /// </summary>		
        public int EmployeeID { get; set; }
        /// <summary>
        /// 员工登录名
        /// </summary>		
        public string EmployeeName { get; set; }
        /// <summary>
        /// 注册类型： 0：兼职，1：正式
        /// </summary>		
        public string RegType { get; set; }
        /// <summary>
        /// 密码
        /// </summary>		
        public string Password { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>		
        public string IdentityCard { get; set; }
        /// <summary>
        /// 公司ID
        /// </summary>		
        public int CompanyID { get; set; }
        /// <summary>
        /// 部门ID
        /// </summary>		
        public int DepartmentID { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>		
        public int RoleID { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>		
        public string TrueName { get; set; }
        /// <summary>
        /// 联系人手机号
        /// </summary>		
        public string ContactPhone { get; set; }
        /// <summary>
        /// QQ
        /// </summary>		
        public string QQ { get; set; }
        /// <summary>
        /// 办公电话
        /// </summary>		
        public string OfficeTelephone { get; set; }
        /// <summary>
        /// Email
        /// </summary>		
        public string Email { get; set; }
        /// <summary>
        /// 居住地址
        /// </summary>		
        public string Address { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>		
        public string BankAccount { get; set; }
        /// <summary>
        /// 开户人
        /// </summary>		
        public string UserAccount { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>		
        public string CardNo { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>		
        public bool Enabled { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>		
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 操作用户
        /// </summary>		
        public string CreateUser { get; set; }

    }
}

