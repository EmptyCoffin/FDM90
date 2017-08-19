using FDM90.Handlers;
using FDM90.Models;
using FDM90.Singleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace FDM90.Pages.Content
{
    public partial class Scheduler : System.Web.UI.Page
    {
        private ISchedulerHandler _schedulerHandler;
        private static List<ScheduledPost> _scheduledPosts;
        private string[] imageSuffixes = new string[] { "jpg", "png" };
        static List<string> channels = new List<string>();
        static int[] hoursInTheDay = new int[24];

        public Scheduler():this(new SchedulerHandler())
        {

        }

        public Scheduler(ISchedulerHandler schedulerHandler)
        {
            _schedulerHandler = schedulerHandler;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(!Page.IsPostBack)
            {
                channels.Clear();
                hoursInTheDay.ToList().Clear();
                
                for (int i = 0; i < hoursInTheDay.Length; i++)
                {
                    hoursInTheDay[i] = i;
                }

                HoursDropDown.DataSource = hoursInTheDay;
                HoursDropDown.DataBind();
                QuarterDropDowns.DataSource = new int[] { 00, 15, 30, 45 };
                QuarterDropDowns.DataBind();

                foreach (var prop in UserSingleton.Instance.CurrentUser.GetType().GetProperties())
                {
                    object[] attrs = prop.GetCustomAttributes(true);
                    foreach (object attr in attrs)
                    {
                        IntegratedMediaChannelAttribute channel = attr as IntegratedMediaChannelAttribute;
                        if (channel != null && (bool)prop.GetValue(UserSingleton.Instance.CurrentUser) == true)
                        {
                            channels.Add(channel.MediaChannelName);
                        }
                    }
                }

                MediaChannelsCheckBoxList.DataSource = channels;
                MediaChannelsCheckBoxList.DataBind();

                GetUserSchedule();
            }
        }

        private void GetUserSchedule()
        {
            _scheduledPosts = _schedulerHandler.GetSchedulerPostsForUser(UserSingleton.Instance.CurrentUser.UserId).ToList();

            SchedulerPanel.Visible = true;
            ScheduledPostsList.DataSource = _scheduledPosts;
            ScheduledPostsList.DataBind();
        }

        protected void PostButton_Click(object sender, EventArgs e)
        {
            if (PostAttachement.HasFile)
            {
                if (imageSuffixes.Contains(PostAttachement.FileName.Substring(PostAttachement.FileName.LastIndexOf('.') + 1)))
                {
                    PostAttachement.SaveAs(ConfigSingleton.FileSaveLocation + PostAttachement.FileName);
                }
            }

            ScheduledPost newPost = new ScheduledPost()
            {
                UserId = UserSingleton.Instance.CurrentUser.UserId,
                PostText = PostText.Text,
                AttachmentPath = PostAttachement.HasFile ? ConfigSingleton.FileSaveLocation + PostAttachement.FileName : null,
                MediaChannels = string.Join(", ", MediaChannelsCheckBoxList.Items.Cast<ListItem>().Where(w => w.Selected).Select(s => s.Text))
            };

            DateTime postDate;
            if(DateTime.TryParse(PostDateButton.Text, out postDate))
                newPost.PostTime = new DateTime(postDate.Year, postDate.Month, postDate.Day, int.Parse(HoursDropDown.SelectedValue), int.Parse(QuarterDropDowns.SelectedValue), 00);

            if (PostNowCheckbox.Checked)
            {
                _schedulerHandler.PostNow(newPost);
            }
            else
            {                
                _schedulerHandler.CreateScheduledPost(newPost);
                GetUserSchedule();
            }
        }

        protected void ScheduledPostsList_ItemEditing(object sender, ListViewEditEventArgs e)
        {
            ScheduledPostsList.EditIndex = e.NewEditIndex;
            ScheduledPost edittingPost = null;
            var label = (ScheduledPostsList.Items[e.NewEditIndex].FindControl("PostIdLabel")) as Label;
            if (label != null && _scheduledPosts.Any(x => x.PostId.Equals(Guid.Parse(label.Text))))
                edittingPost = _scheduledPosts.First(x => x.PostId.Equals(Guid.Parse(label.Text)));

            ScheduledPostsList.DataSource = _scheduledPosts;
            ScheduledPostsList.DataBind();

            (ScheduledPostsList.Items[ScheduledPostsList.EditIndex].FindControl("EditPostDateButton") as Button).Text = edittingPost.PostTime.Date.ToShortDateString();

            var edittingHourDropDown = (ScheduledPostsList.Items[ScheduledPostsList.EditIndex].FindControl("EditHoursDropDown") as DropDownList);
            edittingHourDropDown.DataSource = hoursInTheDay;
            edittingHourDropDown.DataBind();
            edittingHourDropDown.SelectedIndex = edittingPost.PostTime.Hour;

            var edittingQuarterDropDown = (ScheduledPostsList.Items[ScheduledPostsList.EditIndex].FindControl("EditQuarterDropDowns") as DropDownList);
            edittingQuarterDropDown.DataSource = new int[] { 00, 15, 30, 45 };
            edittingQuarterDropDown.DataBind();
            edittingQuarterDropDown.SelectedValue = edittingPost.PostTime.Minute.ToString();
        }

        protected void ScheduledPostsList_ItemCanceling(object sender, ListViewCancelEventArgs e)
        {
            ScheduledPostsList.EditIndex = -1;
            ScheduledPostsList.DataSource = _scheduledPosts;
            ScheduledPostsList.DataBind();
        }

        protected void ScheduledPostsList_ItemUpdating(object sender, ListViewUpdateEventArgs e)
        {
            ScheduledPost editedPost = null;
            string errorMessage = string.Empty;
            var label = (ScheduledPostsList.Items[e.ItemIndex].FindControl("PostIdLabel")) as Label;
            if (label != null && _scheduledPosts.Any(x => x.PostId.Equals(Guid.Parse(label.Text))))
            {
                editedPost = _scheduledPosts.First(x => x.PostId.Equals(Guid.Parse(label.Text)));
            }
            else
            {
                errorMessage += "Post doesn't have a Post ID!";
            }

            var date = (ScheduledPostsList.Items[e.ItemIndex].FindControl("EditPostDateButton")) as Button;
            var hour = (ScheduledPostsList.Items[e.ItemIndex].FindControl("EditHoursDropDown")) as DropDownList;
            var minute = (ScheduledPostsList.Items[e.ItemIndex].FindControl("EditQuarterDropDowns")) as DropDownList;
            if (date != null && hour != null && minute != null)
            {
                string[] dateBreakDown = date.Text.Split(new[] { '/' });
                var parsedDatetime = new DateTime(int.Parse(dateBreakDown[2]), int.Parse(dateBreakDown[1]),
                                            int.Parse(dateBreakDown[0]), int.Parse(hour.SelectedValue), int.Parse(minute.SelectedValue), 00);

                if (parsedDatetime != editedPost.PostTime)
                {
                    if (parsedDatetime > DateTime.Now)
                    {
                        editedPost.PostTime = parsedDatetime;
                    }
                    else
                    {
                        errorMessage += " Post cannot be in the past!";
                    }
                }
            }
            else
            {
                errorMessage += " Post doesn't have a Post Date/Time!";
            }

            var textBox = (ScheduledPostsList.Items[e.ItemIndex].FindControl("PostTextLabel")) as TextBox;
            if (textBox != null && editedPost.PostText != textBox.Text)
                editedPost.PostText = textBox.Text;


            var image = (ScheduledPostsList.Items[e.ItemIndex].FindControl("PostImage")) as Image;
            var newImage = ScheduledPostsList.Items[e.ItemIndex].FindControl("NewImageUpload") as FileUpload;
            if (image != null)
            {
                if (string.IsNullOrEmpty(image.ImageUrl))
                {
                    editedPost.AttachmentPath = null;
                }

                if (newImage.HasFile)
                {
                    var filePath = ConfigSingleton.FileSaveLocation + newImage.FileName;

                    newImage.SaveAs(filePath);
                    editedPost.AttachmentPath = filePath;
                }
            }

            textBox = (ScheduledPostsList.Items[e.ItemIndex].FindControl("MediaChannelsLabel")) as TextBox;
            if (textBox != null && !string.IsNullOrEmpty(textBox.Text) && editedPost.MediaChannels != textBox.Text
                        && !textBox.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Except(channels).Any())
                editedPost.MediaChannels = textBox.Text;

            if(string.IsNullOrEmpty(editedPost.PostText) && string.IsNullOrEmpty(editedPost.AttachmentPath))
            {
                errorMessage += " Post doesn't have Post Text or an Attachment!";
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                _schedulerHandler.UpdateScheduledPost(editedPost);
                ScheduledPostsList.EditIndex = -1;
                GetUserSchedule();
            }
            else
            {
                (ScheduledPostsList.Items[e.ItemIndex].FindControl("EditingErrorLabel") as Label).Text = errorMessage;
            }
        }

        protected void ScheduledPostsList_ItemDeleting(object sender, ListViewDeleteEventArgs e)
        {
            Label lbl = (ScheduledPostsList.Items[e.ItemIndex].FindControl("PostIdLabel")) as Label;
            if (lbl != null && _scheduledPosts.Any(x => x.PostId.Equals(Guid.Parse(lbl.Text))))
            {
                _schedulerHandler.DeleteScheduledPost(Guid.Parse(lbl.Text));
                GetUserSchedule();
            }
        }

        protected void PostDateButton_Click(object sender, EventArgs e)
        {
            DateButtonEvent(sender, calendar, calendarArea);
        }

        private void DateButtonEvent(object sender, Calendar calendar, HtmlGenericControl area)
        {
            DateTime setDate = new DateTime();
            calendar.SelectedDate = DateTime.TryParse(((Button)sender).Text, out setDate) ? setDate : calendar.TodaysDate;
            calendar.VisibleDate = calendar.SelectedDate;

            ((Button)sender).Text = "Setting...";

            area.Visible = true;
        }

        protected void setCalendarDate_Click(object sender, EventArgs e)
        {
            SetCalendar(calendar, calendarArea, PostDateButton, calendarErrorLabel);
        }

        private void SetCalendar(Calendar calendar, HtmlGenericControl area, Button originalButton, Label errorLabel)
        {
            errorLabel.Visible = false;
            errorLabel.Text = string.Empty;

            if (calendar.SelectedDate >= calendar.TodaysDate)
            {
                originalButton.Text = calendar.SelectedDate.ToShortDateString();
            }
            else
            {
                errorLabel.Visible = true;
                errorLabel.Text = "Cannot Post in the Past";
            }

            area.Visible = !string.IsNullOrWhiteSpace(errorLabel.Text);
        }

        protected void PostNowCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            schedulerArea.Visible = !PostNowCheckbox.Checked;
            PostButton.Text = PostNowCheckbox.Checked ? "Post" : "Schedule";
        }

        protected void DeleteImageButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty((ScheduledPostsList.Items[ScheduledPostsList.EditIndex].FindControl("PostTextLabel") as TextBox).Text))
            {
                var postID = ScheduledPostsList.Items[ScheduledPostsList.EditIndex].FindControl("PostIdLabel") as Label;
                var currentImage = ScheduledPostsList.Items[ScheduledPostsList.EditIndex].FindControl("PostImage") as Image;

                _schedulerHandler.DeletePostImage(Guid.Parse(postID.Text), currentImage.ImageUrl);

                currentImage.ImageUrl = "";
                currentImage.Visible = false;
            }
        }

        protected void EditPostDateButton_Click(object sender, EventArgs e)
        {
            DateButtonEvent(sender, ScheduledPostsList.Items[ScheduledPostsList.EditIndex].FindControl("EditCalendar") as Calendar, 
                                ScheduledPostsList.Items[ScheduledPostsList.EditIndex].FindControl("EditCalendarArea") as HtmlGenericControl);
        }

        protected void EditSetCalendarDate_Click(object sender, EventArgs e)
        {
            SetCalendar(ScheduledPostsList.Items[ScheduledPostsList.EditIndex].FindControl("EditCalendar") as Calendar, 
                            ScheduledPostsList.Items[ScheduledPostsList.EditIndex].FindControl("EditCalendarArea") as HtmlGenericControl, 
                                ScheduledPostsList.Items[ScheduledPostsList.EditIndex].FindControl("EditPostDateButton") as Button, 
                                    ScheduledPostsList.Items[ScheduledPostsList.EditIndex].FindControl("EditCalendarErrorLabel") as Label);
        }
    }
}