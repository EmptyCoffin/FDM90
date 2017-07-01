using FDM90.Handlers;
using FDM90.Models;
using FDM90.Singleton;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;

namespace FDM90.Pages.Content
{
    public partial class Home : System.Web.UI.Page
    {
        private string[] metrics = { "Exposure", "Influence", "Engagement" };
        private IGoalHandler _goalHandler;
        private List<string> tableIds = new List<string>();
        private List<Goals> _userGoals;

        public Home() : this(new GoalHandler())
        {

        }

        public Home(IGoalHandler goalHandler)
        {
            _goalHandler = goalHandler;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (UserSingleton.Instance.CurrentUser != null)
            {
                facebookSetUpButton.Visible = !UserSingleton.Instance.CurrentUser.Facebook;
                twitterSetUpButton.Visible = !UserSingleton.Instance.CurrentUser.Twitter;
                goalArea.Visible = true;
                _userGoals = _goalHandler.GetUserGoals(UserSingleton.Instance.CurrentUser.UserId);
                currentGoalDropDown.DataSource = _userGoals?.Count() > 0 ? _userGoals.Select(s => s.GoalName) : new string[] { "No Current Goals" };
                currentGoalDropDown.DataBind();

                UpdateGoalCharts();

                SetUpTableControls();
            }
        }

        private void UpdateGoalCharts()
        {
            if (_userGoals.Count() > 0)
            {
                metricDropDown.DataSource = metrics;
                metricDropDown.DataBind();

                DataTable dt = new DataTable();
                dt.Columns.Add("Source", typeof(string));
                dt.Columns.Add("Week", typeof(string));
                dt.Columns.Add("Metric", typeof(string));
                dt.Columns.Add("Target", typeof(int));
                dt.Columns.Add("Progress", typeof(int));
                dt.Columns.Add("AccumulatedProgress", typeof(int));
                Goals selectedGoal = _userGoals.Where(x => x.GoalName == currentGoalDropDown.SelectedValue).First();
                JObject progress = JObject.Parse(selectedGoal.Progress);
                JObject target = JObject.Parse(selectedGoal.Targets);

                foreach (JProperty media in progress.Children())
                {
                    foreach (JProperty week in media.Values().OrderBy(o => o.Path.Substring(4)))
                    {
                        foreach (JProperty metric in week.Values())
                        {
                            DataRow row = dt.NewRow();
                            row[0] = media.Name;
                            row[1] = week.Name.Substring(4);
                            row[2] = metric.Name;
                            row[3] = target[media.Name][metric.Name];
                            row[4] = metric.Value;

                            if (dt.AsEnumerable().Where(w => w[0].ToString() == media.Name && w[2].ToString() == metric.Name).Count() == 0)
                            {
                                row[5] = metric.Value;
                            }
                            else
                            {
                                row[5] = int.Parse(metric.Value.ToString()) 
                                            + int.Parse(dt.AsEnumerable().Where(w => w[0].ToString() == media.Name && w[2].ToString() == metric.Name).Last()[5].ToString());
                            }

                            dt.Rows.Add(row);
                        }
                    }
                }

                foreach (var mediaRows in dt.AsEnumerable().Where(w => w[2].ToString() == metricDropDown.SelectedValue.ToString())
                                                .OrderBy(o => o[1]).GroupBy(x => x[0]))
                {
                    Series progressSeries = new Series();
                    Series limitSeries = new Series();
                    ChartArea mediaChart = new ChartArea();

                    mediaChart.Name = mediaRows.First()[0].ToString();
                    mediaChart.AxisX.IsMarginVisible = false;
                    mediaChart.AxisX.IsMarginVisible = false;
                    mediaChart.AxisX.Title = dt.Columns[1].ToString();
                    mediaChart.AxisY.Minimum = double.Parse(mediaRows.First()[5].ToString());
                    mediaChart.AxisY.Maximum = double.Parse(mediaRows.Last()[5].ToString()) > double.Parse(mediaRows.Last()[3].ToString()) 
                                                    ? double.Parse(mediaRows.Last()[5].ToString()) : double.Parse(mediaRows.Last()[3].ToString());
                    progressSeries.Name = mediaRows.First()[0].ToString() + "Progress";
                    progressSeries.ChartType = SeriesChartType.Line;
                    progressSeries.Points.DataBind(mediaRows, dt.Columns[1].ToString(), dt.Columns[5].ToString(), null);

                    limitSeries.Name = mediaRows.First()[0].ToString() + "Limit";
                    limitSeries.ChartType = SeriesChartType.Line;
                    limitSeries.Points.DataBind(mediaRows, dt.Columns[1].ToString(), dt.Columns[3].ToString(), null);

                    goalChart.ChartAreas.Add(mediaChart);
                    goalChart.Series.Add(progressSeries);
                    goalChart.Series.Add(limitSeries);

                    foreach(Series series in goalChart.Series.Where(s => s.Name.Contains(mediaRows.First()[0].ToString())))
                        series.ChartArea = mediaChart.Name;

                    goalChart.Titles.Add(mediaChart.Name);
                    goalChart.Titles[goalChart.ChartAreas.Count() - 1].DockedToChartArea = mediaChart.Name;
                    goalChart.Titles[goalChart.ChartAreas.Count() - 1].IsDockedInsideChartArea = false;
                }

                goalChart.DataBind();
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

                newGoalGrid.Rows.Add(tableRow);
            }
        }

        protected void setupGoalButton_Click(object sender, EventArgs e)
        {
            newGoalArea.Visible = true;
            setupGoalButton.Visible = false;
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
                    runningTotal += int.Parse(((TextBox)newGoalGrid.FindControl(media + metric)).Text);
                }

                var overallControl = (Label)newGoalGrid.FindControl("overall" + metric);
                overallControl.Text = runningTotal.ToString();
            }

            for (int i = 2; i < newGoalGrid.Controls.Count; i++)
            {
                for (int j = 1; j < newGoalGrid.Controls[i].Controls.Count; j++)
                {
                    if (newGoalGrid.Controls[i].Controls[j].Controls[0].ID == metricSender)
                    {
                        if (newGoalGrid.Controls[i].Controls.Count != j + 1)
                        {
                            newGoalGrid.Controls[i].Controls[j + 1].Controls[0].Focus();
                        }
                        else
                        {
                            if (newGoalGrid.Controls.Count != i + 1)
                                newGoalGrid.Controls[i + 1].Controls[1].Controls[0].Focus();
                        }
                    }
                }
            }
        }

        protected void newGoalButton_Click(object sender, EventArgs e)
        {
            JObject targets = new JObject();

            foreach (string media in UserSingleton.Instance.CurrentUser.GetType().GetProperties().Where(x =>
                    x.PropertyType == typeof(bool) && bool.Parse(x.GetValue(UserSingleton.Instance.CurrentUser).ToString()))
                       .Select(s => s.Name))
            {
                JObject mediaTarget = new JObject();

                foreach (string metric in metrics)
                {
                    mediaTarget.Add(metric, ((TextBox)newGoalGrid.FindControl(media + metric)).Text);
                }

                targets.Add(media, mediaTarget);
            }
            _goalHandler.CreateGoal(UserSingleton.Instance.CurrentUser.UserId, 
                                goalName.Text, startDateButton.Text, endDateButton.Text, targets.ToString());

            newGoalArea.Visible = false;
            setupGoalButton.Visible = true;
        }
    }
}