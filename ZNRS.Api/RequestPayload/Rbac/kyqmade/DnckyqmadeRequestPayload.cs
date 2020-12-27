﻿
using static ZNRS.Api.Entities.Enums.CommonEnum;

namespace ZNRS.Api.RequestPayload.Rbac.Kyqmade
{
    /// <summary>
    /// 
    /// </summary>
    public class DnckyqmadeRequestPayload : RequestPayload
    {
        /// <summary>
        /// 是否已被删除
        /// </summary>
        public IsDeleted IsDeleted { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public Status Status { get; set; }

        public string kyq { get; set; }
    }
}






