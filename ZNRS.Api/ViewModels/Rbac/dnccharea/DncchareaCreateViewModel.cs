﻿using System;
using ZNRS.Api.Entities.Enums;
using static ZNRS.Api.Entities.Enums.CommonEnum;

namespace ZNRS.Api.ViewModels.Rbac.Dnccharea
{
	public class DncchareaCreateViewModel
	{
    
        public System.Int32 Id = 0;


        /// <summary>
        /// 序号
        /// </summary>

        public DateTime? RealTime { get; set; }
        public System.Double Wrlhigh_Val { get; set; }

        /// <summary>
    	/// 区域描述
    	/// </summary>
        public System.String K_Name_kw { get; set; } 
    	
	
    
        /// <summary>
    	/// 备注
    	/// </summary>
        public System.String Remarks { get; set; } 
    	
	
    
        /// <summary>
    	/// 锅炉ID
    	/// </summary>
    	public ZNRS.Api.Entities.Dncboiler DncBoiler { get; set; } 
    	
	
    
        /// <summary>
    	/// 锅炉名称
    	/// </summary>
        public System.String DncBoiler_Name { get; set; } 
    	
	
    
        /// <summary>
    	/// 吹灰类型ID
    	/// </summary>
    	public ZNRS.Api.Entities.Dncchtype DncChtype { get; set; } 
    	
	
    
        /// <summary>
    	/// 吹灰类型名称
    	/// </summary>
        public System.String DncChtype_Name { get; set; } 
    	
	
    
        /// <summary>
    	/// 污染率
    	/// </summary>
        public System.Double Wrl_Val { get; set; } 
    	
	
	
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
