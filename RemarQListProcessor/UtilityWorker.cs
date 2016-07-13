// -----------------------------------------------------------------------------
//  Copyright 9/2/2014 (c) Balsamic Solutions, Inc. All rights reserved.
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
using System.Threading;
using System.Threading.Tasks;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	public abstract class UtilityWorker : IDisposable
	{
		//our worker thread
		Thread _Thread = null;
		//a general lock for internal operations
		readonly object _LockProxy = new object();
		//a flag to indicate that we need to stop working
		long _AbortLongRunningProcesses = 0;
		int _ManagedThreadId = 0;
		//a flag to indicate that we are paused
		bool _Paused = false;
		//the trace name of this process
		string _Name = string.Empty;
		bool _Disposed = false;
		int _Interval = 1000;

		public int Interval
		{
			get { return this._Interval; }
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				this._Interval = value;
			}
		}

		//derived classes implement this and are called
		//whenever they are supposed to execute one
		//unit of work it is required to implement
		protected abstract void DoOneThing();

		//derived classes may implement this and are activated in
		//the worker thread process at an unhandled error
		//return true if we want to throw the error
		protected virtual bool OnError(Exception threadError)
		{
			return true;
		}

		protected string Name
		{
			get { return this._Name; }
			set { this._Name = value; }
		}

		protected string Threadname
		{
			get { return this._Name + "(" + this._ManagedThreadId.ToString() + ")"; }
		}

		/// <summary>
		/// our simple worker thread
		/// </summary>
		void ThreadProcess()
		{
			//Need a small delay
			Thread.Sleep(1000);
   
			bool okDokey = true;
			while (okDokey)
			{
				try
				{
					lock (this._LockProxy)
					{
						if (this._Paused)
						{
							Monitor.Wait(this._LockProxy);
						}
						if (!this.AbortLongRunningProcesses)
						{
							this.DoOneThing();
						}
					}
					Thread.Sleep(this._Interval);
				}
				catch (ThreadAbortException)
				{
					okDokey = false;
				}
				catch (Exception threadError)
				{
					if (this.OnError(threadError))
					{
						throw;
					}
				}
			}
		}

		//pause execution
		public virtual void Pause()
		{
			if (this._Disposed)
			{
				throw new ObjectDisposedException("UtilityWorker");
			}
			this.Paused = true;
		}

		//continue execution
		public virtual void Continue()
		{
			if (this._Disposed)
			{
				throw new ObjectDisposedException("UtilityWorker");
			}
			this.Paused = false;
		}

		//derived client accessor for abort indicators 
		protected bool AbortLongRunningProcesses
		{
			get
			{
				long flagValue = Interlocked.Read(ref this._AbortLongRunningProcesses);
				return flagValue == 1;
			}
			set
			{
				if (value)
				{
					Interlocked.Exchange(ref this._AbortLongRunningProcesses, 1);
				}
				else
				{
					Interlocked.Exchange(ref this._AbortLongRunningProcesses, 0);
				}
			}
		}

		//accessor for paused state
		public bool Paused
		{
			get
			{
				lock (this._LockProxy)
				{
					return this._Paused;
				}
			}
			set
			{
				this.AbortLongRunningProcesses = value;
				lock (this._LockProxy)
				{
					this._Paused = value;
					Monitor.PulseAll(this._LockProxy);
				}
			}
		}

		//start up the worker
		public virtual void Start()
		{
			if (this._Disposed)
			{
				throw new ObjectDisposedException("UtilityWorker");
			}

			this.AbortLongRunningProcesses = false;
			this._Thread = new Thread(new ThreadStart(this.ThreadProcess));
			this._Thread.Name = this._Name;
			this._ManagedThreadId = this._Thread.ManagedThreadId;
			this._Thread.Start();
		}

		//shut down the worker
		public virtual void Stop()
		{
			if (this._Disposed)
			{
				throw new ObjectDisposedException("UtilityWorker");
			}

			this.AbortLongRunningProcesses = true;
			if (null != this._Thread)
			{
				this._Thread.Abort();
				this._Thread.Join(1000);
				this._Thread = null;
				this._ManagedThreadId = 0;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this._Disposed)
			{
				throw new ObjectDisposedException("UtilityWorker");
			}
			if (disposing)
			{
				this.Stop();
				this._Disposed = true;
			}
		}

		//clean up the system
		public void Dispose()
		{
			if (this._Disposed)
			{
				throw new ObjectDisposedException("UtilityWorker");
			}
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}