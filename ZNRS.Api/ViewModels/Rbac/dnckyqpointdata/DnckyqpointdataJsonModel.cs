﻿using System;
using ZNRS.Api.Entities.Enums;
using static ZNRS.Api.Entities.Enums.CommonEnum;

namespace ZNRS.Api.ViewModels.Rbac.Dnckyqpointdata
{
	public class DnckyqpointdataJsonModel
	{
    
    
    
        /// <summary>
    	/// 
    	/// </summary>
        public System.Int32 Id { get; set; } 
	
    
        /// <summary>
    	/// 类型
    	/// </summary>
    	public ZNRS.Api.Entities.Dnckyqtype DncKyqTpye { get; set; } 
        public int DncKyqTpyeId { get; set; } 
	
    
        /// <summary>
    	/// 类型
    	/// </summary>
        public System.String DncKyqTpye_Name { get; set; } 
	
    
        /// <summary>
    	/// 测点值
    	/// </summary>
        public System.String Pval { get; set; } 
	
    
        /// <summary>
    	/// 类型
    	/// </summary>
        public System.Single Point_Val { get; set; } 
	
    
        /// <summary>
    	/// 锅炉ID
    	/// </summary>
    	public ZNRS.Api.Entities.Dncboiler DncBoiler { get; set; } 
        public int DncBoilerId { get; set; } 
	
    
        /// <summary>
    	/// 锅炉描述
    	/// </summary>
        public System.String DncBoiler_Name { get; set; }
        /// <summary>
        /// 空预器Id
        /// </summary>

        public ZNRS.Api.Entities.Dnckyq DncKyq { get; set; }
        public System.String DncKyq_Name { get; set; }

        public int DncKyqId { get; set; }

        /// <summary>
        /// 实际时间
        /// </summary>
        public DateTime? RealTime { get; set; } 
	
	
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
