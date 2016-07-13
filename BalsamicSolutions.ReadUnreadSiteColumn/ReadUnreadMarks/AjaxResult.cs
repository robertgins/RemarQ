// -----------------------------------------------------------------------------
//  Copyright 5/28/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	[Serializable]
	public class AjaxResult
	{
		public AjaxResult()
		{
		}

		public AjaxResult(System.Collections.ICollection ajaxRows)
		{
			if (null == ajaxRows)
			{
				throw new ArgumentNullException("ajaxRows");
			}
			this.Total =ajaxRows.Count;
			this.Data = ajaxRows;
		}

		public AjaxResult(System.Collections.IEnumerable ajaxRows, int totalCount)
		{
			this.Data = ajaxRows;
			this.Total = totalCount;
		}

		//this will always be null, unless we have to support
		//aggregrates, in which case we should switch to the 
		//MVC wrapper "ToDataSourceResult() and let that generate the
		//grid details
		public System.Collections.IEnumerable AggregateResults { get; set; }

		public System.Collections.IEnumerable Data { get; set; }

		public object Errors { get; set; }

		public int Total { get; set; }
	}
}