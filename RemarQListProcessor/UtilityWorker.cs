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