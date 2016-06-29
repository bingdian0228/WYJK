using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 权限
    /// </summary>
    public class Permissions
    {
        /// <summary>
        /// 权限ID
        /// </summary>
        public int PermissionID { get; set; }
        /// <summary>
        /// 权限编码
        /// </summary>
        [Required(ErrorMessage = "权限编码必填")]
        public string Code { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [Required(ErrorMessage = "权限名称必填")]
        public string Description { get; set; }
        /// <summary>
        /// Controller
        /// </summary>
        [Required(ErrorMessage = "Controller必填")]
        public string Controller { get; set; }
        /// <summary>
        /// Action
        /// </summary>
        [Required(ErrorMessage = "Action必填")]
        public string Action { get; set; }
        /// <summary>
        /// 父级编码
        /// </summary>
        public string ParentID { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 是否修改
        /// </summary>
        public bool IsModified { get; set; }


    }


    public class TempPermissions
    {
        public int id { get; set; }
        public int pId { get; set; }
        public string name { get; set; }
        public bool @checked { get; set; }
    }
}
