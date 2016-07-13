// -----------------------------------------------------------------------------
//  Copyright 7/9/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
 
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BalsamicSolutions.ReadUnreadSiteColumn;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Utilities
{
	partial	class Program
	{
		static void Validate()
		{
			//collect all installed fields
			List<ListConfiguration> allFields = SqlRemarQ.GetListConfigurations();
			Console.WriteLine(string.Format("{0} Fields to check ", allFields.Count));
			if (allFields.Count > 0)
			{
				foreach (ListConfiguration listconfig in allFields)
				{
					SqlRemarQ.VerifyReadUnreadTableSchema(listconfig.ListId, true);
				}
			}
			Console.WriteLine(string.Format("Done checking schema {0} verified or updated ", allFields.Count));
		}
	}
}