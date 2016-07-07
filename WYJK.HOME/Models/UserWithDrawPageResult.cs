using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WYJK.Entity;

namespace WYJK.HOME.Models
{
    public class UserWithDrawPageResult<TEntity> : PagedResult<TEntity> where TEntity : class, new()
    {
        public UserWithDrawPageModel Parameter { get; set; }

    }
}