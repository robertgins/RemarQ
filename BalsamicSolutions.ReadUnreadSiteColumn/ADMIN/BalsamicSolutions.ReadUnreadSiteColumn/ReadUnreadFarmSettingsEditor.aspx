<%@ Assembly Name="Microsoft.SharePoint.ApplicationPages.Administration, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReadUnreadFarmSettingsEditor.aspx.cs" Inherits="BalsamicSolutions.ReadUnreadSiteColumn.Administration.ReadUnreadFarmSettingsEditor" MasterPageFile="~/_admin/admin.master" %>

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

<asp:Content ID="Content2" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" Text="PlaceHolderPageTitleInTitleArea" runat="server">
    <asp:ImageButton  Height="100px" OnClientClick="showDialogWindow(showAboutEx); return false;" runat="server" ID="btnAbout" AlternateText="About" ImageUrl="/_layouts/15/images/BalsamicSolutions.ReadUnreadSiteColumn/Remarq-Logo.png" />
    <%--<SharePoint:EncodedLiteral ID="FarmSettingsTitleInTitleLiteral" runat="server" Text="RES:ERROR" EncodeMethod="HtmlEncodeAllowSimpleTextFormatting" />--%>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
      <br />
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="PlaceHolderPageDescription" runat="server">
    <SharePoint:EncodedLiteral ID="EncodedLiteral3" runat="server" Text="" EncodeMethod='HtmlEncodeAllowSimpleTextFormatting' />
    <br />
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <table border="0" cellspacing="0" cellpadding="0" class="ms-propertysheet" width="100%">
        <wssuc:InputFormSection runat="server">
            <template_title><asp:Literal ID="FarmSettingsTitleLiteral" runat="server" Text="RES:ERROR" /></template_title>
            <template_description>
                <asp:Literal ID="FarmSettingsDescriptionLiteral" runat="server" Text="RES:ERROR" />
            </template_description>
            <template_inputformcontrols>
                <wssuc:InputFormControl runat="server" LabelText="">
                    <Template_Control>
                        <div style="width:205px" >
                           <asp:Button ID="btnConnection" width="200px" runat="server" Text="RES:ERROR" OnClientClick="showDialogWindow(showConnectionEx); return false;"/>
                           <asp:Button ID="btnAdvanced" width="200px" runat="server" Text="RES:ERROR" OnClientClick="showDialogWindow(showAdvancedEx); return false;"/>
                          </div>
                    </Template_Control>
                    </wssuc:InputFormControl>
           </template_inputformcontrols>
       </wssuc:InputFormSection>
        <wssuc:InputFormSection  runat="server">
             <template_title><asp:Literal ID="FarmSettingsMaintenanceTitleLiteral" runat="server" Text="RES:ERROR" /></template_title>
            <template_description>
               <asp:Literal ID="FarmSettingsMaintenanceDescriptionLiteral" runat="server" Text="RES:ERROR" />
               </template_description>
            <template_inputformcontrols>
                <wssuc:InputFormControl runat="server" LabelText="">
                    <Template_Control>
                         <div style="width:205px" >
                            <asp:Button ID="btnMaintenance" width="200px" runat="server" Text="RES:ERROR" OnClientClick="showDialogWindow(showMaintenanceEx); return false;"/>
                          </div>
                    </Template_Control>
                    </wssuc:InputFormControl>
                 </template_inputformcontrols>
        </wssuc:InputFormSection>

    </table>
    <script type="text/javascript">
        function showDialogWindow(dialogFunction) {
            SP.SOD.executeFunc('sp.js', 'SP.ClientContext', dialogFunction);
        }

        function showAboutEx() {
            var dialogOpts = SP.UI.$create_DialogOptions();
            dialogOpts.width = 500;
            dialogOpts.height = 400;
            dialogOpts.title = "<%=AboutLabel%>";
            dialogOpts.url = "/_layouts/15/BalsamicSolutions.ReadUnreadSiteColumn/About.aspx";
            SP.UI.ModalDialog.showModalDialog(dialogOpts);
        }

        function showAdvancedEx() {
            var dialogOpts = SP.UI.$create_DialogOptions();
            dialogOpts.width = 500;
            dialogOpts.height = 500;
            dialogOpts.title = "<%=AdvancedSettingsLabel%>";
            dialogOpts.url = "/_admin/BalsamicSolutions.ReadUnreadSiteColumn/ReadUnreadFarmSettingsEditorAdvanced.aspx";
            SP.UI.ModalDialog.showModalDialog(dialogOpts);
            return false;
        }


        function showMaintenanceEx() {
            var dialogOpts = SP.UI.$create_DialogOptions();
            dialogOpts.width = 500;
            dialogOpts.height = 400;
            dialogOpts.title = "<%=MaintenanceLabel%>";
            dialogOpts.url = "/_admin/BalsamicSolutions.ReadUnreadSiteColumn/ReadUnreadFarmSettingsEditorMaintenance.aspx";
            SP.UI.ModalDialog.showModalDialog(dialogOpts);
            return false;
        }
        
        function showConnectionEx() {
            var dialogOpts = SP.UI.$create_DialogOptions();
            dialogOpts.width = 500;
            dialogOpts.height = 400;
            dialogOpts.title = "<%=ConnectionSettingsLabel%>";
            dialogOpts.url = "/_admin/BalsamicSolutions.ReadUnreadSiteColumn/ReadUnreadFarmSettingsEditorConnection.aspx";
            SP.UI.ModalDialog.showModalDialog(dialogOpts);
            return false;
        }
 
    </script>
</asp:Content>
