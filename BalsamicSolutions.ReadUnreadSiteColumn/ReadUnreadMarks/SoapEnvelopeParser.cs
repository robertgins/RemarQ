// -----------------------------------------------------------------------------
//  Copyright 7/31/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// handles cloning of the Request Stream so we can read it
	/// when we intercept an Office documenet call to the
	/// cellstorageservice, we cant use an actual 
	/// Request.Filter stream beacuse SharePoint munged it up
	/// however since we are 
	/// </summary>
	public class SoapEnvelopeParser  
	{
		public string SoapEnvelope { get; private set; }

		public SoapEnvelopeParser(Stream sourceStream)
		{
			this.SoapEnvelope = this.ExtractSoapEnvelope(sourceStream);
		}

		/// <summary>
		/// parse a svc call into its soap envelope
		/// </summary>
		/// <param name="httpText"></param>
		/// <returns></returns>
		string ExtractSoapEnvelope(Stream sourceStream)
		{
			if (sourceStream.CanSeek)
			{
				sourceStream.Seek(0, System.IO.SeekOrigin.Begin);
			}
			string returnValue = string.Empty;
			try
			{
				using (StreamReader strReader = new StreamReader(sourceStream, Encoding.UTF8, true, 4096, true))
				{
					//peek at the first characters and see if it has our soap action flag indiciators 
					char[] firstBytes = new char[64];
					int bytesRead = strReader.ReadBlock(firstBytes, 0, 64);
					if (bytesRead == 64)
					{
						string leadStr = new string(firstBytes);
						int posUid = leadStr.IndexOf("--urn:uuid:");
						if (posUid > -1)
						{
							string httpText = leadStr + strReader.ReadToEnd();
							int endUid = httpText.IndexOf("\r\n", posUid);
							if (endUid > -1)
							{
								string uidText = httpText.Substring(posUid, endUid - posUid);
								int posStart = httpText.IndexOf("\r\n\r\n", endUid);
								if (posStart > -1)
								{
									posStart += 4;
									int posEnd = httpText.IndexOf(uidText, posStart);
									if (posEnd > -1)
									{
										returnValue = httpText.Substring(posStart, posEnd - posStart);
									}
								}
							}
						}
					}
				}
			}
			catch (System.IO.IOException)
			{
				returnValue = string.Empty;
			}
			if (sourceStream.CanSeek)
			{
				sourceStream.Seek(0, System.IO.SeekOrigin.Begin);
			}
			return returnValue;
		}
	}
}