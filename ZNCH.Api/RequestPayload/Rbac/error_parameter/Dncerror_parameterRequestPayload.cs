﻿
using static ZNCH.Api.Entities.Enums.CommonEnum;

namespace ZNCH.Api.RequestPayload.Rbac.Error_parameter
{
    /// <summary>
    /// 
    /// </summary>
    public class Dncerror_parameterRequestPayload : RequestPayload
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






