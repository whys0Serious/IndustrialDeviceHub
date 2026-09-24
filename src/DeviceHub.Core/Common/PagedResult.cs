using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Common
{
    /// <summary>
    /// 分页查询结果
    /// 泛型 列表共用
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>
        /// 当前页数据
        /// </summary>
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 当前页码（从1开始）
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每页条数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPrevious => Page > 1;

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNext => Page < TotalPages;
    }
}
