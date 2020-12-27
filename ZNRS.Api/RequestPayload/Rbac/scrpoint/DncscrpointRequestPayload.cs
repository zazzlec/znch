﻿
using static ZNRS.Api.Entities.Enums.CommonEnum;

namespace ZNRS.Api.RequestPayload.Rbac.Scrpoint
{
    /// <summary>
    /// 
    /// </summary>
    public class DncscrpointRequestPayload : RequestPayload
    {
        /// <summary>
        /// 是否已被删除
        /// </summary>
        public IsDeleted IsDeleted { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public Status Status { get; set; }
    }
}





