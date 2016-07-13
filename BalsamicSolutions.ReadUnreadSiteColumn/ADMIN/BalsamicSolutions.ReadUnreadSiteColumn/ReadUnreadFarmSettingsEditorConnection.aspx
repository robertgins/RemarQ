<%@ Assembly Name="Microsoft.SharePoint.ApplicationPages.Administration, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReadUnreadFarmSettingsEditorConnection.aspx.cs" Inherits="BalsamicSolutions.ReadUnreadSiteColumn.Administration.ReadUnreadFarmSettingsEditorConnection" MasterPageFile="~/_admin/admin.master" %>

<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Register TagPrefix="wssawc" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="AdminControls" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint.ApplicationPages.Administration" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" Src="~/_controltemplates/15/InputFormSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" Src="~/_controltemplates/15/InputFormControl.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="ButtonSection" Src="~/_controltemplates/15/ButtonSection.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="PlaceHolderMain" runat="server">
  <table style="width:100%;border:0;padding:0;border-spacing:0;border-collapse:collapse;" class="propertysheet">
        <wssuc:InputFormSection Title="" runat="server">
            <template_description>
			<SharePoint:EncodedLiteral ID="FarmSettingsSqlConnectionStringInstructionsLiteral" runat="server" text="RES:ERROR" EncodeMethod='HtmlEncodeAllowSimpleTextFormatting'/>
		 </template_description>
            <template_inputformcontrols>
			<wssuc:InputFormControl   runat="server">
				<Template_control>
                    <asp:Label ID="FarmSettingsAdvancedLabelSqlServernameLabel" LabelText='RES:ERROR' LabelAssociatedControlID="txtSqlServer" runat="server"/>
					<wssawc:InputFormTextBox  CssClass="ms-input" ID="txtSqlServer" Columns="35" Runat="server"  />
				</Template_control>
			</wssuc:InputFormControl>
			<wssuc:InputFormControl   runat="server">
			   <Template_control>
                   <asp:Label ID="FarmSettingsAdvancedLabelSqlDatabaseNameLabel" LabelText='RES:ERROR' LabelAssociatedControlID="txtDataBaseName" runat="server"/>
				   <wssawc:InputFormTextBox  CssClass="ms-input" ID="txtDataBaseName" Columns="35" Runat="server" />
			   </Template_control>
			</wssuc:InputFormControl>
			<wssuc:InputFormControl   runat="server" >
				<Template_control>
                    <asp:Label ID="FarmSettingsAdvancedLabelSqlUserIdLabel" LabelText='RES:ERROR' LabelAssociatedControlID="txtUserId" runat="server" />
					<wssawc:InputFormTextBox CssClass="ms-input" ID="txtUserId" Columns="35" Runat="server"  />
				</Template_control>
			</wssuc:InputFormControl>
                <wssuc:InputFormControl  runat="server" >
				<Template_control>
                    <asp:Label ID="FarmSettingsAdvancedLabelSqlPasswordLabel" LabelText='RES:ERROR' LabelAssociatedControlID="txtPassword" runat="server" />
					<wssawc:InputFormTextBox  CssClass="ms-input" ID="txtPassword" Columns="35" Runat="server"  TextMode="Password" ClientIDMode="Static"/>
				</Template_control>
			</wssuc:InputFormControl>
                <wssuc:InputFormControl  runat="server" >
				<Template_control>
                    <asp:HiddenField ID="checkedPw" runat="server" ClientIDMode="Static" />
                    <asp:Label ID="FarmSettingsAdvancedLabelConfirmSqlPasswordLabel" LabelText='RES:ERROR' LabelAssociatedControlID="txtPasswordConfirm" runat="server" />
					<wssawc:InputFormTextBox  CssClass="ms-input" ID="txtPasswordConfirm" Columns="35" Runat="server"  TextMode="Password" ClientIDMode="Static"/>
				</Template_control>
			</wssuc:InputFormControl>
            <wssuc:InputFormControl  runat="server" >
				<Template_control>
                    <asp:Label  ID="FarmSettingsAdvancedLabelSqlIntegratedSecurityLabel" LabelText='RES:ERROR' LabelAssociatedControlID="chkIntegrated" runat="server" />
					<asp:CheckBox runat="server" ID="chkIntegrated" />
				</Template_control>
			</wssuc:InputFormControl>
		 </template_inputformcontrols>
        </wssuc:InputFormSection>

        <wssuc:ButtonSection runat="server" ShowStandardCancelButton="false">
            <template_buttons>
                 <asp:Button ID="btnTest" runat="server"  CssClass="ms-ButtonHeightWidth" Text='RES:ERROR' />
                 <asp:Button ID="btnApply" runat="server"  CssClass="ms-ButtonHeightWidth" Text="<%$Resources:wss,multipages_savebutton_text%>" />
			</template_buttons>
        </wssuc:ButtonSection>
    </table>
      <table style="width:100%;border:0;padding:0;border-spacing:0;border-collapse:collapse;" class="propertysheet">
        <tr>
            <td class="ms-descriptionText">
                <asp:Label ID="lblMessage" runat="server" EnableViewState="False" />
            </td>
        </tr>
        <tr>
            <td class="ms-error">
                <asp:Label ID="lblErrorMessage" runat="server" EnableViewState="False" /></td>
        </tr>
    </table>
     <script type="text/javascript">
         var ckPw = document.getElementById("checkedPw");
         var txtPw = document.getElementById("txtPassword");
         var txtConfirmPw = document.getElementById("txtPasswordConfirm");
         txtPw.value = ckPw.value;
         txtConfirmPw.value = ckPw.value;
    </script>
</asp:Content>
