﻿using System;
using ZNCH.Api.Entities.Enums;
using static ZNCH.Api.Entities.Enums.CommonEnum;

namespace ZNCH.Api.ViewModels.Rbac.Dnco2nox_point
{
	public class Dnco2nox_pointJsonModel
	{
    
    
    
        /// <summary>
    	/// 序号
    	/// </summary>
        public System.Int64 Id { get; set; } 
	
    
        /// <summary>
    	/// 测点类型
    	/// </summary>
    	public System.String DncType { get; set; }
        public System.Int32 DncTypeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public System.String DncType_Name { get; set; } 
	
    
        /// <summary>
    	/// 实际取值时间
    	/// </summary>
        public DateTime? RealTime { get; set; } 
	
    
        /// <summary>
    	/// 测点值
    	/// </summary>
        public System.String Ovalue { get; set; } 
	
    
        /// <summary>
    	/// 巡测系统取值编码
    	/// </summary>
        public System.String TagCode { get; set; } 
	
    
	
    
        /// <summary>
    	/// 
    	/// </summary>
    	public System.String DncBoiler { get; set; } 
	
    
        /// <summary>
    	/// 
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
