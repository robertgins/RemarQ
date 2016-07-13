<%@ Assembly Name="Microsoft.SharePoint.ApplicationPages.Administration, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReadUnreadFarmSettingsEditorAdvanced.aspx.cs" Inherits="BalsamicSolutions.ReadUnreadSiteColumn.Administration.ReadUnreadFarmSettingsEditorAdvanced" MasterPageFile="~/_admin/admin.master" %>

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
<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server"></asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <table style="width: 100%; border: 0; padding: 0; border-spacing: 0; border-collapse: collapse;" class="propertysheet">
        <tr>
            <td colspan="3">
                <SharePoint:EncodedLiteral ID="FarmSettingsTrackDocumentsDescriptionLiteral" runat="server" Text="RES:ERROR" EncodeMethod="HtmlEncodeAllowSimpleTextFormatting" />
            </td>
        </tr>
        <tr>
            <td colspan="3">
                <br />
            </td>
        </tr>
        <tr>
            <td style="width: 15%">
                <img src="/_layouts/15/images/blank.gif" width='1' height='1' alt="" />
            </td>
            <td style="width: 70%; text-align: center">
                <asp:CheckBox ID="chkDocumentTracking" Text="" runat="server" />
                <asp:Literal ID="FarmSettingsChkDocumentTrackingLabelLiteral" runat="server" Text="RES:ERROR" />
            </td>
            <td style="width: 15%">
                <img src="/_layouts/15/images/blank.gif" width='1' height='1' alt="" />
            </td>
        </tr>
        <tr>
            <td colspan="3">
                <hr />
            </td>
        </tr>
        <tr>
            <td colspan="3">
                <SharePoint:EncodedLiteral ID="FarmSettingsCDNDescription" runat="server" Text="RES:ERROR" EncodeMethod="HtmlEncodeAllowSimpleTextFormatting" />
            </td>
        </tr>
        <tr>
            <td colspan="3">
                <br />
            </td>
        </tr>
        <tr>
            <td style="width: 25%">
                <asp:Literal ID="txtJQueryPathLabel" runat="server" Text="RES:ERROR" />
            </td>
            <td style="width: 70%">
                <asp:TextBox ID="txtJQueryPath" runat="server" Width="100%"></asp:TextBox>
            </td>
            <td style="width: 5%">
                <img src="/_layouts/15/images/blank.gif" width='1' height='1' alt="" />
            </td>
        </tr>
        <tr>
            <td style="width: 25%">
                <asp:Literal ID="txtKendoScriptLabel" runat="server" Text="RES:ERROR" />
            </td>
            <td style="width: 70%">
                <asp:TextBox ID="txtKendoScriptPath" runat="server" Width="100%"></asp:TextBox>
            </td>
            <td style="width: 5%">
                <img src="/_layouts/15/images/blank.gif" width='1' height='1' alt="" />
            </td>
        </tr>
        <tr>
            <td style="width: 25%">
                <asp:Literal ID="txtKendoStyePathLabel" runat="server" Text="RES:ERROR" />
            </td>
            <td style="width: 70%">
                <asp:TextBox ID="txtKendoStyePath" runat="server" Width="100%"></asp:TextBox>
            </td>
            <td style="width: 5%">
                <img src="/_layouts/15/images/blank.gif" width='1' height='1' alt="" />
            </td>
        </tr>
        <tr>
            <td style="width: 25%">
                <asp:Literal ID="txtKendoThemePathLabel" runat="server" Text="RES:ERROR" />
            </td>
            <td style="width: 70%">
                <asp:TextBox ID="txtKendoThemePath" runat="server" Width="100%"></asp:TextBox>
            </td>
            <td style="width: 5%">
                <img src="/_layouts/15/images/blank.gif" width='1' height='1' alt="" />
            </td>
        </tr>
         <tr>
            <td colspan="3">
                <hr />
            </td>
        </tr>
        <tr>
            <td colspan="3">
                <SharePoint:EncodedLiteral ID="FarmSettingsRefreshIntervalDescription" runat="server" Text="RES:ERROR" EncodeMethod="HtmlEncodeAllowSimpleTextFormatting" />
            </td>
        </tr>
        <tr>
            <td colspan="3">
                <br />
            </td>
        </tr>
         <tr>
            <td style="width: 50%">
                <asp:Literal ID="PropEditRefreshIntervalDescription" runat="server" Text="RES:ERROR" />
            </td>
            <td style="width: 15%">
                <asp:TextBox ID="txtRefreshInterval" runat="server" Width="100%"></asp:TextBox>
            </td>
            <td style="width: 35%">
                <img src="/_layouts/15/images/blank.gif" width='1' height='1' alt="" />
            </td>
        </tr>
        <tr>
            <td colspan="3">
                <br />
            </td>
        </tr>
        <tr>
            <td style="width: 15%"></td>
            <td style="width: 70%"></td>
            <td style="width: 15%">
                <asp:Button ID="btnApply" runat="server" CssClass="ms-ButtonHeightWidth" Text="<%$Resources:wss,multipages_savebutton_text%>" />
            </td>
        </tr>
    </table>
    <table style="width: 100%; border: 0; padding: 0; border-spacing: 0; border-collapse: collapse;" class="propertysheet">
        <tr>
            <td class="ms-descriptionText">
                <asp:Label ID="lblMessage" runat="server" EnableViewState="False" />
            </td>
        </tr>
        <tr>
            <td class="ms-error">
                <asp:Label ID="lblErrorMessage" runat="server" EnableViewState="False" />
            </td>
        </tr>
    </table>

</asp:Content>
