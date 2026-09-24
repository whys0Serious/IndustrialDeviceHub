using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Common
{
    /// <summary>
    /// 分页查询请求
    /// 所有列表查询共用
    /// </summary>
    public class PagedRequest
    {
        private int _pageIndex = 1;
        private int _pageSize = 20;

        /// <summary>
        /// 页码
        /// 从1开始小于1时修正为1
        /// </summary>
        public int PageIndex
        {
            get => _pageIndex;
            set => _pageIndex = value < 1 ? 1 : value;
        }

        /// <summary>
        /// 每页条数
        /// 限制 1–200边界控制
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value switch
            {
                < 1 => 1,
                > 200 => 200,
                _ => value
            };
        }
    }
}
