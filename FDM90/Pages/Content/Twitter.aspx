<%@ Page Async="true" Title="" MaintainScrollPositionOnPostback="true" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Twitter.aspx.cs" Inherits="FDM90.Pages.Content.Twitter" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
                <div class="row">
                <div class="col-md-7">
    <asp:ScriptManager runat="server" />
    <asp:UpdatePanel ID="twitterPanel" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <asp:Timer ID="twitterUpdateTimer" runat="server" Interval="10000" OnTick="twitterUpdateTimer_Tick" />
                    <h4>Twitter Timeline</h4>
                    <asp:ListView ID="tweetList" runat="server" OnItemDeleting="tweetList_ItemDeleting">
                        <LayoutTemplate>
                            <table cellpadding="2" width="640px" border="1" id="tbl1" runat="server">
                                <tr runat="server" id="itemPlaceholder" />
                            </table>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <tr runat="server">
                                <td colspan="2" style="text-align: center">
                                    <asp:Label ID="StatusIdLabel" runat="server" Visible="false" Text='<%#Eval("StatusID") %>' />
                                    <strong>Posted: </strong>
                                    <asp:Label ID="CreatedTimeLabel" runat="server"
                                        Text='<%#Eval("CreatedAt") %>' />
                                    <br />
                                    <strong>Message: </strong>
                                    <asp:Label ID="MessageLabel" runat="server" Visible='<%#Eval("Text") != null %>'
                                        Text='<%#Eval("Text") %>' />
                                    <br />
                                    <strong>Picture: </strong><asp:Image runat="server" ID="PostImage" Height="150px" Width="250px"
                                         Visible='<%#(Eval("ImageUrl") != null && !string.IsNullOrEmpty(Eval("ImageUrl").ToString()))%>' ImageUrl='<%#Eval("ImageUrl") %>' />
                                    <br />
                                    <asp:Button ID="DeleteButton" class="btn btn-danger" runat="server" CommandName="Delete" Text="Delete" />
                                </td>
                            </tr>
                            <tr runat="server" style="text-align: center">
                                <td>Retweets:
                                <asp:Label ID="TweetRetweetLabel" runat="server"
                                    Text='<%#Eval("RetweetCount") %>' />
                                    Favorited:
                                <asp:Label ID="PostCommentLabel" runat="server"
                                    Text='<%#Eval("FavoriteCount") %>' />
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:ListView>

        </ContentTemplate>
    </asp:UpdatePanel>
                                    </div>
                <div class="col-md-4" style="position: sticky; position: -webkit-sticky; top: 127px; padding: 5px;">
                    <h4>Twiter Update</h4>
                    <br />
                    Message:
                    <asp:TextBox ID="TwitterPostText" TextMode="MultiLine" Height="100px" Width="250px" runat="server"></asp:TextBox>
<%--                    <br />
                    Picture:
                    <asp:FileUpload ID="TwitterPostAttachement" runat="server" />--%>
                    <br />
                    <asp:Button ID="PostButton" runat="server" class="btn btn-primary" Text="Post" OnClick="PostButton_Click" />
                    <br />
                    <br />
                    <strong>--------------------------------------</strong>
                    <br />
                    <br />
                    Number of Followers:
                    <asp:Label ID="numberOfFollowers" runat="server"></asp:Label>
                    <br />
                    Number of New Followers (Past 7 Days):
                    <asp:Label ID="numberOfNewFollowers" runat="server"></asp:Label>
                    <br />
                    Number of Retweets (Past 7 Days):
                    <asp:Label ID="numberOfRetweets" runat="server"></asp:Label>
                    <br />
                    Number of Favorited (Past 7 Days): 
                    <asp:Label ID="numberOfFavorite" runat="server"></asp:Label>
                </div>
            </div>
</asp:Content>
