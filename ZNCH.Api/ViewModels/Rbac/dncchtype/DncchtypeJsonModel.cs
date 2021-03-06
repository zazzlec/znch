﻿using System;
using ZNCH.Api.Entities.Enums;
using static ZNCH.Api.Entities.Enums.CommonEnum;

namespace ZNCH.Api.ViewModels.Rbac.Dncchtype
{
	public class DncchtypeJsonModel
	{
    
    
    
        /// <summary>
    	/// 序号
    	/// </summary>
        public System.Int32 Id { get; set; } 
	
    
        /// <summary>
    	/// 吹灰类型描述
    	/// </summary>
        public System.String K_Name_kw { get; set; } 
	
    
        /// <summary>
    	/// 备注
    	/// </summary>
        public System.String Remarks { get; set; } 
	
    
        /// <summary>
    	/// 吹灰时长（秒）
    	/// </summary>
        public System.Int32 Ch_time_Val { get; set; } 
	
    
        /// <summary>
    	/// 锅炉ID
    	/// </summary>
    	public ZNCH.Api.Entities.Dncboiler DncBoiler { get; set; } 
        public int DncBoilerId { get; set; } 
	
    
        /// <summary>
    	/// 锅炉名称
    	/// </summary>
        public System.String DncBoiler_Name { get; set; } 
	
	
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
