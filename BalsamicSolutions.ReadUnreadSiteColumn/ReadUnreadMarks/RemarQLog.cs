// -----------------------------------------------------------------------------
//  Copyright 7/4/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// SharePoint logging , mostly self explanatory
	/// </summary>
	internal class RemarQLog : SPDiagnosticsServiceBase
	{
		private static RemarQLog _Log;

		const string ERROR = "Error";
		const string WARNING = "Warning";
		const string LOGGING = "Logging";

		static readonly SPDiagnosticsCategory _ErrorCategory = new SPDiagnosticsCategory(ERROR, TraceSeverity.Unexpected, EventSeverity.Error);
		static readonly SPDiagnosticsCategory _WarningCategory = new SPDiagnosticsCategory(WARNING, TraceSeverity.Medium, EventSeverity.Error);
		static readonly SPDiagnosticsCategory _LoggingCategory = new SPDiagnosticsCategory(LOGGING, TraceSeverity.Verbose, EventSeverity.Error);
		public const uint LOGGING_ID = 512;
		public const uint WARNING_ID = LOGGING_ID + 512;
		public const uint ERROR_ID = WARNING_ID + 512;
		public const uint ERRORSTACK_ID = ERROR_ID + 512;

		public static SPDiagnosticsCategory CategoryError
		{
			get { return _ErrorCategory; }
		}

		public static SPDiagnosticsCategory CategoryWarning
		{
			get { return _WarningCategory; }
		}

		public static SPDiagnosticsCategory CategoryLogging
		{
			get { return _LoggingCategory; }
		}

		public static RemarQLog Log
		{
			get
			{
				if (_Log == null)
				{
					_Log = new RemarQLog();
				}
				return _Log;
			}
		}

		private RemarQLog()
			: base(Constants.ApplicationName, SPFarm.Local)
		{
		}

		protected override IEnumerable<SPDiagnosticsArea> ProvideAreas()
		{
			List<SPDiagnosticsArea> areas = new List<SPDiagnosticsArea>        
												{
													new SPDiagnosticsArea(Constants.ApplicationName, new List<SPDiagnosticsCategory>
																										 {
																											 CategoryError,
																											 CategoryWarning,
																											 CategoryLogging
																										 })
												};
			return areas;
		}

		public static void LogMessage(string logThis)
		{
#if SHAREPOINTLOG
			SPDiagnosticsService.Local.WriteTrace(LOGGING_ID, CategoryLogging, TraceSeverity.Verbose, logThis);
#else
			Log.WriteTrace(LOGGING_ID, CategoryLogging, TraceSeverity.Verbose, logThis);
#endif
		}

		public static void LogWarning(string logThis)
		{
#if SHAREPOINTLOG
			SPDiagnosticsService.Local.WriteTrace(WARNING_ID, CategoryLogging, TraceSeverity.Medium, logThis);
#else
			Log.WriteTrace(WARNING_ID, CategoryLogging, TraceSeverity.Medium, logThis);
#endif
		}

		public static void LogError(string logThis, Exception errorToLog)
		{
#if SHAREPOINTLOG
			SPDiagnosticsService.Local.WriteTrace(ERROR_ID, CategoryLogging, TraceSeverity.Unexpected, logThis);
#else
			Log.WriteTrace(ERROR_ID, CategoryLogging, TraceSeverity.Unexpected, logThis);
#endif
			LogError(errorToLog);
		}

		public static void LogError(Exception errorToLog)
		{
			if (null != errorToLog)
			{
				string stackTrace = GetExceptionText(errorToLog, true);
#if SHAREPOINTLOG
				SPDiagnosticsService.Local.WriteTrace(ERRORSTACK_ID, CategoryLogging, TraceSeverity.Unexpected, stackTrace);
#else
				Log.WriteTrace(ERRORSTACK_ID, CategoryLogging, TraceSeverity.Unexpected, stackTrace);
#endif
			}
		}

		public static void TraceError(Exception errorToLog)
		{
			if (null != errorToLog)
			{
				string stackTrace = GetExceptionText(errorToLog, false);
#if SHAREPOINTLOG
				SPDiagnosticsService.Local.WriteTrace(ERRORSTACK_ID, CategoryLogging, TraceSeverity.Verbose, stackTrace);
#else
				Log.WriteTrace(ERRORSTACK_ID, CategoryLogging, TraceSeverity.Verbose, stackTrace);
#endif
				Log.WriteTrace(ERRORSTACK_ID, CategoryLogging, TraceSeverity.Verbose, stackTrace);
			}
		}

		/// <summary>
		/// Expand an exception into a readable string
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="withStackTrace"></param>
		/// <param name="includeInnerException"></param>
		/// <returns></returns>
		public static string GetExceptionText(Exception ex, bool withStackTrace, bool includeInnerException)
		{
			if (null == ex)
			{
				throw new ArgumentNullException("ex");
			}
			StringBuilder returnValue = new StringBuilder();
			Exception innerException = ex.InnerException;

			returnValue.AppendLine(ex.Message);
			if (withStackTrace)
			{
				returnValue.AppendLine(ex.StackTrace);
			}

			while (innerException != null && includeInnerException)
			{
				returnValue.AppendLine(innerException.Message);
				if (withStackTrace)
				{
					returnValue.AppendLine(innerException.StackTrace);
				}

				innerException = innerException.InnerException;
			}

			return returnValue.ToString();
		}

		/// <summary>
		/// Expand an exception into a readable string
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		public static string GetExceptionText(Exception ex)
		{
			return GetExceptionText(ex, true, true);
		}

		/// <summary>
		/// Expand an exception into a readable string
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="withStackTrace"></param>
		/// <returns></returns>
		public static string GetExceptionText(Exception ex, bool withStackTrace)
		{
			return GetExceptionText(ex, withStackTrace, true);
		}
	}
}