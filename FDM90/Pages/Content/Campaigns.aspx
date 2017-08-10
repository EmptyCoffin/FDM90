<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Campaigns.aspx.cs" Inherits="FDM90.Pages.Content.Campaigns" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    Campaign Name:
            <asp:DropDownList runat="server" ID="currentCampaignDropDown" AutoPostBack="true" OnSelectedIndexChanged="currentCampaignDropDown_SelectedIndexChanged"></asp:DropDownList>

    <br />
    <asp:DropDownList runat="server" ID="metricDropDown" AutoPostBack="true" OnSelectedIndexChanged="metricDropDown_SelectedIndexChanged"></asp:DropDownList>
    <br />
    <asp:Chart ID="campaignChart" runat="server" Height="702px" Width="937px">
    </asp:Chart>
    <br />
</asp:Content>
