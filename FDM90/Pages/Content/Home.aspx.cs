using FDM90.Handlers;
using FDM90.Models;
using FDM90.Singleton;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;

namespace FDM90.Pages.Content
{
    [ExcludeFromCodeCoverage]
    public partial class Home : System.Web.UI.Page
    {
        private string[] metrics = { "Exposure", "Influence", "Engagement", "Acquisition" };
        private ICampaignHandler _campaignHandler;
        private List<string> tableIds = new List<string>();

        public Home() : this(new CampaignHandler())
        {

        }

        public Home(ICampaignHandler campaignHandler)
        {
            _campaignHandler = campaignHandler;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (UserSingleton.Instance.CurrentUser != null)
            {
                if (!Page.IsPostBack)
                {
                    facebookSetUpButton.Visible = !UserSingleton.Instance.CurrentUser.Facebook;
                    twitterSetUpButton.Visible = !UserSingleton.Instance.CurrentUser.Twitter;
                    campaignArea.Visible = true;
                }
                SetUpTableControls();
            }
        }

        protected void StartCalendar(object sender, EventArgs e)
        {
            DateTime setDate = new DateTime();
            calendar.SelectedDate = DateTime.TryParse(((Button)sender).Text, out setDate) ? setDate : calendar.TodaysDate;
            calendar.VisibleDate = calendar.SelectedDate;

            ((Button)sender).Text = "Setting...";

            calendarArea.Visible = true;
        }

        protected void SetDate(object sender, EventArgs e)
        {
            calendarErrorLabel.Visible = false;
            calendarErrorLabel.Text = string.Empty;
            if (startDateButton.Text == "Setting...")
            {
                if (calendar.SelectedDate > calendar.TodaysDate.AddDays(-30))
                {
                    startDateButton.Text = calendar.SelectedDate.ToShortDateString();
                }
                else
                {
                    calendarErrorLabel.Visible = true;
                    calendarErrorLabel.Text = "Start date must be within the past 30 days";
                }
            }
            else
            {
                if (calendar.SelectedDate > DateTime.Parse(startDateButton.Text) && calendar.SelectedDate < DateTime.Parse(startDateButton.Text).AddYears(1))
                {
                    endDateButton.Text = calendar.SelectedDate.ToShortDateString();
                }
                else
                {
                    calendarErrorLabel.Visible = true;
                    calendarErrorLabel.Text = "End date must be greater than start date and within a year of the start date";
                }
            }
            calendarArea.Visible = !string.IsNullOrWhiteSpace(calendarErrorLabel.Text);
        }

        protected void facebookSetUpButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("Facebook.aspx");
        }
        protected void twitterSetUpButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("Twitter.aspx");
        }

        private void SetUpTableControls()
        {
            foreach (string media in UserSingleton.Instance.CurrentUser.GetType().GetProperties().Where(x =>
                    x.PropertyType == typeof(bool) && bool.Parse(x.GetValue(UserSingleton.Instance.CurrentUser).ToString()))
                        .Select(s => s.Name))
            {
                TableRow tableRow = new TableRow();
                TableCell metricCell = new TableCell();
                metricCell.Text = media;
                tableRow.Cells.Add(metricCell);

                foreach (string metric in metrics)
                {
                    TableCell mediaCell = new TableCell();
                    TextBox textBox = new TextBox();
                    textBox.ID = media + metric;
                    tableIds.Add(media + metric);
                    textBox.TextChanged += new EventHandler(textBox_Changed);
                    textBox.Text = "0";
                    textBox.AutoPostBack = true;
                    mediaCell.Controls.Add(textBox);

                    tableRow.Cells.Add(mediaCell);
                }

                newCampaignGrid.Rows.Add(tableRow);
            }
        }

        protected void setupCampaignButton_Click(object sender, EventArgs e)
        {
            newCampaignArea.Visible = true;
            setupCampaignButton.Visible = false;
        }

        protected void textBox_Changed(object sender, EventArgs e)
        {
            string metricSender = ((TextBox)sender).ID;

            foreach (string metric in metrics.Where(w => metricSender.Contains(w)))
            {
                int runningTotal = 0;
                foreach (string media in UserSingleton.Instance.CurrentUser.GetType().GetProperties().Where(x =>
                     x.PropertyType == typeof(bool) && bool.Parse(x.GetValue(UserSingleton.Instance.CurrentUser).ToString()))
                        .Select(s => s.Name))
                {
                    runningTotal += int.Parse(((TextBox)newCampaignGrid.FindControl(media + metric)).Text);
                }

                var overallControl = (Label)newCampaignGrid.FindControl("overall" + metric);
                overallControl.Text = runningTotal.ToString();
            }

            for (int i = 2; i < newCampaignGrid.Controls.Count; i++)
            {
                for (int j = 1; j < newCampaignGrid.Controls[i].Controls.Count; j++)
                {
                    if (newCampaignGrid.Controls[i].Controls[j].Controls[0].ID == metricSender)
                    {
                        if (newCampaignGrid.Controls[i].Controls.Count != j + 1)
                        {
                            newCampaignGrid.Controls[i].Controls[j + 1].Controls[0].Focus();
                        }
                        else
                        {
                            if (newCampaignGrid.Controls.Count != i + 1)
                                newCampaignGrid.Controls[i + 1].Controls[1].Controls[0].Focus();
                        }
                    }
                }
            }
        }

        protected void newCampaignButton_Click(object sender, EventArgs e)
        {
            JObject targets = new JObject();

            foreach (string media in UserSingleton.Instance.CurrentUser.GetType().GetProperties().Where(x =>
                    x.PropertyType == typeof(bool) && bool.Parse(x.GetValue(UserSingleton.Instance.CurrentUser).ToString()))
                       .Select(s => s.Name))
            {
                JObject mediaTarget = new JObject();

                foreach (string metric in metrics)
                {
                    mediaTarget.Add(metric, ((TextBox)newCampaignGrid.FindControl(media + metric)).Text);
                }

                targets.Add(media, mediaTarget);
            }

            TaskListSingleton.Instance.CurrentTasks = new List<System.Threading.Tasks.Task>() {
                _campaignHandler.CreateCampaign(UserSingleton.Instance.CurrentUser,
                                    campaignName.Text, startDateButton.Text, endDateButton.Text, targets.ToString())
            };

            UserSingleton.Instance.CurrentUser.Campaigns++;

            newCampaignArea.Visible = false;
            setupCampaignButton.Visible = true;

            Response.Redirect("Campaigns.aspx?CampaignName=" + campaignName.Text);
        }
    }
}