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
using System.Threading.Tasks;
using System.Web.UI;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Mobile.Controls;
using Microsoft.SharePoint.MobileControls;
using Microsoft.SharePoint.WebControls;

//disable ASP.NET mobile warning because its not true for SharePoint
#pragma warning disable  0618

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// Simple wrapper for a mobile control
	/// it marks the item read, then removes itself
	/// from the rendering map (so its not visible)
	/// </summary>
	public class ReadUnreadMobileFieldControl : SPMobileBaseFieldControl
	{
		protected Label _Label = null;
		readonly ReadUnreadField _ParentField = null;
		string _Value = string.Empty;

		public ReadUnreadMobileFieldControl(ReadUnreadField parentField)
		{
			this._ParentField = parentField;
		}

		public override object Value
		{
			get { return this._Value; }
			set
			{
				if (null == value)
				{
					this._Value = string.Empty;
				}
				else
				{
					this._Value = value.ToString();
				}
			}
		}

		protected override void CreateChildControls()
		{
			if (!FarmSettings.Settings.IsOk)
			{
				this.Page.LoadComplete += this.Page_LoadComplete;
				base.CreateChildControls();
				this._Label = (Label)this.NamingContainer.FindControl("labelReadUnreadField");
				if (null == this._Label)
				{
					this._Label = new Label();
					this.Controls.Add(this._Label);
				}
				ReadUnreadField readUnreadField = this.Field as ReadUnreadField;
				if (null != readUnreadField && SPControlMode.Invalid != this.ControlMode && !this.ListItem.IsFolder())
				{
					this._Label.ID = "lblRUMDF";
					this._Label.Text = "***";

					if (SPControlMode.New == this.ControlMode)
					{
						//We dont clear the read marks here because the item has not been
						//saved. We do that in the event handler for the item
					}
					else
					{
						if (null != this.ItemFieldValue)
						{
							this._Value = this.ItemFieldValue.ToString();
							ReadUnreadFieldValue fieldValue = new ReadUnreadFieldValue(this._Value);
							if (fieldValue.IsValid)
							{
								if(null != this._ParentField.ParentList && null != this._ParentField.ParentList.ParentWeb)
								{
									//If we are in display mode we need to set the read mark for this user
									//we would have to call SQL to see if its read, so just make the
									//update call and dont waste two trips
									SPListItem listItem = this._ParentField.ParentList.GetReadUnreadItemById(fieldValue.ItemId);
									SharePointRemarQ.MarkRead(listItem, this._ParentField.ParentList.ParentWeb.CurrentUser);
									this._Label.Text = "***";
								}
							}
						}
					}
				}
			}
		}

		void Page_LoadComplete(object sender, EventArgs e)
		{
			//we could not remove them before because the API blocks it
			//but in this event we can find the table row we are on and remove
			//ourselves and our label, no need for client side javascript
			//Control mlblCtrl = FindControlEx(Page.Master.Controls, "lblRUMDF");
			if (!FarmSettings.Settings.IsOk)
			{
				if (null != this._Label)
				{
					Control pParentCtrl = this._Label.Parent;
					if (null != pParentCtrl)
					{
						Control ppParentCtrl = pParentCtrl.Parent;
						if (null != ppParentCtrl)
						{
							Control pppParentCtrl = ppParentCtrl.Parent;
							if (null != pppParentCtrl && null != pppParentCtrl.Parent)
							{
								//pppParentCtrl is the table row that displays the label
								//and value on the property forms
								pppParentCtrl.Parent.Controls.Remove(pppParentCtrl);
							}
						}
					}
				}
			}
		}
	}
}