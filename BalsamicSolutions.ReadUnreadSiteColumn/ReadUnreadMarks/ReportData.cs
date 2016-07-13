// -----------------------------------------------------------------------------
//  Copyright 5/29/2014 (c) Balsamic Solutions, Inc. All rights reserved.
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
	/// <summary>
	/// class that represents one row in a document read report
	/// </summary>
	public class ReportData
	{
		public string Id { get; set; }

		public int Version { get; set; }

		public DateTime ReadOn { get; set; }

		public int UserId { get; set; }

		public string ReadBy { get; set; }
		
		public string ReadByEmail { get; set; }

		public ReportData()
		{
			this.Id = Guid.NewGuid().ToString("B");
		}
	}
}