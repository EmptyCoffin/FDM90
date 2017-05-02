<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="FDM90.Pages.Content.Home" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div>
    <h3>
        Home page
    </h3>

    <p>
        About this site!
    </p>
            <asp:Button id="facebookSetUpButton" visible="false" runat="server" class="btn btn-primary btn-block" Text="Facebook Set Up" OnClick="facebookSetUpButton_Click" />
            <asp:Button id="twitterSetUpButton" visible="false" runat="server" class="btn btn-primary btn-block" Text="Twitter Set Up" OnClick="twitterSetUpButton_Click" />
        <!-- Modal -->
        <!--<div id="credentialPopup" class="modal fade" role="dialog">
            <credential-popup></credential-popup>
        </div>-->
</div>
</asp:Content>
