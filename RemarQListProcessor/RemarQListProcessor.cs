// -----------------------------------------------------------------------------
//  Copyright 7/8/2016 (c) Balsamic Software, LLC. All rights reserved.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  ANY KIND, EITHER 
//  EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR 
//  A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	public partial class RemarQListProcessor : ServiceBase
	{
		object _LockProxy = new object();
		string _AppPath = string.Empty;
		Configuration _Config = null;
		ListProcessor _LargeListProcessor = null;
		ListProcessor _ListProcessor = null;

		public RemarQListProcessor(string applicationPath)
		{
			this.InitializeComponent();
			this._AppPath = applicationPath;

			this.CanStop = true;
			this.CanPauseAndContinue = true;
		}

		internal void RunOnce()
		{
			lock (this._LockProxy)
			{
				using (ListProcessor listP = new ListProcessor(true, this.EventLog))
				{
					listP.BatchSize = this._Config.LargeListBatchSize;
					listP.Interval = this._Config.PollingIntervalInSeconds * 1000;
					listP.RunOnce();
				}

				using (ListProcessor listP = new ListProcessor(false, this.EventLog))
				{
					listP.BatchSize = this._Config.ListBatchSize;
					listP.Interval = this._Config.PollingIntervalInSeconds * 1000;
					listP.RunOnce();
				}
			}
		}

		protected override void OnStart(string[] args)
		{
			this.OnStop();
			lock (this._LockProxy)
			{
				this._Config = Configuration.GetConfiguration();
				if (this._Config.LargeListBatchSize > 0)
				{
					this._LargeListProcessor = new ListProcessor(true, this.EventLog);
					this._LargeListProcessor.BatchSize = this._Config.LargeListBatchSize;
					this._LargeListProcessor.Interval = this._Config.PollingIntervalInSeconds * 1000;
					this._LargeListProcessor.Start();
				}
				if (this._Config.ListBatchSize > 0)
				{
					this._ListProcessor = new ListProcessor(false, this.EventLog);
					this._ListProcessor.BatchSize = this._Config.ListBatchSize;
					this._ListProcessor.Interval = this._Config.PollingIntervalInSeconds * 1000;
					this._ListProcessor.Start();
				}
			}
		}

		protected override void OnPause()
		{
			lock (this._LockProxy)
			{
				if (null != this._LargeListProcessor)
				{
					this._LargeListProcessor.Pause();
				}
				if (null != this._ListProcessor)
				{
					this._ListProcessor.Pause();
				}
			}
		}

		protected override void OnContinue()
		{
			lock (this._LockProxy)
			{
				this._Config = Configuration.GetConfiguration();
				if (null != this._LargeListProcessor)
				{
					this._ListProcessor.BatchSize = this._Config.LargeListBatchSize;
					this._LargeListProcessor.Interval = this._Config.PollingIntervalInSeconds * 1000;
					this._LargeListProcessor.Continue();
				}
				if (null != this._ListProcessor)
				{
					this._ListProcessor.BatchSize = this._Config.ListBatchSize;
					this._ListProcessor.Interval = this._Config.PollingIntervalInSeconds * 1000;
					this._ListProcessor.Continue();
				}
			}
		}

		protected override void OnStop()
		{
			lock (this._LockProxy)
			{
				if (null != this._LargeListProcessor)
				{
					this._LargeListProcessor.Stop();
					this._LargeListProcessor.Dispose();
					this._LargeListProcessor = null;
				}
				if (null != this._ListProcessor)
				{
					this._ListProcessor.Stop();
					this._ListProcessor.Dispose();
					this._ListProcessor = null;
				}
			}
		}
	}
}