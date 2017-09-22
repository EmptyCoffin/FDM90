﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Registration.aspx.cs" Inherits="FDM90.Pages.Account.Registration" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <form class="form-group">
        <div class="col-md-6 col-md-offset-2">
            <asp:Label for="inputUserName" runat="server">Username</asp:Label>
            <asp:TextBox ID="userNameTextBox" CssClass="form-control" runat="server" placeholder="Username"></asp:TextBox>
            <br />
            <asp:Label for="inputEmailAddress" runat="server">Email Address</asp:Label>
            <asp:TextBox ID="inputEmailAddress" CssClass="form-control" runat="server" placeholder="Email Address"></asp:TextBox>
            <br />
            <asp:Label for="confirmEmailAddress" runat="server">Confirm Email Address</asp:Label>
            <asp:TextBox ID="ConfirmEmailAddress" CssClass="form-control" runat="server" placeholder="Confirm Email Address"></asp:TextBox>
            <br />
            <asp:Label for="inputPassword" runat="server">Password</asp:Label>
            <asp:TextBox ID="passwordTextBox" CssClass="form-control" runat="server" placeholder="Password" TextMode="Password"></asp:TextBox>
            <br />
            <asp:Button ID="registerButton" runat="server" class="btn btn-primary" Text="Register" OnClick="registerButton_Click" />
        </div>
    </form>
    <p>
        By registering with this site you are agreeing that application will be able to read information and write to the Social Networking sites that you implement. The application will also analyse user interaction and make estimations on said interactions.
    </p>
</asp:Content>
