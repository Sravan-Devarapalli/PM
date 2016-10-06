﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using PraticeManagement.Reports;


namespace PraticeManagement.Controls.Reports.ConsultantDemand
{
    public partial class ConsultingDemandGraphs : System.Web.UI.UserControl
    {
        private const string MAIN_CHART_AREA_NAME = "MainArea";

        public string PipeLineTitle = "Pipeline Demand By Month";

        public string PipeLineSkill = "Skill Set Demand By Month";

        public LinkButton hlinkGraphs { get { return hlnkGraph; } }

        public ModalPopupExtender ConsultantDetailPopup
        {
            get
            {
                return mpeDetailView;

            }
        }

        private PraticeManagement.Reports.ConsultingDemand_New HostingPage
        {
            get { return ((PraticeManagement.Reports.ConsultingDemand_New)Page); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                chartConsultnDemandPipeline.Visible = false;
            }
        }

        protected void chartConsultnDemandPipeline_Click(object sender, ImageMapEventArgs e)
        {
            string[] postBackDetails = e.PostBackValue.Split(',');

            ctrDetails.BtnExportPipeLineSelectedValue = postBackDetails[0].ToString();

            if (hlnkGraph.Text == PipeLineSkill)
            {
                HostingPage.GraphType = ConsultingDemand_New.PipelineTitle;
            }
            else
            {
                HostingPage.GraphType = ConsultingDemand_New.PipelineSkill;
            }
            lblMonth.Text = ctrDetails.BtnExportPipeLineSelectedValue;
            ctrDetails._hdIsGraphPage.Value = true.ToString();
            ctrDetails.PopulateData(true);
            lblCount.Text = HostingPage.RolesCount.ToString();
            lblCount.Text += lblCount.Text != "1" ? " Roles" : " Role";
            mpeDetailView.Show();
        }

        protected void chartConsultngDemand_Click(object sender, ImageMapEventArgs e)
        {
            string[] postBackDetails = e.PostBackValue.Split(',');
            ctrDetails._hdIsGraphPage.Value = true.ToString();
            DateTime SelectedMonthStartdate = Utils.Calendar.MonthStartDate(Convert.ToDateTime(DateTime.ParseExact(postBackDetails[0], "MMM yy", null)));
            DateTime SelectedMonthEnddate = Utils.Calendar.MonthEndDate(Convert.ToDateTime(DateTime.ParseExact(postBackDetails[0], "MMM yy", null)));
            ctrDetails.BtnExportSelectedStartDate = SelectedMonthStartdate > HostingPage.StartDate ? SelectedMonthStartdate.ToString() : HostingPage.StartDate.ToString();
            ctrDetails.BtnExportSelectedEndDate = SelectedMonthEnddate < HostingPage.EndDate ? SelectedMonthEnddate.ToString() : HostingPage.EndDate.ToString();
            lblMonth.Text = "Month: " + "" + Utils.Calendar.MonthStartDate(Convert.ToDateTime(DateTime.ParseExact(postBackDetails[0], "MMM yy", null))).ToString(Constants.Formatting.FullMonthYearFormat);
            ctrDetails.PopulateData(true);
            lblCount.Text = HostingPage.RolesCount.ToString();
            lblCount.Text += lblCount.Text != "1" ? " Roles" : " Role";
            mpeDetailView.Show();
        }

        public void PopulateGraph()
        {
            Dictionary<string, int> data = new Dictionary<string, int>();
            if (HostingPage.GraphType == ConsultingDemand_New.TransactionTitle)
            {
                chartConsultnDemandPipeline.Visible = false;
                chartConsultngDemand.Visible = true;
                hlnkGraph.Visible = false;
                data = ServiceCallers.Custom.Report(r => r.ConsultingDemandGraphsByTitle(HostingPage.StartDate.Value, HostingPage.EndDate.Value, HostingPage.isSelectAllTitles ? null : HostingPage.hdnTitlesProp, HostingPage.isSelectAllSalesStages ? null : HostingPage.hdnSalesStagesProp));
                chartConsultngDemand.DataSource = data.Select(p => new { month = p.Key, count = p.Value }).ToList();
                chartConsultngDemand.DataBind();
                InitChart(data.Count, data.Max(p => p.Value));
            }
            else if (HostingPage.GraphType == ConsultingDemand_New.TransactionSkill)
            {
                chartConsultnDemandPipeline.Visible = false;
                chartConsultngDemand.Visible = true;
                hlnkGraph.Visible = false;
                data = ServiceCallers.Custom.Report(r => r.ConsultingDemandGraphsBySkills(HostingPage.StartDate.Value, HostingPage.EndDate.Value, HostingPage.isSelectAllSkills ? null : HostingPage.hdnSkillsProp, HostingPage.isSelectAllSalesStages ? null : HostingPage.hdnSalesStagesProp));
                chartConsultngDemand.DataSource = data.Select(p => new { month = p.Key, count = p.Value }).ToList();
                chartConsultngDemand.DataBind();
                InitChart(data.Count, data.Max(p => p.Value));
            }
            else
            {
                chartConsultnDemandPipeline.Visible = true;
                chartConsultngDemand.Visible = false;
                hlnkGraph.Visible = true;
                if (HostingPage.GraphType == ConsultingDemand_New.PipelineTitle)
                {
                    hlnkGraph.Text = PipeLineSkill;
                    data = ServiceCallers.Custom.Report(r => r.ConsultingDemandGrphsGroupsByTitle(HostingPage.StartDate.Value, HostingPage.EndDate.Value, HostingPage.isSelectAllSalesStages ? null : HostingPage.hdnSalesStagesProp));
                }
                else if (HostingPage.GraphType == ConsultingDemand_New.PipelineSkill)
                {
                    data = ServiceCallers.Custom.Report(r => r.ConsultingDemandGrphsGroupsBySkill(HostingPage.StartDate.Value, HostingPage.EndDate.Value, HostingPage.isSelectAllSalesStages ? null : HostingPage.hdnSalesStagesProp));
                }
                chartConsultnDemandPipeline.DataSource = data.Select(p => new { title = p.Key, count = p.Value }).ToList();
                chartConsultnDemandPipeline.DataBind();
                InitChart(data.Count, data.Max(p => p.Value));
            }
        }

        private void InitChart(int count, int maxValue)
        {
            if (HostingPage.GraphType == ConsultingDemand_New.TransactionTitle || HostingPage.GraphType == ConsultingDemand_New.TransactionSkill)
            {
                chartConsultngDemand.Width = ((count < 5) ? 5 : count) * 75;
                chartConsultngDemand.Height = 570;
                //chartConsultngDemand.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
                InitAxis(chartConsultngDemand.ChartAreas[MAIN_CHART_AREA_NAME].AxisX, "Month", false);
                InitAxis(chartConsultngDemand.ChartAreas[MAIN_CHART_AREA_NAME].AxisY, "Number of Resources", true);
            }
            else
            {
                chartConsultnDemandPipeline.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
                int width = ((maxValue < 10) ? 10 : maxValue) * 50;
                width = width < 800 ? width : 800;
                chartConsultnDemandPipeline.Width = width;
                InitAxis(chartConsultnDemandPipeline.ChartAreas[0].AxisY, "Quantity", false, 0);
                chartConsultnDemandPipeline.ChartAreas[0].AxisY.Interval = (double)(maxValue / 10);
                if (HostingPage.GraphType == ConsultingDemand_New.PipelineTitle)
                {
                    chartConsultnDemandPipeline.Height = count * 50 >= 520 ? count * 50 : 520;
                    InitAxis(chartConsultnDemandPipeline.ChartAreas[0].AxisX, "Title", true);
                }
                else
                {
                    chartConsultnDemandPipeline.Height = count * 50 >= 520 ? count * 50 : 520;
                    InitAxis(chartConsultnDemandPipeline.ChartAreas[0].AxisX, "Skill Set", true);
                }
            }
            chartConsultnDemandPipeline.ChartAreas[0].AxisY2.TextOrientation = TextOrientation.Rotated90;
            UpdateChartTitle();
        }

        /// <summary>
        /// Updates the title of graph depends on link button's text.
        /// </summary>
        private void UpdateChartTitle()
        {
            int multipleSelected;
            int multipleSalesStagesSelected = HostingPage.hdnSalesStagesProp.Where(t => t == ',').Count();
            Title salesStageTitle = new Title();
            string salesStageToolTip = HostingPage.isSelectAllSalesStages ? "All Sales Stages" : HostingPage.hdnSalesStagesProp.TrimEnd(',');
            salesStageTitle.Text = HostingPage.isSelectAllSalesStages ? "All Sales Stages" : (multipleSalesStagesSelected > 1 ? "Multiple Sales Stages Selected" : HostingPage.hdnSalesStagesProp.TrimEnd(','));
            salesStageTitle.ToolTip = GetShrinkToolTip(salesStageToolTip);
            chartConsultngDemand.Titles.Clear();
            chartConsultnDemandPipeline.Titles.Clear();
            if (HostingPage.GraphType == ConsultingDemand_New.TransactionTitle)
            {
                multipleSelected = HostingPage.hdnTitlesProp.Where(t => t == ',').Count();
                chartConsultngDemand.Titles.Add("Resource Demand By Title");
                Title title = new Title();
                title.Text = HostingPage.isSelectAllTitles ? "All Titles" : (multipleSelected > 1 ? "Multiple Titles Selected" : HostingPage.hdnTitlesProp.TrimEnd(','));
                title.ToolTip = GetShrinkToolTip(HostingPage.isSelectAllTitles ? "All Titles" : HostingPage.hdnTitlesProp.TrimEnd(','));
                chartConsultngDemand.Titles.Add(title);
                chartConsultngDemand.Titles.Add(salesStageTitle);                
                chartConsultngDemand.Titles[0].Font = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
                chartConsultngDemand.Titles[1].Font = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
                chartConsultngDemand.Titles[2].Font = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
            }
            else if (HostingPage.GraphType == ConsultingDemand_New.TransactionSkill)
            {
                multipleSelected = HostingPage.hdnSkillsProp.Where(s => s == ',').Count();
 
                chartConsultngDemand.Titles.Add("Resource Demand By Skill");
                Title title = new Title();
                title.Text = HostingPage.isSelectAllSkills ? "All Skills" : (multipleSelected > 1 ? "Multiple Skills Selected" : HostingPage.hdnSkillsProp.TrimEnd(','));
                title.ToolTip = GetShrinkToolTip(HostingPage.isSelectAllSkills ? "All Skills" : HostingPage.hdnSkillsProp.TrimEnd(','));
                chartConsultngDemand.Titles.Add(title);
                chartConsultngDemand.Titles.Add(salesStageTitle);
                chartConsultngDemand.Titles[0].Font = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
                chartConsultngDemand.Titles[1].Font = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
                chartConsultngDemand.Titles[2].Font = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
            }
            else
            {
                if (HostingPage.GraphType == ConsultingDemand_New.PipelineSkill)
                {
                    chartConsultnDemandPipeline.Titles.Add(PipeLineSkill);
                }
                else
                {
                    chartConsultnDemandPipeline.Titles.Add(PipeLineTitle);
                }
                
                chartConsultnDemandPipeline.Titles.Add(HostingPage.StartDate.Value.Month == HostingPage.EndDate.Value.Month ? HostingPage.StartDate.Value.ToString(Constants.Formatting.FullMonthYearFormat) : HostingPage.StartDate.Value.ToString(Constants.Formatting.FullMonthYearFormat) + " - " + HostingPage.EndDate.Value.ToString(Constants.Formatting.FullMonthYearFormat));
                chartConsultnDemandPipeline.Titles.Add(salesStageTitle);
                chartConsultnDemandPipeline.Titles[0].Font = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
                chartConsultnDemandPipeline.Titles[1].Font = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
                chartConsultnDemandPipeline.Titles[2].Font = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
               
            }
        }

        private string GetShrinkToolTip(string toolTip)
        {
            int index = 80;
            while (index < toolTip.Length)
            {
                index = toolTip.Substring(0, index).LastIndexOf(',');
                toolTip = toolTip.Substring(0, index + 1) + " " + toolTip.Substring(index + 1);
                index = index + 80;
            }
            return toolTip;
        }

        /// <summary>
        /// Intiates axis's style.
        /// </summary
        private void InitAxis(Axis horizAxis, string title, bool isVertical, int labelAngle = -1)
        {
            horizAxis.IsStartedFromZero = true;
            horizAxis.TextOrientation = isVertical ? TextOrientation.Rotated270 : TextOrientation.Horizontal;
            //  horizAxis.LabelStyle.Angle = labelAngle != -1 ? labelAngle : isVertical ? 0 : 45;
            horizAxis.LabelStyle.Font = new Font("Arial", 10f);
            horizAxis.TitleFont = new System.Drawing.Font("Arial", 14, FontStyle.Bold);
            horizAxis.ArrowStyle = AxisArrowStyle.None;
            horizAxis.MajorGrid.Enabled = false;
            horizAxis.ToolTip = horizAxis.Title = title;
        }

        protected void hlnkGraph_Click(object sender, EventArgs e)
        {
            if (hlnkGraph.Text == PipeLineTitle)
            {
                hlnkGraph.Text = PipeLineSkill;
                HostingPage.GraphType = ConsultingDemand_New.PipelineTitle;
            }
            else
            {
                hlnkGraph.Text = PipeLineTitle;
                HostingPage.GraphType = ConsultingDemand_New.PipelineSkill;
            }
            PopulateGraph();
        }
    }
}

