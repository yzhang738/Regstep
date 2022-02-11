<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

    <div style="margin: 10px; text-align:center">
        <h2><%: ViewBag.Message %></h2>
    </div>

</asp:Content>
