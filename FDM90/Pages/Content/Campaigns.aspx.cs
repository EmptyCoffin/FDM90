using FDM90.Handlers;
using FDM90.Models;
using FDM90.Singleton;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;

namespace FDM90.Pages.Content
{
    [ExcludeFromCodeCoverage]
    public partial class Campaigns : System.Web.UI.Page
    {
        private List<Campaign> _userCampaigns;
        private ICampaignHandler _campaignHandler;
        private IMarketingModelHandler _marketingModelHandler;
        private string[] metrics = { "Exposure", "Influence", "Engagement" };
        private static DataTable campaignDataTable;
        private static MarketingModel[] marketingModels;
        private int Exposure {
            get
            {
                return campaignDataTable == null ? 0 
                    : campaignDataTable.AsEnumerable().Where(w => w[0].ToString() == "Overall" && w[2].ToString() == "Exposure").Sum(s => int.Parse(s[4].ToString()));
            }
        }

        private int Influence
        {
            get
            {
                return campaignDataTable == null ? 0
                    : campaignDataTable.AsEnumerable().Where(w => w[0].ToString() == "Overall" && w[2].ToString() == "Influence").Sum(s => int.Parse(s[4].ToString()));
            }
        }

        private int Engagement
        {
            get
            {
                return campaignDataTable == null ? 0
                    : campaignDataTable.AsEnumerable().Where(w => w[0].ToString() == "Overall" && w[2].ToString() == "Engagement").Sum(s => int.Parse(s[4].ToString()));
            }
        }

        public Campaigns():this(new CampaignHandler(), new MarketingModelHandler())
        {

        }

        public Campaigns(ICampaignHandler campaignHandler, IMarketingModelHandler marketingModelHandler)
        {
            _campaignHandler = campaignHandler;
            _marketingModelHandler = marketingModelHandler;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (TaskListSingleton.Instance.CurrentTasks.Count() > 0)
                    TaskListSingleton.Instance.CurrentTasks.First().Wait();

                LoadData();
            }
        }

        private void LoadData()
        {
            _userCampaigns = _campaignHandler.GetUserCampaigns(UserSingleton.Instance.CurrentUser.UserId);
            currentCampaignDropDown.DataSource = _userCampaigns.Select(s => s.CampaignName);
            currentCampaignDropDown.DataBind();

            if (!string.IsNullOrEmpty(Request.QueryString["CampaignName"]))
                currentCampaignDropDown.SelectedIndex = _userCampaigns.FindIndex(campaign => campaign.CampaignName == Request.QueryString["CampaignName"]);

            metricDropDown.DataSource = metrics;
            metricDropDown.DataBind();

            marketingModels = _marketingModelHandler.GetAllMarketingModels().ToArray();
            var dropdownItems = marketingModels.Select(s => s.Name).ToList();
            dropdownItems.Insert(0, "Please Select A Calculation to Perform");
            marketingModelsDropDown.DataSource = dropdownItems;
            marketingModelsDropDown.DataBind();

            UpdateCampaignDataTable();
        }

        private void UpdateCampaignDataTable()
        {
            Campaign selectedCampaign = _userCampaigns.Where(x => x.CampaignName == currentCampaignDropDown.SelectedValue).First();

            if (_userCampaigns.Count() > 0 && !string.IsNullOrWhiteSpace(selectedCampaign.Progress))
            {
                campaignDataTable = _campaignHandler.GenerateCampaignDataTable(selectedCampaign);

                UpdateCampaigns();
            }
        }

        private void UpdateCampaigns()
        {
            if (campaignChart.Series.Any() || campaignChart.ChartAreas.Any() || campaignChart.Titles.Any())
            {
                campaignChart.Series.Clear();
                campaignChart.ChartAreas.Clear();
                campaignChart.Titles.Clear();
            }

            foreach (var mediaRows in campaignDataTable.AsEnumerable().Where(w => w[2].ToString() == metricDropDown.SelectedValue.ToString())
                                            .OrderBy(o => o[1]).GroupBy(x => x[0]))
            {
                Series progressSeries = new Series();
                Series limitSeries = new Series();
                ChartArea mediaChart = new ChartArea();

                mediaChart.Name = mediaRows.First()[0].ToString();
                mediaChart.AxisX.IsMarginVisible = false;
                mediaChart.AxisX.IsMarginVisible = false;
                mediaChart.AxisX.MajorGrid.Enabled = false;
                mediaChart.AxisY.MajorGrid.Enabled = false;
                mediaChart.AxisX.Title = campaignDataTable.Columns[1].ToString();
                mediaChart.AxisY.Minimum = double.Parse(mediaRows.First()[5].ToString()) > double.Parse(mediaRows.Last()[3].ToString()) ?
                                           double.Parse(mediaRows.Last()[3].ToString()) - (double.Parse(mediaRows.Last()[3].ToString()) / 2) :
                                           double.Parse(mediaRows.First()[5].ToString());
                mediaChart.AxisY.Maximum = double.Parse(mediaRows.Last()[5].ToString()) > double.Parse(mediaRows.Last()[3].ToString())
                                                ? double.Parse(mediaRows.Last()[5].ToString()) + (double.Parse(mediaRows.Last()[5].ToString()) / 10) :
                                                double.Parse(mediaRows.Last()[3].ToString()) + (double.Parse(mediaRows.Last()[3].ToString()) / 10);
                progressSeries.Name = mediaRows.First()[0].ToString() + "Progress";
                progressSeries.ChartType = SeriesChartType.Line;
                progressSeries.MarkerSize = 10;
                progressSeries.MarkerStyle = MarkerStyle.Star10;
                progressSeries.ToolTip = "#VALY";
                progressSeries.IsValueShownAsLabel = true;
                progressSeries.Points.DataBind(mediaRows, campaignDataTable.Columns[1].ToString(), campaignDataTable.Columns[5].ToString(), null);

                limitSeries.Name = mediaRows.First()[0].ToString() + "Limit";
                limitSeries.ChartType = SeriesChartType.Line;
                limitSeries.ToolTip = "#VALY";
                limitSeries.Points.DataBind(mediaRows, campaignDataTable.Columns[1].ToString(), campaignDataTable.Columns[3].ToString(), null);
                limitSeries.Points.Last().IsValueShownAsLabel = true;

                if(mediaChart.Name == "Overall")
                {
                    campaignChart.ChartAreas.Insert(0, mediaChart);
                }
                else
                {
                    campaignChart.ChartAreas.Add(mediaChart);
                }

                campaignChart.Series.Add(progressSeries);
                campaignChart.Series.Add(limitSeries);

                foreach (Series series in campaignChart.Series.Where(s => s.Name.Contains(mediaRows.First()[0].ToString())))
                    series.ChartArea = mediaChart.Name;

                campaignChart.Titles.Add(mediaChart.Name);
                campaignChart.Titles[campaignChart.ChartAreas.Count() - 1].DockedToChartArea = mediaChart.Name;
                campaignChart.Titles[campaignChart.ChartAreas.Count() - 1].IsDockedInsideChartArea = false;
            }

            campaignChart.DataBind();
        }

        protected void metricDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCampaigns();
        }

        protected void currentCampaignDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCampaignDataTable();
        }

        protected void marketingModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            MarketingModel newModel = marketingModels.FirstOrDefault(x => x.Name == marketingModelsDropDown.SelectedValue);
            if (newModel == null) return;

            modelDescriptionLabel.Text = newModel.Description;
            modelMetricLabel.Text = newModel.MetricsUsed;
            modelResultMetricLabel.Text = newModel.ResultMetric;

            var calculationExpression = 
                System.Linq.Dynamic.DynamicExpression.ParseLambda(new[] 
                {
                    Expression.Parameter(typeof(double), "exposure"),
                    Expression.Parameter(typeof(double), "influence"),
                    Expression.Parameter(typeof(double), "engagement"),
                    Expression.Parameter(typeof(double), "totalCost"),
                    Expression.Parameter(typeof(double), "price") },
                    null, newModel.CalculationExpression);

            modelCalculationResultLabel.Text = calculationExpression.Compile()
                                    .DynamicInvoke(Exposure, Influence, Engagement,
                                                    double.Parse(CampaignCostTextBox.Text), double.Parse(AverageCostOfProductsTextBox.Text)).ToString()
                                                            + newModel.ResultMetric;
        }
    }
}