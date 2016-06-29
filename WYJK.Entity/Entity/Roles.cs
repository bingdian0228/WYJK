using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 角色
    /// </summary>
    public class Roles
    {
        /// <summary>
        /// 角色id
        /// </summary>
        public int RoleID { get; set; }
        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }

    public class TempRoles
    {
        /// <summary>
        /// 角色id
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 角色名称
        /// </summary>
        public string text { get; set; }

        public bool check { get; set; }
    }

    public class RolesParameter : PagedParameter
    {
        public string RoleName { get; set; }
    }
}
