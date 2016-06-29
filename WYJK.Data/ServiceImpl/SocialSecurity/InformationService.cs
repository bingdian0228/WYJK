using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Data.ServiceImpl
{
    /// <summary>
    /// 信息接口
    /// </summary>
    public class InformationService : IInformationService
    {

        public async Task<PagedResult<Information>> GetInsuredIntroduceList(InformationParameter parameter)
        {
            string sql = $"select * from (select ROW_NUMBER() OVER(ORDER BY i.ID )AS Row,i.* from Information i where Name like @Name) ii where ii.Row between @StartIndex AND @EndIndex";

            List<Information> informationList = await DbHelper.QueryAsync<Information>(sql, new
            {
                Name = "%" + parameter.Name + "%",
                Type = parameter.Type,
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });
            int totalCount = await DbHelper.QuerySingleAsync<int>("select count(0) from Information where Name like @Name ", new { Name = "%" + parameter.Name + "%" });

            return new PagedResult<Information>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = informationList
            };
        }

        /// <summary>
        /// 信息添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> InformationAdd(Information model)
        {
            string ImgUrls = string.Join(";", model.ImgUrls);
            string sql = $"insert into Information(Name,ImgUrl,StrContent) values('{model.Name}','{ImgUrls}','{model.StrContent}')";
            int result = await DbHelper.ExecuteSqlCommandAsync(sql);
            return result > 0;
        }

        /// <summary>
        /// 根据信息ID获取基本信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public async Task<Information> GetInfomationDetail(int ID)
        {
            string sql = $"select * from Information where ID={ID}";
            Information info = await DbHelper.QuerySingleAsync<Information>(sql);
            return info;
        }

        /// <summary>
        /// 更新信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> ModifyInformation(Information model)
        {
            string sql = $"update Information set Name = @Name, Type = @Type, ImgUrl = @ImgUrl where ID = @ID";

            DbParameter[] parameters = new DbParameter[] {
                new SqlParameter("@Name", model.Name),
                new SqlParameter("@Type", model.Type),
                new SqlParameter("@ImgUrl", model.ImgUrl ?? (object)DBNull.Value),
                new SqlParameter("@ID", model.ID)
            };

            int result = await DbHelper.ExecuteSqlCommandAsync(sql, parameters);
            return result > 0;
        }

        /// <summary>
        /// 批量删除信息
        /// </summary>
        /// <param name="infoidsStr"></param>
        /// <returns></returns>
        public bool BatchDeleteInfos(string infoidsStr)
        {
            string sql = $"delete from Information where id in({infoidsStr})";
            int result = DbHelper.ExecuteSqlCommand(sql, null);
            return result > 0;
        }
    }
}
