<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Scheduler.aspx.cs" Inherits="FDM90.Pages.Content.Scheduler" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager runat="server" />
    <asp:UpdatePanel ID="SchedulerPanel" runat="server" UpdateMode="Always" Visible="false">
        <ContentTemplate>
            <div class="row">
                <div class="col-md-7">
                    <h4>Scheduled Posts</h4>
                    <asp:ListView ID="ScheduledPostsList" runat="server" OnItemEditing="ScheduledPostsList_ItemEditing"
                        OnItemCanceling="ScheduledPostsList_ItemCanceling" OnItemUpdating="ScheduledPostsList_ItemUpdating"
                        OnItemDeleting="ScheduledPostsList_ItemDeleting">
                        <LayoutTemplate>
                            <table cellpadding="2" width="640px" border="1" id="tbl1" runat="server">
                                <tr runat="server" id="itemPlaceholder" />
                            </table>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <tr runat="server">
                                <td colspan="2" style="text-align: center">
                                    <asp:Label ID="PostIdLabel" runat="server" Visible="false"
                                        Text='<%#Eval("PostId") %>' />
                                    <strong>Scheduled For: </strong>
                                    <asp:Label ID="PostTimeLabel" runat="server"
                                        Text='<%#Eval("PostTime") %>' />
                                    <br />
                                    <strong>Message: </strong>
                                    <asp:Label ID="PostTextLabel" runat="server" Visible='<%#Eval("PostText") != null %>'
                                        Text='<%#Eval("PostText") %>' />
                                    <br />
                                    <strong>Image: </strong>
                                    <asp:Image runat="server" ID="PostImage" Visible='<%#(!string.IsNullOrEmpty(Eval("AttachmentPath").ToString()))%>'
                                        ImageUrl='<%#Eval("AttachmentPath") %>' />
                                    <asp:Label ID="Label1" runat="server" Visible='<%#(string.IsNullOrEmpty(Eval("AttachmentPath").ToString()))%>'>None</asp:Label>
                                    <br />
                                    <strong>Post To: </strong>
                                    <asp:Label ID="MediaChannelsLabel" runat="server"
                                        Text='<%#Eval("MediaChannels") %>' />
                                    <br />
                                    <asp:Button ID="EditButton" class="btn btn-success"  runat="server" CommandName="Edit"
                                        Text="Edit" />
                                    <asp:Button ID="DeleteButton" class="btn btn-danger" runat="server" CommandName="Delete"
                                        Text="Delete" />
                                </td>
                            </tr>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <tr runat="server">
                                <td colspan="2" style="text-align: center">
                                    <asp:Label ID="PostIdLabel" runat="server" Visible="false"
                                        Text='<%#Eval("PostId") %>' />
                                    Edit Date: <asp:Button runat="server" ID="EditPostDateButton" class="btn btn-primary" OnClick="EditPostDateButton_Click" Text="Not Set"></asp:Button>
                                    <div runat="server" id="EditCalendarArea" visible="false">
                                        <br />
                                        <asp:Calendar runat="server" ID="EditCalendar"></asp:Calendar>
                                        <br />
                                        <asp:Button runat="server" ID="EditSetCalendarDate" class="btn btn-primary" OnClick="EditSetCalendarDate_Click" Text="Set Date"></asp:Button>
                                        <asp:Label runat="server" ID="EditCalendarErrorLabel" Visible="false"></asp:Label>
                                    </div>
                                    Edit Time: <asp:DropDownList runat="server" ID="EditHoursDropDown" class="btn btn-default"></asp:DropDownList>: 
                                    <asp:DropDownList runat="server" ID="EditQuarterDropDowns" class="btn btn-default"></asp:DropDownList>
                                    <br />
                                    <asp:TextBox ID="PostTextLabel" TextMode="MultiLine" Height="100px" Width="100%" runat="server" Visible='<%#Eval("PostText") != null %>'
                                        Text='<%#Eval("PostText") %>' />
                                    <br />
                                   <asp:Panel runat="server" Visible='<%# !string.IsNullOrEmpty(Eval("AttachmentPath").ToString()) %>'>
                                        Edit Attachment: <asp:Image runat="server" ID="PostImage"
                                            ImageUrl='<%#Eval("AttachmentPath") %>' />
                                        <asp:Button ID="DeleteImageButton" class="btn btn-danger" runat="server" Text="Delete Image (This will permanently delete the image)" OnClick="DeleteImageButton_Click" />
                                    </asp:Panel>
                                    Add Attachment: <asp:FileUpload ID="NewImageUpload" runat="server" />
                                    Uploading new attachment will override existing one on update.
                                    <br />
                                    <asp:TextBox ID="MediaChannelsLabel" runat="server"
                                        Text='<%# Eval("MediaChannels") %>' />
                                    <br />
                                    <asp:UpdatePanel ID="UpdateButtonPanel" runat="server" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <asp:Button ID="UpdateButton" class="btn btn-success" runat="server" CommandName="Update" Text="Update" />
                                            <asp:Button ID="CancelButton" class="btn btn-danger" runat="server" CommandName="Cancel" Text="Cancel" />
                                        </ContentTemplate>
                                        <Triggers>
                                            <asp:PostBackTrigger ControlID="UpdateButton" />
                                            <asp:PostBackTrigger ControlID="CancelButton" />
                                        </Triggers>
                                    </asp:UpdatePanel>
                                    <asp:Label ID="EditingErrorLabel" runat="server"></asp:Label>
                                </td>
                            </tr>
                        </EditItemTemplate>
                    </asp:ListView>
                </div>
                <div class="col-md-4">
                    <h4>Schedule New Posts</h4>
                    Post Message: <asp:TextBox ID="PostText" runat="server" TextMode="MultiLine" Height="100px" Width="100%"></asp:TextBox>
                    <br />
                    <br />
                    Post Image (Optional): <asp:FileUpload ID="PostAttachement" runat="server" />
                    <br />
                    <asp:CheckBox runat="server" ID="PostNowCheckbox" Checked="false" Text="Post Right Now?" AutoPostBack="true" OnCheckedChanged="PostNowCheckbox_CheckedChanged" />
                    <br />
                    <asp:CheckBoxList runat="server" ID="MediaChannelsCheckBoxList" />
                    <br />
                    <div runat="server" id="schedulerArea" visible="true">
                        Post Date: <asp:Button runat="server" ID="PostDateButton" OnClick="PostDateButton_Click" class="btn btn-primary" Text="Not Set"></asp:Button>
                        Post Time: <asp:DropDownList runat="server" class="btn btn-default" ID="HoursDropDown"></asp:DropDownList>: 
                        <asp:DropDownList runat="server" class="btn btn-default" ID="QuarterDropDowns"></asp:DropDownList>
                        <div runat="server" id="calendarArea" visible="false">
                            <br />
                            <asp:Calendar runat="server" ID="calendar"></asp:Calendar>
                            <br />
                            <asp:Button runat="server" ID="setCalendarDate" OnClick="setCalendarDate_Click" class="btn btn-primary" Text="Set Date"></asp:Button>
                            <asp:Label runat="server" ID="calendarErrorLabel" Visible="false"></asp:Label>
                        </div>
                    </div>
                    <br />
                    <asp:Button ID="PostButton" runat="server" class="btn btn-primary" Text="Schedule" OnClick="PostButton_Click" />
                    <asp:Label ID="SchedulerError" runat="server" Visible="false" />
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
