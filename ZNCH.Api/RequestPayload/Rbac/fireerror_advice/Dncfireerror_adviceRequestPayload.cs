
using static ZNCH.Api.Entities.Enums.CommonEnum;

namespace ZNCH.Api.RequestPayload.Rbac.Fireerror_advice
{
    /// <summary>
    /// 
    /// </summary>
    public class Dncfireerror_adviceRequestPayload : RequestPayload
    {
        /// <summary>
        /// 是否已被删除
        /// </summary>
        public IsDeleted IsDeleted { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public Status Status { get; set; }

        public string boilertime { get; set; }

        public string t { get; set; }
    }
}






