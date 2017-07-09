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
    public partial class Goals : System.Web.UI.Page
    {
        private List<Goal> _userGoals;
        private IGoalHandler _goalHandler;
        private string[] metrics = { "Exposure", "Influence", "Engagement" };
        private static DataTable goalDataTable;

        protected void Page_Load(object sender, EventArgs e)
        {
             _userGoals = _goalHandler.GetUserGoals(UserSingleton.Instance.CurrentUser.UserId);
            currentGoalDropDown.DataSource = _userGoals.Select(s => s.GoalName);
            currentGoalDropDown.DataBind();

            if(!string.IsNullOrEmpty(Request.QueryString["GoalName"]))
                currentGoalDropDown.SelectedIndex = _userGoals.FindIndex(goal => goal.GoalName == Request.QueryString["GoalName"]);

            metricDropDown.DataSource = metrics;
            metricDropDown.DataBind();

            UpdateGoalDataTable();
        }

        private void UpdateGoalDataTable()
        {
            if (_userGoals.Count() > 0)
            {
                goalDataTable = new DataTable();
                goalDataTable.Columns.Add("Source", typeof(string));
                goalDataTable.Columns.Add("Week", typeof(string));
                goalDataTable.Columns.Add("Metric", typeof(string));
                goalDataTable.Columns.Add("Target", typeof(int));
                goalDataTable.Columns.Add("Progress", typeof(int));
                goalDataTable.Columns.Add("AccumulatedProgress", typeof(int));
                Goal selectedGoal = _userGoals.Where(x => x.GoalName == currentGoalDropDown.SelectedValue).First();
                JObject progress = JObject.Parse(selectedGoal.Progress);
                JObject target = JObject.Parse(selectedGoal.Targets);

                foreach (JProperty media in progress.Children())
                {
                    foreach (JProperty week in media.Values().OrderBy(o => o.Path.Substring(4)))
                    {
                        foreach (JProperty metric in week.Values())
                        {
                            DataRow row = goalDataTable.NewRow();
                            row[0] = media.Name;
                            row[1] = week.Name.Substring(4);
                            row[2] = metric.Name;
                            row[3] = target[media.Name][metric.Name];
                            row[4] = metric.Value;

                            if (goalDataTable.AsEnumerable().Where(w => w[0].ToString() == media.Name && w[2].ToString() == metric.Name).Count() == 0)
                            {
                                row[5] = metric.Value;
                            }
                            else
                            {
                                row[5] = int.Parse(metric.Value.ToString())
                                            + int.Parse(goalDataTable.AsEnumerable().Where(w => w[0].ToString() == media.Name && w[2].ToString() == metric.Name).Last()[5].ToString());
                            }

                            goalDataTable.Rows.Add(row);
                        }
                    }
                }

                foreach (var groupRow in goalDataTable.AsEnumerable().GroupBy(g => new { Week = g[1], Metric = g[2] }))
                {
                    DataRow row = goalDataTable.NewRow();
                    row[0] = "Overall";
                    row[1] = groupRow.First()[1];
                    row[2] = groupRow.First()[2];
                    row[3] = groupRow.Sum(x => int.Parse(x[3].ToString()));
                    row[4] = groupRow.Sum(x => int.Parse(x[4].ToString()));

                    if (goalDataTable.AsEnumerable().Where(w => w[0].ToString() == "Overall" && w[2].ToString() == groupRow.First()[1].ToString()).Count() == 0)
                    {
                        row[5] = groupRow.Sum(x => int.Parse(x[5].ToString()));
                    }
                    else
                    {
                        row[5] = groupRow.Sum(x => int.Parse(x[5].ToString()))
                                    + int.Parse(goalDataTable.AsEnumerable().Where(w => w[0].ToString() == "Overall" && w[2].ToString() == groupRow.First()[1].ToString()).Last()[5].ToString());
                    }

                    goalDataTable.Rows.Add(row);

                }

                UpdateGoals();
            }
        }

        private void UpdateGoals()
        {
            if (goalChart.Series.Any() || goalChart.ChartAreas.Any() || goalChart.Titles.Any())
            {
                goalChart.Series.Clear();
                goalChart.ChartAreas.Clear();
                goalChart.Titles.Clear();
            }

            foreach (var mediaRows in goalDataTable.AsEnumerable().Where(w => w[2].ToString() == metricDropDown.SelectedValue.ToString())
                                            .OrderBy(o => o[1]).GroupBy(x => x[0]))
            {
                Series progressSeries = new Series();
                Series limitSeries = new Series();
                ChartArea mediaChart = new ChartArea();

                mediaChart.Name = mediaRows.First()[0].ToString();
                mediaChart.AxisX.IsMarginVisible = false;
                mediaChart.AxisX.IsMarginVisible = false;
                mediaChart.AxisX.Title = goalDataTable.Columns[1].ToString();
                mediaChart.AxisY.Minimum = double.Parse(mediaRows.First()[5].ToString());
                mediaChart.AxisY.Maximum = double.Parse(mediaRows.Last()[5].ToString()) > double.Parse(mediaRows.Last()[3].ToString())
                                                ? double.Parse(mediaRows.Last()[5].ToString()) + (double.Parse(mediaRows.Last()[5].ToString()) / 10) :
                                                double.Parse(mediaRows.Last()[3].ToString()) + (double.Parse(mediaRows.Last()[3].ToString()) / 10);
                progressSeries.Name = mediaRows.First()[0].ToString() + "Progress";
                progressSeries.ChartType = SeriesChartType.Line;
                progressSeries.Points.DataBind(mediaRows, goalDataTable.Columns[1].ToString(), goalDataTable.Columns[5].ToString(), null);

                limitSeries.Name = mediaRows.First()[0].ToString() + "Limit";
                limitSeries.ChartType = SeriesChartType.Line;
                limitSeries.Points.DataBind(mediaRows, goalDataTable.Columns[1].ToString(), goalDataTable.Columns[3].ToString(), null);

                goalChart.ChartAreas.Add(mediaChart);
                goalChart.Series.Add(progressSeries);
                goalChart.Series.Add(limitSeries);

                foreach (Series series in goalChart.Series.Where(s => s.Name.Contains(mediaRows.First()[0].ToString())))
                    series.ChartArea = mediaChart.Name;

                goalChart.Titles.Add(mediaChart.Name);
                goalChart.Titles[goalChart.ChartAreas.Count() - 1].DockedToChartArea = mediaChart.Name;
                goalChart.Titles[goalChart.ChartAreas.Count() - 1].IsDockedInsideChartArea = false;
            }

            goalChart.DataBind();
        }

        protected void metricDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGoals();
        }

        protected void currentGoalDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGoalDataTable();
        }
    }
}