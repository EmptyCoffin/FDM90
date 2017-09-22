<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="FDM90.Pages.Account.Login" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <form class="form-group">
        <div class="col-md-6 col-md-offset-2">
            <asp:Label for="inputUserName" runat="server">Username</asp:Label>
            <asp:TextBox ID="userNameTextBox" CssClass="form-control" runat="server" placeholder="Username"></asp:TextBox>
            <br />
            <asp:Label for="inputPassword" runat="server">Password</asp:Label>
            <asp:TextBox ID="passwordTextBox" CssClass="form-control" runat="server" placeholder="Password" TextMode="Password"></asp:TextBox>
            <br />
            <asp:Button ID="LoginButton" runat="server" class="btn btn-primary" Text="Sign in" OnClick="LoginButton_Click" />
            <asp:Label ID="errorLabel" runat="server"></asp:Label>
        </div>
    </form>
</asp:Content>
