using FDM90.Models;
using FDM90.Singleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FDM90.Pages.Content
{
    public partial class Home : System.Web.UI.Page
    {
        private string[] metrics = { "Exposure", "Influence" };
        protected void Page_Load(object sender, EventArgs e)
        {
            if (UserSingleton.Instance.CurrentUser != null)
            {
                facebookSetUpButton.Visible = !UserSingleton.Instance.CurrentUser.Facebook;
                twitterSetUpButton.Visible = !UserSingleton.Instance.CurrentUser.Twitter;
                goalArea.Visible = true;
                string[] userGoals = null;
                currentGoalDropDown.DataSource = userGoals?.Count() > 0 ? userGoals : new string[] { "No Current Goals" };
                SetUpTableControls();
                startDateButton.Click += new EventHandler(StartCalendar);
                endDateButton.Click += new EventHandler(StartCalendar);
                setCalendarDate.Click += new EventHandler(SetDate);
            }
        }

        protected void StartCalendar(object sender, EventArgs e)
        {
            DateTime setDate = new DateTime();
            calendar.SelectedDate = DateTime.TryParse(((Button)sender).Text, out setDate) ? setDate : calendar.TodaysDate;

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
                if (calendar.SelectedDate < DateTime.Parse(startDateButton.Text))
                {
                    endDateButton.Text = calendar.SelectedDate.ToShortDateString();
                }
                else
                {
                    calendarErrorLabel.Visible = true;
                    calendarErrorLabel.Text = "End date must be greater than start date";
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
                    textBox.TextChanged += new EventHandler(textBox_Changed);
                    textBox.Text = "0";
                    mediaCell.Controls.Add(textBox);

                    tableRow.Cells.Add(mediaCell);
                }

                newGoalGrid.Rows.Add(tableRow);
            }
        }

        protected void setupGoalButton_Click(object sender, EventArgs e)
        {
            newGoalArea.Visible = true;
        }

        protected void textBox_Changed(object sender, EventArgs e)
        {
            string metricSender = ((TextBox)sender).ID;

            foreach(string metric in metrics.Where(w => metricSender.Contains(w)))
            {
                int runningTotal = 0;
                foreach (string media in UserSingleton.Instance.CurrentUser.GetType().GetProperties().Where(x =>
                     x.PropertyType == typeof(bool) && bool.Parse(x.GetValue(UserSingleton.Instance.CurrentUser).ToString()))
                        .Select(s => s.Name))
                {
                    runningTotal += int.Parse(((TextBox)newGoalGrid.FindControl(media + metric)).Text);
                }

                var overallControl = (Label)newGoalGrid.FindControl("overall" + metric);
                overallControl.Text = runningTotal.ToString();
            }
        }
    }
}