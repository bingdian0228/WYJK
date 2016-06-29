using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IService
{
    /// <summary>
    /// 后台用户
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 获取用户角色
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        Roles GetRoles(int UserID);

        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <param name="RoleID"></param>
        /// <returns></returns>
        List<Permissions> GetPermissions(int RoleID);

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns></returns>
         Task<PagedResult<Roles>> GetAllRoles(RolesParameter parameter);

        /// <summary>
        /// 添加角色
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task<int> RoleAdd(Roles parameter);

        /// <summary>
        /// 根据角色名称判断角色是否已存在
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        Task<Boolean> IsExistsRole(string roleName);

        /// <summary>
        /// 获取角色详情
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        Task<Roles> GetRolesInfo(int roleID);

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        Task<Boolean> UpdateRoles(Roles role);

        /// <summary>
        /// 获取所有权限
        /// </summary>
        /// <returns></returns>
        Task<List<Permissions>> GetAllPermissions();

        /// <summary>
        /// 权限添加
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        Task<Boolean> PermissionAdd(Permissions permission);

        /// <summary>
        /// 根据权限编号判断是否存在权限
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<Boolean> IsExistsPermission(string code);

        /// <summary>
        /// 获取权限
        /// </summary>
        /// <param name="permissionID"></param>
        /// <returns></returns>
        Task<Permissions> GetPermission(int? permissionID);

        /// <summary>
        /// 更新权限
        /// </summary>
        /// <param name="Permission"></param>
        /// <returns></returns>
        Task<Boolean> UpdatePermissions(Permissions Permission);

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <returns></returns>
        Users GetUserInfo(string InviteCode);

        /// <summary>
        /// 根据UserID获取用户信息
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        Users GetUserInfoByUserID(string UserID);

        /// <summary>
        /// 获取员工列表
        /// </summary>
        /// <returns></returns>
        PagedResult<Users> GetUserList(UsersParameter parameter);

        /// <summary>
        /// 获取员工列表
        /// </summary>
        /// <returns></returns>
        List<Users> GetUserList();
    }
}
