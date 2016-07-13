<%@ Assembly Name="Microsoft.SharePoint.ApplicationPages.Administration, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReadUnreadFarmSettingsEditorMaintenance.aspx.cs" Inherits="BalsamicSolutions.ReadUnreadSiteColumn.Administration.ReadUnreadFarmSettingsEditorMaintenance" MasterPageFile="~/_admin/admin.master" %>

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
            <td>
                <SharePoint:EncodedLiteral ID="FarmSettingsMaintenanceDescriptionLiteral" runat="server" Text="RES:ERROR" EncodeMethod="HtmlEncodeAllowSimpleTextFormatting" />
            </td>
        </tr>
        <tr>
            <td>
            <img src="/_layouts/15/images/blank.gif" width='1' height='20' alt="" /></td>
        </tr>
        <tr>
            <td style="text-align: center">
                <asp:Button ID="btnResetJobs" Width="75%" runat="server" CssClass="ms-ButtonHeightWidth" Text="RES:ERROR" /></td>
        </tr>
        <tr>
            <td>
            <img src="/_layouts/15/images/blank.gif" width='1' height='20' alt="" /></td>
        </tr>
        <tr>
            <td style="text-align: center">
                <asp:Button ID="btnRunDailyNow" Width="75%" runat="server" CssClass="ms-ButtonHeightWidth" Text="RES:ERROR" /></td>
        </tr>
 
         <tr>
            <td>
            <img src="/_layouts/15/images/blank.gif" width='1' height='20' alt="" /></td>
        </tr>
        <tr>
            <td style="text-align: center">
                <asp:Button ID="btnResetLanguage" Width="75%" runat="server" CssClass="ms-ButtonHeightWidth" Text="RES:ERROR" /></td>
        </tr>
        <tr>
            <td>
            <img src="/_layouts/15/images/blank.gif" width='1' height='20' alt="" /></td>
        </tr>
        <tr>
            <td style="text-align: center">
                <asp:Button ID="btnRemoveAllFields" Width="75%" runat="server" CssClass="ms-ButtonHeightWidth" Text="RES:ERROR" />
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
                <asp:Label ID="lblErrorMessage" runat="server" EnableViewState="False" /></td>
        </tr>
    </table>

</asp:Content>
