// -----------------------------------------------------------------------------
//  Copyright 5/1/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// simple wrapper for the helper values we store in the
	/// actual SharePoint data column
	/// </summary>
	[Serializable]
	public class ReadUnreadFieldValue
	{
		public int ItemId { get; private set; }

		public Guid ListId { get; private set; }

		public bool IsFolder { get; private set; }

		public bool IsValid { get; private set; }

		#region CTORs
        
		public ReadUnreadFieldValue()
		{
			this.ItemId = -1;
			this.ListId = Guid.Empty;
			this.IsFolder = false;
			this.IsValid = false;
		}
        
		public ReadUnreadFieldValue(SPItemEventProperties properties)
		{
			if (null == properties)
			{
				throw new ArgumentNullException("properties");
			}
			this.ItemId = properties.ListItemId;
			this.ListId = properties.ListId;
			this.IsFolder = properties.ListItem.IsFolder();
			this.IsValid = true;
		}
        
		public ReadUnreadFieldValue(SPListItem listItem)
		{
			if (null == listItem)
			{
				throw new ArgumentNullException("listItem");
			}
			this.ItemId = listItem.ID;
			this.ListId = listItem.ParentList.ID;
			this.IsFolder = listItem.IsFolder();
			this.IsValid = true;
		}
        
		public ReadUnreadFieldValue(string itemValue)
		{
			this.IsValid = false;
			if (!string.IsNullOrEmpty(itemValue))
			{
				string[] rowValues = itemValue.Split(',');
				if (rowValues.Length == 3)
				{
					try
					{
						this.ItemId = int.Parse(rowValues[0], CultureInfo.InvariantCulture);
						this.ListId = new Guid(rowValues[1]);
						this.IsFolder = bool.Parse(rowValues[2]);
						this.IsValid = true;
					}
					catch (FormatException)
					{
						this.IsValid = false;
					}
				}
			}
		}
        
		#endregion
        
		public override string ToString()
		{
			StringBuilder returnValue = new StringBuilder(128);
			returnValue.Append(this.ItemId.ToString(CultureInfo.InvariantCulture));
			returnValue.Append(",");
			returnValue.Append(this.ListId.ToString("N", CultureInfo.InvariantCulture));
			returnValue.Append(",");
			returnValue.Append(this.IsFolder.ToString());
			return returnValue.ToString();
		}
        
		public override bool Equals(object obj)
		{
			if (null == obj)
			{
				return false;
			}
			ReadUnreadFieldValue otherObj = obj as ReadUnreadFieldValue;
			if (null == otherObj)
			{
				string stringCandidate = obj as string;
				if (null == stringCandidate)
				{
					return false;
				}
				otherObj = new ReadUnreadFieldValue(stringCandidate);
				if (!otherObj.IsValid)
				{
					return false;
				}
			}
            
			return (this.ItemId == otherObj.ItemId && this.ListId == otherObj.ListId && this.IsFolder == otherObj.IsFolder);
		}
        
		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}
	}
}