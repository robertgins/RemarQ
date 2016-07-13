<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReadUnreadReport.aspx.cs" Inherits="BalsamicSolutions.ReadUnreadSiteColumn.Layouts.BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadReport" %>

<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title></title>
    <sharepoint:cssregistration runat="server" name="default" />
    <sharepoint:csslink runat="server" />
    <asp:Literal ID="kendoStuff" runat="server" />
    <style type='text/css'>
        html,
        body {
            margin: 0;
            padding: 0;
            height: 100%;
        }

        html {
            overflow: hidden;
        }

        table {
            width: 100%;
            margin: 0;
            padding: 0;
        }
        #svcGrid
        {
            border-width:0;
            height:100%;
        }
        td {
            text-align: center;
            font: bold;
        }
    </style>
</head>
<body>
     <%--Kendo script / lsitview template--%>
    <script id="listView-template" type="text/x-kendo-template">
         <table style="width:100%;padding:0px;">
            <tr>
            <td style="width:100px;"><span style="overflow: hidden; white-space: nowrap; text-overflow: clip;"><a href="mailto:#:ReadByEmail#">#:ReadBy#</a></span></td>
            <td style="width:200px;"><span style="overflow: hidden; white-space: nowrap; text-overflow: clip;">#:ReadOn#</span></td>
            </tr>
        </table>
    </script>
    <script id="listView-templateAlt" type="text/x-kendo-template">
         <table style="width:100%;padding:0px;background-color:lightgrey">
            <tr>
            <td style="width:100px;"><span style="overflow: hidden; white-space: nowrap; text-overflow: clip;"><a href="mailto:#:ReadByEmail#">#:ReadBy#</a></span></td>
            <td style="width:200px;"><span style="overflow: hidden; white-space: nowrap; text-overflow: clip;">#:ReadOn#</span></td>
            </tr>
        </table>
    </script>
    <form runat="server" id="frmMain">
        <asp:Table ID="errorTable" runat="server" Visible="false"></asp:Table>
        <%--Draw the table with the drop down box, search box and column headers--%>
        <table style="width:100%;padding:0px">
            <tr id="fileInfo">
                <td style="width: 225px;text-align: left">
                     <span class="k-textbox k-space-right" style="width: 225px;">
                        <input type="text" id="searchText" style="width: 200px;"/>
                        <a href="#" id="searchButton" class="k-icon k-i-search">&nbsp;</a>
                    </span> 
		        </td>
                <td style="text-align: right">
                    <span class="k-textbox k-space-right" style="width: 225px;">
                      <asp:DropDownList ID="fileVersions"  class="k-textbox" runat="server" ClientIDMode="Static" style="width:218px;" />
                    </span>
		        </td>
            </tr>
            <tr>
                <%--clickable column headers--%>
                <td style="width: 100px; font: bold">
                    <a href="#" id="sortByNameButton">
                        <%=this.NameColumnTitle%>
                    </a>
                </td>
                <td style="width: 200px;font: bold">
                    <a href="#" id="sortByDateButton">
                        <%=this.ReadOnColumnTitle%>
                    </a>
                </td>
            </tr>
        </table>
        <hr />
        <div id="reportItems" style="width: 100%; height:300px; overflow-x: hidden; overflow-y: auto; border-style:none;display: table;"></div>
	    <div class="k-page-wrap">
		    <div id="reportPager"></div>
	    </div>
    </form>
    <script type="text/javascript">

        var sortByNameOrder = "desc";
        var sortByDateOrder = "desc";

        var dataSource = new kendo.data.DataSource({
                transport: {
                    read: {
                        type: "POST",
                        url: "<%=this.QueryUrl%>",
                        dataType: "json"
                    }
                },
                pageSize: 12,
                serverPaging: true,
                serverFiltering:true,
                schema: {
                    data: "Data",
                    total: "Total",
                    errors: "Errors",
                    model: {
                        id: "Id",
                        fields: {
                            Id: { type: "string" },
                            ReadBy: { type: "string" },
                            ReadByEmail: { type: "string" },
                            UserId: { type: "int" },
                            Version: { type: "int" },
                            ReadOn: { type: "date" }
                        }
                    }
                }
        });

        $(function () {
 

            var svcListView = $("#reportItems").kendoListView({
                template: kendo.template($('#listView-template').html()),
                altTemplate: kendo.template($("#listView-templateAlt").html()),
                dataSource: dataSource
            });

            $("#searchButton").click(function (event) {
                event.preventDefault();
                var searchValue = $("#searchText").val().trim();
                if (searchValue.length > 0) {
                    dataSource.filter({ field: "ReadBy", operator: "contains", value: searchValue });
                }
                else {
                     dataSource.filter({});
                }
            });

            $("#sortByNameButton").click(function (event) {
                 event.preventDefault();
                 if (sortByNameOrder == "asc") {
                     sortByNameOrder = "desc";
                 }
                 else {
                     sortByNameOrder = "asc";
                 }
                 dataSource.filter({ sort: "ReadBy", dir: sortByNameOrder});
             });

            $("#sortByDateButton").click(function (event) {
                 event.preventDefault();
                 if (sortByDateOrder == "asc") {
                     sortByDateOrder = "desc";
                 }
                 else {
                     sortByDateOrder = "asc";
                 }
                 dataSource.filter({ sort: "ReadOn", dir: sortByDateOrder});
             });

            $("#reportPager").kendoPager({
                dataSource: dataSource
            });

            $("#fileVersions").kendoComboBox({
                select: function (e) {
                    var dataItem = this.dataItem(e.item.index());
                    queryUrl = "<%=this.QueryUrl%>" + "&hId=" + dataItem.value;
                    dataSource.transport.options.read.url = queryUrl;
                    dataSource.read();
                }
            });
        });
    </script>
</body>
</html>

