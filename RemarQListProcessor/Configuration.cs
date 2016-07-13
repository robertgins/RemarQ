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