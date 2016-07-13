// -----------------------------------------------------------------------------
//  Copyright 6/28/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// typed exception
	/// </summary>
	[Serializable]
	public class RemarQException : System.Exception
	{
		public RemarQException()
			: base()
		{
		}

		public RemarQException(string errorText)
			: base(errorText)
		{
		}

		public RemarQException(string errorText, Exception innerException)
			: base(errorText, innerException)
		{
		}

		protected RemarQException(SerializationInfo info, StreamingContext context)
			: base(info,context)
		{
		}
	}
}