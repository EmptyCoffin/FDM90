<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="FDM90.Pages.Content.Home" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="notUserArea" runat="server" visible="true">
        This site is designed for a company can link their Social Media accounts in one place, where they can manage their accounts and create and evaluate marketing campaigns. We have a duty of care to the end consumer, this means that all messages that are wished to be posted will be tested for ethical and social breaches.
    </div>
    <div id="userArea" runat="server" visible="false">
        <p>
            From this page you can link your Social Media sites to this application and create new marketing campaigns
        </p>
        <div id="campaignArea" runat="server" visible="false">
            <asp:Button ID="setupCampaignButton" runat="server" class="btn btn-primary btn-lg" Text="New Campaign" OnClick="setupCampaignButton_Click" />
            <br />
            <div id="newCampaignArea" runat="server" visible="false">
                Campaign Name:
                <asp:TextBox runat="server" ID="campaignName"></asp:TextBox>
                Campaign Start Date:
                <asp:Button runat="server" ID="startDateButton" class="btn btn-primary" OnClick="StartCalendar" Text="Not Set"></asp:Button>
                Campaign End Date:
                <asp:Button runat="server" ID="endDateButton" class="btn btn-primary" OnClick="StartCalendar" Text="Not Set"></asp:Button>
                <div runat="server" id="calendarArea" visible="false">
                    <br />
                    <asp:Calendar runat="server" ID="calendar"></asp:Calendar>
                    <br />
                    <asp:Button runat="server" class="btn btn-primary" ID="setCalendarDate" OnClick="SetDate" Text="Set Date"></asp:Button>
                    <asp:Label runat="server" ID="calendarErrorLabel" Visible="false"></asp:Label>
                </div>
                <br />
                <br />
                <asp:Table runat="server" ID="newCampaignGrid">
                    <asp:TableHeaderRow>
                        <asp:TableHeaderCell>
                        Channel
                        </asp:TableHeaderCell>
                        <asp:TableHeaderCell>
                        Exposure
                        </asp:TableHeaderCell>
                        <asp:TableHeaderCell>
                        Influence
                        </asp:TableHeaderCell>
                        <asp:TableHeaderCell>
                        Engagement
                        </asp:TableHeaderCell>
                        <asp:TableHeaderCell>
                        Acquisition
                        </asp:TableHeaderCell>
                    </asp:TableHeaderRow>
                    <asp:TableRow>
                        <asp:TableCell>
                        Overall
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:Label runat="server" ID="overallExposure" Text="0"></asp:Label>
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:Label runat="server" ID="overallInfluence" Text="0"></asp:Label>
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:Label runat="server" ID="overallEngagement" Text="0"></asp:Label>
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:Label runat="server" ID="overallAcquisition" Text="0"></asp:Label>
                        </asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
                <br />
                <br />
                <asp:Button ID="newCampaignButton" runat="server" class="btn btn-primary" Text="Set Campaign" OnClick="newCampaignButton_Click" />
                <asp:Button ID="CancelCampaignButton" runat="server" class="btn btn-primary" Text="Cancel" OnClick="CancelCampaignButton_Click" />
            </div>
        </div>
        <br />
        <br />
        <div class="text-center">
            <asp:ImageButton ID="facebookSetUpButton" ImageUrl="~/Pages/Images/FacebookIcon.png" Visible="false" runat="server" AlternateText="Facebook Set Up" OnClick="facebookSetUpButton_Click" Height="62px" Width="59px" />
            <asp:ImageButton ID="twitterSetUpButton" ImageUrl="~/Pages/Images/TwitterIcon.png" Visible="false" runat="server" AlternateText="Twitter Set Up" OnClick="twitterSetUpButton_Click" Height="66px" Width="59px" />
        </div>
    </div>
</asp:Content>
