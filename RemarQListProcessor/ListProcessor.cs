// -----------------------------------------------------------------------------
//  Copyright 7/8/2016 (c) Balsamic Software, LLC. All rights reserved.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  ANY KIND, EITHER 
//  EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR 
//  A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	public class ListProcessor : UtilityWorker
	{
		ListInitializationProcessor _JobProcessor = null;
		readonly bool _LargeList = false;
		int _BatchSize = 5;
		readonly EventLog _EventLog = null;
		readonly object _LockProxy = new object();

		public ListProcessor(bool largeList, EventLog eLog)
		{
			this._LargeList = largeList;
			this._EventLog = eLog;
			if (this._LargeList)
			{
				base.Name = "Large list processor";
			}
			else
			{
				base.Name = "List processor";
			}
		}

		public int BatchSize
		{
			get { return this._BatchSize; }
			set
			{
				this._BatchSize = value;
				if (this._BatchSize <= 1)
				{
					this._BatchSize = 1;
				}
			}
		}

		public void LogMessage(string messageToLog)
		{
			this._EventLog.WriteEntry(messageToLog, EventLogEntryType.Information);
		}

		public void LogWarning(string messageToLog)
		{
			this._EventLog.WriteEntry(messageToLog, EventLogEntryType.Warning);
		}

		public void LogError(string messageToLog)
		{
			this._EventLog.WriteEntry(messageToLog, EventLogEntryType.Error);
		}

		public void RunOnce()
		{
			this.DoOneThing();
		}

		protected override void DoOneThing()
		{
			lock (this._LockProxy)
			{
				if (null != this._JobProcessor)
				{
					throw new InvalidOperationException("Thread deadlocked, cannot cancel job processor");
				}
				if (this._LargeList)
				{
					this._JobProcessor = new ListInitializationProcessor(Constants.ReadUnreadLargeQueueTableName,"External RemarQ Large List Initialization");
				}
				else
				{
					this._JobProcessor = new ListInitializationProcessor(Constants.ReadUnreadQueueTableName,"External RemarQ List Initialization");
				}
			}

			this._JobProcessor.Execute(); 

			lock (this._LockProxy)
			{
				this._JobProcessor = null;
			}
		}

		public override void Stop()
		{
			lock (this._LockProxy)
			{
				if (null != this._JobProcessor)
				{
					this._JobProcessor.Canceled = true;
				}
			}
			base.Stop();
		}

		public override void Pause()
		{
			lock (this._LockProxy)
			{
				if (null != this._JobProcessor)
				{
					this._JobProcessor.Paused = true;
				}
			}
			base.Pause();
		}
		
		public override void Continue()
		{
			lock (this._LockProxy)
			{
				if (null != this._JobProcessor)
				{
					this._JobProcessor.Paused = false;
				}
			}
			base.Continue();
		}
	}
}