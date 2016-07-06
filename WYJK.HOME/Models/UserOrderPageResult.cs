using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WYJK.Entity;

namespace WYJK.HOME.Models
{

    public class UserOrderPageResult<TEntity>: PagedResult<TEntity> where TEntity : class, new()
    {
        public int? Status { get; set; }


    }
}