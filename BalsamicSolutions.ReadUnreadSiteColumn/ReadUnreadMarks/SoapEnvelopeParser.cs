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