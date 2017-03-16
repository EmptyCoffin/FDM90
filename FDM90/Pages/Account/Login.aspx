<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="FDM90.Pages.Account.Login" %>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h4>
    Sign in
    </h4>

<form class="form-group">
    <asp:Label for="inputUserName" runat="server">Username</asp:Label>
    <asp:TextBox ID="userNameTextBox" CssClass="form-control" runat="server" placeholder="Username" ></asp:TextBox>
    <asp:Label for="inputPassword" runat="server">Password</asp:Label>
    <asp:TextBox ID="passwordTextBox" CssClass="form-control" runat="server" placeholder="Password" TextMode="Password" ></asp:TextBox>
<%--    <div class="help-block text-right"><a href="Registration.aspx">Forget the password ?</a></div>--%>
    <asp:Button ID="LoginButton" runat="server" class="btn btn-primary" Text="Sign in" OnClick="LoginButton_Click" />
</form>
</asp:Content>
