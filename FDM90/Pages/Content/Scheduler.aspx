<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Scheduler.aspx.cs" Inherits="FDM90.Pages.Content.Scheduler" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h4>Scheduler</h4>
    <asp:TextBox ID="PostText" runat="server"></asp:TextBox>
    <asp:FileUpload ID="PostAttachement" runat="server" />
    <br />
    <asp:CheckBox runat="server" ID="PostNowCheckbox" Checked="false" Text="Post Right Now?" AutoPostBack="true" OnCheckedChanged="PostNowCheckbox_CheckedChanged" />
    <br />
    <asp:CheckBoxList runat="server" ID="MediaChannelsCheckBoxList" />
    <br />
    <div runat="server" id="schedulerArea" visible="true">
        <asp:Button runat="server" ID="PostDateButton" OnClick="PostDateButton_Click" Text="Not Set"></asp:Button>
        <asp:DropDownList runat="server" ID="HoursDropDown"></asp:DropDownList>: 
        <asp:DropDownList runat="server" ID="QuarterDropDowns"></asp:DropDownList>
        <div runat="server" id="calendarArea" visible="false">
            <br />
            <asp:Calendar runat="server" ID="calendar"></asp:Calendar>
            <br />
            <asp:Button runat="server" ID="setCalendarDate" OnClick="setCalendarDate_Click" Text="Set Date"></asp:Button>
            <asp:Label runat="server" ID="calendarErrorLabel" Visible="false"></asp:Label>
        </div>
    </div>
    <asp:Button ID="PostButton" runat="server" class="btn btn-primary btn-block" Text="Schedule" OnClick="PostButton_Click" />
    <asp:ScriptManager runat="server" />
    <asp:UpdatePanel ID="SchedulerPanel" runat="server" UpdateMode="Always" Visible="false">
        <ContentTemplate>
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
                            <asp:Label ID="PostTimeLabel" runat="server"
                                Text='<%#Eval("PostTime") %>' />
                            <br />
                            <asp:Label ID="PostTextLabel" runat="server" Visible='<%#Eval("PostText") != null %>'
                                Text='<%#Eval("PostText") %>' />
                            <br />
                            <asp:Image runat="server" ID="PostImage" Visible='<%#Eval("AttachmentPath") != null %>'
                                ImageUrl='<%#Eval("AttachmentPath") %>' />
                            <br />
                            <asp:Label ID="MediaChannelsLabel" runat="server"
                                Text='<%#Eval("MediaChannels") %>' />
                            <br />
                            <asp:Button ID="EditButton" runat="server" CommandName="Edit"
                                Text="Edit" />
                            <asp:Button ID="DeleteButton" runat="server" CommandName="Delete"
                                Text="Delete" />
                        </td>
                    </tr>
                </ItemTemplate>
                <EditItemTemplate>
                    <tr runat="server">
                        <td colspan="2" style="text-align: center">
                            <asp:Label ID="PostIdLabel" runat="server" Visible="false"
                                Text='<%#Eval("PostId") %>' />
                                <asp:Button runat="server" ID="EditPostDateButton" OnClick="EditPostDateButton_Click" Text="Not Set"></asp:Button>
                                <div runat="server" id="EditCalendarArea" visible="false">
                                    <br />
                                    <asp:Calendar runat="server" ID="EditCalendar"></asp:Calendar>
                                    <br />
                                    <asp:Button runat="server" ID="EditSetCalendarDate" OnClick="EditSetCalendarDate_Click" Text="Set Date"></asp:Button>
                                    <asp:Label runat="server" ID="EditCalendarErrorLabel" Visible="false"></asp:Label>
                                </div>
                                <asp:DropDownList runat="server" ID="EditHoursDropDown"></asp:DropDownList>: 
                                <asp:DropDownList runat="server" ID="EditQuarterDropDowns"></asp:DropDownList>
                            <br />
                            <asp:TextBox ID="PostTextLabel" runat="server" Visible='<%#Eval("PostText") != null %>'
                                Text='<%#Eval("PostText") %>' />
                            <br />
                            <asp:Panel runat="server" Visible='<%# !string.IsNullOrEmpty(Eval("AttachmentPath").ToString()) %>'>
                                <asp:Image runat="server" ID="PostImage"
                                    ImageUrl='<%#Eval("AttachmentPath") %>' />
                            <asp:Button ID="DeleteImageButton" runat="server" Text="Delete Image (This will permanently delete the image)" OnClick="DeleteImageButton_Click"  />
                            </asp:Panel>
                            <asp:FileUpload ID="NewImageUpload" runat="server" /> Uploading new attachment will override existing one on update.
                            <br />
                            <asp:TextBox ID="MediaChannelsLabel" runat="server"
                                Text='<%# Eval("MediaChannels") %>' />
                            <br />
                            <asp:UpdatePanel ID="UpdateButtonPanel" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <asp:Button ID="UpdateButton" runat="server" CommandName="Update" Text="Update" />
                                </ContentTemplate>
                                <Triggers>
                                    <asp:PostBackTrigger ControlID="UpdateButton" />
                                </Triggers>
                            </asp:UpdatePanel>
                            <asp:Button ID="CancelButton" runat="server" CommandName="Cancel" Text="Cancel" />
                            <asp:Label ID="EditingErrorLabel" runat="server"></asp:Label>
                        </td>
                    </tr>
                </EditItemTemplate>
            </asp:ListView>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
