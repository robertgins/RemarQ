// -----------------------------------------------------------------------------
//  Copyright 8/31/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
 
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	public class Configuration : ConfigurationSection
	{
		public int PollingIntervalInSeconds
		{
			get { return (int)this["pollingIntervalInSeconds"]; }
		}

		[ConfigurationProperty("listBatchSize", IsRequired = false, DefaultValue = "5")]
		public int ListBatchSize
		{
			get { return (int)this["listBatchSize"]; }
		}

		[ConfigurationProperty("largeListBatchSize", IsRequired = false, DefaultValue = "2")]
		public int LargeListBatchSize
		{
			get { return (int)this["largeListBatchSize"]; }
		}

		public static Configuration GetConfiguration()
		{
			Configuration configuration = ConfigurationManager.GetSection("remarQListProcessor")as Configuration;

			if (configuration != null)
			{
				return configuration;
			}

			return new Configuration();
		}
	}
}