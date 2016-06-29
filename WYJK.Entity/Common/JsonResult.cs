using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WYJK.Entity
{
    #region 调用服务时返回的数据结果,用于返回给第三方的结果信息
    /// <summary>
    /// 调用服务时返回的数据结果,用于返回给第三方的结果信息  where TEntity : class
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class JsonResult<TEntity>
    {
        /// <summary>
        /// 初始化对象的实例
        /// </summary>
        public JsonResult() { }
        /// <summary>
        /// 初始化对象的实例
        /// </summary>
        /// <param name="status">错误码</param>
        /// <param name="mssage">错误信息</param>
        public JsonResult(Boolean status, string mssage)
        {
            this.status = status;
            this.Message = mssage;
        }
        /// <summary>
        /// 错误码
        /// </summary>
        public bool status { set; get; }
        /// <summary>
        /// 响应信息
        /// </summary>
        public string Message { set; get; }
        /// <summary>
        /// 返回的数据
        /// </summary>
        public TEntity Data { set; get; }
        /// <summary>
        /// 返回字符串的表达形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    #endregion

    #region 分页基础类
    /// <summary>
    /// 分页参数接口
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public  class PagedParameter
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        [JsonProperty(PropertyName = "pageIndex")]
        public virtual int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页记录数量
        /// </summary>
        [JsonProperty(PropertyName = "pageSize")]
        public virtual int PageSize { set; get; } = 10;
        /// <summary>
        /// 搜索关键字[可选]
        /// </summary>
        [JsonProperty(PropertyName = "keyword")]
        public virtual string Keyword { set; get; }

        /// <summary>
        /// 排序类型
        /// </summary>
        [JsonProperty(PropertyName = "orderBy")]
        public virtual int OrderBy { set; get; } = 0;

        /// <summary>
        /// 是否是降序
        /// </summary>
        [JsonProperty(PropertyName = "isDescending")]
        public virtual bool IsDescending { set; get; } = true;
        /// <summary>
        /// 获取跳过的记录数量
        /// </summary>
        public int SkipCount => (PageIndex - 1) * PageSize + 1;
        /// <summary>
        /// 获取的条数
        /// </summary>
        public int TakeCount => PageSize * PageIndex;
    }
    /// <summary>
    /// 分页结果统一接口
    /// </summary>
    public interface IPagedResult : IEnumerable
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        int PageIndex { get; set; }
        /// <summary>
        /// 每页记录数量
        /// </summary>
        int PageSize { set; get; }
        /// <summary>
        /// 总页数
        /// </summary>
        int TotalPageCount { get; }
        /// <summary>
        /// 总记录数量
        /// </summary>
        int TotalItemCount { set; get; }
    }

    /// <summary>
    /// 统一分页泛型接口
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IPagedResult<TEntity> : IEnumerable<TEntity>, IPagedResult
    {
        /// <summary>
        /// 分页集合数据列表
        /// </summary>
        IEnumerable<TEntity> Items { set; get; }
    }
    /// <summary>
    /// 分页基类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [JsonObject(MemberSerialization.OptIn)]
    public class PagedResult<TEntity> : IPagedResult<TEntity> where TEntity : class, new()
    {
        /// <summary>
        /// 初始化对象的实例
        /// </summary>
        public PagedResult()
        {
            PageSize = 2;
            Items = new List<TEntity>();
        }
        /// <summary>
        /// 记录集合
        /// </summary>
        [JsonProperty(PropertyName = "list")]
        public virtual IEnumerable<TEntity> Items { get; set; }
        /// <summary>
        /// 当前页索引
        /// </summary>
        [JsonProperty(PropertyName = "pageIndex")]
        public int PageIndex { get; set; }
        /// <summary>
        /// 每页记录数量
        /// </summary>
        [JsonProperty(PropertyName = "pageSize")]
        public int PageSize { set; get; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPageCount
        {
            get { return (int)Math.Ceiling(TotalItemCount / (double)PageSize); }
        }
        /// <summary>
        /// 总记录数量
        /// </summary>
        [JsonProperty(PropertyName = "total")]
        public int TotalItemCount { get; set; }

        /// <summary>
        /// 实现迭代
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
    #endregion
}
