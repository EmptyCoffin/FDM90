<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Registration.aspx.cs" Inherits="FDM90.Pages.Account.Registration" %>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <form class="form-group">
        <asp:Label for="inputUserName" runat="server">Username</asp:Label>
        <asp:TextBox ID="userNameTextBox" CssClass="form-control" runat="server" placeholder="Username" ></asp:TextBox>
        <asp:Label for="inputEmailAddress" runat="server">Username</asp:Label>
        <asp:TextBox ID="inputEmailAddress" CssClass="form-control" runat="server" placeholder="Email Address" ></asp:TextBox>
        <asp:Label for="confirmEmailAddress" runat="server">Username</asp:Label>
        <asp:TextBox ID="ConfirmEmailAddress" CssClass="form-control" runat="server" placeholder="Confirm Email Address" ></asp:TextBox>
        <asp:Label for="inputPassword" runat="server">Password</asp:Label>
        <asp:TextBox ID="passwordTextBox" CssClass="form-control" runat="server" placeholder="Password" TextMode="Password" ></asp:TextBox>
        <asp:Button ID="registerButton" runat="server" class="btn btn-primary" Text="Register" OnClick="registerButton_Click" />
    </form>
</asp:Content>
