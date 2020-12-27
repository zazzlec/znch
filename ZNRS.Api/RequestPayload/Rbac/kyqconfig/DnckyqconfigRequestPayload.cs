﻿
using static ZNRS.Api.Entities.Enums.CommonEnum;

namespace ZNRS.Api.RequestPayload.Rbac.Kyqconfig
{
    /// <summary>
    /// 
    /// </summary>
    public class DnckyqconfigRequestPayload : RequestPayload
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






