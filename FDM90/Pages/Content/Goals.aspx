<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Goals.aspx.cs" Inherits="FDM90.Pages.Content.Goals" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    Goal Name:
            <asp:DropDownList runat="server" ID="currentGoalDropDown" AutoPostBack="true" OnSelectedIndexChanged="currentGoalDropDown_SelectedIndexChanged"></asp:DropDownList>

    <br />
    <asp:DropDownList runat="server" ID="metricDropDown" AutoPostBack="true" OnSelectedIndexChanged="metricDropDown_SelectedIndexChanged"></asp:DropDownList>
    <br />
    <asp:Chart ID="goalChart" runat="server" Height="702px" Width="937px">
    </asp:Chart>
    <br />
</asp:Content>
