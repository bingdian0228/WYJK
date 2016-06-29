using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;

namespace WYJK.Data.ServiceImpl
{
    public class UserService : IUserService
    {
        /// <summary>
        /// 获取用户角色
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public Roles GetRoles(int UserID)
        {
            string sql = $"select roles.* from Users left join UserRole on Users.UserID = UserRole.UserID left join Roles on Roles.RoleID = UserRole.RoleID where Users.UserID={UserID}";
            Roles role = DbHelper.Query<Roles>(sql, new { UserID = UserID }).FirstOrDefault();
            return role;
        }

        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <param name="RoleID"></param>
        /// <returns></returns>
        public List<Permissions> GetPermissions(int RoleID)
        {
            string sql = $"select Permissions.* from roles left join RolePermission on roles.roleID=RolePermission.RoleID left join Permissions on RolePermission.PermissionID =  Permissions.PermissionID where roles.RoleID={RoleID}";
            List<Permissions> PermissionsList = DbHelper.Query<Permissions>(sql, new { UserID = RoleID });
            return PermissionsList;
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns></returns>
        public async Task<PagedResult<Roles>> GetAllRoles(RolesParameter parameter)
        {
            string sql = $"select * from (select ROW_NUMBER() OVER(ORDER BY r.RoleID )AS Row,r.* from Roles r where Rolename like @Rolename) rr where rr.Row between @StartIndex AND @EndIndex";


            List<Roles> rolesList = await DbHelper.QueryAsync<Roles>(sql, new
            {
                RoleName = "%" + parameter.RoleName + "%",
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });
            int totalCount = await DbHelper.QuerySingleAsync<int>("select count(0) from Roles where RoleName like @RoleName ", new { RoleName = "%" + parameter.RoleName + "%" });

            return new PagedResult<Roles>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = rolesList
            };
        }

        /// <summary>
        /// 角色添加
        /// </summary>
        /// <param name="roles"></param>
        /// <returns></returns>
        public async Task<int> RoleAdd(Roles roles)
        {
            string sql = "insert into Roles(RoleName,Description) values(@RoleName,@Description)";

            SqlParameter[] parameters = {
                new SqlParameter("@RoleName",SqlDbType.NVarChar,50) {Value=roles.RoleName },
                new SqlParameter("@Description",SqlDbType.NVarChar,50) { Value=roles.Description?? (Object)DBNull.Value}
            };

            return  await DbHelper.ExecuteSqlCommandScalarAsync(sql, parameters);

        }

        /// <summary>
        /// 判断是否存在该角色名
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public async Task<bool> IsExistsRole(string roleName)
        {
            string sql = "select count(1) from Roles where Rolename = @Rolename";
            SqlParameter[] parameters = {
                new SqlParameter("@RoleName",SqlDbType.NVarChar,50) {Value=roleName }
            };

            int result = await DbHelper.ExecuteSqlCommandScalarAsync(sql, parameters);

            return result > 0;
        }

        public async Task<Roles> GetRolesInfo(int roleID)
        {
            string sql = "select * from Roles where RoleID=@RoleID";
            Roles role = await DbHelper.QuerySingleAsync<Roles>(sql, new { RoleID = roleID });
            return role;
        }

        public async Task<bool> UpdateRoles(Roles role)
        {
            string sql = "update Roles set RoleName =@RoleName ,Description=@Description where RoleID=@RoleID";
            DbParameter[] parameters = {
                new SqlParameter("@RoleName",SqlDbType.NVarChar,50) { Value=role.RoleName},
                new SqlParameter("@Description",SqlDbType.NVarChar,50) { Value=role.Description},
                new SqlParameter("@RoleID",SqlDbType.Int) { Value=role.RoleID},
            };
            int result = await DbHelper.ExecuteSqlCommandAsync(sql, parameters);
            return result > 0;

        }

        /// <summary>
        /// 获取所有权限
        /// </summary>
        /// <returns></returns>
        public async Task<List<Permissions>> GetAllPermissions()
        {
            string sql = "select * from Permissions ";
            List<Permissions> permissionList = await DbHelper.QueryAsync<Permissions>(sql);
            return permissionList;
        }

        /// <summary>
        /// 添加权限
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public async Task<bool> PermissionAdd(Permissions permission)
        {
            string sql = "insert into Permissions(Code,Description,Controller,Action,ParentID) values(@Code,@Description,@Controller,@Action,@ParentID)";

            DbParameter[] parameters = {
                new SqlParameter("@Code", permission.Code),
                new SqlParameter("@Description", permission.Description),
                new SqlParameter("@Controller", permission.Controller),
                new SqlParameter("@Action", permission.Action),
                new SqlParameter("@ParentID", permission.ParentID ?? "0")
            };

            int result = await DbHelper.ExecuteSqlCommandAsync(sql, parameters);
            return result > 0;
        }

        public async Task<bool> IsExistsPermission(string code)
        {
            string sql = "select COUNT(*) from Permissions where Code = @Code";

            int result = await DbHelper.QuerySingleAsync<int>(sql, new { Code = code });
            return result > 0;
        }

        /// <summary>
        /// 根据Code获取权限
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<Permissions> GetPermission(int? permissionID)
        {
            string sql = "select * from Permissions where PermissionID = @PermissionID";

            Permissions model = await DbHelper.QuerySingleAsync<Permissions>(sql, new { PermissionID = permissionID });
            return model;
        }

        public async Task<bool> UpdatePermissions(Permissions Permission)
        {
            string sql = "update Permissions set Code =@Code ,Description=@Description,Controller=@Controller,Action=@Action where PermissionID=@PermissionID";
            DbParameter[] parameters = {
                new SqlParameter("@Code",Permission.Code) ,
                new SqlParameter("@Description",Permission.Description) ,
                new SqlParameter("@Controller",Permission.Controller),
                new SqlParameter("@Action",Permission.Action),
                new SqlParameter("@PermissionID",Permission.PermissionID),
            };
            int result = await DbHelper.ExecuteSqlCommandAsync(sql, parameters);
            return result > 0;
        }

        /// <summary>
        /// 根据用户名获取用户
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public Users GetUserInfo(string InviteCode)
        {
            string sqlstr = $"select * from Users where InviteCode ='{InviteCode}'";
            Users user = DbHelper.QuerySingle<Users>(sqlstr);
            return user;
        }

        /// <summary>
        /// 获取员工列表
        /// </summary>
        /// <returns></returns>
        public PagedResult<Users> GetUserList(UsersParameter parameter)
        {
            string sqlstr = $"select * from (select ROW_NUMBER() OVER(ORDER BY s.UserID )AS Row,s.* from Users s) ss  WHERE ss.Row BETWEEN @StartIndex AND @EndIndex";

            List<Users> modelList = DbHelper.Query<Users>(sqlstr, new
            {
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });

            int totalCount = DbHelper.QuerySingle<int>("select count(*) from Users");

            return new PagedResult<Users>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = modelList
            };
        }

        /// <summary>
        /// 获取员工列表
        /// </summary>
        /// <returns></returns>
        public List<Users> GetUserList()
        {
            string sqlstr = "select * from Users";
            List<Users> userList = DbHelper.Query<Users>(sqlstr);
            return userList;
        }

        /// <summary>
        /// 根据UserID获取用户信息
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public Users GetUserInfoByUserID(string UserID)
        {
            string sqlstr = $"select * from Users where UserID={UserID}";
            Users user = DbHelper.QuerySingle<Users>(sqlstr);
            return user;
        }
    }
}
