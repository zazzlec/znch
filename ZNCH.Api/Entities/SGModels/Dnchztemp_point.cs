﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ZNCH.Api.Entities.Enums.CommonEnum;

namespace ZNCH.Api.Entities
{
	[Serializable]
	public class Dnchztemp_point
	{
    
    
    
        /// <summary>
    	/// 序号
    	/// </summary>
        [Key,Required]
   
        public System.Int64 Id { get; set; } 
        
    
        /// <summary>
    	/// 测点类型
    	/// </summary>
        
    	public Dnctype DncType { get; set; } 
        public System.Int32 DncTypeId { get; set; } 
    
        /// <summary>
    	/// 测点类型名称
    	/// </summary>
        
   
        public System.String DncType_Name { get; set; } 
        
    
        /// <summary>
    	/// 实际时间
    	/// </summary>
        
   
        public DateTime? RealTime { get; set; } 
        
    
        /// <summary>
    	/// 测点数值（数组）
    	/// </summary>
        
   
        public System.String Pvalue { get; set; } 
        
    
        /// <summary>
    	/// 备注
    	/// </summary>
        
   
        public System.String Remarks { get; set; } 
        
    
        /// <summary>
    	/// 
    	/// </summary>
        
    	public Dncboiler DncBoiler { get; set; } 
        public System.Int32 DncBoilerId { get; set; } 
    
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
