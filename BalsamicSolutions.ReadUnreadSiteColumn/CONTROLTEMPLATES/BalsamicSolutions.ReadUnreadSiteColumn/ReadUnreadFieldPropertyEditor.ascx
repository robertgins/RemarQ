<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReadUnreadFieldPropertyEditor.ascx.cs" Inherits="BalsamicSolutions.ReadUnreadSiteColumn.CONTROLTEMPLATES.BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadFieldPropertyEditor" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" Src="~/_controltemplates/15/InputFormSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" Src="~/_controltemplates/15/InputFormControl.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="ButtonSection" Src="~/_controltemplates/15/ButtonSection.ascx" %>
<wssuc:InputFormSection runat="server" id="ReadUnreadPropertySection" Title="Read mark configuration" Collapsible="false">
    <template_description>
         <asp:Literal ID="ReadUnreadPropertySectionDescriptionLiteral" runat="server" Text="RES:ERROR" />
         <asp:ImageButton  Height="32px" OnClientClick="showDialogWindow(showAboutEx,this); return false;" runat="server" ID="btnAbout" AlternateText="RES:ERROR" ImageUrl="/_layouts/15/images/BalsamicSolutions.ReadUnreadSiteColumn/ReadUnreadLogoSmall.png" />
      
         <asp:ImageButton  Height="32px" OnClientClick="showStatusWindow(showAdvancedBig,this); return false;" runat="server" ID="btnBlank" ClientIDMode="Static" AlternateText="" ImageUrl="/_layouts/15/images/BalsamicSolutions.ReadUnreadSiteColumn/Blank32.png" />
       
    </template_description>
    <template_inputformcontrols>
        <wssuc:InputFormControl runat="server" SmallIndent="true">
            <Template_Control>
                <asp:RadioButton  GroupName="columnDisplayGroup" AutoPostBack="true" ID="radColumnDisplayBold" runat="server" />
                <asp:Literal ID="RadColumnDisplayBoldTextLiteral" runat="server" Text="RES:ERROR" />
           </Template_Control>          
        </wssuc:InputFormControl>
        <wssuc:InputFormControl runat="server" SmallIndent="true">
            <Template_Control>
                <asp:RadioButton GroupName="columnDisplayGroup"  AutoPostBack="true"  ID="radColumnDisplayIconic" runat="server" />
                <asp:Literal ID="RadColumnDisplayIconicTextLiteral" runat="server" Text="RES:ERROR" />
            </Template_Control>
        </wssuc:InputFormControl>        
        <wssuc:InputFormControl runat="server"  SmallIndent="true">
         <Template_Control>
             <div style="width:210px">
                 <asp:Button  ID="btnAdvanced" width="200px" runat="server" Text="RES:ERROR" OnClientClick="showDialogWindow(showAdvancedBig,this); return false;"/>
                 <asp:Button  ID="btnInit" width="200px" runat="server" Text="RES:ERROR" OnClientClick="showInitWindow(showAdvanced,this); return false;"/>
            </div>
        </Template_Control> 
        </wssuc:InputFormControl>
    </template_inputformcontrols>
</wssuc:InputFormSection>
<asp:HiddenField ID="JSListIdField" ClientIDMode="Static" runat="server"/>
<asp:HiddenField ID="IsDiscussionBoard" ClientIDMode="Static" runat="server"/>
 <script type="text/javascript">
        
     var _Title = "RES:ERROR";
     var _Url = "";

 
     function showDialogWindow(dialogFunction, dialogButton) {
         _Title = dialogButton.value;
         var jsListId = document.getElementById("JSListIdField").value;
         var jsIsDiscussionboard = document.getElementById("IsDiscussionBoard").value;
         _Url = "/_layouts/BalsamicSolutions.ReadUnreadSiteColumn/ReadUnreadAdvancedSettings.aspx?listId=" + jsListId + "&isDb=" + jsIsDiscussionboard;
         SP.SOD.executeFunc('sp.js', 'SP.ClientContext', dialogFunction);
     }

      function showInitWindow(dialogFunction, dialogButton) {
         _Title = dialogButton.value;
         var jsListId = document.getElementById("JSListIdField").value;
         _Url = "/_layouts/BalsamicSolutions.ReadUnreadSiteColumn/ReadUnreadListUtilities.aspx?listId=" + jsListId;
         SP.SOD.executeFunc('sp.js', 'SP.ClientContext', dialogFunction);
      }

     function showAdvancedBig() {
         var dialogOpts = SP.UI.$create_DialogOptions();
         dialogOpts.width = 500;
         dialogOpts.height = 500;
         dialogOpts.title = _Title;
         dialogOpts.url = _Url;
         SP.UI.ModalDialog.showModalDialog(dialogOpts);
         return false;
     }

      function showAdvanced() {
         var dialogOpts = SP.UI.$create_DialogOptions();
         dialogOpts.width = 500;
         dialogOpts.height = 300;
         dialogOpts.title = _Title;
         dialogOpts.url = _Url;
         SP.UI.ModalDialog.showModalDialog(dialogOpts);
         return false;
      }

       function showAboutEx() {
            var dialogOpts = SP.UI.$create_DialogOptions();
            dialogOpts.width = 500;
            dialogOpts.height = 400;
            dialogOpts.title = "<%=AboutLabel%>";
            dialogOpts.url = "/_layouts/15/BalsamicSolutions.ReadUnreadSiteColumn/About.aspx";
            SP.UI.ModalDialog.showModalDialog(dialogOpts);
        }
 </script>