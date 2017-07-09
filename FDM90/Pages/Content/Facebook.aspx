<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Facebook.aspx.cs" Inherits="FDM90.Pages.Content.Facebook" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h4>Facebook Page</h4>
    <asp:Button ID="DetailsButton" runat="server" class="btn btn-primary" Text="Details" OnClick="DetailsButton_Click" AutoPostBack="false" />
    <div runat="server" id="detailsPanel" visible="false">
        <asp:Label runat="server" for="inputPageName">Page Name</asp:Label>
        <asp:TextBox runat="server" class="form-control" ID="inputPageName" placeholder="Page Name" />

        <div class="checkbox">
            <label>
                <input type="checkbox" />
                Please Check For Write Access?
            </label>
        </div>
        <asp:Button ID="facebookLoginButton" runat="server" class="btn btn-primary btn-block" Text="Sign in and Save" OnClick="facebookLoginButton_Click" />
    </div>
    <br />
    <br />
    <div runat="server" id="facebookData" visible="false" class="container">
        <div class="row">
            <div class="col-md-4">
                <asp:Button ID="likesButton" runat="server" class="btn btn-primary column-content" Text="Likes: " AutoPostBack="false" OnClick="likesButton_Click" />
                <div runat="server" id="likesDetails" visible="false" class="column-content">
                    <asp:Label ID="newLikeLabel" runat="server">Number of new likes: </asp:Label>
                    <asp:ListView ID="likeListView" runat="server">
                        <LayoutTemplate>
                            <table cellpadding="2" width="640px" border="1" id="tbl1" runat="server">
                                <tr runat="server" id="itemPlaceholder" />
                            </table>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <tr runat="server">
                                <td colspan="2" style="text-align: center">
                                    <asp:Label ID="CreatedTimeLabel" runat="server"
                                        Text='<%#Eval("EndDate") %>' />
                                    <br />
                                    <asp:Label ID="MessageLabel" runat="server" Visible='<%#Eval("Message") != null %>'
                                        Text='<%#Eval("Value") %>' />
                                    <br />
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:ListView>
                </div>
            </div>
            <div class="col-md-4">
                <asp:Label ID="peopleTalkingLabel" runat="server">People Talking: </asp:Label>
            </div>
        </div>
        <div class="row">
            <asp:Button runat="server" ID="postsButton" class="btn btn-primary column-content" Text="Your Posts: " AutoPostBack="false" OnClick="postsButton_Click" />
            <div runat="server" id="posts" visible="false" class="column-content">
                <asp:ListView ID="postList" runat="server">
                    <LayoutTemplate>
                        <table cellpadding="2" width="640px" border="1" id="tbl1" runat="server">
                            <tr runat="server" id="itemPlaceholder" />
                        </table>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <tr runat="server">
                            <td colspan="2" style="text-align: center">
                                <asp:Label ID="CreatedTimeLabel" runat="server"
                                    Text='<%#Eval("CreatedTime") %>' />
                                <br />
                                <asp:Label ID="MessageLabel" runat="server" Visible='<%#Eval("Message") != null %>'
                                    Text='<%#Eval("Message") %>' />
                                <br />
                                <asp:Image runat="server" ID="PostImage" Visible='<%#Eval("PictureUrl") != null %>'
                                    ImageUrl='<%#Eval("PictureUrl") %>' />
                            </td>
                        </tr>
                        <tr runat="server">
                            <td>Likes:
                                <asp:Label ID="PostLikeLabel" runat="server"
                                    Text='<%#Eval("Likes.Count") %>' />
                                <br />
                                Number of Comments:
                                <asp:Label ID="PostCommentLabel" runat="server"
                                    Text='<%#Eval("Comments.Count") %>' />
                            </td>
                            <td>Total Fan Reach:
                                <asp:Label ID="TotalFanReachLabel" runat="server"
                                    Text='<%#Eval("TotalReach.Values[0].Value") %>' />
                                <%--                                <br />
                                Negative Feedback: <asp:Label ID="NegativeFeedback" runat="server"
                                    Text='<%#Eval("NegativeFeedback.Values[0].Value") %>' />--%>
                            </td>

                        </tr>
                    </ItemTemplate>
                </asp:ListView>
            </div>
        </div>
    </div>
</asp:Content>
