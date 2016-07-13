// -----------------------------------------------------------------------------
//This is free and unencumbered software released into the public domain.
//Anyone is free to copy, modify, publish, use, compile, sell, or
//distribute this software, either in source code form or as a compiled
//binary, for any purpose, commercial or non-commercial, and by any
//means.
//In jurisdictions that recognize copyright laws, the author or authors
//of this software dedicate any and all copyright interest in the
//software to the public domain.We make this dedication for the benefit
//of the public at large and to the detriment of our heirs and
//successors.We intend this dedication to be an overt act of
//relinquishment in perpetuity of all present and future rights to this
//software under copyright law.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
//OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
//OTHER DEALINGS IN THE SOFTWARE.
//For more information, please refer to<http://unlicense.org>
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