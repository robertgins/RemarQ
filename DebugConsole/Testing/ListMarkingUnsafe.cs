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
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using BalsamicSolutions.ReadUnreadSiteColumn;
using Microsoft.Office.Server.Utilities;
using Microsoft.SharePoint;

namespace DebugConsole.Testing
{
	static class ListMarking
	{
		
		 
		/// <summary>
		/// marks an item read for a user
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="listId"></param>
		/// <param name="loginName"></param>
		internal static void MarkRead(int[] itemIds, Guid listId, string loginName)
		{
			//loginName = Utilities.FormatLoginName(loginName);
			//string storedProcName = "dbo.rq_MarkItemRead" + listId.ToString("N");
			//ExecRqStoredProc(storedProcName, itemId, loginName);
			//loginName = Utilities.FormatLoginName(loginName);
			string storedProcName = "dbo.rq_MarkItemsRead" + listId.ToString("N");
			ExecRqStoredProc(storedProcName, itemIds, loginName);
		}

		internal static DataTable CreateItemIdsTableParamater(IEnumerable<int> itemIds)
		{
			if (null == itemIds)
			{
				throw new ArgumentNullException("itemIds");
			}
			DataTable returnValue = new DataTable();
			returnValue.Columns.Add("Id", typeof(int));
			foreach (int id in itemIds)
			{
				returnValue.Rows.Add(id);
			}
			return returnValue;
		}

		/// <summary>
		/// call an rq structured stored procedure
		/// </summary>
		/// <param name="storedProcedureName"></param>
		/// <param name="itemIds"></param>
		/// <param name="loginName"></param>
		static void ExecRqStoredProc(string storedProcedureName, int[] itemIds, string loginName)
		{
			List<int[]> itemBatchs = ConvertToBatchs(itemIds, 500);
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(FarmSettings.Settings.SqlConnectionString))
				{
					sqlConn.Open();
					foreach (int[] itemBatch in itemBatchs)
					{
						using (SqlCommand sqlCommand = sqlConn.CreateCommand())
						{
							sqlCommand.CommandText = storedProcedureName;
							sqlCommand.CommandType = CommandType.StoredProcedure;
							SqlParameter itemIdParam = sqlCommand.Parameters.Add("@itemIds", SqlDbType.Structured);
							itemIdParam.Value = CreateItemIdsTableParamater(itemBatch);
							if (null != loginName)
							{
								SqlParameter userIdParam = sqlCommand.Parameters.Add("@userId", SqlDbType.NVarChar, 64);
								userIdParam.Value = loginName;
							}
							sqlCommand.ExecuteNonQuery();
						}
					}
				}
			}
			finally
			{
				if (null != securityContext)
				{
					securityContext.Dispose();
				}
			}
		}

		static List<int[]> ConvertToBatchs(int[] intArray, int batchSize)
		{
			List<int[]> returnValue = new List<int[]>();
			List<int> currentBatch = new List<int>();
			
			for (int arryIdx = 0; arryIdx < intArray.Length; arryIdx ++)
			{
				currentBatch.Add(intArray[arryIdx]);
				if (currentBatch.Count >= batchSize)
				{
					returnValue.Add(currentBatch.ToArray());
					currentBatch.Clear();
				}
			}
			if (currentBatch.Count > 0)
			{
				returnValue.Add(currentBatch.ToArray());
			}
			return returnValue;
		}

		
	}
}