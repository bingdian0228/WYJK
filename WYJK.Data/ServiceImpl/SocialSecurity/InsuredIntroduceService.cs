using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;

namespace WYJK.Data.ServiceImpl
{
    /// <summary>
    /// 参保介绍
    /// </summary>
    public class InsuredIntroduceService : IInsuredIntroduceService
    {
        public bool BatchDeleteInfos(string idsStr)
        {
            string sql = $"delete from InsuredIntroduce where id in({idsStr})";
            int result = DbHelper.ExecuteSqlCommand(sql, null);
            return result > 0;
        }

        public async Task<InsuredIntroduce> GetInsuredIntroduceDetail(int ID)
        {
            string sqlstr = $"select * from InsuredIntroduce where ID = {ID}";
            InsuredIntroduce insuredIntroduce = await DbHelper.QuerySingleAsync<InsuredIntroduce>(sqlstr);
            return insuredIntroduce;
        }

        public async Task<PagedResult<InsuredIntroduce>> GetInsuredIntroduceList(PagedParameter parameter)
        {
            string sql = $"select * from (select ROW_NUMBER() OVER(ORDER BY i.ID )AS Row,i.* from InsuredIntroduce i ) ii where ii.Row between @StartIndex AND @EndIndex";


            List<InsuredIntroduce> insuredIntroduceList = await DbHelper.QueryAsync<InsuredIntroduce>(sql, new
            {
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });
            int totalCount = await DbHelper.QuerySingleAsync<int>($"select count(0) from InsuredIntroduce");

            return new PagedResult<InsuredIntroduce>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = insuredIntroduceList
            };
        }

        public async Task<List<InsuredIntroduce>> GetInsuredIntroduceList()
        {
            string sql = $"select * from InsuredIntroduce";

            List<InsuredIntroduce> insuredIntroduceList = await DbHelper.QueryAsync<InsuredIntroduce>(sql);

            return insuredIntroduceList;
        }

        /// <summary>
        /// 参保信息添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> InsuredIntroduceAdd(InsuredIntroduce model)
        {
            string sql = $"insert into InsuredIntroduce(Name,StrContent) values('{model.Name}','{model.StrContent}')";
            int result = await DbHelper.ExecuteSqlCommandAsync(sql);
            return result > 0;
        }

        /// <summary>
        /// 参保信息修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> ModifyInsuredIntroduce(InsuredIntroduce model)
        {
            string sql = $"update InsuredIntroduce set Name='{model.Name}',StrContent='{model.StrContent}' where ID={model.ID}";

            int result = await DbHelper.ExecuteSqlCommandAsync(sql);
            return result > 0;
        }
    }
}
