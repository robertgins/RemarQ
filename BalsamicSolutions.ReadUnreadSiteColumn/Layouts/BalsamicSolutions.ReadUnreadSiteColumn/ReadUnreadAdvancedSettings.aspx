<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReadUnreadAdvancedSettings.aspx.cs" Inherits="BalsamicSolutions.ReadUnreadSiteColumn.Layouts.BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadAdvancedSettings" DynamicMasterPageFile="~masterurl/default.master" %>

<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Register TagPrefix="wssawc" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" Src="~/_controltemplates/15/InputFormSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" Src="~/_controltemplates/15/InputFormControl.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="ButtonSection" Src="~/_controltemplates/15/ButtonSection.ascx" %>
<asp:Content ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <asp:Literal ID="kendoStuff" runat="server" />
     <style  type="text/css" >
         /*.k-colorpicker {
            width:150px;
            }*/
        </style>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server"></asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <table style="width: 100%; border: 0; padding: 0; border-spacing: 0; border-collapse: collapse;" class="propertysheet">
        <wssuc:InputFormSection Title="" runat="server">
            <template_description>
			<SharePoint:EncodedLiteral ID="PropEditInstructionsLiteral" runat="server" text="RES:ERROR" EncodeMethod='HtmlEncodeAllowSimpleTextFormatting'/>
		 </template_description>
            <template_inputformcontrols>
			<wssuc:InputFormControl   runat="server">
				<Template_control>
                    <asp:CheckBox id="chkShowInlineEditingTools" runat="server" />
                    <asp:Literal ID="ChkShowInlineEditingToolsTextLiteral" runat="server" Text="RES:ERROR" />
				</Template_control>
			</wssuc:InputFormControl>
             <wssuc:InputFormControl   runat="server">
				<Template_control>
                    <asp:CheckBox id="chkShowReportingMenu" runat="server" />
                    <asp:Literal ID="chkShowReportingMenuLiteral" runat="server" Text="RES:ERROR" />
				</Template_control>
			</wssuc:InputFormControl>

             <wssuc:InputFormControl   runat="server">
				<Template_control>
                    <asp:CheckBox id="chkVersionFlags" runat="server" />
                    <asp:Literal ID="chkVersionFlagsLiteral" runat="server" Text="RES:ERROR" />
				</Template_control>
			</wssuc:InputFormControl>

                <wssuc:InputFormControl  runat="server" >
				<Template_control>
                    <table style="width:100%;border:0;padding:0;">
                        <tr><td style="text-align:left"><asp:Label ID="UnreadImageUrlLabel" LabelText='RES:ERROR'   runat="server"/></td></tr>
                        <tr><td style="text-align:left;"><asp:TextBox style="text-align:left;width:250px;"  runat="server" id="txtUnreadImageUrl" /></td></tr>
                    </table>
				</Template_control>
			</wssuc:InputFormControl>
                <wssuc:InputFormControl  runat="server" >
				<Template_control>
                     <table style="width:100%;border:0;padding:0;">
                        <tr><td style="text-align:left"><asp:Label ID="ReadImageUrlLabel" LabelText='RES:ERROR'  runat="server"/></td></tr>
                        <tr><td style="text-align:left;"><asp:TextBox style="text-align:left;width:250px;" runat="server" id="txtReadImageUrl" /></td></tr>
                    </table>
				</Template_control>
			</wssuc:InputFormControl>
                <wssuc:InputFormControl  runat="server" >
				<Template_control>
                    <table style="width:100%;border:0;padding:0;">
                        <tr><td style="text-align:left"><asp:Label ID="RefreshIntervalLabel" LabelText='RES:ERROR'  runat="server"/></td></tr>
                        <tr><td style="text-align:left"><asp:TextBox  runat="server" id="txtRefreshInterval" ClientIDMode="Static" /></td></tr>
                    </table>
				</Template_control>
			</wssuc:InputFormControl>
			<wssuc:InputFormControl   runat="server">
			   <Template_control>
                     <table style="width:100%;border:0;padding:0;">
                        <tr><td style="text-align:left;"><asp:Label ID="ColumnDisplayColorTextLabel" LabelText='RES:ERROR'    runat="server"/></td></tr>
                        <tr><td style="text-align:left;"><asp:TextBox TextMode="Color"  runat="server" id="txtUnreadHTMLColor" ClientIDMode="Static" /></td></tr>
                    </table>
			   </Template_control>
			</wssuc:InputFormControl>

		 </template_inputformcontrols>
        </wssuc:InputFormSection>
        <wssuc:ButtonSection runat="server" ShowStandardCancelButton="false">
            <template_buttons>
                 <asp:Button ID="btnApply" runat="server"  CssClass="ms-ButtonHeightWidth" Text="<%$Resources:wss,multipages_savebutton_text%>" />
			</template_buttons>
        </wssuc:ButtonSection>
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
    <script type="text/javascript">
        $("#txtUnreadHTMLColor").kendoColorPicker({
            buttons: true,
        });

        $("#txtRefreshInterval").kendoNumericTextBox({
            format: "#",
            decimals:0,
            min:0,
            max: 3600
        });

    </script>
</asp:Content>
