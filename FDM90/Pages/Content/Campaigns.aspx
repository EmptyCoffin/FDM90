<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Campaigns.aspx.cs" Inherits="FDM90.Pages.Content.Campaigns" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    Campaign Name:
            <asp:DropDownList runat="server" ID="currentCampaignDropDown" AutoPostBack="true" OnSelectedIndexChanged="currentCampaignDropDown_SelectedIndexChanged"></asp:DropDownList>

    <br />
    <asp:DropDownList runat="server" ID="metricDropDown" AutoPostBack="true" OnSelectedIndexChanged="metricDropDown_SelectedIndexChanged"></asp:DropDownList>
    <br />
    <asp:Chart ID="campaignChart" runat="server" Height="702px" Width="937px" EnableViewState="true">
    </asp:Chart>
    <br />
    <asp:Button runat="server" ID="DownloadChart" Text="Download Chart" OnClick="DownloadChart_Click" />
    <br />
    <asp:ScriptManager runat="server" />
    <asp:UpdatePanel ID="campaignPanel" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <asp:Label runat="server">Total Cost of Marketing Campaign</asp:Label>
            <asp:TextBox ID="CampaignCostTextBox" AutoPostBack="true" OnTextChanged="ModelVariabesChanged" runat="server">0</asp:TextBox>
            <br />
            <asp:Label runat="server">Average Cost of Products</asp:Label>
            <asp:TextBox ID="AverageCostOfProductsTextBox" AutoPostBack="true" OnTextChanged="ModelVariabesChanged" runat="server">0</asp:TextBox>
            <br />
            <asp:DropDownList runat="server" ID="marketingModelsDropDown" AutoPostBack="true" OnSelectedIndexChanged="marketingModels_SelectedIndexChanged"></asp:DropDownList>
            <asp:Label ID="modelDescriptionLabel" runat="server"></asp:Label>
            <br />
            <asp:Label ID="modelMetricLabel" runat="server"></asp:Label>
            <br />
            <asp:Table runat="server" ID="marketingBreakdownTable">
            </asp:Table>

            <br />
        </ContentTemplate>
    </asp:UpdatePanel>
    <br />
    <br />
    <br />
</asp:Content>
