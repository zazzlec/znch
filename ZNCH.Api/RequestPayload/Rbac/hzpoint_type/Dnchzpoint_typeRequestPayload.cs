﻿
using static ZNCH.Api.Entities.Enums.CommonEnum;

namespace ZNCH.Api.RequestPayload.Rbac.Hzpoint_type
{
    /// <summary>
    /// 
    /// </summary>
    public class Dnchzpoint_typeRequestPayload : RequestPayload
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






