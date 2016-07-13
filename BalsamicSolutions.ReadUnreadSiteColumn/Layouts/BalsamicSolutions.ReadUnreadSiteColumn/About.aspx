<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="BalsamicSolutions.ReadUnreadSiteColumn.Layouts.BalsamicSolutions.ReadUnreadSiteColumn.About" %>
<!DOCTYPE html>

<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title><asp:Literal ID="aboutTitle" runat="server" /></title>
</head>
<body>
    <table style="border: 0; margin: 0">
        <tr>
            <td>
                <img style="height: 100px" src="/_layouts/15/images/BalsamicSolutions.ReadUnreadSiteColumn/Remarq-Logo.png" /></td>
        </tr>
        <tr>
            <td>
                <p>
                    <asp:Literal ID="aboutText" runat="server" />
                </p>
            </td>
        </tr>

        <tr>
            <td><a style="border: 0; outline: none; text-decoration: none" href="http://www.balsamicsolutions.com">
                <img style="border: 0; outline: none; text-decoration: none" src="/_layouts/15/images/BalsamicSolutions.ReadUnreadSiteColumn/BalsamicSolutions.png" /></a></td>
        </tr>
                <tr>
            <td><span style="font: lighter,smaller,italic">
                <asp:Label ID="versionLabel" runat="server" /></span></td>
        </tr>
    </table>
</body>
</html>

