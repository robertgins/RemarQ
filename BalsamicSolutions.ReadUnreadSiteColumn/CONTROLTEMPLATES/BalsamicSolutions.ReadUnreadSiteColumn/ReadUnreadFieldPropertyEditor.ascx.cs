// -----------------------------------------------------------------------------
//  Copyright 6/11/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace BalsamicSolutions.ReadUnreadSiteColumn.CONTROLTEMPLATES.BalsamicSolutions.ReadUnreadSiteColumn
{
	//This is the control that renders on SharePoint column configuration
	//the host control is ReadUnreadFieldPropertyEditor.ascx
	//page. Nothing fancy here, just plain old C# codebehind
	public partial class ReadUnreadFieldPropertyEditor : UserControl, IFieldEditor
	{
		#region Declarations
		
		bool _IsDiscusssionBoard = false;
		bool _Looping = false;
		
		ColumnRenderMode _ColumnRenderMode = Constants.DefaultColumnRenderMode;
		
		#endregion
		
		protected void Page_Load(object sender, EventArgs e)
		{
			this.btnAbout.AlternateText = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsBtnAboutLabel") ;
			this.RadColumnDisplayBoldTextLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"PropEditRadColumnDisplayBoldText");
			this.RadColumnDisplayIconicTextLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"PropEditRadColumnDisplayIconicText");

			this.btnAdvanced.Enabled = false;
			this.btnInit.Enabled = false;
			
			this.btnAdvanced.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsBtnAdvancedLabel");
			this.btnInit.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsBtnInitLabel");
			this.ReadUnreadPropertySectionDescriptionLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"PropEditReadUnreadPropertySectionDescription");
			if (FarmSettings.Settings.IsOk)
			{
			
				this.btnInit.Enabled = true;
				FarmLicense currentLicense = FarmLicense.License;
				if (currentLicense.LicenseMode >= LicenseModeType.Professional)
				{
					this.btnAdvanced.Enabled = true;
				}
			}
		}
		
		public bool DisplayAsNewSection
		{
			get { return true; }
		}
		
		public void InitializeWithField(Microsoft.SharePoint.SPField field)
		{
			if (FarmSettings.Settings.IsOk)
			{
				if (null != field)
				{
					ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(field.ParentList.ID);
					if (null != listConfig)
					{
						this._ColumnRenderMode = listConfig.ColumnRenderMode;
						this._IsDiscusssionBoard = field.ParentList.IsDiscussionBoard();
						this.JSListIdField.Value = field.ParentList.ID.ToString("N");
					}
				}
			}
		}
		
		public void OnSaveChange(Microsoft.SharePoint.SPField field, bool isNewField)
		{
			ReadUnreadField readUnreadField = (ReadUnreadField)field;
			ColumnRenderMode colRenderMode = ColumnRenderMode.BoldDisplay;
			if (this.radColumnDisplayIconic.Checked)
			{
				colRenderMode = ColumnRenderMode.Iconic;
			}
			ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(field.ParentList.ID);
			if (listConfig.ColumnRenderMode != colRenderMode)
			{
				string layoutsUrl = readUnreadField.ParentList.ParentWebUrl;
				SqlRemarQ.UpdateListConfiguration(readUnreadField.ParentList.ID,
					readUnreadField.ParentList.ParentWeb.ID,
					readUnreadField.ParentList.ParentWeb.Site.ID,
					readUnreadField.Id,
					colRenderMode,
					listConfig.ReadImagePath,
					listConfig.UnreadImagePath,
					listConfig.UnreadHtmlColor,
					listConfig.UnreadHtmlColor,
					listConfig.ContextMenu,
					listConfig.VersionUpdate,
					readUnreadField.InternalName,
					readUnreadField.ParentList.ParentWeb.Language,
					layoutsUrl,
					listConfig.RefreshInterval);
			}
		}
		
		#region UserControl imlementation
		
		public override void Focus()
		{
			this.EnsureChildControls();
		}
		
		protected override void CreateChildControls()
		{
			if (FarmSettings.Settings.IsOk)
			{
				base.CreateChildControls();
				
				if (!this.Page.IsPostBack)
				{
					this.radColumnDisplayIconic.Enabled = false;
					this.radColumnDisplayBold.Enabled = false;
					this.radColumnDisplayIconic.Checked = false;
					this.radColumnDisplayBold.Checked = false;
					switch (this._ColumnRenderMode)
					{
						case ColumnRenderMode.Iconic:
							this.radColumnDisplayIconic.Checked = true;
							break;
						case ColumnRenderMode.BoldDisplay:
						default:
							this.radColumnDisplayBold.Checked = true;
							break;
					}
					if (!this._IsDiscusssionBoard)
					{
						this.radColumnDisplayIconic.Enabled = true;
						this.radColumnDisplayBold.Enabled = true;
					}
				}
				
				this.IsDiscussionBoard.Value = this._IsDiscusssionBoard.ToString(CultureInfo.InvariantCulture).ToLower();
				if (!this._IsDiscusssionBoard)
				{
					this.radColumnDisplayBold.CheckedChanged += new EventHandler(this.ColumnDisplayBold_CheckedChanged);
					this.radColumnDisplayIconic.CheckedChanged += new EventHandler(this.ColumnDisplayIconic_CheckedChanged);
				}
			}
		}
		
		#endregion
		
		#region Radio button stuff
		
		//The radio buttion group names do not hold accross the HTML areas
		//created by the SharePoint input form areas, so we will wire them
		//up to event handlers and do the UI work publicly
		void ColumnDisplayIconic_CheckedChanged(object sender, EventArgs e)
		{
			if (!this._Looping)
			{
				this._Looping = true;
				this.radColumnDisplayBold.Checked = !this.radColumnDisplayIconic.Checked;
				this._Looping = false;
			}
		}
		
		void ColumnDisplayBold_CheckedChanged(object sender, EventArgs e)
		{
			if (!this._Looping)
			{
				this._Looping = true;
				this.radColumnDisplayIconic.Checked = !this.radColumnDisplayBold.Checked;
				this._Looping = false;
			}
		}
		
		#endregion

		protected string AboutLabel
		{
			get { return Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsBtnAboutLabel"); }
		}
	}
}