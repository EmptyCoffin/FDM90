﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="FDM90.Pages.Content.Home" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Home page
    </h3>
    <p>
        About this site!
    </p>
    <div id="userArea">
        <div id="goalArea" runat="server" visible="false">
            Goal Name:
            <asp:DropDownList runat="server" ID="currentGoalDropDown"></asp:DropDownList>

            <br />
            <br />
            <asp:Button ID="setupGoalButton" runat="server" Text="New Goal" OnClick="setupGoalButton_Click" />
            <br />
            <br />
            <div id="newGoalArea" runat="server" visible="false">
                Goal Name:
                <asp:TextBox runat="server" ID="goalName"></asp:TextBox>
                Goal Start Date:
                <asp:Button runat="server" ID="startDateButton" Text="Not Set"></asp:Button>
                Goal End Date:
                <asp:Button runat="server" ID="endDateButton" Text="Not Set"></asp:Button>
                <div runat="server" id="calendarArea" visible="false">
                    <br />
                    <asp:Calendar runat="server" ID="calendar"></asp:Calendar>
                    <br />
                    <asp:Button runat="server" ID="setCalendarDate" Text="Set Date"></asp:Button>
                    <asp:Label runat="server" ID="calendarErrorLabel" Visible="false"></asp:Label>
                </div>
                <br />
                <br />
                <asp:Table runat="server" ID="newGoalGrid">
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
                    </asp:TableRow>
                </asp:Table>
                <br />
                <br />
                <asp:Button ID="newGoalButton" runat="server" Text="Set Goal" />
            </div>
        </div>
        <br />
        <br />
        <asp:Button ID="facebookSetUpButton" Visible="false" runat="server" class="btn btn-primary btn-block" Text="Facebook Set Up" OnClick="facebookSetUpButton_Click" />
        <asp:Button ID="twitterSetUpButton" Visible="false" runat="server" class="btn btn-primary btn-block" Text="Twitter Set Up" OnClick="twitterSetUpButton_Click" />
    </div>
</asp:Content>
