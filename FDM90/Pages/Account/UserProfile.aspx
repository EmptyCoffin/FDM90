<%@ Page Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="UserProfile.aspx.cs" Inherits="FDM90.Pages.Account.UserProfile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <p>
        This is you user profile where you can removed any implelemented social media from the site or you account in total
    </p>
    <br />
    <asp:DropDownList runat="server" class="btn btn-default" ID="DeleteItemDropDownList"></asp:DropDownList> 
    <asp:Button runat="server" ID="DeleteButton" CssClass="btn btn-danger" Text="Delete" OnClick="DeleteButton_Click" />
</asp:Content>