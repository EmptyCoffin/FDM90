<%@ Page Title="" Language="C#" EnableViewState="true" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Pages/Site.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Facebook.aspx.cs" Inherits="FDM90.Pages.Content.Facebook" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div runat="server" id="signInArea">
        <asp:Button ID="DetailsButton" runat="server" class="btn btn-primary" Text="Details" OnClick="DetailsButton_Click" AutoPostBack="false" />
        <div runat="server" id="detailsPanel" visible="false">
            <asp:Label runat="server" for="inputPageName">Page Name</asp:Label>
            <asp:TextBox runat="server" class="form-control" ID="inputPageName" placeholder="Page Name" />
            <asp:Button ID="facebookLoginButton" runat="server" class="btn btn-primary btn-block" Text="Sign in and Save" OnClick="facebookLoginButton_Click" />
            <asp:Label ID="facebookDetailsErrorLabel" runat="server"></asp:Label>
        </div>
    </div>
    <div class="row">
        <div class="col-md-7">
            <asp:ScriptManager runat="server" />
            <asp:UpdatePanel ID="facebookPanel" runat="server" UpdateMode="Always">
                <ContentTemplate>
                    <asp:Timer ID="facebookUpdateTimer" runat="server" Interval="10000" OnTick="facebookUpdateTimer_Tick" />
                    <h4>Facebook Timeline</h4>
                    <asp:ListView ID="postList" runat="server" OnItemEditing="postList_ItemEditing" OnItemCanceling="postList_ItemCanceling"
                        OnItemUpdating="postList_ItemUpdating" OnItemDeleting="postList_ItemDeleting">
                        <LayoutTemplate>
                            <table cellpadding="2" width="640px" border="1" id="tbl1" runat="server">
                                <tr runat="server" id="itemPlaceholder" />
                            </table>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <tr runat="server">
                                <td colspan="2" style="text-align: center">
                                    <asp:Label ID="PostIdLabel" runat="server" Visible="false" Text='<%#Eval("Id") %>' />
                                    <strong>Posted: </strong>
                                    <asp:Label ID="CreatedTimeLabel" runat="server"
                                        Text='<%#Eval("CreatedTime") %>' />
                                    <br />
                                    <strong>Message: </strong>
                                    <asp:Label ID="MessageLabel" runat="server" Visible='<%#Eval("Message") != null %>'
                                        Text='<%#Eval("Message") %>' />
                                    <br />
                                    <strong>Picture: </strong>
                                    <asp:Image runat="server" ID="PostImage" Visible='<%#Eval("PictureUrl") != null %>'
                                        ImageUrl='<%#Eval("PictureUrl") %>' />
                                    <br />
                                    <br />
                                    <asp:Button ID="EditButton" runat="server" class="btn btn-success" CommandName="Edit" Text="Edit" />
                                    <asp:Button ID="DeleteButton" runat="server" class="btn btn-danger" CommandName="Delete" Text="Delete" />
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
                                </td>
                            </tr>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <tr runat="server">
                                <td colspan="2" style="text-align: center">
                                    <asp:Label ID="PostIdLabel" runat="server" Visible="false" Text='<%#Eval("Id") %>' />
                                    Posted:
                                    <asp:Label ID="CreatedTimeLabel" runat="server"
                                        Text='<%#Eval("CreatedTime") %>' />
                                    <br />
                                    <strong>Message: </strong>
                                    <asp:TextBox ID="MessagePostTextBox" TextMode="MultiLine" Height="100px" Width="100%" runat="server" Visible='<%#Eval("Message") != null %>'
                                        Text='<%#Eval("Message") %>' />
                                    <br />
                                    Picture:
                                    <asp:Image runat="server" ID="PostImage" Visible='<%#Eval("PictureUrl") != null %>'
                                        ImageUrl='<%#Eval("PictureUrl") %>' />

                                    <br />
                                    <br />
                                    <asp:UpdatePanel ID="UpdateButtonPanel" runat="server" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <asp:Button ID="UpdateButton" runat="server" class="btn btn-success" CommandName="Update" Text="Update" />
                                            <asp:Button ID="CancelButton" runat="server" class="btn btn-danger" CommandName="Cancel" Text="Cancel" />
                                        </ContentTemplate>
                                        <Triggers>
                                            <asp:PostBackTrigger ControlID="UpdateButton" />
                                            <asp:PostBackTrigger ControlID="CancelButton" />
                                        </Triggers>
                                    </asp:UpdatePanel>
                                    <asp:Label ID="EditingErrorLabel" runat="server"></asp:Label>
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
                                </td>
                            </tr>
                        </EditItemTemplate>
                    </asp:ListView>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div class="col-md-4" style="position: sticky; position: -webkit-sticky; top: 127px; padding: 5px;">
            <div runat="server" id="facebookData" visible="false" class="container">
                <h4>Facebook Update</h4>
                <br />
                Message:
                        <asp:TextBox ID="FacebookPostText" TextMode="MultiLine" Height="100px" Width="250px" runat="server"></asp:TextBox>
                <br />
                Picture:
                        <asp:FileUpload EnableViewState="true" ID="FacebookPostAttachement" runat="server" />
                <br />
                <asp:Button ID="PostButton" runat="server" class="btn btn-primary" Text="Post" OnClick="PostButton_Click" />
                <asp:Label ID="PostFacebookError" runat="server" Visible="false" />
                <br />
                <br />
                <strong>--------------------------------------</strong>
                <br />
                <br />
                Number of Page Likes:
                        <asp:Label ID="numberOfPageLikes" runat="server"></asp:Label>
                <br />
                Number of New Page Likes (Past 7 Days):
                        <asp:Label ID="numberOfNewLikes" runat="server"></asp:Label>
                <br />
                Number of Page Stories (Past 7 Days):
                        <asp:Label ID="numberOfTalkingAbout" runat="server"></asp:Label>
                <br />
                Number of Post Likes (Past 7 Days):
                        <asp:Label ID="numberOfPostLikes" runat="server"></asp:Label>
                <br />
                Number of Post Comments (Past 7 Days):
                        <asp:Label ID="numberOfPostComments" runat="server"></asp:Label>
            </div>
        </div>
    </div>
</asp:Content>
