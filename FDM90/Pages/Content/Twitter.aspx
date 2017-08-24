<%@ Page Async="true" Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Twitter.aspx.cs" Inherits="FDM90.Pages.Content.Twitter" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    Twitter Update
    <asp:TextBox ID="TwitterPostText" runat="server"></asp:TextBox>
    <asp:FileUpload ID="TwitterPostAttachement" runat="server" />
    <asp:Button ID="PostButton" runat="server" class="btn btn-primary btn-block" Text="Post" OnClick="PostButton_Click" />
    <asp:ScriptManager runat="server" />
    <asp:UpdatePanel ID="twitterPanel" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <asp:Timer ID="twitterUpdateTimer" runat="server" Interval="10000" OnTick="twitterUpdateTimer_Tick" />
            <div class="row">
                <div class="col-md-7">
                    <asp:ListView ID="tweetList" runat="server">
                        <LayoutTemplate>
                            <table cellpadding="2" width="640px" border="1" id="tbl1" runat="server">
                                <tr runat="server" id="itemPlaceholder" />
                            </table>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <tr runat="server">
                                <td colspan="2" style="text-align: center">
                                    <asp:Label ID="CreatedTimeLabel" runat="server"
                                        Text='<%#Eval("CreatedAt") %>' />
                                    <br />
                                    <asp:Label ID="MessageLabel" runat="server" Visible='<%#Eval("Text") != null %>'
                                        Text='<%#Eval("Text") %>' />
                                    <br />
                                    <%--                                <asp:Image runat="server" ID="PostImage" Visible='<%#Eval("ProfileImageUrl") != null %>'
                                    ImageUrl='<%#Eval("ProfileImageUrl") %>' />--%>
                                </td>
                            </tr>
                            <tr runat="server">
                                <td>Retweets:
                                <asp:Label ID="TweetRetweetLabel" runat="server"
                                    Text='<%#Eval("RetweetCount") %>' />
                                    <br />
                                    Favorited:
                                <asp:Label ID="PostCommentLabel" runat="server"
                                    Text='<%#Eval("FavoriteCount") %>' />
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:ListView>
                </div>
                <div class="col-md-4">
                    <asp:Label ID="numberOfFollowers" runat="server">Number of Followers: </asp:Label>
                    <br />
                    <asp:Label ID="numberOfRetweets" runat="server">Number of Retweets: </asp:Label>
                    <br />
                    <asp:Label ID="numberOfFavorite" runat="server">Number of Favorited: </asp:Label>
                </div>
            </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
