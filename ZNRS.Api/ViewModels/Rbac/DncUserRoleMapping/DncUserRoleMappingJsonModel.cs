using System;
using ZNRS.Api.Entities.Enums;
using static ZNRS.Api.Entities.Enums.CommonEnum;

namespace ZNRS.Api.ViewModels.Rbac.DncUserRoleMapping
{
	public class DncUserRoleMappingJsonModel
	{
    
    
    
        /// <summary>
    	/// 
    	/// </summary>
        public System.Guid UserGuid { get; set; } 
	
    
        /// <summary>
    	/// 
    	/// </summary>
    	public System.String RoleCode { get; set; } 
	
    
        /// <summary>
    	/// 
    	/// </summary>
        public System.DateTime CreatedOn { get; set; } 
	
	
        /// <summary>
        /// 是否可用(0:禁用,1:可用)
        /// </summary>
        public Status Status { get; set; }
        /// <summary>
        /// 是否已删
        /// </summary>
        public IsDeleted IsDeleted { get; set; }
		
	}
}
